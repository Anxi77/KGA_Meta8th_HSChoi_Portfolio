using UnityEngine;
using TMPro;

public class StageTimeUI : MonoBehaviour, IInitializable
{
    [SerializeField] private TextMeshProUGUI remainingTimeText;

    public bool IsInitialized { get; private set; }
    private bool isUIReady = false;
    public bool IsUIReady => isUIReady;

    public void Initialize()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("StageTimeUI: Required components are missing!");
            return;
        }

        isUIReady = true;
        IsInitialized = true;
        Debug.Log("StageTimeUI initialized successfully");
    }

    private bool ValidateComponents()
    {
        if (remainingTimeText == null)
        {
            Debug.LogError("StageTimeUI: Remaining Time Text component is not assigned!");
            return false;
        }

        return true;
    }

    private void Update()
    {
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (StageTimeManager.Instance == null) return;

        float remainingTime = StageTimeManager.Instance.GetRemainingTime();

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        remainingTimeText.text = $"Remaining: {minutes:00}:{seconds:00}";
    }

    public void Clear()
    {
        if (remainingTimeText) remainingTimeText.text = "Remaining: 00:00";
    }

    private void OnEnable()
    {
        if (IsInitialized && !isUIReady)
        {
            isUIReady = true;
        }
    }

    private void OnDisable()
    {
        Clear();
    }
}

