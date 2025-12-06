using Bosch.ESA.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class HookProcedure : MonoBehaviour, IProcedure
{
    public int stepNumber;
    public string pickObject;
    public string hookTargetLocation;
    public List<ClimbInteractorObject> climbInteractors = new List<ClimbInteractorObject>();
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
    public HookInteractionManager hookInteractionManager;
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

    private bool isPickComplete = false;


    public void Execute()
    {
        isPickComplete = false;

        Debug.Log("hookinteraction started.");
        completedCount = 0;
        totalCount = 0;

        //if (genericObjectManager != null && genericObjects != null && genericObjects.Count > 0)
        //{
        //    UnityEngine.Debug.Log("generic object");
        //    totalCount++;
        //    genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
        //}
        if (hookInteractionManager != null && pickObject != null && hookTargetLocation != null)
        {
            totalCount++;
            hookInteractionManager.HandleHookInteraction(pickObject, hookTargetLocation, climbInteractors, OnHookInteractionCompleted);
        }
        if (highlightGameObject.Count > 0)
        {
            highlightingManager.EnableTheOutline(highlightGameObject, true);
        }
        if (texts.Count > 0 && texts != null)
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




        if (totalCount == 0)
        {
            OnAllFunctionCompleted();
        }
    }

    private void OnHookInteractionCompleted()
    {
        isPickComplete = true;
        completedCount++;

        UnityEngine.Debug.Log("OnHookInteractionCompleted");
        if (completedCount == totalCount)
        {
            OnAllFunctionCompleted();
        }
        if (genericObjectManager != null && genericObjects != null && genericObjects.Count > 0)
        {
            UnityEngine.Debug.Log("generic object");
            //totalCount++;
            genericObjectManager.SetGameObjectStates(genericObjects, OnGameObjectStateSet);
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

    public void OnInteractionAudioCompleted()
    {
        completedCount++;
        UnityEngine.Debug.Log("Audio interaction complted");
        if (completedCount == totalCount)
        {
            OnAllFunctionCompleted();
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


    public bool IsComplete()
    {
        return isPickComplete;
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

    public void OnCompleted()
    {
        if (highlightGameObject.Count > 0)
        {
            highlightingManager.EnableTheOutline(highlightGameObject, false);
        }

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

