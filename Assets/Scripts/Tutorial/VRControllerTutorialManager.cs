// <VRControllerTutorialManager.cs>
//
// Copyright SX/EDA3 2024 all rights reserved.
//
// Authors:
//  Nikhitha Sannapuneni
//
// Defines:
//   [C] Bosch.DigiGear.VRControllerTutorialManager
// This script will makes the user to understand buttons in controllers


using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Bosch.Evc.Sequence;

namespace Bosch.DigiGear
{

    [System.Serializable]
    public class Instruction
    {
        public string stepName;
        public string instructionText;
    }


    public class VRControllerTutorialManager : MonoBehaviour
    {
        #region Private
        private bool _triggerPressed = false;
        private bool _gripPressed = false;
        private bool _teleportPressed = false;
        private bool _leftTriggerPressed = false;
        private bool _rightTriggerPressed = false;
        private bool _leftGripPressed = false;
        private bool _rightGripPressed = false;
        private bool _primaryButtonPressed = false;
        private bool _rightHandAPressed = false;
        private bool _leftHandXPressed = false;
        #endregion

        #region Public
        public VRInteractionTutorialManager vrInteractionTutorialManager;
        public CommonTutorialFunctionManager commonTutorialFunctionManager;
        public TMP_Text stepIntructions;
        public GameObject lefttrigger;
        public GameObject leftgrip;
        public GameObject righttrigger;
        public GameObject rightgrip;
        public GameObject rightTeleport;
        public GameObject rightAxisTurn;
        public GameObject rightHandAButton;
        public GameObject leftHandXButton;
        public GameObject skipButtonInTutorialPanel;
        #endregion


        #region Unity
        /// <summary>
        /// OnEnable Disables the tutorial module UI and VR interaction script.
        /// </summary>
        private void OnEnable()
        {
            vrInteractionTutorialManager.enabled = false;
        }

        /// <summary>
        /// On start excute start sequence
        /// </summary>
        public void Start()
        {
            commonTutorialFunctionManager.correctCheckBoxIcon.SetActive(false);
            StartCoroutine(StartSequnence());
        }

        /// <summary>
        /// OnDisable remove all listener and update the blink state to false
        /// </summary>
        private void OnDisable()
        {
            commonTutorialFunctionManager.lefttriggerPressAction.performed -= OnTriggerPress;
            commonTutorialFunctionManager.leftgripPressAction.performed -= OnGripPress;
            commonTutorialFunctionManager.rightTeleportAction.performed -= OnJoyStickMove;
            commonTutorialFunctionManager.rightTriggerPressAction.performed -= OnTriggerPress;
            commonTutorialFunctionManager.rightGripPressAction.performed -= OnGripPress;

            if (lefttrigger != null && leftgrip != null && rightgrip != null && righttrigger != null)
            {
                lefttrigger.GetComponent<Blink>().isBlink = false;
                leftgrip.GetComponent<Blink>().isBlink = false;
                righttrigger.GetComponent<Blink>().isBlink = false;
                rightgrip.GetComponent<Blink>().isBlink = false;
            }
        }
        #endregion

        /// <summary>
        /// Manages the sequence of tutorial steps for a VR controller introduction. 
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
        public IEnumerator StartSequnence()
        {
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("Welcome");
            UpdateInstructionsText("Welcome");
            yield return new WaitForSeconds(9f);
            UpdateInstructionsText("Start");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("Start");
            skipButtonInTutorialPanel.SetActive(false);
            commonTutorialFunctionManager.tutorialModuleUIPanel.SetActive(true);
            yield return new WaitForSeconds(6.5f);
            commonTutorialFunctionManager.headlineText.text = "Tutorial 1 - Introduction to VR Controllers";
            InitializeInputListeners();
            UpdateInstructionsText("LocateTrigger");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LocateTrigger");
            commonTutorialFunctionManager.vrControllerTutorialName.color = commonTutorialFunctionManager.hightlightNameColour;
            commonTutorialFunctionManager.ToggleBlinkState(lefttrigger, righttrigger, true);
        }

