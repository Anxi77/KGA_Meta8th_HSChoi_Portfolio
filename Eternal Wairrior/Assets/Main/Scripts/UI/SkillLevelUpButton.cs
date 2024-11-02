using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkillLevelUpButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Button button;

    public void SetSkillSelectButton(SkillData skillData, Action onClick)
    {
        if (skillIcon != null)
        {
            skillIcon.sprite = skillData.icon?.sprite;
            skillIcon.gameObject.SetActive(skillData.icon != null);
        }

        if (skillNameText != null)
        {
            skillNameText.text = skillData.metadata.Name;
        }

        if (descriptionText != null)
        {
            string elementDesc = GetElementalDescription(skillData.metadata.Element);
            descriptionText.text = $"{skillData.metadata.Description}\n{elementDesc}";
        }

        if (elementIcon != null)
        {
            elementIcon.sprite = GetElementSprite(skillData.metadata.Element);
            elementIcon.gameObject.SetActive(skillData.metadata.Element != ElementType.None);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetDisabledButton(string message)
    {
        if (skillIcon != null) skillIcon.gameObject.SetActive(false);
        if (elementIcon != null) elementIcon.gameObject.SetActive(false);
        if (skillNameText != null) skillNameText.text = message;
        if (descriptionText != null) descriptionText.text = "";
        if (button != null) button.interactable = false;
    }

    private string GetElementalDescription(ElementType element)
    {
        return element switch
        {
            ElementType.Dark => "Dark: Reduces defense",
            ElementType.Water => "Water: Slows movement",
            ElementType.Fire => "Fire: Deals damage over time",
            ElementType.Earth => "Earth: Stuns targets",
            _ => ""
        };
    }

    private Sprite GetElementSprite(ElementType element)
    {
        string iconPath = element switch
        {
            ElementType.Dark => "Icons/DarkElement",
            ElementType.Water => "Icons/WaterElement",
            ElementType.Fire => "Icons/FireElement",
            ElementType.Earth => "Icons/EarthElement",
            _ => ""
        };
        return !string.IsNullOrEmpty(iconPath) ? Resources.Load<Sprite>(iconPath) : null;
    }
}