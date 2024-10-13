using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestroyButton : MonoBehaviour
{
    public Button destroyButton;
    public GameObject objectToDestroy;
    void Start()
    {
        // Добавляем обработчик события нажатия кнопки
        destroyButton.onClick.AddListener(OnDestroyButtonClick);
    }

    private void showMainTasks()
    {
        TaskListEdit taskListEdit = objectToDestroy.GetComponent<TaskListEdit>();
        Transform mainTasksPtr =  taskListEdit._mainTasksPtr;
        mainTasksPtr.gameObject.SetActive(true);
    }

    void OnDestroyButtonClick()
    {
        // Уничтожаем объект
        Destroy(objectToDestroy);

        // Отладочное сообщение
        Debug.Log("Object destroyed: " + objectToDestroy.name);
        showMainTasks();
    }
}
