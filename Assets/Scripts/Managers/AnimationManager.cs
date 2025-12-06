using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Playables;
using System.Collections.Generic;
using Bosch.ESA.Managers;
//json
public class AnimationManager : MonoBehaviour
{
    /// <summary>
    /// Plays animations on game object based on the provided animation data,start frame to the end frame of each animation clip.
    /// </summary>
    /// <param name="animationData">A list of AnimationData objects </param>
    /// <param name="callback">A callback action that is invoked once all animations have finished playing.</param> 
    public void PlayAnimation(AnimationData animationData, Action callback)
    {
        GameObject obj = GameObject.Find(animationData.gameObjectName);
        if (obj == null)
        {
            Debug.LogError($"GameObject '{animationData.gameObjectName}' not found.");
            return;
        }
        Animator animator = obj.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the target GameObject.");
            return;
        }
        string myClip = animationData.animationClipName;
        float totalFrames = 0;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (myClip == clip.name)
            {
                totalFrames = (clip.length * clip.frameRate);
            }
        }
        PlayAnimationBetweenFrames(obj, myClip, animationData.startFrame, animationData.endFrame, totalFrames, callback);
    }

    /// <summary>
    /// Starts the process of playing an animation between specific start and end frames on a target game object.
    /// </summary>
    public void PlayAnimationBetweenFrames(GameObject target, string animationClip, float startFrame, float endFrame, float totalFrames, Action callback)
    {
        StartCoroutine(PlayAnimationCoroutine(target, animationClip, startFrame, endFrame, totalFrames, callback));
    }

    /// <summary>
    /// A coroutine that plays the animation on a target game object between the specified start and end frames.
    /// It pauses and waits until the animation reaches the specified end frame before invoking the callback.
    /// </summary>
    private IEnumerator PlayAnimationCoroutine(GameObject target, string animationClip, float startFrame, float endFrame, float totalFrames, Action callback)
    {
        float startNormalizedTime = startFrame / totalFrames;
        float endNormalizedTime = endFrame / totalFrames;
        Animator animator = target.GetComponent<Animator>();
        animator.Play(animationClip, 0, startNormalizedTime);

        yield return new WaitForSeconds(0.1f);
        animator.speed = 1;


        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < endNormalizedTime)
        {
            yield return null;
        }

        animator.speed = 0;
        Debug.Log("Animation is playing");
        callback?.Invoke();
    }
}
