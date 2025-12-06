using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class MultiTeleport : MonoBehaviour
{
    [Header("XR Origin Reference")]
    public XROrigin xrOrigin;

    [Header("Teleport Targets")]
    public Transform teleportTargetA;             // Default A location
    public Transform commonTeleportTargetB;       // ✅ One common B location

    [Header("Interaction Manager")]
    public ObjectInteractionManager interactionManager;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private bool hasPrevious = false;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        if (xrOrigin != null)
        {
            startPosition = xrOrigin.transform.position;
            startRotation = xrOrigin.transform.rotation;
            previousPosition = startPosition;
            previousRotation = startRotation;
            hasPrevious = true;
        }
    }

    public void TeleportToA()
    {
        TeleportTo(teleportTargetA);
    }

    public void TeleportToCurrentStepB()
    {
        // ✅ Instead of looking into step.teleportTargetB, use global common target
        if (commonTeleportTargetB != null)
        {
            TeleportTo(commonTeleportTargetB.position, commonTeleportTargetB.rotation);
            if (interactionManager != null)
            {
                interactionManager.OnTeleportedToTargetB();
            }
        }
        else
        {
            Debug.LogWarning("⚠ No commonTeleportTargetB assigned in MultiTeleport.");
        }
    }

    private void TeleportTo(Transform target)
    {
        if (xrOrigin == null || target == null) return;

        previousPosition = xrOrigin.transform.position;
        previousRotation = xrOrigin.transform.rotation;
        hasPrevious = true;

        xrOrigin.MoveCameraToWorldLocation(target.position);

        // Flatten forward vector so player doesn’t tilt
        Vector3 flatForward = new Vector3(target.forward.x, 0, target.forward.z).normalized;
        xrOrigin.transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        if (xrOrigin == null) return;

        previousPosition = xrOrigin.transform.position;
        previousRotation = xrOrigin.transform.rotation;
        hasPrevious = true;

        xrOrigin.MoveCameraToWorldLocation(position);
        xrOrigin.transform.rotation = rotation;
    }

    public void TeleportBack()
    {
        if (xrOrigin == null || !hasPrevious) return;

        xrOrigin.MoveCameraToWorldLocation(previousPosition);
        xrOrigin.transform.rotation = previousRotation;
    }

    public void TeleportToStart()
    {
        if (xrOrigin == null) return;

        xrOrigin.MoveCameraToWorldLocation(startPosition);
        xrOrigin.transform.rotation = startRotation;
    }
}
