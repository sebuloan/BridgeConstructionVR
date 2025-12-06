using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Assuming we disable grab during return

public class ReturningState : IComponentState
{
    public void EnterState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} entered ReturningState. Initiating return to miniature.");
        // Disable grab interactable during transition to prevent interruption
        XRGrabInteractable grabInteractable = component.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
    }

    public void ExitState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} exiting ReturningState.");
        // Re-enable grab interactable as it's now back in MiniatureState
        XRGrabInteractable grabInteractable = component.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }

    // During returning, most interactions are ignored or should be handled carefully
    public void OnActivated(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in ReturningState: Ignoring activation during reset.");
    }

    public void OnGrabbed(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // If grabbed during return, you might want to stop the return or prevent it.
        // For simplicity, we just log, but the XRGrabInteractable is disabled.
        Debug.Log($"{component.gameObject.name} in ReturningState: Grabbed during reset (interaction disabled).");
    }

    public void OnReleased(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in ReturningState: Released during reset (interaction disabled).");
    }

    public void OnPulledOutOfAssembly(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in ReturningState: OnPulledOutOfAssembly (not applicable).");
    }

    public void OnResetRequested(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // If a reset is requested while already returning, ignore or ensure it doesn't cause issues
        Debug.Log($"{component.gameObject.name} in ReturningState: Already resetting. Ignoring OnResetRequested.");
    }
}