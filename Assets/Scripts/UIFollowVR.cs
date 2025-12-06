using UnityEngine;

public class UIFollowVR : MonoBehaviour
{
    [System.Serializable]
    public class UIFollowData
    {
        [Header("UI Reference")]
        public RectTransform uiRectTransform;

        [Header("Offsets")]
        public Vector3 positionOffset = new Vector3(0f, 0f, 2f);
        public Vector3 rotationOffset = Vector3.zero;
    }

    [Header("Target Settings")]
    public Transform target; // VR camera / head

    [Header("Follow Settings")]
    [Range(0f, 20f)] public float positionSmoothSpeed = 5f;
    [Range(0f, 20f)] public float rotationSmoothSpeed = 5f;
    public bool faceTarget = true;

    [Header("UI Panels")]
    public UIFollowData[] uiPanels; // Array for multiple UIs

    private void LateUpdate()
    {
        if (target == null || uiPanels == null) return;

        foreach (var ui in uiPanels)
        {
            if (ui.uiRectTransform == null) continue;

            // --- Smooth position in front of target ---
            Vector3 desiredPosition = target.position
                                    + target.forward * ui.positionOffset.z
                                    + target.up * ui.positionOffset.y
                                    + target.right * ui.positionOffset.x;

            ui.uiRectTransform.position = Vector3.Lerp(
                ui.uiRectTransform.position,
                desiredPosition,
                Time.deltaTime * positionSmoothSpeed
            );

            // --- Smooth rotation to face target ---
            if (faceTarget)
            {
                Vector3 lookDirection = (target.position - ui.uiRectTransform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
                targetRotation *= Quaternion.Euler(ui.rotationOffset);

                ui.uiRectTransform.rotation = Quaternion.Slerp(
                    ui.uiRectTransform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSmoothSpeed
                );
            }
        }
    }
}
