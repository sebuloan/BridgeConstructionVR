using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
//json
namespace Bosch.ESA.Procedures
{
    public class AnimationProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public AnimationData animations;
        public List<string> highlightGameObject = new List<string>();
        public List<string> stepDescriptionAudioClips = new List<string>();
        public string stepInteractionAudioClip;
        public List<string> videoClips = new List<string>();
        public List<string> texts = new List<string>();
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public float delay;
        public bool userConfirmation;
        public string confirmationText;
        public event Action<IProcedure> ProcedureCompleted;
        public AnimationManager animationManager;
        public HighlightingManager highlightingManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public DelayManager delayManager;
        public VideoManager videoManager;
        public GenericObjectManager genericObjectManager;
        int totalCount = 0;
        int completedCount = 0;
  //      public UserConfirmationManager userConfirmationManager;

        public void Execute()
        {
            completedCount = 0;
            totalCount = 0;

            if (highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, true);
            }
            if (texts.Count > 0)
            {
                uiManager.ShowText(texts, stepNumber);
            }

            if (stepDescriptionAudioClips != null && stepDescriptionAudioClips.Count > 0 && audioManager != null)
            {
                totalCount++;

                audioManager.PlayStepDescriptionAudioClip(stepDescriptionAudioClips, OnAudioCompleted);
            }
            if (!string.IsNullOrEmpty(stepInteractionAudioClip) && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepInteractionAudioclip(stepInteractionAudioClip, OnInteractionAudioCompleted);
            }
            if (animations != null && animationManager != null)
            {
                totalCount++;
                animationManager.PlayAnimation(animations, OnAnimationCompleted);
                UnityEngine.Debug.Log("animation is playimg in i procedure");
            }

            if (genericObjectManager != null && genericObjects != null && genericObjects.Count > 0)
            {
                UnityEngine.Debug.Log("generic object");
                totalCount++;
                genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
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

            if (totalCount == 0)
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
        private IEnumerator CompleteAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            //OnCompleted();
        }

        public bool IsComplete()
        {
            throw new NotImplementedException();
        }
        private void ShowUserConfirmation()
        {
            uiManager.SwitchToConfimationPanel(confirmationText, OnCompleted);
        }

        public void OnCompleted()
        {
            if (highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, false);
            }

            //if (videoManager != null && videoClips != null && videoClips.Count > 0)
            //{
            //    videoManager.ResetVideoPlayer();
            //}
            ProcedureCompleted.Invoke(this);
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

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
