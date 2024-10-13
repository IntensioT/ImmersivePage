using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// using WebSocketSharp;
using NativeWebSocket;



public class TaskListManager : MonoBehaviour
{
    private WebSocket webSocket;
    bool isConnected = false;
    private string _webSocketUrl = "ws://127.0.0.1:5051";
    // public static WebSocket _webSocket;
    private string _getTasksEndpoint = "http://127.0.0.1:5051/tasks";
    private string _filterTasksEndpoint = "http://127.0.0.1:5051/tasks/filter-tasks";
    static string _refreshTokenEndpoint = "http://127.0.0.1:5051/account/refresh-token";


    public TextMeshProUGUI _notificationText;
    public float updateInterval = 1f;
    public GameObject taskPrefab;
    public Transform tasksListParent;
    public TMP_Dropdown statusDropdown;

    // public EditButton editBtnProps;

    public Button _loginBtn;
    public TextMeshProUGUI _jwtTokenText;

    private void Start()
    {
        MainThreadDispatcher.EnsureCreated();
        ConnectToWebSocket();
        StartCoroutine(AutoUpdateTasks());
    }
    private void ConnectToWebSocket()
    {
        string webSocketUrlWithToken = _webSocketUrl;
        string jwtToken = PlayerPrefs.GetString("JWTToken");

        if (!string.IsNullOrEmpty(jwtToken))
        {
            webSocketUrlWithToken += "?token=" + UnityWebRequest.EscapeURL(jwtToken);
        }

        Debug.Log("JWT Token being sent: " + jwtToken);

        WebSocketController.webSocket = new WebSocket(webSocketUrlWithToken);

        WebSocketController.webSocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened");
            _notificationText.text = "Connected to server!";
            isConnected = true;
        };

        WebSocketController.webSocket.OnMessage += (bytes) =>
        {
            try
            {
                string message = Encoding.UTF8.GetString(bytes);
                Debug.Log("Message received from server: " + message);

                if (string.IsNullOrEmpty(message))
                {
                    Debug.LogError("Received empty or null data.");
                    return;
                }

                TasksResponse response = JsonUtility.FromJson<TasksResponse>(message);
                if (response.type == "TASKS_UPDATE")
                {
                    // UpdateTasks(response.tasks);
                    Debug.Log("Trying to update");
                    MainThreadDispatcher.Instance().Enqueue(() => UpdateTasks(response.tasks));
                }
                if (response.type == "FILTERED_TASKS")
                {
                    MainThreadDispatcher.Instance().Enqueue(() => UpdateFilteredTasks(response.tasks));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("General error during message handling: " + ex.Message);
            }
        };

        WebSocketController.webSocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
            _notificationText.text = "WebSocket error!";
        };

