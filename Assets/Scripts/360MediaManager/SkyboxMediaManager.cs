using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class SkyboxMediaManager : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoTexture;

    [Header("UI Prefabs")]
    public Button imageButtonPrefab;
    public Button videoButtonPrefab;
    public Transform imageContentParent;
    public Transform videoContentParent;
    public GameObject videoPlayerPanel;
    public Button backButton;

    [Header("Extra Panels")]
    public GameObject videoSelectionPanel;

    [Header("UI Controls")]
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Slider progressSlider;
    public Button forward10SecButton;
    public Button backward10SecButton;
    public Button nextVideoButton;
    public Button previousVideoButton;
    public TMP_Text currentTimeText;
    public TMP_Text totalTimeText;
    public TMP_Text currentVideoText;

    [Header("Media Sources")]
    public Texture2D[] cubemapPreviews;
    public Cubemap[] hdrCubemaps;
    public Texture2D[] videoPreviews;
    public string[] videoFileNames;

    [Header("Skybox Materials")]
    public Material skyboxCubemap;      // For HDR cubemaps
    public Material skyboxPanoramic;    // For 360 videos
    private Material originalSkybox;     // Store original material
    private Cubemap initialCubemap;      // Store the very first cubemap
                                        

    private int currentVideoIndex = 0;
    private bool isDraggingProgress = false;
    private bool isVideoPrepared = false;
    private bool autoPlayAfterPrepare = false;

    void Start()
    {
        if (RenderSettings.skybox != null)
            originalSkybox = RenderSettings.skybox;

        // Save the initial cubemap from the material (if assigned)
        if (skyboxCubemap != null && skyboxCubemap.HasProperty("_Tex"))
            initialCubemap = skyboxCubemap.GetTexture("_Tex") as Cubemap;

        // Set initial skybox
        if (skyboxCubemap != null)
            RenderSettings.skybox = skyboxCubemap;

        InitializeVideoPlayer();
        SetupUIEvents();
        CreateMediaButtons();
        UpdateVideoUIForLoading();
    }


    void InitializeVideoPlayer()
    {
        if (videoTexture != null)
            videoPlayer.targetTexture = videoTexture;

        videoPlayer.isLooping = true;
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void SetupUIEvents()
    {
        playButton?.onClick.AddListener(PlayVideo);
        pauseButton?.onClick.AddListener(PauseVideo);
        stopButton?.onClick.AddListener(StopVideo);

        forward10SecButton?.onClick.AddListener(() => SeekRelative(10f));
        backward10SecButton?.onClick.AddListener(() => SeekRelative(-10f));

        nextVideoButton?.onClick.AddListener(NextVideo);
        previousVideoButton?.onClick.AddListener(PreviousVideo);

        backButton?.onClick.AddListener(HideVideoPlayerPanel);

        if (progressSlider != null)
        {
            progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);

            var sliderEvents = progressSlider.GetComponent<SliderEvents>();
            if (sliderEvents == null) sliderEvents = progressSlider.gameObject.AddComponent<SliderEvents>();
            sliderEvents.onBeginDrag += () => isDraggingProgress = true;
            sliderEvents.onEndDrag += OnProgressSliderDragEnd;
        }
    }

    private void HideVideoPlayerPanel()
    {
        if (videoPlayerPanel != null)
            videoPlayerPanel.SetActive(false);

        StopVideo();

        // Restore original skybox or Cubemap material
        if (originalSkybox != null)
            RenderSettings.skybox = originalSkybox;
        else if (skyboxCubemap != null)
            RenderSettings.skybox = skyboxCubemap;

        DynamicGI.UpdateEnvironment();

        if (videoSelectionPanel != null)
            videoSelectionPanel.SetActive(true);
    }

    void CreateMediaButtons()
    {
        // HDR cubemaps
        if (imageButtonPrefab != null && imageContentParent != null)
        {
            for (int i = 0; i < hdrCubemaps.Length; i++)
            {
                int index = i;
                var cubemap = hdrCubemaps[i];
                var btn = Instantiate(imageButtonPrefab, imageContentParent);

                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = cubemap.name;

                var rawImage = btn.GetComponentInChildren<RawImage>();
                if (rawImage != null && cubemapPreviews.Length > i)
                    rawImage.texture = cubemapPreviews[i];

                btn.onClick.AddListener(() => ShowHDRCubemap(cubemap));
            }
        }

        // Video buttons
        if (videoButtonPrefab != null && videoContentParent != null)
        {
            for (int i = 0; i < videoPreviews.Length; i++)
            {
                int index = i;
                var btn = Instantiate(videoButtonPrefab, videoContentParent);

                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = videoFileNames.Length > index ? videoFileNames[index] : videoPreviews[index].name;

                var rawImage = btn.GetComponentInChildren<RawImage>();
                if (rawImage != null && videoPreviews.Length > index)
                    rawImage.texture = videoPreviews[index];

                btn.onClick.AddListener(() => ShowVideo(index));
            }
        }
    }

    public void ShowHDRCubemap(Cubemap cubemap)
    {
        StopVideo();

        if (cubemap != null && skyboxCubemap != null)
        {
            skyboxCubemap.shader = Shader.Find("Skybox/Cubemap");
            skyboxCubemap.SetTexture("_Tex", cubemap);
            RenderSettings.skybox = skyboxCubemap;
            DynamicGI.UpdateEnvironment();
        }
    }

    public void ShowVideo(int videoIndex)
    {
        if (videoIndex < 0 || videoIndex >= videoFileNames.Length) return;

        currentVideoIndex = videoIndex;

        if (videoPlayerPanel != null)
            videoPlayerPanel.SetActive(true);

        if (videoSelectionPanel != null)
            videoSelectionPanel.SetActive(false);

        autoPlayAfterPrepare = true;
        UpdateVideoUIForLoading();

        if (videoTexture != null)
            videoPlayer.targetTexture = videoTexture;

        if (skyboxPanoramic != null)
        {
            skyboxPanoramic.shader = Shader.Find("Skybox/Panoramic");
            skyboxPanoramic.SetTexture("_MainTex", videoTexture);
            skyboxPanoramic.SetFloat("_Mapping", 1);
            skyboxPanoramic.SetFloat("_ImageType", 0);
            skyboxPanoramic.SetFloat("_Exposure", 1f);
            skyboxPanoramic.SetFloat("_Rotation", 0f);

            RenderSettings.skybox = skyboxPanoramic;
            DynamicGI.UpdateEnvironment();
        }

        StartCoroutine(PlayVideoFromStreamingAssets(videoIndex));
    }

    private IEnumerator PlayVideoFromStreamingAssets(int index)
    {
        if (index < 0 || index >= videoFileNames.Length)
        {
            Debug.LogError("Invalid video index: " + index);
            yield break;
        }

        string fileName = videoFileNames[index] + ".mp4";
        string relativePath = System.IO.Path.Combine("360Videos", fileName);

        string persistentPath = System.IO.Path.Combine(Application.persistentDataPath, relativePath);
        string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, relativePath);

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!System.IO.File.Exists(persistentPath))
        {
            using UnityWebRequest www = UnityWebRequest.Get(streamingPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to copy video: " + www.error);
                yield break;
            }

            try
            {
                string dir = System.IO.Path.GetDirectoryName(persistentPath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                System.IO.File.WriteAllBytes(persistentPath, www.downloadHandler.data);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to save video: " + ex.Message);
                yield break;
            }
        }

        string finalPath = "file://" + persistentPath;
#else
        string finalPath = "file://" + streamingPath;
#endif

        Debug.Log("Loading video: " + finalPath);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = finalPath;
        videoPlayer.Prepare();

        float timeout = 10f;
        float timer = 0f;
        while (!videoPlayer.isPrepared && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("Video failed to prepare.");
            yield break;
        }

        isVideoPrepared = true;

        if (autoPlayAfterPrepare)
        {
            videoPlayer.Play();
            autoPlayAfterPrepare = false;
        }
    }

    void Update()
    {
        if (!isVideoPrepared) return;

        UpdateVideoUI();
    }

    void UpdateVideoUI()
    {
        if (!isVideoPrepared || videoPlayer == null) return;

        if (!isDraggingProgress && progressSlider != null && videoPlayer.frameCount > 0)
            progressSlider.SetValueWithoutNotify((float)videoPlayer.frame / (float)videoPlayer.frameCount);

        if (currentTimeText != null) currentTimeText.text = FormatTime(videoPlayer.time);
        if (totalTimeText != null) totalTimeText.text = FormatTime(videoPlayer.length);

        if (playButton != null) playButton.gameObject.SetActive(!videoPlayer.isPlaying);
        if (pauseButton != null) pauseButton.gameObject.SetActive(videoPlayer.isPlaying);

        if (stopButton != null) stopButton.interactable = videoPlayer.isPlaying || videoPlayer.isPaused;
        if (previousVideoButton != null) previousVideoButton.interactable = currentVideoIndex > 0;
        if (nextVideoButton != null) nextVideoButton.interactable = currentVideoIndex < videoFileNames.Length - 1;

        if (currentVideoText != null && videoFileNames.Length > 0 && currentVideoIndex < videoFileNames.Length)
            currentVideoText.text = $"{currentVideoIndex + 1}/{videoFileNames.Length}: {videoFileNames[currentVideoIndex]}";
    }

    private void UpdateVideoUIForLoading()
    {
        progressSlider?.SetValueWithoutNotify(0f);

        if (currentTimeText != null) currentTimeText.text = "00:00";
        if (totalTimeText != null) totalTimeText.text = "00:00";

        if (playButton != null) playButton.gameObject.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
        if (stopButton != null) stopButton.interactable = false;

        if (previousVideoButton != null) previousVideoButton.interactable = currentVideoIndex > 0;
        if (nextVideoButton != null) nextVideoButton.interactable = currentVideoIndex < videoFileNames.Length - 1;

        if (currentVideoText != null && videoFileNames.Length > 0 && currentVideoIndex < videoFileNames.Length)
            currentVideoText.text = $"{currentVideoIndex + 1}/{videoFileNames.Length}: {videoFileNames[currentVideoIndex]}";
    }

    public void PlayVideo() => videoPlayer?.Play();
    public void PauseVideo() => videoPlayer?.Pause();

    public void StopVideo()
    {
        videoPlayer.Stop();
        isVideoPrepared = false;
        autoPlayAfterPrepare = false;
        UpdateVideoUIForLoading();
    }

    public void NextVideo()
    {
        int nextIndex = Mathf.Min(currentVideoIndex + 1, videoFileNames.Length - 1);
        if (nextIndex != currentVideoIndex)
            ShowVideo(nextIndex);
    }

    public void PreviousVideo()
    {
        int prevIndex = Mathf.Max(currentVideoIndex - 1, 0);
        if (prevIndex != currentVideoIndex)
            ShowVideo(prevIndex);
    }

    private void SeekRelative(float seconds)
    {
        if (isVideoPrepared && videoPlayer.canSetTime)
        {
            double newTime = Mathf.Clamp((float)videoPlayer.time + seconds, 0f, (float)videoPlayer.length);
            videoPlayer.time = newTime;
        }
    }

    private void OnProgressSliderChanged(float value)
    {
        if (isDraggingProgress && currentTimeText != null && isVideoPrepared)
            currentTimeText.text = FormatTime(value * videoPlayer.length);
    }

    private void OnProgressSliderDragEnd()
    {
        isDraggingProgress = false;
        if (isVideoPrepared && progressSlider != null && videoPlayer.canSetTime)
            videoPlayer.time = progressSlider.value * videoPlayer.length;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        isVideoPrepared = true;
        progressSlider?.SetValueWithoutNotify(0f);

        if (autoPlayAfterPrepare)
        {
            videoPlayer.Play();
            autoPlayAfterPrepare = false;
        }
    }

    private string FormatTime(double seconds)
    {
        var time = System.TimeSpan.FromSeconds(seconds);
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    public void RestoreOriginalSkybox()
    {
        if (skyboxCubemap != null && initialCubemap != null)
        {
            skyboxCubemap.shader = Shader.Find("Skybox/Cubemap");
            skyboxCubemap.SetTexture("_Tex", initialCubemap);
            RenderSettings.skybox = skyboxCubemap;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox restored to initial cubemap.");
        }
        else if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
            DynamicGI.UpdateEnvironment();
            Debug.Log("Skybox restored to original material.");
        }
    }


}
