using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private string[] loadingTips;

    private void Start()
    {
        if (loadingTips != null && loadingTips.Length > 0)
        {
            ShowRandomTip();
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }

    public void ResetProgress() 
    {
        if(progressBar != null) 
        {
            progressBar.value = 0;
        }
    }

    private void ShowRandomTip()
    {
        if (loadingText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            loadingText.text = loadingTips[randomIndex];
        }
    }
}