using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;
using TMPro;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public TMP_Text videoHeader;
    private Coroutine videoCoroutine;
    public void PlayVideoClips(List<VideoClip> clipNames, bool requireCallback = false, Action callback = null)
    {
        if (videoCoroutine != null)
        {
            StopCoroutine(videoCoroutine);
        }
        StartCoroutine(PlayVideoSequence(clipNames, requireCallback, callback));
    }
    private IEnumerator PlayVideoSequence(List<VideoClip> clipNames, bool requireCallback, Action callback)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned.");
            yield break;
        }

        foreach (VideoClip clipRef in clipNames)
        {
            videoPlayer.Stop();
            if (clipRef == null)
            {
                Debug.LogWarning("Video clip is null. Skipping...");
                continue;
            }
            VideoClip clip = Resources.Load<VideoClip>("Video/" + clipRef.name);

            if (clip != null)
            {
                videoPlayer.clip = clip;
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoHeader.text = clipRef.name;
                videoPlayer.Play();

                // Wait until the video finishes playing
                while (videoPlayer.isPlaying || videoPlayer.frame < (long)videoPlayer.frameCount - 1)
                {
                    yield return null;
                }

                // No need to call Stop here, it will be done automatically at the end of the video
            }
            else
            {
                Debug.LogWarning("Video clip not found in Resources: " + clipRef.name);
            }
        }

        // Invoke the callback if it's required
        if (requireCallback)
        {
            callback?.Invoke();
        }
    }
    public void ResetVideoPlayer()
    {
        if (videoCoroutine != null)
        {
            StopCoroutine(videoCoroutine);
            videoCoroutine = null;
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("VideoPlayer is not assigned.");
            return;
        }
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.frame = 0;
        videoPlayer.time = 0;
        videoHeader.text = "";
        Debug.Log("Video player has been reset.");
    }
}

    