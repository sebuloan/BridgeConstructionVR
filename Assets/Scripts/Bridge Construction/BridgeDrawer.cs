using UnityEngine;
using System.Collections;
using TMPro;
using Bosch.ESA.Managers;

public class BridgeDrawer : MonoBehaviour
{
    // Public references for the start and end points and the Text label
    public Transform startPoint;
    public Transform endPoint;
    public TMP_Text bridgeLengthText;
    public TMP_Text bridgeStartText;
    public TMP_Text bridgeEndText;

    // Configuration for the drawing animation
    public float animationDuration = 2.0f;
    public float verticalLineHeight = 5.0f;
    public float lineWidth = 0.1f;

    // Colors for the lines
    public Color verticalLineColor = Color.blue;
    public Color horizontalLineColor = Color.red;

    public GameObject inventoryPanel;
    public BaileyBridgeCalculator baileyBridgeCalculator;
    public GameObject lengthCanvas;

    public SequenceManager sequenceManager;
    public GameObject sequenceUIPanel;

    public GameObject interactableObjects;

    public AudioClip bridgeConstructionModuleAudio;
    public AudioClip inventoryModuleAudio;

    private float currentAnimationTime = 0f;
    private bool animationStarted = false;

    // Private references for the dynamically created components
    private LineRenderer verticalLineRenderer1;
    private LineRenderer verticalLineRenderer2;
    private LineRenderer horizontalLineRenderer;

    private Camera mainCamera;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.PlayOneShot(bridgeConstructionModuleAudio);

