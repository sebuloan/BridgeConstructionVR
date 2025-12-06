using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PPEChecklistManager : MonoBehaviour
{
    [Header("Checklist UI")]
    public GameObject checklistItemPrefab;
    public Transform checklistParent;

    private Dictionary<string, Toggle> checklistMap = new Dictionary<string, Toggle>();
    public Color completedColor;

    /// <summary>
    /// Initializes the checklist UI by clearing existing entries
    /// and creating toggles for each item in the list.
    /// </summary>
    /// <param name="itemNames">List of item names to show in checklist.</param>
    public void InitChecklist(List<string> itemNames)
    {
        //foreach (Transform child in checklistParent)
        //    Destroy(child.gameObject);

        //checklistMap.Clear();

        foreach (string itemName in itemNames)
        {
            AddItem(itemName); // Use the AddItem logic
        }
    }

    public void AddItem(string itemName)
    {
        if (checklistMap.ContainsKey(itemName))
        {
            Debug.Log($"Item '{itemName}' already exists in checklist.");
            return;
        }

        GameObject entry = Instantiate(checklistItemPrefab, checklistParent);
        TMP_Text label = entry.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = ToPascalCase(itemName);
        else
            Debug.LogError("TMP_Text not found in prefab!");

        Toggle toggle = entry.GetComponentInChildren<Toggle>();
        if (toggle != null)
            toggle.isOn = false;
        else
            Debug.LogError("Toggle not found in prefab!");

        checklistMap[itemName] = toggle;

        Debug.Log($"Checklist item '{itemName}' added.");
    }

    /// <summary>
    /// Marks the checklist toggle for the given item as completed (checked).
    /// Also changes the color of a corresponding icon if available.
    /// </summary>
    /// <param name="itemName">Name of the item to mark completed.</param>
    public void MarkCompleted(string itemName)
    {
        if (checklistMap.TryGetValue(itemName, out Toggle toggle))
        {
            toggle.isOn = true;
        }

        // Try to find icon GameObject named like "helmet_icon"
        string iconName = itemName + "_icon";
        GameObject iconObj = GameObject.Find(iconName);
        if (iconObj != null)
        {
            UnityEngine.UI.Image img = iconObj.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = completedColor;
            }
        }
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (word.Length > 0)
                words[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }

        return string.Join("", words);
    }
}