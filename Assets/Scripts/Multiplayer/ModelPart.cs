using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ModelPart : NetworkBehaviour
{
    public int correctOrder;
    public Transform snapPoint;
    private Vector3 _originalPos;
    private Quaternion _originalRot;

    private XRGrabInteractable _grab;
    public bool IsPlaced { get; private set; } = false;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _originalPos = transform.position;
        _originalRot = transform.rotation;
    }

    public void ResetToOriginalPosition()
    {
        transform.SetPositionAndRotation(_originalPos, _originalRot);
        IsPlaced = false;
        _grab.enabled = true;
    }

    public void LockAtSnap()
    {
        transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
        IsPlaced = true;
        _grab.enabled = false;
    }

    public void TriggerHaptics(float amplitude = 0.5f, float duration = 0.1f)
   {
        if (_grab.isSelected && _grab.firstInteractorSelecting is XRBaseInputInteractor interactor)
        {
            interactor.SendHapticImpulse(amplitude, duration);
        }
   }

}
