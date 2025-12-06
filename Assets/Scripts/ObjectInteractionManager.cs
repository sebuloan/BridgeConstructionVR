using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class ObjectInteractionManager : MonoBehaviour
{
    [System.Serializable]
    public class InteractionStep
    {
        [Header("Part Settings")]
        public string StepName;
        public GameObject TargetObject;
        public GameObject AssemblyGhost;
        public GameObject AssemblyReal;

        [Header("Special Flags")]
        public bool requiresTeleportToB;

        // ❌ Removed per-step teleport target (not needed anymore)
        // public Transform teleportTargetB;
    }

    [Header("Interaction Steps")]
    public InteractionStep[] interactionSteps;

    [Header("Movement Settings")]
    public float moveUpDistance = 0.5f;
    public float moveDuration = 1f;

    [System.Serializable]
    public class AudioSubtitle
    {
        [Header("English")]
        public AudioClip englishClip;
        [TextArea] public string englishSubtitle;

        [Header("Hindi")]
        public AudioClip hindiClip;
        [TextArea] public string hindiSubtitle;
    }

    public enum Language { English, Hindi }

    [Header("Language Settings")]
    public Language currentLanguage = Language.English;

    [Header("Audio & Subtitles")]
    public AudioSubtitle Pick;
    public AudioSubtitle Teleport;
    public AudioSubtitle Place;
    public AudioSubtitle Completion;
    public AudioSubtitle TeleportBack;
    public AudioSubtitle Welcome;
    public AudioSubtitle FinalCompletion;

    [Header("Audio + UI")]
    public AudioSource AudioSource;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Teleport System")]
    public MultiTeleport teleportSystem;

    [Header("Pickup Spawn Point")]
    public Transform pickupPoint;

    [Header("Pointer Settings")]
    public GameObject fingerPointer;
    public float indicatorHeight = 0.3f;

    [Header("Common Teleport Target B (for all steps)")]
    public Transform commonTeleportTargetB;   // ✅ Added one global teleport target

    [Header("Parent Movement Reference")]
    public Transform assemblyParent;  // 👈 Parent containing ghost + real parts

    private int currentStepIndex = 0;
    private bool interactionInProgress = false;
    private Coroutine pointerAnimRoutine;
    private bool pickPlayedForCurrentStep = false;

    private bool firstStepStartedManually = false;

    private List<GameObject> placedObjects = new List<GameObject>();

    void Start()
    {
        DisableAllObjects();
        if (fingerPointer != null) fingerPointer.SetActive(false);
    }

    void DisableAllObjects()
    {
        foreach (var step in interactionSteps)
        {
            if (step.TargetObject != null) step.TargetObject.SetActive(false);
            if (step.AssemblyGhost != null) step.AssemblyGhost.SetActive(false);
            if (step.AssemblyReal != null) step.AssemblyReal.SetActive(false);
        }
    }

    #region Manual Start
    public void StartTraining()
    {
        AudioClip clip = GetClip(Welcome);
        if (clip != null)
        {
            PlayAudio(Welcome);
            StartCoroutine(StartAfterAudio(clip.length));
        }
        else
        {
            StartFirstStepManually();
        }
    }

    IEnumerator StartAfterAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartFirstStepManually();
    }

    public void StartFirstStepManually()
    {
        firstStepStartedManually = true;
        currentStepIndex = 0;
        StartInteraction();
        PlayPickForCurrentStep();
    }
    #endregion

    void StartInteraction()
    {
        if (currentStepIndex >= interactionSteps.Length)
        {
            Debug.Log("✅ All steps completed.");
            if (GetClip(FinalCompletion) != null) PlayAudio(FinalCompletion);
            return;
        }

        pickPlayedForCurrentStep = false;
        interactionInProgress = true;

        InteractionStep step = interactionSteps[currentStepIndex];

        if (step.TargetObject != null)
        {
            step.TargetObject.SetActive(true);

            if (pickupPoint != null)
            {
                StartCoroutine(MoveToSpawnPoint(step.TargetObject, pickupPoint.position, 1.5f));

                Rigidbody rb = step.TargetObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep();
                }
            }
        }

        if (step.AssemblyGhost != null)
        {
            step.AssemblyGhost.SetActive(true);
            PlacementTrigger trigger = step.AssemblyGhost.GetComponent<PlacementTrigger>();
            if (trigger == null) trigger = step.AssemblyGhost.AddComponent<PlacementTrigger>();
            trigger.manager = this;
            trigger.stepIndex = currentStepIndex;
        }

        Blink b = step.TargetObject?.GetComponent<Blink>();
        if (b != null) b.isBlink = true;

        XRGrabInteractable grab = step.TargetObject?.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectEntered.AddListener(ctx => OnPartGrabbed(step, grab));
    }

    IEnumerator MoveToSpawnPoint(GameObject obj, Vector3 targetPos, float speed = 1.5f)
    {
        while (obj != null && Vector3.Distance(obj.transform.position, targetPos) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        if (obj != null) obj.transform.position = targetPos;
    }

    public void OnPartPlaced(int stepIndex, GameObject placedObj)
    {
        if (!interactionInProgress || stepIndex != currentStepIndex) return;

        InteractionStep step = interactionSteps[stepIndex];

        if (step.TargetObject == placedObj)
        {
            placedObj.transform.position = step.AssemblyGhost.transform.position;
            placedObj.transform.rotation = step.AssemblyGhost.transform.rotation;

            if (step.AssemblyReal != null) step.AssemblyReal.SetActive(true);

            PlaceObject(step);
        }
    }

    void OnPartGrabbed(InteractionStep step, XRGrabInteractable grab)
    {
        Blink b = step.TargetObject?.GetComponent<Blink>();
        if (b != null) b.isBlink = false;

        if (fingerPointer != null)
        {
            fingerPointer.SetActive(false);
            if (pointerAnimRoutine != null) { StopCoroutine(pointerAnimRoutine); pointerAnimRoutine = null; }
        }

        if (GetClip(Teleport) != null) PlayAudio(Teleport);

        if (step.requiresTeleportToB) return;

        StartCoroutine(CheckPlacement(step));
    }

    public void TeleportStepBButton()
    {
        InteractionStep step = interactionSteps[currentStepIndex];
        if (step.requiresTeleportToB && teleportSystem != null)
        {
            teleportSystem.TeleportToCurrentStepB(); // ✅ let MultiTeleport handle callback + audio trigger
            StartCoroutine(DelayedPickAfterTeleport(step, 0.5f));
        }
    }



    IEnumerator DelayedPickAfterTeleport(InteractionStep step, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!pickPlayedForCurrentStep) PlayPickForCurrentStep();
    }

    public void OnTeleportedToTargetB()
    {
        InteractionStep step = interactionSteps[currentStepIndex];
        if (!step.requiresTeleportToB) return;

        if (step.AssemblyGhost != null)
        {
            step.AssemblyGhost.SetActive(true);
            Blink bGhost = step.AssemblyGhost.GetComponent<Blink>();
            if (bGhost != null) bGhost.isBlink = true;
            StartCoroutine(ShowPointerWithDelay(step.AssemblyGhost, 0.5f));
        }

        if (GetClip(Place) != null) PlayAudio(Place);

        StartCoroutine(CheckPlacement(step));
    }

    // Rest of your existing code unchanged...



