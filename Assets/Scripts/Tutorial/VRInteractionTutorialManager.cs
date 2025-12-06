
// <VRInteractionTutorialManager.cs>
//
// Copyright SX/EDA3 2024 all rights reserved.
//
// Authors:
//   Nikhitha Sannapuneni
//
// Defines:

//   [C] Bosch.DigiGear.VRInteractionTutorialManager
// This script will manage the common interaction in vr like grab, select, teleport and rotate

using System.Collections;

using UnityEngine;

using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Bosch.Evc.Sequence;

namespace Bosch.DigiGear
{
    public class VRInteractionTutorialManager : MonoBehaviour
    {

        private bool _leftTriggerPressed = false;
        private bool _rightTriggerPressed = false;
        private bool _isbuttonPressed = false;
        private bool _firstTimeGrab;

        private bool _leftGripPressed;
        private bool _rightGripPressed;

        public GameObject interactiveObject;
        public GameObject lefttrigger;
        public GameObject leftgrip;
        public GameObject righttrigger;
        public GameObject rightgrip;
        public GameObject rightTeleport;
        public GameObject rightAxisTurn;
        public GameObject uiButton;

        public GameObject player;
        public GameObject turn;

        public GameObject teleportPoint;
        public GameObject hightlightButton;

        public Color hoverObjectColor;
        public Color selectedObjectColor;
        public CommonTutorialFunctionManager commonTutorialFunctionManager;
        public VRControllerTutorialManager controllerTutorialManager;

        private void OnEnable()
        {
            InitializeInputListeners();
            disableInteractiveObjectsInitially();
        }

        public void Start()
        {
            commonTutorialFunctionManager.headlineText.text = "Tutorial 2 - VR Interactions";
            controllerTutorialManager.UpdateInstructionsText("StartTutorial2");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("StartTutorial2");
            commonTutorialFunctionManager.vrControllerTutorialName.color = Color.white;
            commonTutorialFunctionManager.vrInteractionTutorialName.color = commonTutorialFunctionManager.hightlightNameColour;
            interactiveObject.GetComponent<Outline>().enabled = false;
            StartCoroutine(StartInteractionTutorial());
        }
        public void disableInteractiveObjectsInitially()
        {
            turn.SetActive(false);
            teleportPoint.SetActive(false);
            uiButton.SetActive(false);
            interactiveObject.SetActive(false);
            hightlightButton.SetActive(false);
        }
        // <summary>
        /// Sets up input listeners for various VR controller actions.
        /// </summary>
        private void InitializeInputListeners()
        {
            commonTutorialFunctionManager.lefttriggerPressAction.performed += OnTriggerPress;
            commonTutorialFunctionManager.rightTriggerPressAction.performed += OnTriggerPress;
            commonTutorialFunctionManager.leftgripPressAction.performed += OnGripButtonPress;
            commonTutorialFunctionManager.rightGripPressAction.performed += OnGripButtonPress;
        }

        /// <summary>
        /// OnDisable remove all listener and update the blink state to false
        /// </summary>
        private void OnDisable()
        {
            commonTutorialFunctionManager.lefttriggerPressAction.performed -= OnTriggerPress;
            commonTutorialFunctionManager.rightTriggerPressAction.performed -= OnTriggerPress;
            commonTutorialFunctionManager.rightTeleportAction.performed -= OnJoyStickMove;

            commonTutorialFunctionManager.leftgripPressAction.performed -= OnGripButtonPress;
            commonTutorialFunctionManager.rightGripPressAction.performed -= OnGripButtonPress;

            lefttrigger.GetComponent<Blink>().isBlink = false;
            leftgrip.GetComponent<Blink>().isBlink = false;
            righttrigger.GetComponent<Blink>().isBlink = false;
            rightgrip.GetComponent<Blink>().isBlink = false;

        }
        /// <summary>
        /// Handles the grip press actions based on the input context. Updates the state of grip presses,state 
        /// </summary>
        /// <param name="context">The input action callback  providing information about the grip press.</param>
        private void OnGripButtonPress(InputAction.CallbackContext context)
        {
            bool OnGripButtonPressed = context.ReadValue<float>() > 0.1f;

            if (context.action.name == "RightGrab")
            {
                _rightGripPressed = OnGripButtonPressed;
                commonTutorialFunctionManager.rightGripPressAction.performed -= OnGripButtonPress;
            }
            else if (context.action.name == "LeftGrab")
            {
                _leftGripPressed = OnGripButtonPressed;
                commonTutorialFunctionManager.leftgripPressAction.performed -= OnGripButtonPress;
            }
        }

