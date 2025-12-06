using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class TraineeUI : MonoBehaviour
{
    [Header("Final Score UI")]
    public GameObject finalScorePanel;
    public TextMeshProUGUI finalScoreText;
    public static TraineeUI Instance;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI attemptsLeftText;
    public float feedbackDuration = 5f;
    [Header("Scoreboard UI")]
    public GameObject scoreboardPanel;
    public Transform scoreboardContent;
    public GameObject scoreboardEntryPrefab;

    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowFeedback(string message)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(ShowFeedbackRoutine(message));
    }

    private IEnumerator ShowFeedbackRoutine(string message)
    {
        feedbackText.text = message;

        if (message == "Correct!")
        {
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.color = Color.red;
        }
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(feedbackDuration);

        feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    public void UpdateProgress(int attempts, int correct, int step)
    {
        attemptsLeftText.text = $"Attempts Left: {attempts}";
    }

    public void ShowFinalScore(string traineeName, int correct, int total)
    {
        StartCoroutine(ShowTraineeFinalScore(traineeName, correct, total));
    }

    private IEnumerator ShowTraineeFinalScore(string traineeName, int correct, int total)
    {
        yield return new WaitForSeconds(2.5f);
        finalScorePanel.SetActive(true);
        finalScoreText.text = $"{traineeName}: {correct}/{total}";
    }

}
