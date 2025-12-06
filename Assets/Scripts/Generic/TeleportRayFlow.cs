using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRayFlow : MonoBehaviour
{
    public float scrollSpeed = 1f;
    private Material lineMaterial;
    private Vector2 offset;

    void Start()
    {
        // Clone the material to make it unique
        lineMaterial = new Material(GetComponent<LineRenderer>().material);
        GetComponent<LineRenderer>().material = lineMaterial;

        // Enable transparency and ensure it tints
        lineMaterial.SetFloat("_Mode", 3); // Transparent
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.DisableKeyword("_ALPHATEST_ON");
        lineMaterial.EnableKeyword("_ALPHABLEND_ON");
        lineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        lineMaterial.renderQueue = 3000;
    }

    void Update()
    {
        offset.x -= scrollSpeed * Time.deltaTime;
        lineMaterial.mainTextureOffset = offset;

        // Tint the white arrows
        lineMaterial.color = Color.white;
    }
}
