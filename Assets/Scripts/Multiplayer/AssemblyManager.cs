using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class AssemblyManager : NetworkBehaviour
{
    public static AssemblyManager Instance;

    [Header("Settings")]
    public int totalParts = 8;          // Number of bridge parts = total attempts
    [Header("Scoreboard UI")]
    public GameObject scoreboardPanel;
    public Transform scoreboardContent;
    public GameObject scoreboardEntryPrefab;

    private int _attemptsLeft;
    private int _correctCount;
    private int _currentStep = 1;

    void Awake() => Instance = this;

    void Start()
    {
        _attemptsLeft = totalParts;
        _correctCount = 0;
        _currentStep = 1;
    }

    #region SnapPoint Handling
    public void OnPartPlaced(ModelPart part, SnapPoint snap)
    {
        if (!IsServer) return;
        StartCoroutine(HandlePartPlacement(part, snap));
    }

    private IEnumerator HandlePartPlacement(ModelPart part, SnapPoint snap)
    {
        part.TriggerHaptics(0.5f, 0.5f);
        yield return new WaitForSeconds(0.2f);
        XRGrabInteractable grab = part.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;
        ShowTrainerValidationClientRpc(part.NetworkObjectId, part.correctOrder, snap.expectedOrder);
    }
    #endregion

    [ClientRpc]
    void ShowTrainerValidationClientRpc(ulong partNetId, int correctOrder, int expectedOrder)
    {
        // Only run this on the trainer client
        if (NetworkManager.Singleton.IsServer) return;
        Debug.Log("Show trainer UI on client");
        TrainerUI.Instance.ShowValidationUI(partNetId, correctOrder, expectedOrder);
    }

    #region Trainer Feedback

    // Called by trainer button
    [ServerRpc(RequireOwnership = false)]
    public void SubmitFeedbackServerRpc(bool isCorrect, ulong partNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(partNetId)) return;

        ModelPart part = NetworkManager.Singleton.SpawnManager.SpawnedObjects[partNetId].GetComponent<ModelPart>();
        if (part == null) return;

        _attemptsLeft--;

        if (isCorrect)
        {
            part.LockAtSnap(); // Snap to the SnapPoint
            _correctCount++;
            _currentStep++;
        }
        else
        {
            part.ResetToOriginalPosition(); // Move back to original

        }

        TrainerUI.Instance.UpdateProgress(_attemptsLeft, _correctCount, _currentStep);
        UpdateProgressClientRpc(_attemptsLeft, _correctCount, _currentStep);
        HideTrainerButtonsClientRpc();
        ShowFeedbackClientRpc(isCorrect);

        // End condition: all parts placed correctly or attempts exhausted
        if (_correctCount >= totalParts || _attemptsLeft <= 0)
        {
            EndAssemblySession();
        }
    }

    [ClientRpc]
    void HideTrainerButtonsClientRpc()
    {
        if (TrainerUI.Instance != null)
            TrainerUI.Instance.buttonContainer.SetActive(false);
    }

    [ClientRpc]
    void ShowFeedbackClientRpc(bool isCorrect)
    {
        if (!IsServer) return;
        TraineeUI.Instance.ShowFeedback(isCorrect ? "Correct!" : "Incorrect!");
    }

    [ClientRpc]
    void UpdateProgressClientRpc(int attempts, int correct, int step)
    {
        TraineeUI.Instance.UpdateProgress(attempts, correct, step);  // create UpdateProgress method in TraineeUI
    }

    #endregion

    #region End Session

    void EndAssemblySession()
    {
        string traineeName = PlayerPrefs.GetString("TraineeName");
        int score = _correctCount * 10;
        int totalScore = totalParts * 10;

        // Show final score locally
        ShowFinalScoreClientRpc(traineeName, score, totalScore);

        // Send score to trainer (server)
        SubmitTraineeScoreServerRpc(traineeName, score, totalScore);

        // Disable all grab interactions
        foreach (var part in FindObjectsByType<ModelPart>(FindObjectsSortMode.None))
        {
            var grab = part.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitTraineeScoreServerRpc(string traineeName, int correct, int total)
    {
        // Save trainee score on the server (trainer device)
        SaveTraineeResult(traineeName, correct, total);
        string stored = PlayerPrefs.GetString("Scoreboard", "");
      //  string[] entries = string.IsNullOrEmpty(stored) ? new string[0] : stored.Split('|');
        // After saving, tell everyone to show the updated scoreboard
        ShowScoreboardClientRpc(stored);
    }

    void SaveTraineeResult(string traineeName, int correct, int total)
    {
        // Create a new entry
        string newEntry = $"{traineeName}:    {correct}/{total}";

        // Load existing scoreboard
        string existing = PlayerPrefs.GetString("Scoreboard", "");

        // Append new entry with separator
        if (!string.IsNullOrEmpty(existing))
        {
            existing += "|";
        }
        existing += newEntry;

        // Save back to PlayerPrefs
        PlayerPrefs.SetString("Scoreboard", existing);
        PlayerPrefs.Save();

        Debug.Log($"Saved trainee result: {existing}");
    }


    [ClientRpc]
    void ShowScoreboardClientRpc(string allEntries)
    {
        scoreboardPanel.SetActive(true);
        string[] entries = string.IsNullOrEmpty(allEntries) ? new string[0] : allEntries.Split('|');

        foreach (Transform child in scoreboardContent)
            Destroy(child.gameObject);

        // Instantiate prefab entries
      /*  foreach (string entry in entries)
        {
            GameObject row = Instantiate(scoreboardEntryPrefab, scoreboardContent);
            var text = row.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = entry;
        }*/
        for (int i = entries.Length - 1; i >= 0; i--)
       {
            GameObject row = Instantiate(scoreboardEntryPrefab, scoreboardContent);
            var text = row.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = entries[i];
       }
    }


    [ClientRpc]
    void ShowFinalScoreClientRpc(string traineeName, int correct, int total)
    {
        if (IsServer)
        {
            TraineeUI.Instance.ShowFinalScore(traineeName, correct, total);
        }
        else
        {
            TrainerUI.Instance.ShowFinalScore(traineeName, correct, total);
        }
        StartCoroutine(ShowScoreboardAfterDelay(3f));
    }
    
    IEnumerator ShowScoreboardAfterDelay(float delay)
    {
         yield return new WaitForSeconds(delay);

        // On server, get entries from PlayerPrefs and send to clients
        if (IsServer)
        {
            string stored = PlayerPrefs.GetString("Scoreboard", "");
            ShowScoreboardClientRpc(stored);
        }
    }
  #endregion

}