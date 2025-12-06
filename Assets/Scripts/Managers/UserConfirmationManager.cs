using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
//public class UserConfirmationManager : MonoBehaviour
//{
//    public GameObject confirmationUiPanel;
//    public Button confirmButton;
//    public TMP_Text infoText;
//    public GameObject sequenceInfoPanel;
//    public void Start()
//    {
//        if (confirmationUiPanel == null || confirmButton == null || infoText == null)
//        {
//            Debug.LogError("UI elements are not properly assigned in the Inspector.");
//            return;
//        }
//        confirmationUiPanel.SetActive(false);
//        sequenceInfoPanel.SetActive(true);
//    }
//    public void Initialize( string information, Action callback)
//    {
//        if (confirmationUiPanel == null || confirmButton == null || infoText == null)
//        {
//            Debug.LogError("UI elements are not properly assigned in the Inspector.");
//            return;
//        }
//        confirmationUiPanel.SetActive(true);
//        sequenceInfoPanel.SetActive(false);
//        infoText.text = information;
//        confirmButton.onClick.RemoveAllListeners();
//        confirmButton.onClick.AddListener(() =>
//        {
//            infoText.text = "";
//            confirmationUiPanel.SetActive(false);
//            sequenceInfoPanel.SetActive(true);
//            callback?.Invoke();      
//        });
//    }

//}
