using System.Collections.Generic;
using UnityEngine;

public class HighlightingManager : MonoBehaviour
{
    private bool enableOutline = true;

    public bool EnableOutline { get => enableOutline; set => enableOutline = value; }

    public void EnableTheOutline(List<string> objectToHighlight, bool outlineState)
    {
        if (!enableOutline) return;
        foreach (string objectName in objectToHighlight)
        {
            GameObject gameObject = GameObject.Find(objectName);
            if (gameObject == null) continue;

            Outline outline = gameObject.GetComponent<Outline>();
            if (outline == null && outlineState)
            {
                outline = gameObject.AddComponent<Outline>();
                outline.enableBlinking = true;
            }

            if (outline != null)
            {
                Color brightOrange = new Color(0.953f, 0.420f, 0.012f, 1f);
                outline.OutlineColor = brightOrange;
                outline.enabled = outlineState;
                outline.OutlineWidth = 5;
                outline.enableBlinking = true;
            }
        }
    }

    private void OnDisable()
    {
        EnableOutline = false;
    }
}
