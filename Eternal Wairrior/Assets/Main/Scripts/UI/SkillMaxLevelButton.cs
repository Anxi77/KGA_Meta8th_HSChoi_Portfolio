using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkillMaxLevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image backgroundImage;

    public void Initialize(Action onContinue)
    {
        if (titleText != null)
            titleText.text = "All Skills Maxed!";

        if (descriptionText != null)
            descriptionText.text = "Congratulations! All available skills are at maximum level.\nClick to continue.";

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => onContinue?.Invoke());
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.8f, 0.8f, 0.2f, 0.5f);
        }
    }
}