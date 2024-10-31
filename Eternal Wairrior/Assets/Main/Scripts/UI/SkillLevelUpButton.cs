using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkillLevelUpButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image elementIcon;  // 속성 아이콘
    [SerializeField] private Button button;

    public void SetSkillSelectButton(SkillData skillData, Action onClick)
    {
        // 스킬 아이콘 설정
        if (skillIcon != null)
        {
            skillIcon.sprite = skillData.icon?.sprite;
            skillIcon.gameObject.SetActive(skillData.icon != null);
        }

        // 스킬 이름 설정
        if (skillNameText != null)
        {
            skillNameText.text = skillData.Name;
        }

        // 스킬 설명 설정
        if (descriptionText != null)
        {
            string elementDesc = GetElementalDescription(skillData.GetCurrentTypeStat().baseStat.element);
            descriptionText.text = $"{skillData.Description}\n{elementDesc}";
        }

        // 속성 아이콘 설정
        if (elementIcon != null)
        {
            elementIcon.sprite = GetElementSprite(skillData.GetCurrentTypeStat().baseStat.element);
            elementIcon.gameObject.SetActive(skillData.GetCurrentTypeStat().baseStat.element != ElementType.None);
        }

        // 버튼 클릭 이벤트 설정
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    // 비활성화된 버튼용
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
            ElementType.Dark => "어둠 속성: 방어력 감소",
            ElementType.Water => "물 속성: 이동속도 감소",
            ElementType.Fire => "불 속성: 지속 데미지",
            ElementType.Earth => "대지 속성: 스턴",
            _ => ""
        };
    }

    private Sprite GetElementSprite(ElementType element)
    {
        // 리소스에서 속성 아이콘 로드
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