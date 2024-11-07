using UnityEngine;

public class StageTimeManager : SingletonManager<StageTimeManager>
{
    private float stageTimer;
    private float maxStageTime;
    private bool isTimerRunning;

    public float RemainingTime => Mathf.Max(0, maxStageTime - stageTimer);
    public float StageProgress => stageTimer / maxStageTime;

    public event System.Action OnStageTimeUp;

    private void Update()
    {
        if (isTimerRunning)
        {
            stageTimer += Time.deltaTime;

            if (stageTimer >= maxStageTime)
            {
                StopStageTimer();
                OnStageTimeUp?.Invoke();
            }
        }
    }

    public void StartStageTimer(float duration)
    {
        maxStageTime = duration;
        stageTimer = 0f;
        isTimerRunning = true;
    }

    public void StopStageTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        stageTimer = 0f;
        isTimerRunning = false;
    }

    public bool IsStageTimeUp()
    {
        return stageTimer >= maxStageTime;
    }

    public string GetFormattedTime()
    {
        float time = RemainingTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}