        /// <summary>
        /// Sets up input listeners for various VR controller actions.
        /// </summary>
        private void InitializeInputListeners()
        {
            commonTutorialFunctionManager.lefttriggerPressAction.performed += OnTriggerPress;
            commonTutorialFunctionManager.rightTriggerPressAction.performed += OnTriggerPress;
            commonTutorialFunctionManager.rightGripPressAction.performed += OnGripPress;
            commonTutorialFunctionManager.leftgripPressAction.performed += OnGripPress;
            commonTutorialFunctionManager.rightTeleportAction.performed += OnJoyStickMove;
            commonTutorialFunctionManager.rightHandAButton.performed += OnPrimaryButtonPressed;
            commonTutorialFunctionManager.leftHandXButton.performed += OnPrimaryButtonPressed;
        }

        /// <summary>
        /// Updates the text instructions based on the provided step name.
        /// </summary>
        /// <param name="stepName">The name of the tutorial step <param>
        public void UpdateInstructionsText(string stepName)
        {
            stepIntructions.text = commonTutorialFunctionManager.GetInstructionText(stepName);
        }

        /// <summary>
        /// Handles the actions associated with an input press, including updating instructions, disabling visual effects, and playing sound and particles.
        /// </summary>
        /// <param name="inputAction">The GameObject representing the input action to handle.</param>
        /// <param name="instructionText">The text to display.</param>
        private void HandleInputAction(GameObject inputAction, string instructionText)
        {
            UpdateInstructionsText(instructionText);
            inputAction.GetComponent<Blink>().isBlink = false;
            commonTutorialFunctionManager.PlaySoundEffect();
            commonTutorialFunctionManager.PlayParticleSystem(inputAction);
        }

        /// <summary>
        /// Handles the trigger press actions based on the input context. Updates the state of trigger presses, 
        /// manages visual and audio feedback, and performs actions when both triggers are pressed.
        /// </summary>
        /// <param name="context">The input action callback providing information about the trigger press.</param>
        private void OnTriggerPress(InputAction.CallbackContext context)
        {
            bool isTriggerPressed = context.ReadValue<float>() > 0.1f;

            if (context.action.name == "RightTrigger")
            {
                _rightTriggerPressed = isTriggerPressed;
                HandleInputAction(righttrigger, "LeftTrigger");
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LeftTrigger");
                commonTutorialFunctionManager.rightTriggerPressAction.performed -= OnTriggerPress;

            }
            else if (context.action.name == "LeftTrigger")
            {
                _leftTriggerPressed = isTriggerPressed;
                HandleInputAction(lefttrigger, "RightTrigger");
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("RightTrigger");
                commonTutorialFunctionManager.lefttriggerPressAction.performed -= OnTriggerPress;

            }

            if (_leftTriggerPressed && _rightTriggerPressed)
            {
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LocateGrip");
                UpdateInstructionsText("LocateGrip");
                _triggerPressed = true;
                StartCoroutine(StopParticleCoroutine(lefttrigger, righttrigger));
                commonTutorialFunctionManager.ToggleBlinkState(leftgrip, rightgrip, true);
            }
        }

        IEnumerator StopParticleCoroutine(GameObject obj1, GameObject obj2 = null)
        {
            yield return new WaitForSeconds(0.5f);
            commonTutorialFunctionManager.StopParticleSystem(obj1);

            if (obj2 != null)
            {
                commonTutorialFunctionManager.StopParticleSystem(obj2);
            }
        }

        /// <summary>
        /// Handles the grip press actions based on the input context. Updates the state of grip presses, 
        /// manages visual and audio feedback, and performs actions when both grips are pressed after trigger press.
        /// </summary>
        /// <param name="context">The input action callback  providing information about the grip press.</param>
        private void OnGripPress(InputAction.CallbackContext context)
        {
            if (_triggerPressed)
            {

                bool OnGripPressPressed = context.ReadValue<float>() > 0.1f;

                if (context.action.name == "RightGrab")
                {
                    _rightGripPressed = OnGripPressPressed;
                    HandleInputAction(rightgrip, "LeftGrip");
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LeftGrip");
                    commonTutorialFunctionManager.rightGripPressAction.performed -= OnGripPress;
                }
                else if (context.action.name == "LeftGrab")
                {
                    _leftGripPressed = OnGripPressPressed;
                    HandleInputAction(leftgrip, "RightGrip");
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("RightGrip");
                    commonTutorialFunctionManager.leftgripPressAction.performed -= OnGripPress;
                }

                if (_rightGripPressed && _leftGripPressed)
                {

                    commonTutorialFunctionManager.HighlightAroundObject(true, commonTutorialFunctionManager.joystickforwardDirectionblink);
                    UpdateInstructionsText("AxisForward");
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("AxisForward");
                    StartCoroutine(StopParticleCoroutine(rightgrip, leftgrip));

                    _gripPressed = true;
                }
            }
        }

