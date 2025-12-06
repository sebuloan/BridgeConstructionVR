using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeleportAndStartHandler : MonoBehaviour
{
    public MultiTeleport teleportSystem;
    public ObjectInteractionManager interactionManager;

    public void OnTeleportBackAndStartNextStep()
    {
        StartCoroutine(TeleportAndStartSequence());
    }

    private IEnumerator TeleportAndStartSequence()
    {
        // First teleport to A
        teleportSystem.TeleportToA();

        // Wait a moment for teleport to complete
        yield return new WaitForSeconds(0.5f);

        // Then start the next step sequence
        interactionManager.OnTeleportBackAndStartNextStep();
    }
}