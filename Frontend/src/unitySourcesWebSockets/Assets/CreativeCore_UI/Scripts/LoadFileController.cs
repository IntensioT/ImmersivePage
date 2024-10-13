using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadFileController : MonoBehaviour
{
    // Link to the UI picture of the avatar in Canvas
    public Image avatarImage;
    

    // This method is called by the button (Button component)
    public void UpdateAvatar()
    {
        // Requesting a file from the user
        FileUploaderHelper.RequestFile((path, fileBytes) =>
        {
            // If the path is empty, ignore it.
            if (string.IsNullOrWhiteSpace(path))
                return;

            // Run a coroutine to load an image
            StartCoroutine(UploadImage(path));
        });
    }

    // Coroutine for image upload
    IEnumerator UploadImage(string path)
    {
        // This is where the texture will be stored.
        Texture2D texture;

        // using to automatically call Dispose, create a request along the path to the file
        using (UnityWebRequest imageWeb = new UnityWebRequest(path, UnityWebRequest.kHttpVerbGET))
        {
            // We create a "downloader" for textures and pass it to the request
            imageWeb.downloadHandler = new DownloadHandlerTexture();

            // We send a request, execution will continue after the entire file have been downloaded
            yield return imageWeb.SendWebRequest();

            // Getting the texture from the "downloader"
            texture = ((DownloadHandlerTexture)imageWeb.downloadHandler).texture;
        }

        // Create a sprite from a texture and pass it to the avatar image on the UI
        avatarImage.sprite = Sprite.Create(
            texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }
}