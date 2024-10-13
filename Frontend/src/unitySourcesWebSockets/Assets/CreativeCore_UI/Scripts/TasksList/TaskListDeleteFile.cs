using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI;

using NativeWebSocket;

public class TaskListDeleteFile : MonoBehaviour
{
    public GameObject panel;
    public Button _deleteButton;
    public TextMeshProUGUI _titleInputField;
    public TextMeshProUGUI _statusInputField;
    // public TextMeshProUGUI _notificationText;
    public TextMeshProUGUI _idInputField;

    public string taskId;

    private string _deleteTaskEndpoint = "http://127.0.0.1:5051/tasks/delete-task";


    public void OnDeleteButtonClicked()
    {
        StartCoroutine(DeleteTask());
    }
    private IEnumerator DeleteTask()
    {
        // Получаем ID задачи из текстового поля
        taskId = _idInputField.text.Trim();
        if (string.IsNullOrEmpty(taskId))
        {
            yield break;
        }

        // Подготовка сообщения для удаления задачи
        var message = new DeleteTaskMessage
        {
            type = "DELETE_TASK",
            data = new DeleteTaskMessage.TaskData
            {
                _id = taskId
            }
        };

        // Сериализация в JSON
        string jsonMessage = JsonUtility.ToJson(message);
        Debug.Log("Sending delete message: " + jsonMessage);

        // Отправка сообщения через WebSocket
        if (WebSocketController.webSocket.State == WebSocketState.Open)
        {
            WebSocketController.webSocket.SendText(jsonMessage);
            Debug.Log("Delete request sent.");
        }
        else
        {
            Debug.LogError("WebSocket is not connected. Cannot send message.");
        }

        // После отправки сообщения можно закрыть панель
        panel.SetActive(false);
        yield return null;
    }
    [System.Serializable]
    public class DeleteTaskMessage
    {
        public string type;
        public TaskData data;

        [System.Serializable]
        public class TaskData
        {
            public string _id;
        }
    }

    // private IEnumerator DeleteTask()
    // {
    //     string title = _titleInputField.text;
    //     string status = _statusInputField.text;

    //     Task updatedTask = new Task
    //     {
    //         _id = _idInputField.text,
    //         title = title,
    //         status = status,
    //     };

    //     string json = JsonUtility.ToJson(updatedTask);
    //     Debug.Log("trying to DELETE" + json);

    //     UnityWebRequest request = new UnityWebRequest(_deleteTaskEndpoint, "DELETE");
    //     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //     request.downloadHandler = new DownloadHandlerBuffer();
    //     request.SetRequestHeader("Content-Type", "application/json");

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

    //         if (request.responseCode == 200)
    //         {
    //             // _notificationText.text = $"Task deleted successfully!";
    //             Debug.Log("Task deleted successfully!");
    //             panel.SetActive(false);
    //         }
    //         else
    //         {
    //             switch (request.responseCode)
    //             {
    //                 case 400:
    //                     // _notificationText.text = "Bad request";
    //                     break;
    //                 case 404:
    //                     // _notificationText.text = "Task not found";
    //                     break;
    //                 case 500:
    //                     // _notificationText.text = "Server error. Maybe try later";
    //                     break;
    //                 default:
    //                     // _notificationText.text = "Unknown response !";
    //                     break;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Request failed: " + request.error);
    //         // _notificationText.text = "Unable to access server";
    //     }

    //     request.Dispose();
    //     yield return null;
    // }

}
