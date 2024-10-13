using UnityEngine;
using System.Collections.Generic;
using System;

using System.Collections;
using NativeWebSocket;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            Debug.LogError("UnityMainThreadDispatcher: Trying to execute null action.");
            return;
        }

        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() => StartCoroutine(action));
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action action)
    {
        action();
        yield return null;
    }

    private static MainThreadDispatcher _instance = null;

    public static bool Exists()
    {
        return _instance != null;
    }

    public static MainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            throw new Exception("MainThreadDispatcher is not present in the scene. Please add it to your GameObject.");
        }
        return _instance;
    }

    public static void EnsureCreated()
    {
        if (!_instance)
        {
            GameObject obj = new GameObject("ThreadDispatcherObject");
            _instance = obj.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }
    }
}

public static class WebSocketController
{
    public static WebSocket webSocket;
}

