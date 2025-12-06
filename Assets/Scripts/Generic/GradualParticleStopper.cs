using UnityEngine;

public class GradualParticleStopper : MonoBehaviour
{
    public ParticleSystem gasParticleSystem;
    public float stopDuration = 1.5f;

    private ParticleSystem.EmissionModule emissionModule;
    private float originalRate;
    private float stopTimer;
    private bool isStopping = false;

    void Start()
    {
        if (gasParticleSystem != null)
        {
            emissionModule = gasParticleSystem.emission;
            originalRate = emissionModule.rateOverTime.constant;
        }
        StartGradualStop();
    }

    public void StartGradualStop()
    {
        Debug.Log("Gradual stop initiated for gas particle system.");
        if (gasParticleSystem != null)
        {
            stopTimer = 0f;
            isStopping = true;
        }
    }

    void Update()
    {
        if (isStopping)
        {
            stopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stopTimer / stopDuration);
            float newRate = Mathf.Lerp(originalRate, 0f, t);
            emissionModule.rateOverTime = newRate;

            if (t >= 1f)
            {
                isStopping = false;
                gasParticleSystem.Stop();
            }
        }
    }
}