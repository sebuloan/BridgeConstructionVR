using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [Tooltip("The target Transform to follow (e.g., hook or jacket).")]
    public Transform target;

    [Tooltip("Update position only (disable if you also want to follow rotation).")]
    public bool followPosition = true;

    [Tooltip("Update rotation.")]
    public bool followRotation = true;

    void LateUpdate()
    {
        if (target == null) return;

        if (followPosition)
            transform.position = target.position;

        if (followRotation)
            transform.rotation = target.rotation;
    }
}