        /// <summary>
        /// Handles the trigger press actions based on the input context. Updates the state of trigger presses state
        /// </summary>
        /// <param name="context">The input action callback providing information about the trigger press.</param
        private void OnTriggerPress(InputAction.CallbackContext context)
        {
            bool isTriggerPressed = context.ReadValue<float>() > 0.1f;

            if (context.action.name == "RightTrigger")
            {
                _rightTriggerPressed = isTriggerPressed;

            }
            else if (context.action.name == "LeftTrigger")
            {
                _leftTriggerPressed = isTriggerPressed;
            }
        }

        /// <summary>
        /// Begins the interaction tutorial by updating instructions, playing audio.
        /// </summary>
        IEnumerator StartInteractionTutorial()
        {
            yield return new WaitForSeconds(4);
            controllerTutorialManager.UpdateInstructionsText("GrabCube");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("GrabCube");
            commonTutorialFunctionManager.ToggleBlinkState(leftgrip, rightgrip, true);
            interactiveObject.SetActive(true);
            interactiveObject.GetComponent<Blink>().isBlink = true;
        }

        /// <summary>
        /// Handles object grab events, including playing effects and updating instructions.
        /// </summary>
        public void ObjectGrabbed(GameObject interactiveObject)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = interactiveObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (_leftGripPressed == true)
            {
                commonTutorialFunctionManager.PlayParticleSystem(leftgrip);
                _leftGripPressed = false;
            }
            else if (_rightGripPressed == true)
            {
                commonTutorialFunctionManager.PlayParticleSystem(rightgrip);
                _rightGripPressed = false;
            }

            if (interactable.isSelected && !_firstTimeGrab)
            {

                commonTutorialFunctionManager.ToggleBlinkState(leftgrip, rightgrip, false);
                interactiveObject.GetComponent<Outline>().enabled = true;

                interactiveObject.GetComponent<Outline>().OutlineColor = selectedObjectColor;

                interactiveObject.GetComponent<Blink>().isBlink = false;
                controllerTutorialManager.UpdateInstructionsText("CongratsOnGrab");
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("CongratsOnGrab");
                _firstTimeGrab = true;
                commonTutorialFunctionManager.PlaySoundEffect();
                StartCoroutine(selectUiButton());
            }

        }

        /// <summary>
        /// Enable the Ui button screen after 9 seconds along with activate trigger blink state and update the text and audio
        /// </summary>
        /// <returns>An enumerator for coroutine execution</returns>
        IEnumerator selectUiButton()
        {
            yield return new WaitForSeconds(9);
            commonTutorialFunctionManager.ToggleBlinkState(lefttrigger, righttrigger, true);
            controllerTutorialManager.UpdateInstructionsText("SelectUI");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("SelectUI");
            interactiveObject.SetActive(false);
            uiButton.SetActive(true);
            commonTutorialFunctionManager.HighlightAroundObject(true, hightlightButton);
        }

        /// <summary>
        /// Onclick of Button check is it first time pressed if so update the audio and interaction also
        /// </summary>
        public void onclick()
        {
            if (!_isbuttonPressed)
            {
                if (_rightTriggerPressed == true)
                {
                    commonTutorialFunctionManager.PlayParticleSystem(righttrigger);
                }
                else if (_leftTriggerPressed == true)
                {
                    commonTutorialFunctionManager.PlayParticleSystem(lefttrigger);
                }

                controllerTutorialManager.UpdateInstructionsText("CongratsOnSelectButton");
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("CongratsOnSelectButton");
                commonTutorialFunctionManager.ToggleBlinkState(lefttrigger, righttrigger, false);
                commonTutorialFunctionManager.PlaySoundEffect();
                commonTutorialFunctionManager.StopHightlight();

                _isbuttonPressed = true;
                StartCoroutine(EnableTeleport());

            }
        }

