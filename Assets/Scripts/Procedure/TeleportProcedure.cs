using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//json
namespace Bosch.ESA.Procedures
{
    public class TeleportProcedure : MonoBehaviour, IProcedure
    {
        public int stepNumber;
        public string gameObjectName;
        public List<string> highlightGameObject = new List<string>();
        public List<string> stepDescriptionAudioClips = new List<string>();
        public List<string> texts = new List<string>();
        public string stepInteractionAudioClip;
        public List<GenericObjectData> genericObjects = new List<GenericObjectData>();
        public float delay;
        public bool userConfirmation;
        public string confirmationText;
        public event Action<IProcedure> ProcedureCompleted;
        public TeleportManager teleportManager;
        public HighlightingManager highlightingManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public DelayManager delayManager;
        public string teleportParent;
        //  public UserConfirmationManager userConfirmationManager;
        public GenericObjectManager genericObjectManager;
        int totalCount = 0;
        int completedCount = 0;

        public void Execute()
        {
            completedCount = 0;
            totalCount = 0;
            if (gameObjectName != null && teleportManager != null)
            {
                totalCount++;
                //  gameObject.SetActive(true);
                teleportManager.PlayerTeleportArea(teleportParent, gameObjectName, OnTeleportCompleted);
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
            if (!string.IsNullOrEmpty(stepInteractionAudioClip) && audioManager != null)
            {
                totalCount++;
                audioManager.PlayStepInteractionAudioclip(stepInteractionAudioClip, OnInteractionAudioCompleted);
            }
            if (highlightGameObject.Count > 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, true);
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

        private IEnumerator CompleteAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            // OnCompleted();
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
            if (highlightGameObject.Count >= 0)
            {
                highlightingManager.EnableTheOutline(highlightGameObject, false);
            }
            ProcedureCompleted.Invoke(this);
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
        public void OnInteractionAudioCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("Audio interaction complted");
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

        public void OnTeleportCompleted()
        {
            completedCount++;
            UnityEngine.Debug.Log("teleport complted");
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
