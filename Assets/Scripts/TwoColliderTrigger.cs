using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to specific objects
        if (other.gameObject == objectA)
        {
            Debug.Log("Object A entered the zone");
            // Call your interaction manager logic here
            // Example: ObjectInteractionManager.Instance.OnPartPlaced(...)
        }
        else if (other.gameObject == objectB)
        {
            Debug.Log("Object B entered the zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectA)
        {
            Debug.Log("Object A exited the zone");
        }
        else if (other.gameObject == objectB)
        {
            Debug.Log("Object B exited the zone");
        }
    }
}
