using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Bosch.ESA.Procedures
{
    public class PickProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public string pickableObject;
        public AnimationData animations;
        public List<string> highlightGameObjects = new List<string>();
        public List<string> stepDescriptionAudioClips = new List<string>();
        public List<string> texts = new List<string>();
        public List<string> videoClips = new List<string>();
        public string stepInteractionAudioClip;
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public bool retainInHands;
        public float delay;
        public bool userConfirmation;
        public string confirmationText;

        public event Action<IProcedure> ProcedureCompleted;

        public PickManager pickManager;
        public AnimationManager animationManager;
        public HighlightingManager highlightingManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public DelayManager delayManager;
        public VideoManager videoManager;
        int totalCount = 0;
        int completedCount = 0;
     //   public UserConfirmationManager userConfirmationManager;
        public GenericObjectManager genericObjectManager;
        private bool isPickComplete = false;

        public void Execute()
        {
            isPickComplete = false;
            completedCount = 0;
            totalCount = 0;

            if (string.IsNullOrEmpty(pickableObject))
            {
                Debug.LogError($"PickProcedure (Step {stepNumber}): pickableObject is not assigned.");
                OnCompleted();  // Calls the completion even if the object is missing
                return;
            }

            // Start the picking process in the PickManager
            if (pickableObject != null && pickManager != null)
            {
                totalCount++;
                pickManager.BeginPick(pickableObject, retainInHands, OnObjectPicked);
            }

            if (highlightGameObjects.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObjects, true);
            }
            if (texts.Count > 0 && texts != null)
            {
                uiManager.ShowText(texts, stepNumber);
            }
            if (!string.IsNullOrEmpty(stepInteractionAudioClip) && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepInteractionAudioclip(stepInteractionAudioClip, OnInteractionAudioCompleted);
            }
            if (stepDescriptionAudioClips != null && stepDescriptionAudioClips.Count > 0 && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepDescriptionAudioClip(stepDescriptionAudioClips, OnAudioCompleted);
            }

            if (animations != null && animationManager != null && !string.IsNullOrEmpty(animations.gameObjectName) && !string.IsNullOrEmpty(animations.animationClipName))
            {
                totalCount++;
                animationManager.PlayAnimation(animations, OnAnimationCompleted);
            }

            if (videoClips != null && videoClips.Count > 0 && videoManager != null)
            {
                var videoClipList = new List<VideoClip>();
                foreach (var videoClipName in videoClips)
                {
                    videoClipList.Add(Resources.Load<VideoClip>(videoClipName));
                }
                videoManager.PlayVideoClips(videoClipList);
            }

            if (genericObjectManager != null && genericObjects != null && genericObjects.Count > 0)
            {
                UnityEngine.Debug.Log("generic object");
                totalCount++;
                genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
            }

            if (totalCount == 0)
            {
                OnAllFunctionCompleted();
            }
        }

        private void OnObjectPicked(string pickedObject)
        {
            isPickComplete = true;
            completedCount++;
            UnityEngine.Debug.Log("pick and complted");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }

        public bool IsComplete()
        {
            return isPickComplete;
        }

        public void OnCompleted()
        {
            // Stop the pick manager to clean up listeners
            if (pickManager != null)
            {
                pickManager.StopPick();
            }
            if (highlightGameObjects.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObjects, false);
            }
            if (null != ProcedureCompleted)
            {
                ProcedureCompleted.Invoke(this);
            }
        }
        public void OnInteractionAudioCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("Audio interaction complted");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }
        public void OnAudioCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("Audio complted");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }

        public void OnGameObjectStateSet()
        {
            completedCount++;
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
                UnityEngine.Debug.Log("Object state is set");
            }
        }

        public void OnAnimationCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("animation complted");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }
        public void OnAllFunctionCompleted()
        {
            if (userConfirmation)
            {
                if (delay > 0)
                {
                    delayManager.ExecuteAfterDelay(delay, ShowUserConfirmation);
                }
                else
                {
                    ShowUserConfirmation();
                }
            }
            else
            {
                if (delay > 0)
                {
                    delayManager.ExecuteAfterDelay(delay, OnCompleted);
                }
                else
                {
                    OnCompleted();
                }
            }
        }

        private void ShowUserConfirmation()
        {
            uiManager.SwitchToConfimationPanel(confirmationText, OnCompleted);
        }

        public void Stop()
        {
            if (pickManager != null)
            {
                pickManager.StopPick();
            }
            StopAllCoroutines();
        }
    }
}