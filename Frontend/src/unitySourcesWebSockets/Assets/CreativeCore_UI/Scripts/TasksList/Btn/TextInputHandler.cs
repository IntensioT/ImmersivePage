using UnityEngine;
using TMPro;

public class TextInput : MonoBehaviour
{
    public TMP_InputField inputField; 
    public TextMeshProUGUI notificationText;

    public void GetFromInputField(string input)
    {
        if (inputField != null && notificationText != null)
        {
            DisplayReactionToInput(input);
        }
        else
        {
            Debug.LogError("InputField or NotificationText is not assigned.");
        }
    }

    private void DisplayReactionToInput(string input)
    {
        if (inputField != null && notificationText != null)
        {
            // Ваш код для обработки ввода
            notificationText.text = "Input received: " + input;
        }
        else
        {
            Debug.LogError("InputField or NotificationText is not assigned.");
        }
    }
}
