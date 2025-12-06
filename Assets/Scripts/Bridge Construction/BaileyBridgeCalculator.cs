using UnityEngine;
using UnityEngine.UI; // Required for UI components like Text and InputField
using TMPro; // Required for TextMeshPro UI components

public class BaileyBridgeCalculator : MonoBehaviour
{
    // Public references for UI components in the Inspector
    public float bridgeLength;
    //public TMP_Text resultText;
    public TMP_Text bridgeLengthText;
    public TMP_Text panelText;
    public TMP_Text transomsText;
    public TMP_Text bracingFramesText;
    public TMP_Text endPostsText;
    public TMP_Text bearingsText;
    public TMP_Text basePlatesText;
    public TMP_Text panelPinsText;

    // Fixed length of a single bay in feet (10 feet is standard)
    private const float bayLengthFeet = 10.0f;

    /// <summary>
    /// This method is called by a UI Button. It calculates all components
    /// and updates the result text field.
    /// </summary>
    public void CalculateComponents()
    {
        bridgeLengthText.text = bridgeLength.ToString("F2") + " m"; // Update the text field with the bridge length
        // Parse the input length from the text field
        // Ensure the length is positive
        if (bridgeLength <= 0)
        {
            Debug.Log("Bridge length must be greater than zero.");
            return;
        }

        // --- Component Calculation Logic ---

        // 1. Calculate the number of bays
        int numberOfBays = Mathf.CeilToInt(bridgeLength / bayLengthFeet);

        // 2. Calculate the number of Panels (2 per bay for SS)
        int numberOfPanels = numberOfBays * 2;

        // 3. Calculate the number of Transoms (1 per bay + 1)
        int numberOfTransoms = numberOfBays + 1;

        // 4. Calculate the number of Bracing Frames (1 per bay)
        int numberOfBracingFrames = numberOfBays;

        // 5. Calculate the number of End Posts (2 male, 2 female)
        int numberOfEndPosts = 4;

        // 6. Calculate the number of Bearings (4 total)
        int numberOfBearings = 4;

        // 7. Calculate the number of Base Plates (4 total)
        int numberOfBasePlates = 4;

        // 8. Calculate the number of Panel Pins (2 per connection, plus 4 for end posts)
        int numberOfPanelPins = (numberOfBays > 1) ? ((numberOfBays - 1) * 2) + 4 : 4;

        // --- Display the Results ---
        string results = $"<size=150%>Bridge Components for {bridgeLength:F2} ft Bridge:</size>\n\n" +
                         $"<size=125%>Number of Bays: {numberOfBays}</size>\n" +
                         $"----------------------------------------\n" +
                         $"Panels: {numberOfPanels}\n" +
                         $"Transoms: {numberOfTransoms}\n" +
                         $"Bracing Frames: {numberOfBracingFrames}\n" +
                         $"End Posts: {numberOfEndPosts}\n" +
                         $"Bearings: {numberOfBearings}\n" +
                         $"Base Plates: {numberOfBasePlates}\n" +
                         $"Panel Pins: {numberOfPanelPins}";

        Debug.Log(results);
        panelText.text = "Count - " + numberOfPanels.ToString();
        transomsText.text = "Count - " + numberOfTransoms.ToString();
        bracingFramesText.text = "Count - " + numberOfBracingFrames.ToString();
        endPostsText.text = "Count - " + numberOfEndPosts.ToString();
        bearingsText.text = "Count - " + numberOfBearings.ToString();
        basePlatesText.text = "Count - " + numberOfBasePlates.ToString();
        panelPinsText.text = "Count - " + numberOfPanelPins.ToString();
    }
}