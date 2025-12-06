using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Bosch.DigiGear;

public class VisualizationManager : MonoBehaviour
{
    [Header("Visualization Area")]
    public Transform visualizationAreaTarget;
    public float transitionDuration = 1.0f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI Elements")]
    public GameObject infoPanel;
    public TextMeshProUGUI componentNameText;
    public TextMeshProUGUI componentDescriptionText;
    public TextMeshProUGUI componentDimensionText;

    [Header("UI Elements - Second Panel")] 
    public GameObject secondInfoPanel; 
    public TextMeshProUGUI secondComponentNameText; 
    public TextMeshProUGUI secondComponentDescriptionText;
    public TextMeshProUGUI secondComponentDimensionText;

    [Header("External Visualizers")]
    public DimensionManager dimensionVisualizer;

    [Header("X-Ray Mode")]
    public Material transparentMaterial; // Assign your transparent material here in the Inspector
    private bool isXRayModeOn = false; // Global X-Ray mode state

    private AudioSource audioSource;

    private ComponentInteractionTrigger currentVisualizedComponentTrigger;
    private Coroutine currentVisualizationCoroutine;

    private Dictionary<string, Component> componentDataLookup;

    private class ComponentOriginalState
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Transform parent;
    }
    private Dictionary<ComponentInteractionTrigger, ComponentOriginalState> componentOriginalStates =
        new Dictionary<ComponentInteractionTrigger, ComponentOriginalState>();


    public void Initialize()
    {
        componentDataLookup = JsonDataLoader.LoadComponentData();

        foreach (ComponentInteractionTrigger trigger in FindObjectsByType<ComponentInteractionTrigger>(FindObjectsSortMode.None))
        {
            componentOriginalStates[trigger] = new ComponentOriginalState
            {
                localPosition = trigger.transform.localPosition,
                localRotation = trigger.transform.localRotation,
                localScale = trigger.transform.localScale,
                parent = trigger.transform.parent
            };
        }
    }

    void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
        if (secondInfoPanel != null)
        {
            secondInfoPanel.SetActive(false);
        }
        if (visualizationAreaTarget == null)
        {
            Debug.LogError("VisualizationAreaTarget Transform not assigned in VisualizationManager!");
        }
        if (visualizationAreaTarget != null && visualizationAreaTarget.GetComponent<Collider>() == null)
        {
            Debug.LogError("VisualizationAreaTarget must have a Collider component (e.g., BoxCollider set to Is Trigger) for release detection!");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on VisualizationManager! Audio descriptions will not play. Please add an AudioSource component.", this);
        }
    }

    // --- New Public Method for X-Ray Mode Toggle ---
    public void ToggleXRayMode()
    {
        isXRayModeOn = !isXRayModeOn;
        Debug.Log($"X-Ray Mode Toggled: {isXRayModeOn}");

        // Iterate through all known components and tell them to update their material
        // based on the new global X-Ray mode state.
        foreach (var entry in componentOriginalStates)
        {
            ComponentInteractionTrigger componentTrigger = entry.Key;
            // The UpdateMaterialBasedOnXRay method exists on ComponentInteractionTrigger
            componentTrigger.UpdateMaterialBasedOnXRay(isXRayModeOn);
        }
    }

    // New helper method for ComponentInteractionTrigger to query global X-Ray state
    public bool IsXRayModeOn()
    {
        return isXRayModeOn;
    }

    // --- Methods called by ComponentInteractionTrigger's states ---
    public void StartVisualizationRoutine(ComponentInteractionTrigger componentTrigger)
    {
        // 1. Handle any already visualized component (the "old" one)
        if (currentVisualizedComponentTrigger != null && currentVisualizedComponentTrigger != componentTrigger)
        {
            Debug.Log($"Resetting previous visualized component ({currentVisualizedComponentTrigger.gameObject.name}) before visualizing new one ({componentTrigger.gameObject.name}).");
            currentVisualizedComponentTrigger.RequestReset(); // This triggers the old component's return routine.
                                                              // Its coroutine will run independently.
        }

        // 2. Stop any *previous* visualization routine for the *new* component, if applicable.
        // This ensures the current 'visualization' slot is cleared before starting a new one.
        if (currentVisualizationCoroutine != null)
        {
            StopCoroutine(currentVisualizationCoroutine);
            currentVisualizationCoroutine = null; // Clear its reference
        }


        // 3. Now, start visualization for the NEW component
        currentVisualizedComponentTrigger = componentTrigger; // Set the new component as the current one being managed
        currentVisualizationCoroutine = StartCoroutine(VisualizeComponentActualRoutine(componentTrigger)); // Track this new visualization
    }

    public void ReturnToMiniatureRoutine(ComponentInteractionTrigger componentTrigger)
    {
        if (componentTrigger.IsCurrentlyGrabbed)
        {
            Debug.LogWarning($"Attempted to reset {componentTrigger.gameObject.name} but it is currently grabbed. Please release it first.");
            return;
        }

        // IMPORTANT: We do NOT stop 'currentVisualizationCoroutine' here,
        // as that coroutine tracks the *currently visualized* component (which might be the *new* one).
        // This routine is for a component that *might be* the old currentVisualizedComponentTrigger,
        // or one that was visualized and needs to return.
        // This coroutine will run independently of the main currentVisualizationCoroutine.

        // Hide UI info IF this component is the one whose info is currently displayed.
        if (componentTrigger == currentVisualizedComponentTrigger)
        {
            HideComponentInfo();
            // Also, clear currentVisualizedComponentTrigger if it's returning itself
            currentVisualizedComponentTrigger = null;
        }
        // If it's not the current one, its info is already hidden (or will be hidden by the new visualization)

        ComponentOriginalState originalState = componentOriginalStates[componentTrigger];
        Vector3 targetWorldPosition = originalState.parent.TransformPoint(originalState.localPosition);
        Quaternion targetWorldRotation = originalState.parent.rotation * originalState.localRotation;

        // Use a small tolerance for floating point comparisons
        if (Vector3.Distance(componentTrigger.transform.position, targetWorldPosition) < 0.01f &&
            Quaternion.Angle(componentTrigger.transform.rotation, targetWorldRotation) < 0.1f)
        {
            Debug.Log($"{componentTrigger.gameObject.name} is already in its original position. No return animation needed.");
            // We still notify the component that its reset is complete.
            componentTrigger.NotifyResetCompleted();
            return; // Exit the routine early
        }

        StartCoroutine(ReturnToMiniatureActualRoutine(componentTrigger)); // Start return routine independently
    }


    // --- Internal Routines ---
    private IEnumerator VisualizeComponentActualRoutine(ComponentInteractionTrigger componentTrigger)
    {
        Transform componentTransform = componentTrigger.transform;
        Vector3 startPosition = componentTransform.position;
        Quaternion startRotation = componentTransform.rotation;
        Vector3 startScale = componentTransform.localScale;

        componentTransform.SetParent(null);

        Vector3 targetPosition = visualizationAreaTarget.position;
        Quaternion targetRotation = visualizationAreaTarget.rotation;

        string[] namePattern = componentTrigger.gameObject.name.Split('_');
        Component data = GetComponentData(namePattern[0]);
        float actualLifeScaleMultiplier = (data != null) ? GetActualLifeScale(data) : 1f;
        Vector3 targetScale = componentOriginalStates[componentTrigger].localScale * actualLifeScaleMultiplier;

        float timer = 0f;
        while (timer < transitionDuration)
        {
            float progress = timer / transitionDuration;
            float curvedProgressPos = positionCurve.Evaluate(progress);
            float curvedProgressScale = scaleCurve.Evaluate(progress);

            componentTransform.position = Vector3.Lerp(startPosition, targetPosition, curvedProgressPos);
            componentTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, curvedProgressPos);
            componentTransform.localScale = Vector3.Lerp(startScale, targetScale, curvedProgressScale);

            timer += Time.deltaTime;
            yield return null;
        }

        componentTransform.position = targetPosition;
        componentTransform.rotation = targetRotation;
        componentTransform.localScale = targetScale;

        Debug.Log($"{componentTrigger.gameObject.name} fully visualized at actual life scale.");
        componentTrigger.NotifyVisualizationCompleted();
    }

    private IEnumerator ReturnToMiniatureActualRoutine(ComponentInteractionTrigger componentTrigger)
    {
        Transform componentTransform = componentTrigger.transform;
        Vector3 startPosition = componentTransform.position;
        Quaternion startRotation = componentTransform.rotation;
        Vector3 startScale = componentTransform.localScale;

        ComponentOriginalState originalState = componentOriginalStates[componentTrigger];
        Vector3 targetLocalPosition = originalState.localPosition;
        Quaternion targetLocalRotation = originalState.localRotation;
        Vector3 targetScale = originalState.localScale;

        Vector3 targetWorldPosition = originalState.parent.TransformPoint(targetLocalPosition);
        Quaternion targetWorldRotation = originalState.parent.rotation * targetLocalRotation;

        float timer = 0f;
        while (timer < transitionDuration)
        {
            float progress = timer / transitionDuration;
            float curvedProgressPos = positionCurve.Evaluate(progress);
            float curvedProgressScale = scaleCurve.Evaluate(progress);

            componentTransform.position = Vector3.Lerp(startPosition, targetWorldPosition, curvedProgressPos);
            componentTransform.rotation = Quaternion.Slerp(startRotation, targetWorldRotation, curvedProgressPos);
            componentTransform.localScale = Vector3.Lerp(startScale, targetScale, curvedProgressScale);

            timer += Time.deltaTime;
            yield return null;
        }

        componentTransform.SetParent(originalState.parent);
        componentTransform.localPosition = targetLocalPosition;
        componentTransform.localRotation = targetLocalRotation;
        componentTransform.localScale = targetScale;

        Debug.Log($"{componentTrigger.gameObject.name} returned to miniature state.");
        componentTrigger.NotifyResetCompleted();
    }

    // --- UI Management ---
    public void DisplayComponentInfo(Component data)
    {
        // --- Main Panel Updates ---
        if (infoPanel != null) infoPanel.SetActive(true);
        if (componentNameText != null) componentNameText.text = data.name;
        if (componentDescriptionText != null) componentDescriptionText.text = data.description;

        if (componentDimensionText != null && data.dimension != null)
        {
            componentDimensionText.text = $"Length: {data.dimension.length}\nWidth: {data.dimension.width}\nHeight: {data.dimension.height}";
        }
        else if (componentDimensionText != null)
        {
            componentDimensionText.text = "Dimensions: N/A";
        }

        // --- Second Panel Updates --
        if (secondInfoPanel != null) secondInfoPanel.SetActive(true);
        if (secondComponentNameText != null) secondComponentNameText.text = data.name;
        if (secondComponentDescriptionText != null) secondComponentDescriptionText.text = data.description;

        if (secondComponentDimensionText != null && data.dimension != null)
        {
            secondComponentDimensionText.text = $"Length: {data.dimension.length}\nWidth: {data.dimension.width}\nHeight: {data.dimension.height}";
        }
        else if (secondComponentDimensionText != null)
        {
            secondComponentDimensionText.text = "Dimensions: N/A";
        }

        // --- External Visualizers and Audio ---
        if (dimensionVisualizer != null && currentVisualizedComponentTrigger != null && data.dimension != null)
        {
            dimensionVisualizer.EnableDimensions(currentVisualizedComponentTrigger.gameObject, data.dimension);
        }

        if (audioSource != null && data != null && !string.IsNullOrEmpty(data.name))
        {
            //Audio Path Example: "Assets/Resources/Audio/ComponentDescription/Base Plate.mp3" -> "Audio/ComponentDescription/Base Plate"
            string audioPath = "Audio/ComponentDescription/" + data.name;
            AudioClip clip = Resources.Load<AudioClip>(audioPath);

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log($"Playing audio for: {data.name}");
            }
            else
            {
                Debug.LogWarning($"Audio clip not found for: {data.name} at path: Resources/{audioPath}. Check spelling and path.");
            }
        }
    }

    public void HideComponentInfo()
    {
        // --- Main Panel Hidin ---
        if (infoPanel != null) infoPanel.SetActive(false);
        if (componentNameText != null) componentNameText.text = "";
        if (componentDescriptionText != null) componentDescriptionText.text = "";
        if (componentDimensionText != null) componentDimensionText.text = "";

        // --- Second Panel Hiding ---
        if (secondInfoPanel != null) secondInfoPanel.SetActive(false);
        if (secondComponentNameText != null) secondComponentNameText.text = "";
        if (secondComponentDescriptionText != null) secondComponentDescriptionText.text = "";
        if (secondComponentDimensionText != null) secondComponentDimensionText.text = "";


        // --- External Visualizers and Audio ---
        if (dimensionVisualizer != null)
        {
            dimensionVisualizer.DisableDimensions();
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Stopped audio description.");
        }
    }

    // --- New Public Method for Global Reset ---
    public void GlobalResetAllComponents()
    {
        Debug.Log("Global Reset All Components requested.");

        // 1. Hide the UI info immediately, regardless of which component was showing.
        HideComponentInfo();

        // 2. Clear the VisualizationManager's tracking of the currently visualized component.
        // This is crucial because after a full reset, no single component is "current".
        if (currentVisualizedComponentTrigger != null)
        {
            // Stop its potential visualization coroutine if it's still running
            if (currentVisualizationCoroutine != null)
            {
                StopCoroutine(currentVisualizationCoroutine);
                currentVisualizationCoroutine = null;
            }
            currentVisualizedComponentTrigger = null; // The manager no longer tracks a 'current' one.
        }


        // 3. Iterate through all components known to the manager (from componentOriginalStates)
        // and request each one to reset itself.
        foreach (var entry in componentOriginalStates)
        {
            ComponentInteractionTrigger componentTrigger = entry.Key;

            // Request each component to reset. Its own state machine will decide if it needs to do anything.
            // For example:
            // - If it's in VisualizingState or ReturningState, it will start its return animation.
            // - If it's already in MiniatureState, its MiniatureState.OnResetRequested will simply log and do nothing (as per its current implementation).
            // - If it's currently grabbed during this global reset, its ReturnToMiniatureRoutine will log a warning and skip the reset animation for that specific component (as per existing logic).
            componentTrigger.RequestReset();
        }
    }

    // --- Helpers ---
    public Component GetComponentData(string componentName)
    {
        componentDataLookup.TryGetValue(componentName, out Component data);
        return data;
    }

    private float GetActualLifeScale(Component data)
    {
        return 1f;
    }

    public void GlobalResetCurrentComponent()
    {
        if (currentVisualizedComponentTrigger != null)
        {
            Debug.Log($"Global reset requested for {currentVisualizedComponentTrigger.gameObject.name}.");
            currentVisualizedComponentTrigger.RequestReset();
            // The component itself will now handle its return and transition to MiniatureState
            // The VisualizationManager no longer needs to track it as the "current" visualized one.
            currentVisualizedComponentTrigger = null;
        }
        else
        {
            Debug.Log("No component currently visualized to reset.");
        }
    }
}