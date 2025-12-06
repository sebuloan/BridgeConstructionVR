using UnityEngine;
using UnityEngine.InputSystem;


public class DisableTeleportWhileGrabbed : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor;     // For grab detection
    public InputActionReference teleportActivate;   // The teleport activation input action

    void Update()
    {
        if (directInteractor.hasSelection)
        {
            // Disable teleport input while grabbing
            teleportActivate.action.Disable();
        }
        else
        {
            // Enable teleport input when not grabbing
            if (!teleportActivate.action.enabled)
                teleportActivate.action.Enable();
        }
    }
}
