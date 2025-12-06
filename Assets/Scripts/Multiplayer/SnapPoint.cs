using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SnapPoint : MonoBehaviour
{
    public int expectedOrder; // which step this SnapPoint is for
    
    private void OnTriggerEnter(Collider other)
    {
        ModelPart part = other.GetComponent<ModelPart>();
        Debug.Log("On trigger enter " + other.name);

        if (part != null)
        {
            AssemblyManager.Instance.OnPartPlaced(part, this);
        }
    }
}