        //StartDrawingBridge();
        bridgeLengthText.gameObject.SetActive(false);
        bridgeStartText.gameObject.SetActive(false);
        bridgeEndText.gameObject.SetActive(false);
        mainCamera = Camera.main;
    }

    private void Update()
    {
        bridgeLengthText.transform.LookAt(mainCamera.transform);
        bridgeStartText.transform.LookAt(mainCamera.transform);
        bridgeEndText.transform.LookAt(mainCamera.transform);

        bridgeLengthText.transform.Rotate(0, 180, 0);
        bridgeStartText.transform.Rotate(0, 180, 0);
        bridgeEndText.transform.Rotate(0, 180, 0);
    }

    /// <summary>
    /// The public method to start the drawing animation.
    /// It first creates the lines, then starts the animation coroutine.
    /// </summary>
    public void StartDrawingBridge()
    {
        // Destroy any existing lines before starting a new drawing
        DestroyExistingLines();

        lengthCanvas.SetActive(true);
        // Ensure all required components are assigned
        if (startPoint == null || endPoint == null || bridgeLengthText == null)
        {
            Debug.LogError("Start Point, End Point, and Bridge Length Text must be assigned in the Inspector!");
            return;
        }

        // Create the line renderers dynamically
        CreateLineRenderers();

        // Hide the text label until the animation is complete
        bridgeLengthText.enabled = false;

        // Start the coroutine for the drawing animation
        StartCoroutine(DrawBridgeCoroutine());
    }

    /// <summary>
    /// Creates the GameObjects and attaches LineRenderer components at runtime.
    /// </summary>
    private void CreateLineRenderers()
    {
        // Create the first vertical line
        GameObject verticalLine1Object = new GameObject("VerticalLine1");
        verticalLine1Object.transform.SetParent(this.transform);
        verticalLineRenderer1 = verticalLine1Object.AddComponent<LineRenderer>();
        ConfigureLineRenderer(verticalLineRenderer1, verticalLineColor);

        // Create the second vertical line
        GameObject verticalLine2Object = new GameObject("VerticalLine2");
        verticalLine2Object.transform.SetParent(this.transform);
        verticalLineRenderer2 = verticalLine2Object.AddComponent<LineRenderer>();
        ConfigureLineRenderer(verticalLineRenderer2, verticalLineColor);

        // Create the horizontal line
        GameObject horizontalLineObject = new GameObject("HorizontalLine");
        horizontalLineObject.transform.SetParent(this.transform);
        horizontalLineRenderer = horizontalLineObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(horizontalLineRenderer, horizontalLineColor);
    }

    /// <summary>
    /// Helper method to configure a LineRenderer component.
    /// </summary>
    private void ConfigureLineRenderer(LineRenderer lr, Color color)
    {
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // Set a simple material for the LineRenderer.
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.renderQueue = 5000; // Ensure it renders on top of most objects
        lr.positionCount = 0; // Start with no points
    }

    /// <summary>
    /// Destroys any previously created line GameObjects.
    /// </summary>
    public void DestroyExistingLines()
    {
        lengthCanvas.SetActive(false);

        // Find and destroy all children of this GameObject
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator DrawBridgeCoroutine()
    {
        currentAnimationTime = 0f;
        animationStarted = true;

        Vector3 groundStart = startPoint.position;
        Vector3 groundEnd = endPoint.position;

        // 1. Animate drawing the two vertical lines
        while (currentAnimationTime < animationDuration)
        {
            float t = currentAnimationTime / animationDuration;

            Vector3 vertical1Top = Vector3.Lerp(groundStart, groundStart + Vector3.up * verticalLineHeight, t);
            Vector3 vertical2Top = Vector3.Lerp(groundEnd, groundEnd + Vector3.up * verticalLineHeight, t);

            verticalLineRenderer1.positionCount = 2;
            verticalLineRenderer1.SetPosition(0, groundStart);
            verticalLineRenderer1.SetPosition(1, vertical1Top);

            verticalLineRenderer2.positionCount = 2;
            verticalLineRenderer2.SetPosition(0, groundEnd);
            verticalLineRenderer2.SetPosition(1, vertical2Top);

            currentAnimationTime += Time.deltaTime;
            yield return null;
        }

        verticalLineRenderer1.SetPosition(1, groundStart + Vector3.up * verticalLineHeight);
        verticalLineRenderer2.SetPosition(1, groundEnd + Vector3.up * verticalLineHeight);

        bridgeStartText.gameObject.SetActive(true);
        bridgeEndText.gameObject.SetActive(true);

        currentAnimationTime = 0f;

        // 2. Animate drawing the horizontal line
        Vector3 horizontalStart = groundStart + Vector3.up * (verticalLineHeight - 1.5f);
        Vector3 horizontalEnd = groundEnd + Vector3.up * (verticalLineHeight - 1.5f);

        while (currentAnimationTime < animationDuration)
        {
            float t = currentAnimationTime / animationDuration;

            Vector3 currentHorizontalPoint = Vector3.Lerp(horizontalStart, horizontalEnd, t);

            horizontalLineRenderer.positionCount = 2;
            horizontalLineRenderer.SetPosition(0, horizontalStart);
            horizontalLineRenderer.SetPosition(1, currentHorizontalPoint);

            currentAnimationTime += Time.deltaTime;
            yield return null;
        }

        horizontalLineRenderer.SetPosition(1, horizontalEnd);

        // 3. Display the bridge length text
        bridgeLengthText.gameObject.SetActive(true);
        float bridgeLength = Vector3.Distance(startPoint.position, endPoint.position);
        bridgeLengthText.text = $"Bridge Span: {bridgeLength:F2} m";
        bridgeLengthText.enabled = true;

        Vector3 textPosition = (horizontalStart + horizontalEnd) / 2f + Vector3.up * 0.6f;
        bridgeLengthText.transform.position = textPosition;
        //bridgeLengthText.transform.LookAt(Camera.main.transform);
        //bridgeLengthText.transform.Rotate(0, 180, 0);

        animationStarted = false;

        yield return new WaitForSeconds(5f);

        // Enable the inventory panel and set the bridge length in the calculator
        baileyBridgeCalculator.bridgeLength = bridgeLength;
        inventoryPanel.SetActive(true);
        audioSource.PlayOneShot(inventoryModuleAudio);
        baileyBridgeCalculator.CalculateComponents();
    }

    //Call this on continue button
    public void ContinueWithConstruction()
    {
        DestroyExistingLines();
        baileyBridgeCalculator.bridgeLength = 0f;
        lengthCanvas.SetActive(false);
        inventoryPanel.SetActive(false);

        sequenceManager.enabled = true;
        sequenceUIPanel.SetActive(true);
        interactableObjects.SetActive(true);
    }
}