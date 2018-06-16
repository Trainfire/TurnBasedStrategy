using UnityEngine;
using System;
using System.Collections;

public interface IDelayedExecuter
{
    float PreDelay { get; set; }
    float PostDelay { get; set; }

    event Action PostExecute;
    event Action Executed;
}

public class DelayedExecuter : MonoBehaviour, IDelayedExecuter
{
    public float PreDelay { get; set; } = 1f;
    public float PostDelay { get; set; } = 1f;

    public event Action PostExecute;
    public event Action Executed;

    public void Execute()
    {
        StartCoroutine(ExecuteWithDelay());
    }

    private IEnumerator ExecuteWithDelay()
    {
        yield return new WaitForSeconds(PreDelay);

        Executed?.Invoke();

        yield return new WaitForSeconds(PostDelay);

        PostExecute?.Invoke();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        PostExecute = null;
        Executed = null;
    }
}