        /// <summary>
        /// Handles the JoyStick press actions based on the input context. Updates the state of joystick presses, 
        /// manages visual and audio feedback, and performs actions when both grips are pressed after Joystick press.
        /// </summary>
        /// <param name="context">The input action callback  providing information about the Joystick press.</param>
        private void OnJoyStickMove(InputAction.CallbackContext context)
        {
            if (_gripPressed)
            {
                Vector2 axisValue = context.ReadValue<Vector2>();

                if (Mathf.Abs(axisValue.x) < 0.05f && axisValue.y >= 0.5f && !_teleportPressed)
                {
                    HandleInputAction(rightTeleport, "AxisLeftRight");
                    commonTutorialFunctionManager.StopHightlight();
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("AxisLeftRight");
                    commonTutorialFunctionManager.HighlightAroundObject(true, commonTutorialFunctionManager.joyStickSideDirectionblink);
                    _teleportPressed = true;
                }
                if (_teleportPressed)
                {
                    if (Mathf.Abs(axisValue.y) < 0.05f && (axisValue.x >= 0.1f || axisValue.x <= -0.1f))
                    {
                        HandleInputAction(rightTeleport, "LocatePrimaryButtons");
                        commonTutorialFunctionManager.StopHightlight();
                        commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LocatePrimaryButtons");

                        commonTutorialFunctionManager.ToggleBlinkState(leftHandXButton, rightHandAButton, true);
                        StartCoroutine(StopParticleCoroutine(rightTeleport));

                        commonTutorialFunctionManager.rightTeleportAction.performed -= OnJoyStickMove;
                        _primaryButtonPressed = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the primary button press actions based on the input context. Updates the state of primary button presses, 
        /// manages visual and audio feedback
        /// </summary>
        /// <param name="context">The input action callback  providing information about the Primary button press.</param>
        private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
        {
            if (_primaryButtonPressed)
            {
                bool _onPrimaryButtonPressed = context.ReadValue<float>() > 0.1f;
                if (context.action.name == "RightAButton")
                {
                    _rightHandAPressed = _onPrimaryButtonPressed;
                    HandleInputAction(rightHandAButton, "LeftPrimaryButton");

                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("LeftPrimaryButton");
                    commonTutorialFunctionManager.rightHandAButton.performed -= OnPrimaryButtonPressed;

                }
                else if (context.action.name == "LeftXButton")
                {
                    _leftHandXPressed = _onPrimaryButtonPressed;
                    HandleInputAction(leftHandXButton, "RightPrimaryButton");
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("RightPrimaryButton");
                    commonTutorialFunctionManager.leftHandXButton.performed -= OnPrimaryButtonPressed;
                }

                if (_rightHandAPressed && _leftHandXPressed)
                {
                    UpdateInstructionsText("Completion");
                    commonTutorialFunctionManager.LoadAndPlayTutorialAudio("Completion");
                    commonTutorialFunctionManager.correctCheckBoxIcon.SetActive(true);
                    commonTutorialFunctionManager.vrControllerCheckBox.SetActive(true);
                    StartCoroutine(StopParticleCoroutine(leftHandXButton, rightHandAButton));
                    StartCoroutine(enableInteractionTutorial());
                }
            }


        }


        /// <summary>
        /// Delays enabling the VR interaction tutorial and hides the correct icon and the current GameObject.
        /// </summary>
        /// <returns>An enumerator for coroutine execution.</returns>
        IEnumerator enableInteractionTutorial()
        {
            yield return new WaitForSeconds(10);
            vrInteractionTutorialManager.enabled = true;
            commonTutorialFunctionManager.correctCheckBoxIcon.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}