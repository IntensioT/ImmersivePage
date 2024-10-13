using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TaskListDownloadFile : MonoBehaviour
{

    public Button _downloadButton;
    public TextMeshProUGUI _filename;
    public void OnDownloadButtonClicked()
    {
        string filename = _filename.text;
        StartCoroutine(DownloadFile(filename));
    }
    
    private IEnumerator DownloadFile(string filename)
    {
        string downloadUrl = $"http://127.0.0.1:5051/tasks/download/{filename}";

        string downloadPath = "D:/Media/temp";
        

        if (!Directory.Exists(downloadPath))
        {
            Directory.CreateDirectory(downloadPath);
        }

        UnityWebRequest request = UnityWebRequest.Get(downloadUrl);
        // request.downloadHandler = new DownloadHandlerFile(Path.Combine(downloadPath, filename));
        request.downloadHandler = new DownloadHandlerFile(downloadPath + "/" + filename);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Download failed: " + request.error);
        }
        else
        {
            Debug.Log("Download completed: " + downloadUrl + "/" + filename);
        }

        request.Dispose();
    }
}
