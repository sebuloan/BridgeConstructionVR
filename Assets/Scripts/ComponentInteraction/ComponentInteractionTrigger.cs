using System.Collections; // Not strictly needed for THIS version, but often useful
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ComponentInteractionTrigger : MonoBehaviour
{
    // Removed assemblyBoundsTrigger and pullOutDistanceThreshold for this simplified requirement

    private XRGrabInteractable grabInteractable;
    private VisualizationManager visualizationManager;
    private IComponentState currentState;
    private Outline outlineComponent;
    private Renderer componentRenderer;
    private Material originalMaterial;
    private bool isHoveredInternal = false;

    public bool IsCurrentlyGrabbed { get; private set; } = false; // Still useful for manager context

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable not found on " + gameObject.name);
            enabled = false;
            return;
        }
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            Debug.LogWarning("Outline component not found on " + gameObject.name + ". Hover outline effect will not work for this object.", this);
        }
        else
        {
            outlineComponent.enabled = false;
        }

        componentRenderer = GetComponent<Renderer>();
        if (componentRenderer == null)
        {
            Debug.LogError("Renderer component not found on " + gameObject.name + ". Material changes for X-Ray mode will not work.", this);
            enabled = false; // Disable script if no renderer to avoid errors
            return;
        }
        originalMaterial = componentRenderer.material; // Store the material the object has at start
    }

    void Start()
    {
        visualizationManager = FindObjectOfType<VisualizationManager>();
        if (visualizationManager == null)
        {
            Debug.LogError("VisualizationManager not found in scene! Please add one.");
            enabled = false;
            return;
        }

        ChangeState(new MiniatureState()); // Initial state

        // Subscribe to XR Interaction Toolkit events
        grabInteractable.activated.AddListener(OnActivated);
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);

        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);

        UpdateMaterialBasedOnXRay(visualizationManager.IsXRayModeOn());
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);

            grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            grabInteractable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    // --- Public method for state transitions (called by states themselves) ---
    public void ChangeState(IComponentState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this, visualizationManager);
        }
        currentState = newState;
        currentState.EnterState(this, visualizationManager);

        UpdateMaterialBasedOnXRay(visualizationManager.IsXRayModeOn());
    }

    public void RequestReset()
    {
        if (currentState != null)
        {
            currentState.OnResetRequested(this, visualizationManager);
        }
    }

    // --- New method to update material based on X-Ray, hover, and state ---
    public void UpdateMaterialBasedOnXRay(bool globalXRayModeOn)
    {
        if (componentRenderer == null || originalMaterial == null || visualizationManager.transparentMaterial == null)
        {
            // Debug.LogWarning($"Material update skipped for {gameObject.name} due to missing references.");
            return;
        }

        // --- Priority Hierarchy for Material ---
        // 1. If grabbed OR in VisualizingState: Always original material (highest priority)
        if (IsCurrentlyGrabbed || currentState.GetType() == typeof(VisualizingState))
        {
            componentRenderer.material = originalMaterial;
            return;
        }

        // 2. If X-Ray mode is ON:
        if (globalXRayModeOn)
        {
            if (isHoveredInternal) // If hovered, show original
            {
                componentRenderer.material = originalMaterial;
            }
            else // Not hovered, show transparent
            {
                componentRenderer.material = visualizationManager.transparentMaterial;
            }
        }
        // 3. If X-Ray mode is OFF: Always original material (lowest priority if no other conditions apply)
        else
        {
            componentRenderer.material = originalMaterial;
        }
    }

    // --- XR Interaction Toolkit Event Handlers ---
    private void OnActivated(ActivateEventArgs args)
    {
        currentState.OnActivated(this, visualizationManager); // Delegated, but OnActivated in states is empty for now
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        IsCurrentlyGrabbed = true;
        currentState.OnGrabbed(this, visualizationManager); // Delegated
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        IsCurrentlyGrabbed = false;
        currentState.OnReleased(this, visualizationManager); // Delegated - THIS IS THE KEY TRIGGER
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHoveredInternal = true;
        // Only enable outline if the component is not currently grabbed.
        if (outlineComponent != null && !IsCurrentlyGrabbed) // Example: Don't show hover outline if already grabbed
        {
            outlineComponent.enabled = true;
        }
        UpdateMaterialBasedOnXRay(visualizationManager.IsXRayModeOn());
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHoveredInternal = false;
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
        UpdateMaterialBasedOnXRay(visualizationManager.IsXRayModeOn());
    }

    // --- Callbacks from VisualizationManager (only NotifyVisualizationCompleted used for now) ---
    public void NotifyPulledOutOfAssembly()
    {
        // Not used for this simplified requirement
    }

    public void NotifyVisualizationCompleted()
    {
        ChangeState(new VisualizingState()); // Transition to VisualizingState after routine finishes
    }

    public void NotifyResetCompleted()
    {
        ChangeState(new MiniatureState()); // Transition back to MiniatureState after return routine finishes
    }
}