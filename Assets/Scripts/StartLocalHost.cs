using UnityEngine;
using Unity.Netcode;

public class StartLocalHost : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("LOCAL HOST STARTED");
    }
}
