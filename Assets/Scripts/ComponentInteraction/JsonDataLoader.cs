using UnityEngine;
using System.Collections.Generic;

public static class JsonDataLoader
{
    private const string ComponentJsonPath = "ComponnetInfoJSON/ComponentInformation"; // Path within Resources folder

    public static Dictionary<string, Component> LoadComponentData()
    {
        Dictionary<string, Component> dataLookup = new Dictionary<string, Component>();

        TextAsset jsonTextAsset = Resources.Load<TextAsset>(ComponentJsonPath);

        if (jsonTextAsset == null)
        {
            Debug.LogError($"Component JSON file not found at Resources/{ComponentJsonPath}.json");
            return dataLookup; // Return empty dictionary
        }

        string jsonString = jsonTextAsset.text;

        try
        {
            ComponentInfo componentList = JsonUtility.FromJson<ComponentInfo>(jsonString);

            if (componentList != null && componentList.components != null)
            {
                foreach (var component in componentList.components)
                {
                    if (!dataLookup.ContainsKey(component.name))
                    {
                        dataLookup.Add(component.name, component);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate component name '{component.name}' found in JSON. Ignoring duplicate.");
                    }
                }
                Debug.Log($"Loaded {dataLookup.Count} components from JSON.");
            }
            else
            {
                Debug.LogError("JSON parsing failed or 'components' array is missing/empty.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing component JSON: {e.Message}");
        }

        return dataLookup;
    }
}