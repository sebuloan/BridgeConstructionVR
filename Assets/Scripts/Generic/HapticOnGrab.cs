using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class HapticOnGrab : MonoBehaviour
{
    #region Public Variables
    public XRBaseController leftController;
    public XRBaseController rightController;
    public float amplitude = 0.5f;
    public float duration = 0.1f;
    public float delay = 0f;
    #endregion

    #region Private Variables
    private Coroutine _errorFeedbackCoroutine;
    #endregion

    #region Public Methods

    /// <summary>
    /// Call this method when an object is grabbed.
    /// </summary>
    public void ObjectGrabbed()
    {
        TriggerHapticFeedback();
    }

    /// <summary>
    /// Call this method when an object is released.
    /// Stops any ongoing error feedback coroutine.
    /// </summary>
    public void ObjectReleased()
    {
        if (_errorFeedbackCoroutine != null)
        {
            StopCoroutine(_errorFeedbackCoroutine);
            _errorFeedbackCoroutine = null;
        }
    }

    /// <summary>
    /// Call this to trigger error feedback: haptics repeated 3 times.
    /// </summary>
    public IEnumerator TriggerErrorFeedback()
    {
        for (int i = 0; i < 3; i++)
        {
            TriggerHapticFeedback();
            yield return new WaitForSeconds(0.4f);
        }
    }

    /// <summary>
    /// Triggers haptic impulses on both controllers (if assigned).
    /// </summary>
    public void TriggerHapticFeedback()
    {
        if (rightController != null)
            rightController.SendHapticImpulse(amplitude, duration);
        if (leftController != null)
            leftController.SendHapticImpulse(amplitude, duration);
    }
    #endregion
}