        /// <summary>
        /// Enable the teleport after 8 seconds and update the instructions and audio
        /// </summary>
        /// <returns>An enumerator for coroutine execution</returns>
        IEnumerator EnableTeleport()
        {
            yield return new WaitForSeconds(8);
            teleportPoint.SetActive(true);
            commonTutorialFunctionManager.HighlightAroundObject(true, commonTutorialFunctionManager.joystickforwardDirectionblink);
            controllerTutorialManager.UpdateInstructionsText("Teleport");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("Teleport");
            uiButton.SetActive(false);
        }

        /// <summary>
        /// This Event will be called once user is successfully teleported then update congrats audio and text
        /// </summary>
        public void OnTeleportCompleted()
        {
            teleportPoint.SetActive(false);
            commonTutorialFunctionManager.StopHightlight();
            commonTutorialFunctionManager.PlayParticleSystem(rightTeleport);
            commonTutorialFunctionManager.PlaySoundEffect();
            controllerTutorialManager.UpdateInstructionsText("CongratsOnTeleport");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("CongratsOnTeleport");
            StartCoroutine("Rotate");
        }

        /// <summary>
        /// Enable the turn and update the audio and text of rotation instruction
        /// </summary>
        /// <returns>An enumerator for coroutine execution</returns>
        IEnumerator Rotate()
        {
            yield return new WaitForSeconds(8);
            player.transform.position = new Vector3(0, 0, 0);
            player.transform.rotation = Quaternion.identity;
            turn.SetActive(true);
            commonTutorialFunctionManager.StopHightlight();
            commonTutorialFunctionManager.HighlightAroundObject(true, commonTutorialFunctionManager.joyStickSideDirectionblink);

            controllerTutorialManager.UpdateInstructionsText("Rotate");
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("Rotate");
            commonTutorialFunctionManager.rightTeleportAction.performed += OnJoyStickMove;
        }
        /// <summary>
        /// "Hover Entered" event listener for XR Grab Interactable object
        /// </summary>
        /// <param name="hoveredObject">"This is the Hovered component"</param>
        public void OnHoverOnObject(GameObject interactableObject)
        {
            interactableObject.GetComponent<Outline>().enabled = true;
            interactableObject.GetComponent<Outline>().OutlineColor = hoverObjectColor;
        }

        /// <summary>
        /// "Hover Exit" event listener for XR Grab Interactable object
        /// </summary>
        /// <param name="interactableObject">"This is the not Hovered component"</param>
        public void OnHoverExit(GameObject interactableObject)
        {
            interactableObject.GetComponent<Outline>().enabled = false;
        }

        /// <summary>
        /// Handles the JoyStick press actions based on the input context. Updates the state of joystick presses, 
        /// </summary>
        /// <param name="context">The input action callback  providing information about the Joystick press.</param>
        private void OnJoyStickMove(InputAction.CallbackContext context)
        {
            Vector2 axisValue = context.ReadValue<Vector2>();

            if (Mathf.Abs(axisValue.y) < 0.05f && (axisValue.x >= 0.1f || axisValue.x <= -0.1f))
            {
                commonTutorialFunctionManager.StopHightlight();
                commonTutorialFunctionManager.PlayParticleSystem(rightAxisTurn);
                controllerTutorialManager.UpdateInstructionsText("CongratsOnRotate");
                commonTutorialFunctionManager.PlaySoundEffect();
                commonTutorialFunctionManager.LoadAndPlayTutorialAudio("CongratsOnRotate");
                commonTutorialFunctionManager.correctCheckBoxIcon.SetActive(true);
                commonTutorialFunctionManager.vrInteractionCheckBox.SetActive(true);
                commonTutorialFunctionManager.rightTeleportAction.performed -= OnJoyStickMove;

                StartCoroutine(EnableModulePanel());
            }
        }


        /// <summary>
        /// Enable the module selection and disable the other panels
        /// </summary>
        /// <returns>An enumerator for coroutine execution</returns>
        IEnumerator EnableModulePanel()
        {
            yield return new WaitForSeconds(9f);
            player.transform.position = new Vector3(0, 0, 0);
            player.transform.rotation = Quaternion.identity;
            commonTutorialFunctionManager.correctCheckBoxIcon.SetActive(false);
            commonTutorialFunctionManager.tutorialModuleUIPanel.SetActive(false);
            commonTutorialFunctionManager.tutorialPanel.SetActive(false);
            commonTutorialFunctionManager.LoadAndPlayTutorialAudio("ChooseModule");
            commonTutorialFunctionManager.moduleSelectionPanel.SetActive(true);
        }
    }

}