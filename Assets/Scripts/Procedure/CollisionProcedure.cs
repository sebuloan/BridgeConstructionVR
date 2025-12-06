using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
namespace Bosch.ESA.Procedures
{
    public class CollisionProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public string collidingGameObject;
        public string impactedGameObject;
        public List<string> highlightGameObject = new List<string>();
        public List<string> stepDescriptionAudioClips = new List<string>();
        public string stepInteractionAudioClip;
        public List<string> videoClips = new List<string>();
        public List<string> texts = new List<string>();
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public float delay;
        public bool userConfirmation;
        public string confirmationText;
        public bool triggerMode;
        public event Action<IProcedure> ProcedureCompleted;
        public CollisionManager collisionManager;
        public HighlightingManager highlightingManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public DelayManager delayManager;
        public bool isHandCollision;
    //    public UserConfirmationManager userConfirmationManager;
        public GenericObjectManager genericObjectManager;
        public VideoManager videoManager;

        int totalCount = 0;
        int completedCount = 0;

        public void Execute()
        {
            totalCount = 0;
            completedCount = 0;

            if(highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, true);
            }

            if (collidingGameObject != null && impactedGameObject != null && collisionManager != null)
            {
                totalCount++;
                collisionManager.Initialize(collidingGameObject, impactedGameObject, triggerMode,isHandCollision, OnCollisionCompleted);
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
            if (!string.IsNullOrEmpty(stepInteractionAudioClip) && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepInteractionAudioclip(stepInteractionAudioClip, OnInteractionAudioCompleted);
            }
            if (genericObjectManager != null && genericObjects != null && genericObjects.Count > 0)
            {
                UnityEngine.Debug.Log("generic object");
                totalCount++;
                genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
            }

            if (stepDescriptionAudioClips != null && stepDescriptionAudioClips.Count > 0 && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepDescriptionAudioClip(stepDescriptionAudioClips, OnAudioCompleted);
            }

            if (texts.Count > 0)
            {
                uiManager.ShowText(texts, stepNumber);
            }
            if (totalCount == 0)
            {
                OnAllFunctionCompleted();
            }
        }
        public void OnAudioCompleted()
        {
            completedCount++;
            if (completedCount == totalCount)
            {
                UnityEngine.Debug.Log("Audio completed in collision type");
                OnAllFunctionCompleted();
            }
        }

        public void OnCollisionCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("collision completed");
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }
  
        public bool IsComplete()
        {
            throw new NotImplementedException();
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

        public void OnGameObjectStateSet()
        {
            completedCount++;
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
                UnityEngine.Debug.Log("Object state is set");
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
        public void OnCompleted()
        {
            if (highlightGameObject.Count >= 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, false);
            }
            ProcedureCompleted.Invoke(this);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}