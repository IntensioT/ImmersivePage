using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Login : MonoBehaviour
{
    [SerializeField] private Button _loginButton;
    [SerializeField] private TextMeshProUGUI _loginBtnText;
    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private TextMeshProUGUI _jwtTokenText;
    [SerializeField] private TMPro.TMP_InputField _usernameInputField;
    [SerializeField] private TMPro.TMP_InputField _passwordInputField;

    [SerializeField] private string _loginEndpoint = "http://127.0.0.1:5051/account/login";
    private const string _passwordRegEx = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{5,16})";


    public void OnLoginClick()
    {
        TextMeshProUGUI loginBtnText = _loginButton.GetComponent<TextMeshProUGUI>();
        _loginBtnText.text = "Signing in...";
        _loginButton.interactable = false;

        StartCoroutine(TryLogin());
    }

    private IEnumerator TryLogin()
    {

        string username = _usernameInputField.text;
        string password = _passwordInputField.text;

        if (username.Length < 3 || username.Length > 16)
        {
            _notificationText.text = "Invalid username";
            _loginBtnText.text = "Sign in";
            _loginButton.interactable = true;
            yield break;
        }
        // deprecated: 
        // else if (password.Length < 5 || password.Length > 16)
        // {
        //     _notificationText.text = "Invalid password";
        //     _loginBtnText.text = "Sign in";

        //     _loginButton.interactable = true;
        //     yield break;
        // }

        // else if (!Regex.IsMatch(password, _passwordRegEx))
        // {
        //     _notificationText.text = "Invalid credentials";
        //     _loginBtnText.text = "Sign in";

        //     _loginButton.interactable = true;
        //     yield break;
        // }


        string jsCode = @"
        (function() {
            var originalFetch = window.fetch;

            window.fetch = function(input, init) {
                if (init && init.credentials === undefined) {
                    init.credentials = 'include';
                }

                return originalFetch(input, init);
            };
        })();
        ";

        // Выполнение JavaScript-кода
        Application.ExternalEval(jsCode);

        // Stores The raw data to pass as the POST request body when sending the form
        WWWForm formData = new WWWForm();
        formData.AddField("reqUsername", username);
        formData.AddField("reqPassword", password);


        UnityWebRequest request = UnityWebRequest.Post(_loginEndpoint, formData);

        request.SetRequestHeader("Access-Control-Allow-Headers", "Authorization, Refresh-Token");
        request.SetRequestHeader("Access-Control-Allow-Credentials", "true");

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

            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                var reqHeaders = request.GetResponseHeaders();
                Debug.Log("Client get headers:");
                foreach (var header in reqHeaders)
                {
                    Debug.Log(header.Key + " : " + header.Value);
                }
                _loginButton.interactable = false;
                _notificationText.text = "Welcome " + ((response.data.adminFlag == 1) ? "Admin" : "!");

                Debug.Log("Client got: " + JsonUtility.ToJson(response));
                string tokenCookie = response.data.accessToken;
                Debug.Log("Client got Token: " + tokenCookie);

                string refreshToken = response.data.refreshToken;
                Debug.Log("Client got refresh token: " + refreshToken);

                if (!string.IsNullOrEmpty(tokenCookie) && !string.IsNullOrEmpty(refreshToken))
                {
                    _jwtTokenText.text = tokenCookie;
                    PlayerPrefs.SetString("JWTToken", tokenCookie);
                    PlayerPrefs.SetString("RefreshToken", refreshToken);
                    PlayerPrefs.Save();
                    
                }
                else
                {
                    Debug.Log("Token Cookie or refresh on Client is empty");
                }
            }
            else
            {
                switch (request.responseCode)
                {
                    case 400:
                        _notificationText.text = "Username and password are required";
                        _loginButton.interactable = true;
                        _loginBtnText.text = "Sign in";
                        break;
                    case 401:
                        _notificationText.text = "Invalid credentials";
                        _loginButton.interactable = true;
                        _loginBtnText.text = "Sign in";
                        break;
                    case 500:
                        _notificationText.text = "Server error. Maybe try later";
                        _loginButton.interactable = true;
                        _loginBtnText.text = "Sign in";
                        break;
                    default:
                        _notificationText.text = "Unknown response !";
                        _loginButton.interactable = false;
                        break;
                }

            }
        }
        else
        {
            _notificationText.text = "Unable to access server";
            _loginButton.interactable = true;
            _loginBtnText.text = "Sign in";

        }

        request.Dispose();
        yield return null;
    }
}



