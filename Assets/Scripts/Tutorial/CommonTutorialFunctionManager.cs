// <CommonTutorialFunctionManager.cs>
//
// Copyright SX/EDA3 2024 all rights reserved.
//
// Authors:
//   Sagar T Y, Nikhitha Sannapuneni
//
// Defines:

//   [C] Bosch.DigiGear.CommonTutorialFunctionManager
// This script will manage the common functionality of Tutorial


using Bosch.DigiGear;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Bosch.Evc.Sequence;


namespace Bosch.DigiGear
{

    [System.Serializable]
    public class Instructions
    {
        public string stepName;
        public string instructionText;
    }

    public class CommonTutorialFunctionManager : MonoBehaviour
    {
        public Instructions[] instructions;
        public AudioSource instructionAudio;

        public InputActionAsset inputActions;
        public Coroutine coroutineHightLight;

        public InputAction lefttriggerPressAction;
        public InputAction leftgripPressAction;
        public InputAction rightTeleportAction;
        public InputAction rightTriggerPressAction;
        public InputAction rightGripPressAction;
        public InputAction rightHandAButton;
        public InputAction leftHandXButton;

        public TMP_Text vrControllerTutorialName;
        public TMP_Text vrInteractionTutorialName;
        public TMP_Text headlineText;

        public GameObject joystickforwardDirectionblink;
        public GameObject joyStickSideDirectionblink;
        public GameObject vrControllerCheckBox;
        public GameObject vrInteractionCheckBox;
        public GameObject correctCheckBoxIcon;
        public GameObject tutorialPanel;
        public GameObject tutorialModuleUIPanel;
        public GameObject moduleSelectionPanel;

        public Color hightlightNameColour;

        private List<string> _playedClips = new List<string>();

        public VRInteractionTutorialManager vrInteractionTutorialManager;
        public VRControllerTutorialManager vrControllerTutorialManager;

        public AudioSource soundEffectAudioSource;
        public AudioClip soundEffectAudioClip;

        private ParticleSystem _currentEmittingParticle;

        private void OnEnable()
        {
            var leftactionMap = inputActions.FindActionMap("XR Left Controller");
            var rightactionMap = inputActions.FindActionMap("XR Right Controller");

            lefttriggerPressAction = leftactionMap.FindAction("LeftTrigger");
            leftgripPressAction = leftactionMap.FindAction("LeftGrab");

            rightTriggerPressAction = rightactionMap.FindAction("RightTrigger");
            rightGripPressAction = rightactionMap.FindAction("RightGrab");
            rightTeleportAction = rightactionMap.FindAction("RightTeleportActivate");
            rightHandAButton = rightactionMap.FindAction("RightAButton");
            leftHandXButton = leftactionMap.FindAction("LeftXButton");

        }

        /// <summary>
        /// Sets the initial headline text in the UI and disable other ui elements
        /// </summary>
        public void Start()
        {
            headlineText.text = "Welcome To VR Tutorials";
            HideUIElements();
        }

        /// <summary>
        /// Disable UI elements
        /// </summary>
        public void HideUIElements()
        {
            tutorialModuleUIPanel.SetActive(false);
            moduleSelectionPanel.SetActive(false);
            vrControllerCheckBox.SetActive(false);
            vrInteractionCheckBox.SetActive(false);
            correctCheckBoxIcon.SetActive(false);
        }

        /// <summary>
        /// Plays a one-shot sound effect using the specified audio source.
        /// </summary>
        public void PlaySoundEffect()
        {
            soundEffectAudioSource.PlayOneShot(soundEffectAudioClip);
        }


        /// <summary>
        /// Retrieves the instruction text with a given step name.
        /// </summary>
        public string GetInstructionText(string stepName)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.stepName == stepName)
                {
                    return instruction.instructionText;
                }
            }

            return "Instruction not found.";
        }

        /// <summary>
        /// Toggles the blinking effect on the specified left and right action GameObjects.
        /// </summary>
        public void ToggleBlinkState(GameObject leftAction, GameObject rightAction, bool state)
        {
            leftAction.GetComponent<Blink>().isBlink = state;
            rightAction.GetComponent<Blink>().isBlink = state;
        }

        /// <summary>
        /// Starts a coroutine to play a particle system on a specified controller button.
        /// </summary>
        public void PlayParticleSystem(GameObject ControllerButton)
        {
            StartCoroutine(startParticleSystem(ControllerButton));
        }

        /// <summary>
        /// Play the particle system after 0.5 seconds stop particle system
        /// </summary>
        /// <param name="ControllerButton">This is controller buttons</param>
        /// <returns></returns>
        IEnumerator startParticleSystem(GameObject ControllerButton)
        {
            _currentEmittingParticle = ControllerButton.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
            _currentEmittingParticle.Play();
            yield return new WaitForSeconds(0.5f);
            _currentEmittingParticle.Stop();
            _currentEmittingParticle = null;
        }

        /// <summary>
        /// Stops the particle system
        /// </summary>
        /// <param name="ControllerButton">This controller Button</param>
        public void StopParticleSystem(GameObject ControllerButton)
        {
            _currentEmittingParticle = ControllerButton.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
            _currentEmittingParticle.Stop();
        }

        /// <summary>
        /// Loads an audio clip by name from resources, checks if it has been played before, and plays it if not.
        /// </summary>
        public void LoadAndPlayTutorialAudio(string clipName)
        {
            if (_playedClips.Contains(clipName))
            {
                return;
            }
            AudioClip audioClip = Resources.Load<AudioClip>("TutorialAudio/" + clipName);

            if (instructionAudio != null && audioClip != null)
            {
                instructionAudio.Stop();
                instructionAudio.clip = audioClip;
                instructionAudio.Play();
                _playedClips.Add(clipName);
            }
        }

        IEnumerator StartHighlight(GameObject hightlightButton)
        {
            hightlightButton.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            hightlightButton.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// Starts a repeating highlight effect around an object while a button is pressed.
        /// </summary>
        public void HighlightAroundObject(bool pressed, GameObject hightlightButton)
        {
            coroutineHightLight = StartCoroutine(highlightRepeat(pressed, hightlightButton));
        }

        /// <summary>
        /// Stops the currently running highlight effect coroutine.
        /// </summary>
        public void StopHightlight()
        {
            StopCoroutine(coroutineHightLight);
        }

        /// <summary>
        /// Continuously toggles the highlight effect on and off as long as the button is pressed.
        /// </summary>
        public IEnumerator highlightRepeat(bool pressed, GameObject obj)
        {
            while (pressed)
            {
                yield return StartCoroutine(StartHighlight(obj));
            }
        }

        /// <summary>
        /// Onclick of skip button load Module panel by stopping other functionality
        ///</summary>
        public void OnClickSkipButton()
        {

            StopAllCoroutines();

            correctCheckBoxIcon.SetActive(false);
            tutorialPanel.SetActive(false);
            tutorialModuleUIPanel.SetActive(false);
            moduleSelectionPanel.SetActive(true);
            vrControllerTutorialManager.StopAllCoroutines();
            vrInteractionTutorialManager.StopAllCoroutines();
            vrInteractionTutorialManager.enabled = false;
            vrControllerTutorialManager.enabled = false;

            LoadAndPlayTutorialAudio("ChooseModule");
            if (_currentEmittingParticle != null)
            {
                _currentEmittingParticle.Stop();
            }
        }
    }
}