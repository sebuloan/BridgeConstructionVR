using UnityEngine;

public class Blink : MonoBehaviour
{
    public bool isBlink = false;
    public Color blinkColor = Color.yellow;
    public float blinkSpeed = 2f;

    private Renderer[] renderers;
    private Color[] originalColors;
    private Material[] matInstances;

    void Start()
    {
        // Get all renderers in this object and its children
        renderers = GetComponentsInChildren<Renderer>();

        if (renderers != null && renderers.Length > 0)
        {
            matInstances = new Material[renderers.Length];
            originalColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    matInstances[i] = renderers[i].material; // instance copy for safe editing
                    originalColors[i] = matInstances[i].color;
                }
            }
        }
    }

    void Update()
    {
        if (matInstances == null) return;

        if (isBlink)
        {
            float emission = Mathf.PingPong(Time.time * blinkSpeed, 1f);

            for (int i = 0; i < matInstances.Length; i++)
            {
                if (matInstances[i] != null)
                {
                    Color finalColor = Color.Lerp(originalColors[i], blinkColor, emission);
                    matInstances[i].color = finalColor;
                }
            }
        }
        else
        {
            // Reset back to original colors
            for (int i = 0; i < matInstances.Length; i++)
            {
                if (matInstances[i] != null)
                    matInstances[i].color = originalColors[i];
            }
        }
    }
}
