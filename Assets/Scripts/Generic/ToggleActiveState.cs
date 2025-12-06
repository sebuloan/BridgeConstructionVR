using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActiveState : MonoBehaviour
{
    public GameObject targetObject;  // Assign this via Inspector
    public float toggleSpeed = 1f;   // Time between toggles (seconds)
    public float toggleDuration = 0f; // Duration to toggle. 0 = infinite

    private Coroutine toggleCoroutine;

    private void OnEnable()
    {
        StartToggle();
    }

    private void OnDisable()
    {
        StopToggle();
    }

    private void StartToggle()
    {
        if (toggleCoroutine != null)
            StopCoroutine(toggleCoroutine);

        toggleCoroutine = StartCoroutine(ToggleRoutine());
    }

    private void StopToggle()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }

    private IEnumerator ToggleRoutine()
    {
        float elapsedTime = 0f;

        while (toggleDuration == 0f || elapsedTime < toggleDuration)
        {
            if (targetObject != null)
                targetObject.SetActive(!targetObject.activeSelf);

            yield return new WaitForSeconds(toggleSpeed);
            elapsedTime += toggleSpeed;
        }

        toggleCoroutine = null;
    }
}
