using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TimedQueue<T>
{
    public float PreDelay { set { _delayedExecuter.PreDelay = value; } }
    public float PostDelay { set { _delayedExecuter.PostDelay = value; } }

    public Action<T> OnDequeue { get; set; }
    public Action OnEmpty { get; set; }

    private Queue<T> _queue = new Queue<T>();
    private DelayedExecuter _delayedExecuter;

    private TimedQueue(DelayedExecuter runner)
    {
        _delayedExecuter = runner;
        _delayedExecuter.Executed += Runner_Dequeued;
        _delayedExecuter.PostExecute += Runner_PostDequeue;
    }

    private void Runner_Dequeued() => OnDequeue?.Invoke(_queue.Dequeue());

    private void Runner_PostDequeue()
    {
        if (_queue.Count == 0)
        {
            OnEmpty?.Invoke();
        }
        else
        {
            _delayedExecuter.Execute();
        }
    }

    public void Start() => _delayedExecuter.Execute();

    public void Enqueue(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            _queue.Enqueue(item);
        }
    }

    public void Clear()
    {
        _queue.Clear();
    }

    public static TimedQueue<T> Create(Transform parent, string name = "Timed Queue")
    {
        var runner = new GameObject("Generic Timer").AddComponent<DelayedExecuter>();
        runner.transform.SetParent(parent);
        return new TimedQueue<T>(runner);
    }
}