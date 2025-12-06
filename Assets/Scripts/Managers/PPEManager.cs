using Bosch.SXEDA3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PPEManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform headset;
    [Header("Waist Attach Point")]
    public Transform waistAnchor;

    [Header("Hand & Glove Settings")]
    public List<GameObject> handObjects = new List<GameObject>();
    public Material grabbedHandMaterial;
    public bool restoreHandMaterialOnRelease = true;

    private Dictionary<GameObject, Material> originalHandMaterials = new Dictionary<GameObject, Material>();
    public PPEChecklistManager pPEChecklistManager;

    private void Start()
    {
        if (headset == null && Camera.main != null)
        {
            headset = Camera.main.transform;
            Debug.Log("PPEManager: headset auto-assigned to Main Camera.");
        }

        foreach (var hand in handObjects)
        {
            if (hand != null && !originalHandMaterials.ContainsKey(hand))
            {
                originalHandMaterials[hand] = GetMaterial(hand);
            }
        }
    }

    // --- HANDLE GLOVE ---
    public void HandleGlove(string gloveObjectName, Action onComplete)
    {
        pPEChecklistManager?.AddItem(gloveObjectName);
        GameObject gloveObj = GameObject.Find(gloveObjectName);
        EnsureInteractionComponents(gloveObj);
        ApplyOutline(gloveObj);

        if (gloveObj == null)
        {
            Debug.LogWarning($"PPEManager: Glove object '{gloveObjectName}' not found.");
            onComplete?.Invoke();
            return;
        }

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = gloveObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogWarning($"PPEManager: No XRGrabInteractable on glove object '{gloveObjectName}'.");
            onComplete?.Invoke();
            return;
        }

        grabInteractable.selectEntered.RemoveAllListeners();
        grabInteractable.selectEntered.AddListener((args) => OnGloveGrabbed(args, gloveObj, onComplete));
    }

    private void OnGloveGrabbed(SelectEnterEventArgs args, GameObject gloveObj, Action onComplete)
    {
        ChangeHandMaterial(grabbedHandMaterial);
        Outline outline = gloveObj.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        gloveObj.SetActive(false);

        if (restoreHandMaterialOnRelease)
            RestoreHandMaterials();

        pPEChecklistManager?.MarkCompleted(gloveObj.name);
        onComplete?.Invoke();
    }

    // --- HANDLE OTHER SAFETY OBJECTS ---
    public void HandleSafetyObjects(List<string> safetyObjectNames, Action onComplete)
    {
        HashSet<string> completed = new HashSet<string>();
        pPEChecklistManager?.InitChecklist(safetyObjectNames);

        foreach (string objName in safetyObjectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj == null)
            {
                Debug.LogWarning($"PPEManager: Safety object '{objName}' not found.");
                continue;
            }

            EnsureInteractionComponents(obj);
            ApplyOutline(obj);

            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable == null)
            {
                Debug.LogWarning($"PPEManager: No XRGrabInteractable on safety object '{objName}'.");
                continue;
            }

            bool isSpecialObject = IsSpecialSafetyObject(objName);
            bool isHeightObject = IsHeightSafetyObject(objName);

            grabInteractable.selectEntered.RemoveAllListeners();
            grabInteractable.selectEntered.AddListener((args) =>
            {
                if (completed.Contains(objName))
                    return;

                Outline outline = obj.GetComponent<Outline>();
                if (outline != null)/* outline.enabled = false;*/
                    Destroy(outline);

                if (isHeightObject)
                {
                    AttachToWaist(obj);

                    pPEChecklistManager?.MarkCompleted(objName);
                    completed.Add(objName);
                    CheckCompletion(safetyObjectNames, completed, onComplete);
                }
                else if (isSpecialObject)
                {
                    Collider col = obj.GetComponent<Collider>();
                    if (col != null)
                    {
                        StartCoroutine(WaitForTouchAndDisable(obj, () =>
                        {
                            completed.Add(objName);
                            CheckCompletion(safetyObjectNames, completed, onComplete);
                        }));
                    }
                    else
                    {
                        obj.SetActive(false);
                        pPEChecklistManager?.MarkCompleted(objName);
                        completed.Add(objName);
                        CheckCompletion(safetyObjectNames, completed, onComplete);
                    }
                }
                else
                {
                    obj.SetActive(false);
                    pPEChecklistManager?.MarkCompleted(objName);
                    completed.Add(objName);
                    CheckCompletion(safetyObjectNames, completed, onComplete);
                }
            });
        }
    }

    private void CheckCompletion(List<string> allObjects, HashSet<string> completed, Action onComplete)
    {
        if (completed.Count == allObjects.Count)
        {
            Debug.Log("PPEManager: All safety objects completed.");
            onComplete?.Invoke();
        }
    }
    private bool IsHeightSafetyObject(string objName)
    {
        string lower = objName.ToLower();
        return lower.Contains("heightjacket") || lower.Contains("hook");
    }

    private void AttachToWaist(GameObject obj)
    {
        if (waistAnchor == null)
        {
            Debug.LogWarning("PPEManager: Waist anchor not assigned.");
            return;
        }
        XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
        if (grab != null)
            Destroy(grab);

        obj.transform.SetParent(waistAnchor);
       
        if(obj.name == "hook")
        {
        
            obj.transform.localPosition = new Vector3(0.132f, -0.04f, 0.087f);
            obj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        else
        {
            obj.transform.localPosition = new Vector3(0.011f, 0.105f, 0.075f);
            obj.transform.localRotation = Quaternion.Euler(-90f, 0f, 1.702f);
        }
        Debug.Log($"{obj.name} is now child of: {obj.transform.parent.name}");

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

       BoxCollider boxCollider = obj.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Destroy(boxCollider);
        }
     
    }

    private bool IsSpecialSafetyObject(string objName)
    {
        string lower = objName.ToLower();
        return (lower.Contains("helmet") || lower.Contains("glass") || lower.Contains("mask"));
    }

    private IEnumerator WaitForTouchAndDisable(GameObject obj, Action onDisabled)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"PPEManager: No collider on '{obj.name}'. Disabling immediately.");
            pPEChecklistManager?.MarkCompleted(obj.name);
            Outline outline = obj.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            obj.SetActive(false);
            onDisabled?.Invoke();
            yield break;
        }

        while (obj.activeSelf)
        {
            if (IsTouchingHeadset(col))
            {
                Outline outline = obj.GetComponent<Outline>();
                if (outline != null) outline.enabled = false;

                obj.SetActive(false);
                pPEChecklistManager?.MarkCompleted(obj.name);

                onDisabled?.Invoke();
                yield break;
            }
            yield return null;
        }
    }

    // --- Helpers for hand materials ---
    private void ChangeHandMaterial(Material mat)
    {
        foreach (var hand in handObjects)
        {
            if (hand != null)
                SetMaterial(hand, mat);
        }
    }

    private void RestoreHandMaterials()
    {
        foreach (var pair in originalHandMaterials)
        {
            SetMaterial(pair.Key, pair.Value);
        }
    }

    private Material GetMaterial(GameObject go)
    {
        Renderer rend = go.GetComponent<Renderer>();
        return rend != null ? rend.material : null;
    }

    private void SetMaterial(GameObject go, Material mat)
    {
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null && mat != null)
            rend.material = mat;
    }

    // --- Headset touch detection ---
    private bool IsTouchingHeadset(Collider col)
    {
        if (headset == null)
        {
            Debug.LogWarning("PPEManager: headset is not assigned.");
            return false;
        }

        Collider headsetCollider = headset.GetComponent<Collider>();
        if (headsetCollider != null)
        {
            bool isTouching = headsetCollider.bounds.Intersects(col.bounds);
            if (isTouching)
                Debug.Log($"Touch detected: {col.gameObject.name} touched headset collider.");
            return isTouching;
        }
        else
        {
            float dist = Vector3.Distance(headset.position, col.transform.position);
            bool closeEnough = dist < 0.15f; // tweak this threshold if needed
            if (closeEnough)
                Debug.Log($"Touch detected by distance: {col.gameObject.name} is {dist:F3}m close to headset.");

            return closeEnough;
        }
    }

    private void ApplyOutline(GameObject gameObject, bool outlineState = true)
    {
        if (gameObject == null) return;

        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null && outlineState)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.enableBlinking = true;
        }

        if (outline != null)
        {
            Color brightOrange = new Color(0.953f, 0.420f, 0.012f, 1f);
            outline.OutlineColor = brightOrange;
            outline.enabled = outlineState;
            outline.OutlineWidth = 5;
        }
    }

    private void EnsureInteractionComponents(GameObject obj)
    {
        if (obj == null) return;
        UnityUtilities.EnsureCollider(obj);
        UnityUtilities.EnsureRigidbody(obj, true);

        if (obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            interactable.useDynamicAttach = true;
            interactable.farAttachMode = UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode.Near;
        }
    }
}