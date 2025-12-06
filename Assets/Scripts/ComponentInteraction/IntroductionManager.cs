using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroductionManager : MonoBehaviour
{
    // Part 1: Main assembly and visualization area
    public GameObject baileyBridgeGroupedAssembly;
    public Transform visualizationAreaCenter;
    public float introAnimationDuration = 2.0f;
    public float outroAnimationDuration = 1.5f;

    [Header("UI & Audio")]
    public TMP_Text descriptionTextUI;
    public AudioSource audioSource;
    public AudioClip bridgeDescriptionAudio;
    public AudioClip outroAudio;

    // Part 2: Groups and their specific materials/audio
    [Header("Group Settings")]
    public List<GameObject> groupObjects = new List<GameObject>();
    public Material initialMaterial;
    public List<Material> highlightMaterials = new List<Material>();
    public List<AudioClip> groupDescriptionAudios = new List<AudioClip>();
    public List<string> descriptionList = new List<string>();

    public GameObject mainBridgeAssembly;
    public VisualizationManager visualizationManager;

    public float rotationSpeed = 5.0f; // Adjust this value in the Inspector for desired speed
    private Coroutine rotateCoroutine;


    /// <summary>
    /// Public method to start the entire introduction sequence.
    /// </summary>
    public void Start()
    {
        StartCoroutine(IntroductionRoutine());
    }

    /// <summary>
    /// The main coroutine that orchestrates the entire introduction sequence.
    /// </summary>
    private IEnumerator IntroductionRoutine()
    {
        // PHASE 1: BLOOM AND DISPLAY
        // ----------------------------------------------------
        // Instantiate the assembly at the visualization center with zero scale
        baileyBridgeGroupedAssembly.transform.localScale = Vector3.zero;

        // Animate scale up from 0 to 1 over a set duration
        float timer = 0f;
        while (timer < introAnimationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / introAnimationDuration;
            baileyBridgeGroupedAssembly.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), progress);
            yield return null;
        }
        baileyBridgeGroupedAssembly.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Ensure final scale is exact

        // Start the rotation after the intro animation is complete
        rotateCoroutine = StartCoroutine(RotateAssemblyRoutine());

        // Display bridge description and play audio
        descriptionTextUI.text = "A Bailey bridge is a portable, prefabricated steel truss bridge that can be rapidly assembled. It is widely used for emergencies and military purposes";
        audioSource.PlayOneShot(bridgeDescriptionAudio);
        yield return new WaitForSeconds(bridgeDescriptionAudio.length);

        // PHASE 2: GROUP-BY-GROUP EXPLANATION
        // ----------------------------------------------------
        // Loop through and explain each group
        for (int i = 0; i < groupObjects.Count; i++)
        {
            GameObject group = groupObjects[i];
            AudioClip groupAudio = groupDescriptionAudios[i];

            // Highlight the current group with its unique material
            foreach (Renderer r in group.GetComponentsInChildren<Renderer>())
            {
                if (i < highlightMaterials.Count)
                {
                    r.material = highlightMaterials[i];
                }
            }

            // Display group information and play audio
            descriptionTextUI.text = descriptionList[i];
            audioSource.PlayOneShot(groupAudio);

            // Wait for the audio to finish before moving to the next group
            yield return new WaitForSeconds(groupAudio.length);

            // Highlight the current group with its unique material
            foreach (Renderer r in group.GetComponentsInChildren<Renderer>())
            {
                if (i < highlightMaterials.Count)
                {
                    r.material = initialMaterial;
                }
            }

            yield return new WaitForSeconds(0.2f);
        }

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }

        // PHASE 3: REVERT AND ANIMATE OUT
        // ----------------------------------------------------

        descriptionTextUI.text = "Great!! You have understood about the Bailey bridge. Now you can interact with each components to know more about it";
        yield return new WaitForSeconds(1.0f); // A short pause before scaling down

        audioSource.PlayOneShot(outroAudio);

        yield return new WaitForSeconds(1.0f); // A short pause before scaling down


        // Animate scale down to 0
        float outroTimer = 0f;
        while (outroTimer < outroAnimationDuration)
        {
            outroTimer += Time.deltaTime;
            float progress = outroTimer / outroAnimationDuration;
            baileyBridgeGroupedAssembly.transform.localScale = Vector3.Lerp(new Vector3(0.3f, 0.3f, 0.3f), Vector3.zero, progress);
            yield return null;
        }

        // Ensure scale is exactly zero and clean up
        baileyBridgeGroupedAssembly.transform.localScale = Vector3.zero;
        descriptionTextUI.text = "";
        mainBridgeAssembly.SetActive(true);
        baileyBridgeGroupedAssembly.SetActive(false);
        visualizationManager.Initialize();
    }

    /// <summary>
    /// A separate coroutine to handle continuous rotation of the assembly.
    /// </summary>
    private IEnumerator RotateAssemblyRoutine()
    {
        if (baileyBridgeGroupedAssembly == null)
        {
            yield break; // Exit if the assembly doesn't exist
        }

        while (true)
        {
            baileyBridgeGroupedAssembly.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}