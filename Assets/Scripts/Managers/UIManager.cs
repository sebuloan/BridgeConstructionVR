using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    //public GameObject sequenceInfoPanelParent;
    public TextMeshProUGUI headerText;
    public GameObject confirmationUiPanel;
    public TextMeshProUGUI infoTextForConfirmation;
    public GameObject sequenceInfoPanel;
    public GameObject ppePanel;
    public GameObject stepNumberPanel;
    public TextMeshProUGUI stepNumberText;
    public GameObject uiQuitPanel;
    public GameObject uiNonQuitPanel;
    public GameObject nonQuitPanel;
    public TextMeshProUGUI apkversion;
    private bool wasXButtonPressed = false;

    [Header("Buttons")]
    public Button QuitButton;
    public Button restartButton;
    public Button backButton;
    public Button powerButton;
    public Button confirmButton;

    [Header("Toggling UI")]
    public GameObject uiScreenPanel;                     // Set this in Inspector
    public InputActionReference xButtonInput;           // add primary button input reference

    private bool isActive = true;

    public void OnEnable()
    {
        confirmationUiPanel.SetActive(false);
        sequenceInfoPanel.SetActive(false);
        ppePanel.SetActive(false);
        stepNumberPanel.SetActive(false);
        nonQuitPanel.SetActive(true);
        uiNonQuitPanel.SetActive(true);
        uiQuitPanel.SetActive(false);

        xButtonInput.action.performed += OnXButtonPressed;
        xButtonInput.action.Enable();
    }

    public void Start()
    {
        QuitButton.onClick.AddListener(OnQuitButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        powerButton.onClick.AddListener(OnPowerButtonClicked);
    }
    public void SetSequenceInfoPanel()
    {
        sequenceInfoPanel.SetActive(true);
        confirmationUiPanel.SetActive(false);
        ppePanel.SetActive(false);
        ClearText();
    }
  

    private void OnXButtonPressed(InputAction.CallbackContext context)
    {
        if (uiScreenPanel == null) return;

        isActive = !isActive;
        uiScreenPanel.SetActive(isActive);
    }

    //private void Update()
    //{
    //    InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    //    bool xButtonPressed = false;

    //    if (leftHand.isValid &&
    //        leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out xButtonPressed))
    //    {
    //        if (xButtonPressed && !wasXButtonPressed)
    //        {
    //            ToggleSequenceInfoPanel();  // Trigger only on button down
    //        }
    //        wasXButtonPressed = xButtonPressed;
    //    }
    //    //// XR Controller "X" button (primaryButton on left hand)
    //    //InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    //    //bool xButtonPressed = false;
    //    //    if (leftHand.isValid && leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out xButtonPressed) && xButtonPressed)
    //    //{
    //    //    ToggleSequenceInfoPanel();
    //    //}
    //}

    //public void ToggleSequenceInfoPanel()
    //{
    //    if (sequenceInfoPanelParent != null)
    //    {
    //        sequenceInfoPanelParent.SetActive(!sequenceInfoPanelParent.activeSelf);
    //        sequenceInfoPanelParent.GetComponent<UIFollowHead>()._isFollowingEnabled = !sequenceInfoPanelParent.activeSelf;

    //    }

    //}

    public void ShowText(List<string> texts, int stepNumber = 0)
    {
        if (texts == null)
        {
            Debug.LogWarning("ShowText: Provided list of texts is null.");
            displayText.text = "";
            return;
        }
        stepNumberPanel.SetActive(true );
        sequenceInfoPanel.SetActive(true);
        stepNumberText.text = stepNumber.ToString();
        ClearText();
        foreach (string text in texts)
        {
            displayText.text += text + "\n";
        }
    }

    public void ClearText()
    {
        displayText.text = "";
    }


    public void SwitchToConfimationPanel(string information, Action callback)
    {
        if (confirmationUiPanel == null || confirmButton == null || infoTextForConfirmation == null)
        {
            Debug.LogError("UI elements are not properly assigned in the Inspector.");
            return;
        }
        confirmationUiPanel.SetActive(true);
        sequenceInfoPanel.SetActive(false);
        stepNumberPanel.SetActive(false);
      //  stepNumberText.text = stepNumber;
        ppePanel.SetActive(false);
        infoTextForConfirmation.text = information;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            infoTextForConfirmation.text = "";
            confirmationUiPanel.SetActive(false);
            ppePanel.SetActive(false);
            sequenceInfoPanel.SetActive(true);
            callback?.Invoke();
        });
    }

    public void SwitchToPPEPanel(int stepNumber)
    {
        confirmationUiPanel.SetActive(false);
        sequenceInfoPanel.SetActive(false);
        ppePanel.SetActive(true);
        stepNumberPanel.SetActive(true);
        stepNumberText.text = stepNumber.ToString();
    }

    public void SwitchToQuitPanel()
    {
        nonQuitPanel.SetActive(false);
        uiNonQuitPanel.SetActive(false);
        uiQuitPanel.SetActive(true);
    }

    void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked");
        Application.Quit(); 
    }

    void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked");
        SceneManager.LoadScene(0);
    }

    void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");
        nonQuitPanel.SetActive(true);
        uiNonQuitPanel.SetActive(true);
        uiQuitPanel.SetActive(false);
    }

    void OnPowerButtonClicked()
    {
        Debug.Log("Power button clicked");
        SwitchToQuitPanel();
        apkversion.text = Application.version;
    }

    private void OnDisable()
    {
        xButtonInput.action.performed -= OnXButtonPressed;
        xButtonInput.action.Disable();
    }

}
