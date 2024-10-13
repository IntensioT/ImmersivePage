using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
    public class UpdateTaskMessage
    {
        public string type;
        public TaskData data;

        [System.Serializable]
        public class TaskData
        {
            public string _id;
            public string title;
            public string status;
            public string dueDate;
        }
    }
