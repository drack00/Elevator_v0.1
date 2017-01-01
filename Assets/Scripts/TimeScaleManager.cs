using UnityEngine;
using System.Collections;

[System.Serializable]
public enum TimeScaleModifier
{
    None, Duration, Toggle
}
[System.Serializable]
public class ApplyTimeScale
{
    public TimeScaleModifier timeScaleModifier;
    public float timeScale;
    public float timeScaleDuration;

    public void Do(GameObject timeScaler)
    {
        if (timeScaleModifier == TimeScaleModifier.Toggle)
            TimeScaleManager.singleton.SetTimeScale(timeScale, timeScaler);
        else if (timeScaleModifier == TimeScaleModifier.Duration)
            TimeScaleManager.singleton.StartDurationTimeScale(timeScale, timeScaleDuration);
    }
}

public class TimeScaleManager : MonoBehaviour
{
    public static TimeScaleManager singleton
    {
        get
        {
            return FindObjectOfType<TimeScaleManager>();
        }
    }

    private float defaultTimeScale;
    private GameObject timeScaler;
    private bool onTimeScale = false;
    public void StartDurationTimeScale(float timeScale, float timeScaleDuration)
    {
        if (!onTimeScale)
        {
            StartCoroutine(DurationTimeScale(timeScale, timeScaleDuration));
        }
    }
    private IEnumerator DurationTimeScale(float timeScale, float timeScaleDuration)
    {
        onTimeScale = true;

        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        float _releaseTimeScaleDuration = 0.0f;

        while (_releaseTimeScaleDuration < timeScaleDuration)
        {
            _releaseTimeScaleDuration += Time.deltaTime;

            yield return null;
        }

        Time.timeScale = defaultTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        onTimeScale = false;
    }
    public void SetTimeScale(float newTimeScale, GameObject newTimerScaler)
    {
        if(timeScaler == null)
        {
            Time.timeScale = newTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            timeScaler = newTimerScaler;
        }
    }
    public void ResetTimeScale(GameObject refTimeScaler)
    {
        if(timeScaler == refTimeScaler)
        {
            Time.timeScale = defaultTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            timeScaler = null;
        }
    }

    public void Awake ()
    {
        defaultTimeScale = Time.deltaTime;
    }
}