        WebSocketController.webSocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket closed: " + e);
            _notificationText.text = "Disconnected from server";
            isConnected = false;
        };


        WebSocketController.webSocket.Connect();
        if (isConnected)
        {
            Debug.Log("Successfully connected to WebSocket.");
        }
        else
        {
            Debug.LogError("Failed to connect to WebSocket.");
        }
    }


    private void UpdateTasks(Task[] tasks)
    {
        MainThreadDispatcher.ExecuteOnMainThread(() =>
        {
            // Очистка текущего списка задач
            foreach (Transform child in tasksListParent)
            {
                Destroy(child.gameObject);
            }

            // Проход по всем задачам и создание элементов интерфейса для каждой из них
            foreach (var task in tasks)
            {
                // Парсинг даты задачи
                DateTime dueDate;
                if (!DateTime.TryParse(task.dueDate, out dueDate))
                {
                    Debug.LogError("Invalid Date Time received for task: " + task.title);
                    _notificationText.text = "Invalid Date Time";
                    continue; // Пропускаем эту задачу и продолжаем с другими
                }

                // Создание экземпляра префаба задачи
                GameObject taskItem = Instantiate(taskPrefab, tasksListParent);
                if (taskItem == null)
                {
                    Debug.LogError("Failed to instantiate task prefab for task: " + task.title);
                    continue; // Пропускаем эту задачу, если префаб не был создан
                }

                // Найти и заполнить текстовые поля в префабе
                TextMeshProUGUI tasksIdText = taskItem.transform.Find("TasksId")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI tasksText = taskItem.transform.Find("Tasks")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskDateText = taskItem.transform.Find("Dates")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskStatusText = taskItem.transform.Find("Statuses")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI filanameText = taskItem.transform.Find("Filename")?.GetComponent<TextMeshProUGUI>();

                // Проверка на наличие всех необходимых элементов интерфейса
                if (tasksIdText == null || tasksText == null || taskDateText == null || taskStatusText == null || filanameText == null)
                {
                    Debug.LogError("One or more UI elements are missing in the task prefab for task: " + task.title);
                    continue; // Пропускаем эту задачу, если не все элементы найдены
                }

                // Заполнение данных задачи в UI-элементы
                tasksIdText.text = task._id;
                tasksText.text = task.title;
                taskDateText.text = dueDate.ToString("yyyy-MM-dd"); // Форматируем дату для отображения
                taskStatusText.text = task.status;
                filanameText.text = task.file;

                // Найти и настроить кнопку "Edit"
                Button editButton = taskItem.transform.Find("BtnPadding/EditButton")?.GetComponent<Button>();
                if (editButton != null)
                {
                    editButton.onClick.AddListener(() => OnEditButtonClick(task));
                }
                else
                {
                    Debug.LogError("Edit button not found in the task prefab for task: " + task.title);
                }
            }

            // Вывод информации о полученных задачах (для отладки)
            foreach (var task in tasks)
            {
                Debug.Log($"Task: {task.title}, Status: {task.status}, Due Date: {task.dueDate}");
            }

            // Опционально: обновить уведомление, что задачи успешно обновлены
            _notificationText.text = $"Tasks updated successfully!";
        });
    }

    private void UpdateFilteredTasks(Task[] tasks)
    {
        // Очистить текущий список заданий
        foreach (Transform child in tasksListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var task in tasks)
        {
            DateTime dueDate;
            if (!DateTime.TryParse(task.dueDate, out dueDate))
            {
                _notificationText.text = "Invalid Date Time";
                return; // Можно прервать выполнение, если невалидная дата
            }

            // Создать экземпляр префаба
            GameObject taskItem = Instantiate(taskPrefab, tasksListParent);

            // Найти и заполнить текстовые поля
            TextMeshProUGUI tasksText = taskItem.transform.Find("Tasks").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI taskDateText = taskItem.transform.Find("Dates").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI taskStatusText = taskItem.transform.Find("Statuses").GetComponent<TextMeshProUGUI>();

            tasksText.text = task.title;
            taskDateText.text = dueDate.ToString("yyyy-MM-dd");
            taskStatusText.text = task.status;

            // Найти и настроить кнопку "Edit" внутри BtnPadding
            Button editButton = taskItem.transform.Find("BtnPadding/EditButton").GetComponent<Button>();
            editButton.onClick.AddListener(() => OnEditButtonClick(task));
        }

        _notificationText.text = "Tasks filtered successfully!";
    }

    private void OnApplicationQuit()
    {
        if (WebSocketController.webSocket != null)
        {
            WebSocketController.webSocket.Close();
        }
    }




    // -----------------------------------------------------------------------------------------------------
    private IEnumerator AutoUpdateTasks()
    {
        while (!isConnected)
        {
            // yield return StartCoroutine(GetTasks());
            ConnectToWebSocket();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // private IEnumerator GetTasks()
    // {
    //     UnityWebRequest request = UnityWebRequest.Get(_getTasksEndpoint);

    //     // string tokenCookie = PlayerPrefs.GetString("JWTToken");
    //     string refreshToken = PlayerPrefs.GetString("RefreshToken");
    //     // string tokenCookieVar = _jwtTokenText.text;
    //     // if (tokenCookie != tokenCookieVar)
    //     //     Debug.Log("Player Prefs token and var are different. Var:" + tokenCookieVar);

    //     // if (!string.IsNullOrEmpty(tokenCookie))
    //     // {
    //     //     Debug.Log("Token Cookie: " + tokenCookie);
    //     //     request.SetRequestHeader("Authorization", tokenCookie);
    //     // }
    //     // else
    //     // {
    //     //     Debug.Log("Token Cookie is empty");
    //     // }

    //     var handler = request.SendWebRequest();

    //     float startTime = 0.0f;
    //     while (!handler.isDone)
    //     {
    //         startTime += Time.deltaTime;

    //         if (startTime > 10.0f)
    //         {
    //             break;
    //         }
    //         yield return null;
    //     }

    //     if (request.result != UnityWebRequest.Result.ConnectionError)
    //     {
    //         Debug.Log("HttpCode: " + request.responseCode + request.downloadHandler.text);

    //         TasksResponse response = JsonUtility.FromJson<TasksResponse>(request.downloadHandler.text);

    //         if (request.responseCode == 200)
    //         {
    //             _notificationText.text = $"Tasks retrieved successfully!";


    //             foreach (Transform child in tasksListParent)
    //             {
    //                 Destroy(child.gameObject);
    //             }

    //             foreach (var task in response.tasks)
    //             {
    //                 DateTime dueDate;
    //                 if (!DateTime.TryParse(task.dueDate, out dueDate))
    //                 {
    //                     _notificationText.text = "Invalid Date Time";
    //                     yield return null;
    //                 }

    //                 // Создать экземпляр префаба
    //                 GameObject taskItem = Instantiate(taskPrefab, tasksListParent);

    //                 // Найти и заполнить текстовые поля
    //                 TextMeshProUGUI tasksIdText = taskItem.transform.Find("TasksId").GetComponent<TextMeshProUGUI>();
    //                 TextMeshProUGUI tasksText = taskItem.transform.Find("Tasks").GetComponent<TextMeshProUGUI>();
    //                 TextMeshProUGUI taskDateText = taskItem.transform.Find("Dates").GetComponent<TextMeshProUGUI>();
    //                 TextMeshProUGUI taskStatusText = taskItem.transform.Find("Statuses").GetComponent<TextMeshProUGUI>();
    //                 TextMeshProUGUI filanameText = taskItem.transform.Find("Filename").GetComponent<TextMeshProUGUI>();


    //                 tasksIdText.text = task._id;
    //                 Debug.Log("taskPanelHandler" + task._id);
    //                 tasksText.text = task.title;
    //                 taskDateText.text = dueDate.ToString("yyyy-MM-dd");
    //                 taskStatusText.text = task.status;
    //                 filanameText.text = task.file;

    //                 // Найти и настроить кнопку "Edit"
    //                 Button editButton = taskItem.transform.Find("BtnPadding/EditButton").GetComponent<Button>();
    //                 editButton.onClick.AddListener(() => OnEditButtonClick(task));
    //             }

    //             foreach (var task in response.tasks)
    //             {
    //                 Debug.Log($"Task: {task.title}, Status: {task.status}, Due Date: {task.dueDate}");
    //             }
    //         }
    //         else
    //         {
    //             switch (request.responseCode)
    //             {
    //                 case 400:
    //                     _notificationText.text = "Bad request";
    //                     break;
    //                 case 401:
    //                     ClearList();
    //                     Debug.Log("Server JWT unauthorized:" + request.ToString());
    //                     StartCoroutine(RefreshToken(refreshToken));
    //                     _loginBtn.gameObject.SetActive(true);
    //                     _notificationText.text = "User Unauthorized";
    //                     break;
    //                 case 404:
    //                     _notificationText.text = "Tasks not found";
    //                     break;
    //                 case 500:
    //                     _notificationText.text = "Server error. Maybe try later";
    //                     break;
    //                 default:
    //                     _notificationText.text = "Unknown response !";
    //                     break;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Request failed: " + request.error);
    //         _notificationText.text = "Unable to access server";
    //     }

    //     request.Dispose();
    //     yield return null;
    // }

    public void ApplyFilters()
    {
        if (statusDropdown.options[statusDropdown.value].text == "Status:")
        {
            Start();
            return;
        }
        StopAllCoroutines();
        // StartCoroutine(FilterTasks());
        SendFilterMessage();
    }

    private async void SendFilterMessage()
    {
        string status = statusDropdown.options[statusDropdown.value].text;
        string overdue = "upcoming";

        var filterMessage = new FilterMessage
        {
            type = "FILTER_TASKS",
            data = new FilterMessage.FilterData
            {
                status = status,
                overdue = overdue
            }
        };

        string jsonMessage = JsonUtility.ToJson(filterMessage);
        Debug.Log("Client send message to websocket: " + jsonMessage);

        // Отправляем сообщение через WebSocket
        if (WebSocketController.webSocket.State == WebSocketState.Open) // Проверяем, открыт ли WebSocket
        {
            await WebSocketController.webSocket.SendText(jsonMessage);
        }
        else
        {
            Debug.LogError("WebSocket is not connected. Cannot send message.");
        }
    }


    private IEnumerator FilterTasks()
    {
        string status = statusDropdown.options[statusDropdown.value].text;
        // string overdue = overdueDropdown.options[overdueDropdown.value].text;
        string overdue = "upcoming";

        WWWForm form = new WWWForm();
        form.AddField("status", status);
        form.AddField("overdue", overdue);

        Debug.Log("request: " + form.ToString());
        UnityWebRequest request = UnityWebRequest.Post(_filterTasksEndpoint, form);
        var handler = request.SendWebRequest();

        float startTime = 0.0f;
        while (!handler.isDone)
        {
            startTime += Time.deltaTime;

            if (startTime > 10.0f)
            {
                break;
            }
            yield return null;
        }

        if (request.result != UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("HttpCode: " + request.responseCode + request.downloadHandler.text);

            TasksResponse response = JsonUtility.FromJson<TasksResponse>(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                _notificationText.text = $"Tasks filtered successfully!";

                // Очистить текущий список заданий
                foreach (Transform child in tasksListParent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var task in response.tasks)
                {
                    DateTime dueDate;
                    if (!DateTime.TryParse(task.dueDate, out dueDate))
                    {
                        _notificationText.text = "Invalid Date Time";
                        yield return null;
                    }

                    // Создать экземпляр префаба
                    GameObject taskItem = Instantiate(taskPrefab, tasksListParent);

                    // Найти и заполнить текстовые поля
                    TextMeshProUGUI tasksText = taskItem.transform.Find("Tasks").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI taskDateText = taskItem.transform.Find("Dates").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI taskStatusText = taskItem.transform.Find("Statuses").GetComponent<TextMeshProUGUI>();

                    tasksText.text = task.title;
                    taskDateText.text = dueDate.ToString("yyyy-MM-dd");
                    taskStatusText.text = task.status;

                    // Найти и настроить кнопку "Edit" внутри BtnPadding
                    Button editButton = taskItem.transform.Find("BtnPadding/EditButton").GetComponent<Button>();
                    editButton.onClick.AddListener(() => OnEditButtonClick(task));
                }
            }
            else
            {
                switch (request.responseCode)
                {
                    case 400:
                        _notificationText.text = "Bad request";
                        break;
                    case 404:
                        _notificationText.text = "Tasks not found";
                        break;
                    case 500:
                        _notificationText.text = "Server error. Maybe try later";
                        break;
                    default:
                        _notificationText.text = "Unknown response !";
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
            _notificationText.text = "Unable to access server";
        }

        request.Dispose();
        yield return null;
    }

    private void ClearList()
    {
        foreach (Transform child in tasksListParent)
        {
            Destroy(child.gameObject);
        }
    }
    // private IEnumerator RefreshToken(string refreshToken)

    // {
    //     // refreshToken = ""; //test for checking token
    //     UnityWebRequest request = new UnityWebRequest(_refreshTokenEndpoint, "POST");
    //     request.SetRequestHeader("Content-Type", "application/json");

    //     Debug.Log("Client refresh tokne in params: " + refreshToken);

    //     RefreshTokenRequestData requestData = new RefreshTokenRequestData { refreshToken = refreshToken };
    //     string json = JsonUtility.ToJson(requestData);
    //     byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

    //     request.uploadHandler = new UploadHandlerRaw(jsonToSend);
    //     request.downloadHandler = new DownloadHandlerBuffer();

    //     Debug.Log("Client sent request to refresh token:" + json);

    //     var handler = request.SendWebRequest();

    //     yield return handler;

    //     if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
    //     {
    //         Debug.LogError("Error refreshing token: " + request.error);
    //     }
    //     else
    //     {
    //         string newToken = request.GetResponseHeader("Authorization");
    //         if (!string.IsNullOrEmpty(newToken))
    //         {
    //             PlayerPrefs.SetString("JWTToken", newToken);
    //             Debug.Log("Token refreshed: " + newToken);

    //             GetTasks();
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to refresh token");
    //             _notificationText.text = "Failed to refresh token, you have no Access";
    //         }
    //     }
    // }

    [System.Serializable]
    private class RefreshTokenRequestData
    {
        public string refreshToken;
    }

    private void OnEditButtonClick(Task task)
    {
        Debug.Log($"Edit button clicked for task: {task.title}");
    }
}
