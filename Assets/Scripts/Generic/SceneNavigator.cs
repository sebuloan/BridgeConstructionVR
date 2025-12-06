using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    public void LoadBridgeConstructionScene()
    {
        // Load the Bridge Construction scene
        SceneManager.LoadScene(2);
    }

    public void LoadComponentInteractionScene()
    {
        // Load the Component Interaction scene
        SceneManager.LoadScene(1);
    }
}
