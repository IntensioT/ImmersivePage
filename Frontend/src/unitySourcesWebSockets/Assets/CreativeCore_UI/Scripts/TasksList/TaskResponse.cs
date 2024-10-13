using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Task
{
    public string _id;
    public string title;
    public string status;
    public string dueDate;
    public string file;
    
}

[System.Serializable]
public class TasksResponse
{
    public string token;
    public string type;

    public Task[] tasks;
}

[System.Serializable]
public class TokenRefreshResponse
{
    public string type;
    public string accessToken;
}

[System.Serializable]
public class TokenResponse
{
    public string accessToken;
    public string refreshToken;
}

[System.Serializable]
public class FilterMessage
{
    public string type;
    public FilterData data;

    [System.Serializable]
    public class FilterData
    {
        public string status;
        public string overdue;
    }
}


