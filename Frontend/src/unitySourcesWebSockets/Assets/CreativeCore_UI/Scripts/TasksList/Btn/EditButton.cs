using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditButton : MonoBehaviour
{
    public GameObject taskEditFormPrefab;
    public Button editButton;
    public TextMeshProUGUI _titleInputField;
    public TextMeshProUGUI _statusInputField;
    public TextMeshProUGUI _dueDateInputField;
    public TextMeshProUGUI _idInputField;


    public string _id;
    public string _title;
    public string _status;
    public string _date;
    private Transform _mainTasksPtr;

    void Start()
    {
        editButton.onClick.AddListener(OnEditButtonClick);
    }



    void OnEditButtonClick()
    {
        Canvas topCanvas = GetTopCanvas(transform);
        SetMainCanvas(false);

        GameObject taskEditForm = Instantiate(taskEditFormPrefab, topCanvas.transform);
        taskEditForm.SetActive(true);

        taskEditForm.transform.localPosition = Vector3.zero;

        TaskListEdit taskListEdit = taskEditForm.GetComponent<TaskListEdit>();

        if (taskListEdit != null)
        {
            taskListEdit.LoadTaskForEdit(_id, _title, _status, _date, _mainTasksPtr);
        }
        else
        {
            Debug.LogError("TaskListEdit component not found on the instantiated object.");
        }
    }



    private Canvas GetTopCanvas(Transform currentTransform)
    {
        Canvas parCanvas = currentTransform.GetComponentInParent<Canvas>();
        Canvas rootCanvas = parCanvas.rootCanvas;
        return rootCanvas;
    }

    private void SetMainCanvas(bool flag)
    {
        _mainTasksPtr.gameObject.SetActive(flag);
    }

    public void setParams()
    {
        // TaskPanelItemHandler handler = gameObject.GetComponent<TaskPanelItemHandler>();
        _id = _idInputField.text;
        _title = _titleInputField.text;
        _status = _statusInputField.text;
        _date = _dueDateInputField.text;

        Canvas parCanvas = transform.GetComponentInParent<Canvas>();

        Canvas rootCanvas = parCanvas.rootCanvas;

        Transform mainTasksTransform = rootCanvas.transform.Find("Main Tasks Menu");
        _mainTasksPtr = mainTasksTransform;
    }
}
