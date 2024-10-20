using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Helper component to get a file
public class FileUploader : MonoBehaviour
{
    public static byte[] fileBytes;
    private void Start()
    {
        // We don't need to delete it on the new scene, because system is singletone
        Debug.Log("Upload File script started");
        DontDestroyOnLoad(gameObject);
    }

    // This method is called from JS via SendMessage
    // void FileRequestCallback(string path, string base64Data)
    void FileRequestCallback(string combinedData)
    {
        // Convert base64 string to byte array
        string[] dataParts = combinedData.Split('|');
        string path = dataParts[0];
        string base64Data = dataParts[1];

        fileBytes = Convert.FromBase64String(base64Data);
        // Sending the received link back to the FileUploaderHelper
        FileUploaderHelper.SetResult(path, fileBytes);
    }
}

public static class FileUploaderHelper
{
    static FileUploader fileUploaderObject;
    static Action<string, byte[]> pathCallback;

    static FileUploaderHelper()
    {
        string methodName = "FileRequestCallback"; // We will not use reflection, so as not to complicate things, hardcode :)
        string objectName = typeof(FileUploaderHelper).Name; // But not here

        // Create a helper object for the FileUploader system
        var wrapperGameObject = new GameObject(objectName, typeof(FileUploader));
        fileUploaderObject = wrapperGameObject.GetComponent<FileUploader>();

        // Initializing the JS part of the FileUploader system
        InitFileLoader(objectName, methodName);
    }

    /// <summary>
    /// Requests a file from the user.
    /// Should be called when the user clicks!
    /// </summary>
    /// <param name="callback">Will be called after the user selects a file, the Http path to the file is passed as a parameter</param>
    /// <param name="extensions">File extensions that can be selected, example: ".jpg, .jpeg, .png"</param>
    public static void RequestFile(Action<string, byte[]> callback, string extensions = ".jpg, .jpeg, .png")
    {
        RequestUserFile(extensions);
        pathCallback = callback;
    }

    /// <summary>
    /// For internal use
    /// </summary>
    /// <param name="path">The path to the file</param>
    public static void SetResult(string path, byte[] fileBytes)
    {
        pathCallback.Invoke(path, fileBytes);
        Dispose();
    }

    private static void Dispose()
    {
        ResetFileLoader();
        pathCallback = null;
    }

    // Below we declare external functions from our .jslib file
    [DllImport("__Internal")]
    private static extern void InitFileLoader(string objectName, string methodName);

    [DllImport("__Internal")]
    private static extern void RequestUserFile(string extensions);

    [DllImport("__Internal")]
    private static extern void ResetFileLoader();
}