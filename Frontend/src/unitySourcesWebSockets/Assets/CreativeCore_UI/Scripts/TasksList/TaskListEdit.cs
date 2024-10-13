using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using NativeWebSocket;

public class TaskListEdit : MonoBehaviour
{
    private string _editTaskEndpoint = "http://127.0.0.1:5051/tasks/update-task";
    public TextMeshProUGUI _titleInputField;
    public TextMeshProUGUI _statusInputField;
    public TextMeshProUGUI _dueDayDateInputField;
    public TextMeshProUGUI _dueMonthDateInputField;
    public TextMeshProUGUI _dueYearDateInputField;
    public TextMeshProUGUI _fileInputField;
    public TextMeshProUGUI _notificationText;
    public Button _editTaskButton;
    public TextMeshProUGUI _editTaskBtnText;
    public string taskId;

    public Transform _mainTasksPtr;

    public void OnEditTaskButtonClicked()
    {
        _editTaskBtnText.text = "Updating...";
        _editTaskButton.interactable = false;
        StartCoroutine(EditTask());
    }

    private IEnumerator EditTask()
    {
        string title = _titleInputField.text;
        string status = _statusInputField.text;
        string day = _dueDayDateInputField.text.Trim();
        string month = _dueMonthDateInputField.text.Trim();
        string year = _dueYearDateInputField.text.Trim();
        string file = _fileInputField.text.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(status) || string.IsNullOrEmpty(day) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(year))
        {
            _notificationText.text = "All fields are required";
            _editTaskBtnText.text = "Edit Task";
            _editTaskButton.interactable = true;
            yield break;
        }

        // Удаление всех невидимых символов
        day = Regex.Replace(day, @"[^\d]", "");
        month = Regex.Replace(month, @"[^\d]", "");
        year = Regex.Replace(year, @"[^\d]", "");

        int dayInt = 0;
        int monthInt = 0;
        int yearInt = 0;
        if (!int.TryParse(day, out dayInt) || !int.TryParse(month, out monthInt) || !int.TryParse(year, out yearInt))
        {
            Debug.LogError("Invalid day: " + day);
            Debug.LogError("Invalid month: " + month);
            Debug.LogError("Invalid year: " + year);
            _notificationText.text = "Invalid date format";
            _editTaskBtnText.text = "Edit Task";
            _editTaskButton.interactable = true;
            yield break;
        }

        DateTime date = new DateTime(yearInt, monthInt, dayInt);
        string dueDate = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        // Log values before preparing the message
        Debug.Log($"Title: {title}, Status: {status}, Due Date: {dueDate}, Task ID: {taskId}");

        // Prepare message
        var message = new UpdateTaskMessage
        {
            type = "UPDATE_TASK",
            data = new UpdateTaskMessage.TaskData
            {
                _id = taskId,
                title = title,
                status = status,
                dueDate = dueDate
            }
        };

        // Serialize to JSON
        string jsonMessage = JsonUtility.ToJson(message);
        Debug.Log("Sending message: " + jsonMessage);

        // Send message over WebSocket
        // WebSocketController.webSocket.Send(jsonMessage);
        if (WebSocketController.webSocket.State == WebSocketState.Open)
        {
            WebSocketController.webSocket.SendText(jsonMessage);
        }
        else
        {
            Debug.LogError("WebSocket is not connected. Cannot send message.");
        }


        _editTaskBtnText.text = "Edit Task";
        _editTaskButton.interactable = true;

        yield return null;
        //////////////////////////////////////////////////////////////////////////////

        // Task updatedTask = new Task
        // {
        //     _id = taskId,
        //     title = title,
        //     status = status,
        //     dueDate = dueDate,
        //     file = file
        // };

        // string json = JsonUtility.ToJson(updatedTask);
        // Debug.Log("trying to PUT" + json);

        // UnityWebRequest request = new UnityWebRequest(_editTaskEndpoint, "PUT");
        // byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        // request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        // request.downloadHandler = new DownloadHandlerBuffer();
        // request.SetRequestHeader("Content-Type", "application/json");

        // var handler = request.SendWebRequest();


        // float startTime = 0.0f;
        // while (!handler.isDone)
        // {
        //     startTime += Time.deltaTime;

        //     if (startTime > 10.0f)
        //     {
        //         break;
        //     }
        //     yield return null;
        // }

        // if (request.result != UnityWebRequest.Result.ConnectionError)
        // {
        //     Debug.Log("HttpCode: " + request.responseCode + request.downloadHandler.text);

        //     if (request.responseCode == 200)
        //     {
        //         _notificationText.text = $"Task updated successfully!";
        //     }
        //     else
        //     {
        //         switch (request.responseCode)
        //         {
        //             case 400:
        //                 _notificationText.text = "Bad request";
        //                 break;
        //             case 404:
        //                 _notificationText.text = "Task not found";
        //                 break;
        //             case 500:
        //                 _notificationText.text = "Server error. Maybe try later";
        //                 break;
        //             default:
        //                 _notificationText.text = "Unknown response !";
        //                 break;
        //         }
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Request failed: " + request.error);
        //     _notificationText.text = "Unable to access server";
        // }

        // _editTaskBtnText.text = "Edit Task";
        // _editTaskButton.interactable = true;

        // request.Dispose();
        // yield return null;
    }

    [System.Serializable]
    public class UpdateTaskMessage
    {
        public string type;
        public TaskData data;

        [System.Serializable]
        public class TaskData
        {
            public string _id;
            public string title;
            public string status;
            public string dueDate;
        }
    }

    public void LoadTaskForEdit(string taskId, string title, string status, string dueDate, Transform transform)
    {
        this.taskId = taskId;
        _titleInputField.text = title;
        _statusInputField.text = status;
        DateTime date = DateTime.Parse(dueDate);
        _dueDayDateInputField.text = date.Day.ToString();
        _dueMonthDateInputField.text = date.Month.ToString();
        _dueYearDateInputField.text = date.Year.ToString();

        Debug.Log("Loading task for edit: " + taskId);

        _mainTasksPtr = transform;
    }

}
