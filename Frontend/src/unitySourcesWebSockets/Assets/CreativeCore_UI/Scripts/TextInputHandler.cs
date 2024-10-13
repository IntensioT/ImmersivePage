using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextInputHandler : MonoBehaviour
{
    [Header("The value we got from the input field")]
    [SerializeField] private string _inputText;

    [Header("Showing the reaction to the player")]
    [SerializeField] private GameObject _reactionGroup;
    [SerializeField] private TMP_Text _reactionTextBox;

    public void GetFromInputField(string input)
    {
        _inputText += input + "\n";
        DisplayReactionToInput();
    }

    private void DisplayReactionToInput()
    {
        _reactionTextBox.text = "Test: " + _inputText;
    }
}
