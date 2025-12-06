using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MiniatureState : IComponentState
{
    public void EnterState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        component.GetComponent<XRGrabInteractable>().enabled = true; // Enable interaction
        Debug.Log($"{component.gameObject.name} entered MiniatureState");
        component.UpdateMaterialBasedOnXRay(manager.IsXRayModeOn());
    }

    public void ExitState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // No specific exit logic for this simplified requirement
        Debug.Log($"{component.gameObject.name} exiting MiniatureState");
    }

    public void OnActivated(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // For this requirement, ray activation does nothing
        Debug.Log($"{component.gameObject.name} in MiniatureState: Ignoring OnActivated for now.");
    }

    public void OnGrabbed(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // Just acknowledging grab, no state change for this requirement
        Debug.Log($"{component.gameObject.name} in MiniatureState: OnGrabbed.");
    }

    public void OnReleased(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // THIS IS THE CORE LOGIC FOR THIS REQUIREMENT
        Debug.Log($"{component.gameObject.name} in MiniatureState: OnReleased.");

        // Check if the component was released within the VisualizationArea's collider
        if (manager.visualizationAreaTarget != null && manager.visualizationAreaTarget.GetComponent<Collider>() != null)
        {
            Collider mainAreaCollider = manager.visualizationAreaTarget.GetComponent<Collider>();
            if (mainAreaCollider.bounds.Contains(component.transform.position))
            {
                Debug.Log($"{component.gameObject.name} released inside main area. Initiating visualization.");
                // Component will transition to VisualizingState via NotifyVisualizationCompleted
                manager.StartVisualizationRoutine(component);
            }
            else
            {
                Debug.Log($"{component.gameObject.name} released outside main area. Stays in MiniatureState.");
                // Component remains in MiniatureState if released outside the main area
            }
        }
        else
        {
            Debug.LogWarning("VisualizationAreaTarget or its Collider not assigned/found in VisualizationManager. Cannot check release position.");
        }
    }

    public void OnPulledOutOfAssembly(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // Not used for this simplified requirement
        Debug.Log($"{component.gameObject.name} in MiniatureState: OnPulledOutOfAssembly (not used for current requirement).");
    }

    public void OnResetRequested(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        // Not used for this simplified requirement
        Debug.Log($"{component.gameObject.name} in MiniatureState: OnResetRequested (not used for current requirement).");
        manager.ReturnToMiniatureRoutine(component);
    }
}