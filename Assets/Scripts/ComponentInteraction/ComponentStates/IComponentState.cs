public interface IComponentState
{
    void EnterState(ComponentInteractionTrigger component, VisualizationManager manager);
    void ExitState(ComponentInteractionTrigger component, VisualizationManager manager);
    void OnActivated(ComponentInteractionTrigger component, VisualizationManager manager);
    void OnGrabbed(ComponentInteractionTrigger component, VisualizationManager manager);
    void OnReleased(ComponentInteractionTrigger component, VisualizationManager manager);
    void OnPulledOutOfAssembly(ComponentInteractionTrigger component, VisualizationManager manager);
    void OnResetRequested(ComponentInteractionTrigger component, VisualizationManager manager);
}