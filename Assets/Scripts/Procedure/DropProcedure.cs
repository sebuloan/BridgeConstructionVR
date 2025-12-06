using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Bosch.ESA.Procedures
{
    public class DropProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public string objectToDrop;
        public string dropLocation;
        public AnimationData animations;
        public List<string> highlightGameObject = new List<string>();
        public List<string> stepDescriptionAudioClips = new List<string>();
        public List<string> texts = new List<string>();
        public List<string> videoClips = new List<string>();
        public string stepInteractionAudioClip;
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public bool retainInHands;
        public float delay;
        public bool userConfirmation;
        public string confirmationText;
        public GameObject currentlyHeldObject;
        public DropManager dropManager;
        public AnimationManager animationManager;
        public HighlightingManager highlightingManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public DelayManager delayManager;
        public VideoManager videoManager;
        public GenericObjectManager genericObjectManager;
        int totalCount = 0;
        int completedCount = 0;
     //   public UserConfirmationManager userConfirmationManager;
        public event Action<IProcedure> ProcedureCompleted;

        private bool isDropComplete = false;

        public void Execute()
        {
            isDropComplete = false;
            //

            completedCount = 0;
            totalCount = 0;
            if (highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, true);
            }
            if (texts != null && texts.Count > 0 )
            {

                uiManager.ShowText(texts, stepNumber);
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
            if (!string.IsNullOrEmpty(stepInteractionAudioClip) && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepInteractionAudioclip(stepInteractionAudioClip, OnInteractionAudioCompleted);
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

            if (dropManager != null && objectToDrop != null)
            {
                totalCount++;
                dropManager.BeginDrop(objectToDrop);
                dropManager.ObjectDropped += OnObjectDropped;
            }
            else
            {
                Debug.LogWarning($"DropProcedure (Step {stepNumber}): DropManager or objectToDrop is null. Cannot start drop.");
            }


            if (totalCount == 0)
            {
                OnAllFunctionCompleted();
            }
        }

        private void OnObjectDropped(GameObject droppedObject)
        {
            // Detach the listener
            dropManager.ObjectDropped -= OnObjectDropped;
            isDropComplete = true;
            completedCount++;
            UnityEngine.Debug.Log("drop  complted");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }

        public bool IsComplete()
        {
            return isDropComplete;
        }

        public void OnCompleted()
        {
            // ProcedureCompleted.Invoke(this);
            if (highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, false);
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
            if (dropManager != null)
            {
                dropManager.StopDrop();
                dropManager.ObjectDropped -= OnObjectDropped; // Detach here as well
            }
            StopAllCoroutines();
        }
    }
}
