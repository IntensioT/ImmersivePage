using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateAccount : MonoBehaviour
{
    [SerializeField] private Button _createButton;
    [SerializeField] private TMPro.TextMeshProUGUI _createBtnText;
    [SerializeField] private TMPro.TextMeshProUGUI _notificationText;
    [SerializeField] private TMPro.TMP_InputField _usernameInputField;
    [SerializeField] private TMPro.TMP_InputField _passwordInputField;

    [SerializeField] private string _createAccountEndpoint = "http://127.0.0.1:5051/account/create";
    private const string _passwordRegEx = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{5,16})";

    public void OnCreateClick()
    {
        TMPro.TextMeshProUGUI createBtnText = _createButton.GetComponent<TMPro.TextMeshProUGUI>();
        _createBtnText.text = "Creating...";
        _createButton.interactable = false;

        StartCoroutine(TryCreate());
    }

    private IEnumerator TryCreate()
    {

        string username = _usernameInputField.text;
        string password = _passwordInputField.text;

        if (username.Length < 3 || username.Length > 16)
        {
            _notificationText.text = "Username should be in the range 3-16";
            _createBtnText.text = "Create";
            _createButton.interactable = true;
            yield break;
        }
        // deprecated:
        // else if (password.Length < 3 || password.Length > 16)
        // {
        //     _notificationText.text = "Password should be in the range 3-16";
        //     _createBtnText.text = "Create";

        //     _createButton.interactable = true;
        //     yield break;
        // }
        else if (!Regex.IsMatch(password, _passwordRegEx))
        {
            _notificationText.text = "Invalid credentials";
            _createBtnText.text = "Sign in";

            _createButton.interactable = true;
            yield break;
        }

        // Stores The raw data to pass as the POST request body when sending the form
        WWWForm formData = new WWWForm();
        formData.AddField("reqUsername", username);
        formData.AddField("reqPassword", password);

        UnityWebRequest request = UnityWebRequest.Post(_createAccountEndpoint, formData);
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

            CreateResponse response = JsonUtility.FromJson<CreateResponse>(request.downloadHandler.text);

            if (request.responseCode == 201)
            {
                _notificationText.text = $"Account has been created !";
            }
            else
            {
                switch (request.responseCode)
                {
                    case 400:
                        _notificationText.text = "Username and password are required";
                        break;
                    case 409:
                        _notificationText.text = "Username is already taken";
                        break;
                    case 422:
                         _notificationText.text = "Password is unsafe";
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
        _createBtnText.text = "Create";
        _createButton.interactable = true;

        request.Dispose();
        yield return null;
    }
}
