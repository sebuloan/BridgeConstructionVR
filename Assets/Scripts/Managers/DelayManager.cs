using System.Collections;
using System;
using UnityEngine;

public class DelayManager : MonoBehaviour
{
    public void ExecuteAfterDelay(float delay, Action callback)
    {
        StartCoroutine(RunAfterDelay(delay, callback));
    }

    private IEnumerator RunAfterDelay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
