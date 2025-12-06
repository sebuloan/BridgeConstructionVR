using System.Collections;
using UnityEngine;

public class ObjectMoverAndAnimator : MonoBehaviour
{
    [Header("Reference Object")]
    public Transform referenceObject;

    [Header("First Movement")]
    public Vector3 firstMoveDirection = Vector3.forward;
    public float firstMoveDistance = 1f;
    public float firstMoveDuration = 1f;

    [Header("Second Movement")]
    public Vector3 secondMoveDirection = Vector3.right;
    public float secondMoveDistance = 1f;
    public float secondMoveDuration = 1f;

    [Header("Animation")]
    public Animator animator;
    public string WalkTriggerName;
    public string ClimbTriggerName;

    [Header("Trigger Execution")]
    public bool startSequence = false;

    private bool hasStarted = false;

    private void Start()
    {
        startSequence = true;
    }

    void Update()
    {
        if (startSequence && !hasStarted)
        {
            hasStarted = true;
            StartCoroutine(ExecuteSequence());
        }
    }

    private IEnumerator ExecuteSequence()
    {
        if (referenceObject == null)
        {
            Debug.LogError("Reference Object is not assigned.");
            yield break;
        }

        // First movement in local direction
        yield return StartCoroutine(MoveInLocalDirection(referenceObject, firstMoveDirection, firstMoveDistance, firstMoveDuration));

        // Trigger animation
        if (animator != null && !string.IsNullOrEmpty(ClimbTriggerName))
        {
            animator.SetTrigger(ClimbTriggerName);
        }
        else
        {
            Debug.LogWarning("Animator or Animation Trigger Name not set.");
        }

        // Second movement in another local direction
        yield return StartCoroutine(MoveInLocalDirection(referenceObject, secondMoveDirection, secondMoveDistance, secondMoveDuration));

        // Trigger animation
        if (animator != null && !string.IsNullOrEmpty(WalkTriggerName))
        {
            animator.SetTrigger(WalkTriggerName);
        }
        else
        {
            Debug.LogWarning("Animator or Animation Trigger Name not set.");
        }
    }

    private IEnumerator MoveInLocalDirection(Transform obj, Vector3 direction, float distance, float duration)
    {
        Vector3 start = obj.localPosition;
        Vector3 target = start + direction.normalized * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.localPosition = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.localPosition = target;
    }
}
