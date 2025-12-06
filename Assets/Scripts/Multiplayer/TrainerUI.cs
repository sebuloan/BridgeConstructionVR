using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TrainerUI : MonoBehaviour
{
    public static TrainerUI Instance;
    public GameObject buttonContainer;
    public TextMeshProUGUI attemptsLeftText;

    [Header("Final Score UI")]
    public GameObject finalScorePanel;
    public TextMeshProUGUI finalScoreText;

    private Coroutine feedbackCoroutine;
    private ulong currentPartNetId;
    private HashSet<ulong> completedParts = new HashSet<ulong>();

    void Awake() => Instance = this;
    public void ShowValidationUI(ulong partNetId, int partOrder, int snapOrder)
    {
        Debug.Log("Enable correct and incorrect btns");
        if (completedParts.Contains(partNetId)) return;
        if (currentPartNetId == partNetId) return;
        currentPartNetId = partNetId;
        buttonContainer.gameObject.SetActive(true);
    }

    public void HideValidationUI()
    {
        buttonContainer.SetActive(false);
        currentPartNetId = 0; // reset for next part
    }


    public void OnCorrectPressed()
    {
        completedParts.Add(currentPartNetId);
        AssemblyManager.Instance.SubmitFeedbackServerRpc(true, currentPartNetId);
        HideValidationUI();

    }

    public void OnIncorrectPressed()
    {
        AssemblyManager.Instance.SubmitFeedbackServerRpc(false, currentPartNetId);
        HideValidationUI();
    }


    public void UpdateProgress(int attempts, int correct, int step)
    {
        attemptsLeftText.text = $"Attempts Left: {attempts}";
    }

    public void ShowFinalScore(string traineeName, int correct, int total)
    {
        StartCoroutine(ShowTrainerFinalScore(traineeName, correct, total));
    }

    private IEnumerator ShowTrainerFinalScore(string traineeName, int correct, int total)
    {
        yield return new WaitForSeconds(1.5f);
        finalScorePanel.SetActive(true);
        finalScoreText.text = $"{traineeName}: {correct}/{total}";
    }
}
