using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Bosch.ESA.Procedures
{
    public class GeneralProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public List<string> stepDescriptionAudioClips = new List<string>();
        public string stepInteractionAudioClip;
        public List<string> texts = new List<string>();
        public List<string> videoClips = new List<string>();
        public string particleSystemObject;
        public float delay;
        public bool userConfirmation;
        public string confirmationText;
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public event Action<IProcedure> ProcedureCompleted;
        public AudioManager audioManager;
        public UIManager uiManager;
        public GenericObjectManager genericObjectManager;
        public ParticleSystemManager particleSystemManager;
        public DelayManager delayManager;
        //public UserConfirmationManager userConfirmationManager;
        public VideoManager videoManager;
        int completedCount = 0;
        int totalCount = 0;

        public void Execute()
        {
            completedCount = 0;
            totalCount = 0;

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
            if (videoClips != null && videoClips.Count > 0 && videoManager != null)
            {
                var videoClipList = new List<VideoClip>();
                foreach (var videoClipName in videoClips)
                {
                    videoClipList.Add(Resources.Load<VideoClip>(videoClipName));
                }
                videoManager.PlayVideoClips(videoClipList);
            }

            if (particleSystemObject != null && particleSystemManager != null)
            {
                totalCount++;
                particleSystemManager.Initialize(particleSystemObject, OnParticlePlayed);
            }

            if(genericObjectManager != null && genericObjects != null && genericObjects.Count>0)
            {
                Debug.Log("generic object");
                totalCount++;
                genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
            }

            if (texts.Count > 0 && texts != null)
            {
                //totalCount++;
                uiManager.ShowText(texts, stepNumber);
                OnUITextCompleted();
            }
            if (totalCount == 0)
            {
                OnAllFunctionCompleted();
            }

        }

        private void OnUITextCompleted()
        {
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
            }
        }

        private IEnumerator CompleteAfterDelay()
        {
            yield return new WaitForSeconds(delay);
            //  OnCompleted();
        }

        public bool IsComplete()
        {
            throw new NotImplementedException();
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
                Debug.Log("Audio playing");
                OnAllFunctionCompleted();
            }
        }
        public void OnParticlePlayed()
        {
            completedCount++;
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
                UnityEngine.Debug.Log("ParticelPlayed");
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
        public void OnVideoClipCompleted()
        {
            completedCount++;
            if (completedCount == totalCount)
            {
                OnAllFunctionCompleted();
                UnityEngine.Debug.Log("OnvideoPlayerCompleted");
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

        public void OnCompleted()
        {
            if (null != ProcedureCompleted)
            {
                ProcedureCompleted.Invoke(this);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}