IEnumerator CheckPlacement(InteractionStep step)
    {
        while (interactionInProgress)
        {
            if (step.TargetObject != null && step.AssemblyGhost != null)
            {
                float dist = Vector3.Distance(step.TargetObject.transform.position, step.AssemblyGhost.transform.position);
                if (dist <= 0.2f)
                {
                    step.TargetObject.transform.position = step.AssemblyGhost.transform.position;
                    step.TargetObject.transform.rotation = step.AssemblyGhost.transform.rotation;

                    if (step.AssemblyReal != null) step.AssemblyReal.SetActive(true);

                    PlaceObject(step);
                    yield break;
                }
            }
            yield return null;
        }
    }

    void PlaceObject(InteractionStep step)
    {
        if (step.TargetObject != null) step.TargetObject.SetActive(false);

        if (step.AssemblyGhost != null)
            step.AssemblyGhost.SetActive(false);


        Blink bPart = step.TargetObject?.GetComponent<Blink>();
        if (bPart != null) bPart.isBlink = false;

        Blink bReal = step.AssemblyReal?.GetComponent<Blink>();
        if (bReal != null) bReal.isBlink = false;

        if (fingerPointer != null) fingerPointer.SetActive(false);

        if (step.AssemblyReal != null && !placedObjects.Contains(step.AssemblyReal))
            placedObjects.Add(step.AssemblyReal);
        if (currentStepIndex >= 1 && (currentStepIndex + 1) % 2 == 0)
        {
            if (assemblyParent != null)
                StartCoroutine(MoveParentForward(assemblyParent, moveUpDistance, step));  // 👈 moves in +X
            else
                Debug.LogWarning("⚠️ Assembly parent not assigned in ObjectInteractionManager!");
        }


        else
        {
            AudioClip clip = GetClip(Completion);
            if (clip != null)
            {
                PlayAudio(Completion);
                StartCoroutine(WaitForCompletionAndEndStep(clip.length));
            }
            else
            {
                EndStep();
            }
        }

    }

    IEnumerator MoveParentForward(Transform parent, float distance, InteractionStep step)
    {
        if (parent == null) yield break;

        float duration = moveDuration;
        float elapsed = 0f;
        Vector3 startPos = parent.localPosition;
        Vector3 targetPos = startPos + new Vector3(distance, 0, 0);  // 👈 move in +X direction

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            parent.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (step.AssemblyGhost != null) step.AssemblyGhost.SetActive(false);

        AudioClip backClip = GetClip(TeleportBack);
        if (backClip != null)
        {
            PlayAudio(TeleportBack);
            yield return new WaitForSeconds(backClip.length);
        }

        EndStep();
    }




    IEnumerator WaitForCompletionAndEndStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioClip backClip = GetClip(TeleportBack);
        if (backClip != null && firstStepStartedManually)
        {
            PlayAudio(TeleportBack);
            yield return new WaitForSeconds(backClip.length);
        }
        EndStep();
    }

    void EndStep()
    {
        interactionInProgress = false;
        if (currentStepIndex < interactionSteps.Length - 1)
            Debug.Log("✅ Step completed. Ready for teleport back to A.");
        else
            Debug.Log("🎉 Training fully completed.");
    }

    #region Teleport Back and Start Next
    public void OnTeleportBackAndStartNextStep()
    {
        StartCoroutine(TeleportBackAndStartSequence());
    }

    private IEnumerator TeleportBackAndStartSequence()
    {
        teleportSystem.TeleportToA();
        yield return new WaitForSeconds(0.5f);

        if (firstStepStartedManually)
        {
            currentStepIndex++;
            if (currentStepIndex < interactionSteps.Length)
            {
                StartInteraction();
                PlayPickForCurrentStep();
            }
        }
    }
    #endregion

    public void PlayPickForCurrentStep()
    {
        AudioClip clip = GetClip(Pick);
        if (currentStepIndex < interactionSteps.Length && clip != null && !pickPlayedForCurrentStep)
        {
            PlayAudio(Pick);
            StartCoroutine(ShowPointerWithDelay(interactionSteps[currentStepIndex].TargetObject, 1f));
            pickPlayedForCurrentStep = true;
        }
    }

    IEnumerator ShowPointerWithDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowFingerPointer(target);
    }

    void ShowFingerPointer(GameObject target)
    {
        if (fingerPointer != null && target != null)
        {
            fingerPointer.SetActive(true);
            fingerPointer.transform.position = target.transform.position + Vector3.up * indicatorHeight;
            fingerPointer.transform.rotation = Quaternion.identity;

            if (pointerAnimRoutine != null) StopCoroutine(pointerAnimRoutine);
            pointerAnimRoutine = StartCoroutine(AnimatePointer(fingerPointer, target));
        }
    }

    IEnumerator AnimatePointer(GameObject pointer, GameObject target)
    {
        Vector3 baseOffset = Vector3.up * indicatorHeight;
        float amplitude = 0.1f;
        float speed = 2f;

        while (pointer != null && target != null && pointer.activeSelf)
        {
            float newY = baseOffset.y + Mathf.Sin(Time.time * speed) * amplitude;
            pointer.transform.position = target.transform.position + new Vector3(0, newY, 0);

            if (Camera.main != null) pointer.transform.LookAt(Camera.main.transform);

            yield return null;
        }
    }

    void PlayAudio(AudioSubtitle audioSub)
    {
        if (audioSub == null || AudioSource == null) return;

        AudioClip clipToPlay = GetClip(audioSub);
        string subtitleToShow = "";

        switch (currentLanguage)
        {
            case Language.English: subtitleToShow = audioSub.englishSubtitle; break;
            case Language.Hindi: subtitleToShow = audioSub.hindiSubtitle; break;
        }

        if (clipToPlay != null)
        {
            AudioSource.clip = clipToPlay;
            AudioSource.Play();
        }

        if (subtitleText != null) subtitleText.text = subtitleToShow;
    }

    AudioClip GetClip(AudioSubtitle audioSub)
    {
        if (audioSub == null) return null;
        return currentLanguage == Language.English ? audioSub.englishClip : audioSub.hindiClip;
    }

    public void SetLanguage(Language lang) { currentLanguage = lang; }
    public InteractionStep GetCurrentStep()
    {
        if (currentStepIndex >= 0 && currentStepIndex < interactionSteps.Length)
            return interactionSteps[currentStepIndex];
        return null;
    }
    public void SetLanguageToEnglish() { currentLanguage = Language.English; Debug.Log("Language set to English"); }
    public void SetLanguageToHindi() { currentLanguage = Language.Hindi; Debug.Log("Language set to Hindi"); }


}
