using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static TaskListEdit;

using NativeWebSocket;

public class TaskListAdd : MonoBehaviour
{
    private string _createTaskEndpoint = "http://127.0.0.1:5051/tasks/add-task";
    public TextMeshProUGUI _titleInputField;
    public TextMeshProUGUI _statusInputField;
    public TextMeshProUGUI _dueDayDateInputField;
    public TextMeshProUGUI _dueMonthDateInputField;
    public TextMeshProUGUI _dueYearDateInputField;
    public TextMeshProUGUI _fileInputField;
    public TextMeshProUGUI _notificationText;
    public Button _createTaskButton;
    public TextMeshProUGUI _createTaskBtnText;
    public Button _downloadButton;
    // public FileUploader fileUploader;
    public Image texture;

    // private FileUploader _fileUploader;

    public void OnCreateTaskButtonClicked()
    {
        _createTaskBtnText.text = "Creating...";
        _createTaskButton.interactable = false;
        StartCoroutine(CreateTask());
    }


    private IEnumerator CreateTask()
    {
        string title = _titleInputField.text;
        string status = _statusInputField.text;
        string day = _dueDayDateInputField.text.Trim();
        string month = _dueMonthDateInputField.text.Trim();
        string year = _dueYearDateInputField.text.Trim();
        string filePath = _fileInputField.text.Trim();


        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(status) || string.IsNullOrEmpty(day) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(year))
        {
            _notificationText.text = "All fields are required";
            _createTaskBtnText.text = "Create Task";
            _createTaskButton.interactable = true;
            yield break;
        }

        day = Regex.Replace(day, @"[^\d]", "");
        month = Regex.Replace(month, @"[^\d]", "");
        year = Regex.Replace(year, @"[^\d]", "");
        filePath = Regex.Replace(filePath, @"[\u200B-\u200D\uFEFF]", "");
        filePath = Regex.Replace(filePath, "\"", "");
        Debug.Log("File path: " + filePath);

        int dayInt = 0;
        int monthInt = 0;
        int yearInt = 0;
        if (!int.TryParse(day, out dayInt) || !int.TryParse(month, out monthInt) || !int.TryParse(year, out yearInt))
        {
            Debug.LogError("Invalid day: " + day);
            Debug.LogError("Invalid month: " + month);
            Debug.LogError("Invalid year: " + year);
            _notificationText.text = "Invalid date format";
            _createTaskBtnText.text = "Create Task";
            _createTaskButton.interactable = true;
            yield break;
        }

        DateTime date = new DateTime(yearInt, monthInt, dayInt);
        string dueDate = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        // Log values before preparing the message
        Debug.Log($"Title: {title}, Status: {status}, Due Date: {dueDate}");

        // Prepare message for adding a task
        var message = new UpdateTaskMessage
        {
            type = "ADD_TASK", // Change this to ADD_TASK
            data = new UpdateTaskMessage.TaskData
            {
                title = title,
                status = status,
                dueDate = dueDate,
                // _id can be omitted here if the server generates it
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

        // Handle server response if needed (not shown here)

        _createTaskBtnText.text = "Create Task";
        _createTaskButton.interactable = true;

        yield return null;
        //////////////////////////////////////////////////////////

        // byte[] fileData = FileUploader.fileBytes;

        // WWWForm formData = new WWWForm();
        // formData.AddField("title", title);
        // formData.AddField("status", status);
        // formData.AddField("dueDate", dueDate);  
        // formData.AddField("filePath", filePath);
        // if (fileData != null)
        //     formData.AddBinaryData("file", fileData, "application/octet-stream");



        // UnityWebRequest request = UnityWebRequest.Post(_createTaskEndpoint, formData);
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

        //     if (request.responseCode == 201)
        //     {
        //         _notificationText.text = $"Task created successfully!";
        //     }
        //     else
        //     {
        //         switch (request.responseCode)
        //         {
        //             case 400:
        //                 _notificationText.text = "Bad request";
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

        // _createTaskBtnText.text = "Create Task";
        // _createTaskButton.interactable = true;

        // request.Dispose();
        // yield return null;
    }

}
