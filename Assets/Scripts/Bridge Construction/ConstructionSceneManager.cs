using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSceneManager : MonoBehaviour
{
    public GameObject introductionCanvas;
    public GameObject inventoryCanvas;
    public BridgeDrawer bridgeDrawer;
    public BaileyBridgeCalculator baileyBridgeCalculator;

    public void OnClickStartBtn()
    {
        // Initialize the scene with the introduction canvas active
        introductionCanvas.SetActive(false);
        bridgeDrawer.StartDrawingBridge();
    }
}
