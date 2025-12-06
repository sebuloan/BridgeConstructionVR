using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowHead : MonoBehaviour
{
    [Tooltip("The target transform to follow.")]
    public Transform targetTransform;

    [Tooltip("The desired distance from the target.")]
    public float followDistance = 1.5f;

    [Tooltip("How quickly the UI moves to its desired position.")]
    [Range(0f, 20f)]
    public float smoothSpeed = 10f;

    [Tooltip("Movement threshold in meters before the UI starts significantly moving.")]
    public float movementThreshold = 0.05f;

    [Tooltip("How quickly the UI rotates to face the target.")]
    [Range(0f, 10f)]
    public float rotationSpeed = 5f;

    [Tooltip("Should the UI maintain a world-up orientation?")]
    public bool maintainWorldUp = true;

    [Tooltip("Offset in local space to adjust the initial horizontal position.")]
    public Vector3 localOffset = Vector3.zero;

    [Tooltip("Optional: Button to toggle the follow feature.")]
    public Button toggleFollowButton;

    private Vector3 _desiredPosition;
    private float _fixedYOffset;
    private Vector3 _lastTargetPosition;
    public bool _isFollowingEnabled = true;

    private void Start()
    {
        // Find targetTransform if not assigned.
        if (targetTransform == null)
        {
            XROrigin origin = FindObjectOfType<XROrigin>();
            if (origin != null && origin.CameraFloorOffsetObject != null)
            {
                targetTransform = origin.CameraFloorOffsetObject.transform;
            }
            else if (Camera.main != null)
            {
                targetTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("UIFollowHeadSmart: Target Transform not assigned.");
                enabled = false;
                return;
            }
        }

        // Ensure Canvas is in World Space.
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("UIFollowHeadSmart: Canvas Render Mode should be 'World Space'.");
            canvas.renderMode = RenderMode.WorldSpace;
        }
        else if (canvas == null)
        {
            Debug.LogError("UIFollowHeadSmart: Requires a Canvas component.");
            enabled = false;
            return;
        }

        // Calculate initial desired position and fixed Y offset.
        Vector3 targetForwardHorizontal = Vector3.ProjectOnPlane(targetTransform.forward, Vector3.up).normalized;
        _desiredPosition = targetTransform.position + targetForwardHorizontal * followDistance + targetTransform.TransformVector(new Vector3(localOffset.x, localOffset.y, localOffset.z));
        //_fixedYOffset = transform.position.y - targetTransform.position.y;
        //_desiredPosition.y = targetTransform.position.y + _fixedYOffset; // Lock initial Y.
        transform.position = _desiredPosition; // SwitchToConfimationPanel UI position.
        _lastTargetPosition = targetTransform.position;

        // Setup the toggle button if assigned.
        if (toggleFollowButton != null)
        {
            toggleFollowButton.onClick.AddListener(ToggleFollow);
            UpdateToggleButtonText(); // Set initial button text.
        }
    }

    private void Update()
    {
        if (targetTransform == null || !_isFollowingEnabled)
        {
            return;
        }

        // Calculate the target's horizontal forward.
        Vector3 targetForwardHorizontal = Vector3.ProjectOnPlane(targetTransform.forward, Vector3.up).normalized;

        // Calculate the raw desired position.
        Vector3 rawDesiredPosition = targetTransform.position + targetForwardHorizontal * followDistance + targetTransform.TransformVector(new Vector3(localOffset.x, 0f, localOffset.z));
        rawDesiredPosition.y = targetTransform.position.y + _fixedYOffset; // Maintain fixed Y.

        // Check for significant movement.
        if (Vector3.Distance(targetTransform.position, _lastTargetPosition) > movementThreshold)
        {
            // Directly interpolate towards the raw desired position.
            _desiredPosition = rawDesiredPosition;
            transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Gently move towards the raw desired position if movement is small.
            transform.position = Vector3.Lerp(transform.position, rawDesiredPosition, Time.deltaTime * smoothSpeed * 0.2f); // Slower smooth.
        }

        // Calculate the desired rotation to face the target (horizontal only).
        Quaternion desiredRotation = Quaternion.LookRotation(transform.position - targetTransform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);

        _lastTargetPosition = targetTransform.position; // Update last target position.
    }

    public void ToggleFollow()
    {
        _isFollowingEnabled = !_isFollowingEnabled;
        UpdateToggleButtonText();
    }

    private void UpdateToggleButtonText()
    {
        if (toggleFollowButton != null)
        {
            TMP_Text buttonText = toggleFollowButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = _isFollowingEnabled ? "Disable Follow" : "Enable Follow";
            }
            else
            {
                Debug.LogWarning("UIFollowHeadSmart: Toggle Button has no Text component in its children.");
            }
        }
    }
}
