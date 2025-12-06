using UnityEngine;

public class PlacementTrigger : MonoBehaviour
{
    public ObjectInteractionManager manager;
    public int stepIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (manager != null)
        {
            manager.OnPartPlaced(stepIndex, other.gameObject);
        }
    }
}
