using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VisualizingState : IComponentState
{
    public void EnterState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        string[] namePattern = component.gameObject.name.Split('_');
        manager.DisplayComponentInfo(manager.GetComponentData(namePattern[0]));
        component.GetComponent<XRGrabInteractable>().enabled = true; // Allow direct manipulation of scaled model
        Debug.Log($"{component.gameObject.name} entered VisualizingState");
    }

    public void ExitState(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        manager.HideComponentInfo();
        Debug.Log($"{component.gameObject.name} exiting VisualizingState");
    }

    public void OnActivated(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in VisualizingState: Already visualizing. Ignoring activation.");
    }

    public void OnGrabbed(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in VisualizingState: Grabbed. Allowing free manipulation.");
    }

    public void OnReleased(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in VisualizingState: Released. Remains visualized (until reset).");
    }

    public void OnPulledOutOfAssembly(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in VisualizingState: OnPulledOutOfAssembly (not applicable).");
    }

    public void OnResetRequested(ComponentInteractionTrigger component, VisualizationManager manager)
    {
        Debug.Log($"{component.gameObject.name} in VisualizingState: OnResetRequested. Initiating return routine.");
        component.ChangeState(new ReturningState()); // Transition to ReturningState immediately
        manager.ReturnToMiniatureRoutine(component); // Tell manager to start the return animation
    }
}