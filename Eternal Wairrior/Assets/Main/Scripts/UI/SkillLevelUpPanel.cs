using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;
    private const int SKILL_CHOICES = 3;

    public void LevelUpPanelOpen(List<Skill> playerSkills, Action<Skill> callback)
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        // 사용 가능한 모든 스킬 데이터 가져오기
        var allSkillData = SkillDataManager.Instance.GetAllSkillData();

        // 랜덤 속성 선택 (None 제외)
        var availableElements = Enum.GetValues(typeof(ElementType))
            .Cast<ElementType>()
            .Where(e => e != ElementType.None)
            .ToList();

        if (availableElements.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        ElementType selectedElement = availableElements[UnityEngine.Random.Range(0, availableElements.Count)];

        // 선택된 속성의 스킬들 필터링
        var elementalSkills = allSkillData
            .Where(skill =>
            {
                var stats = skill.GetCurrentTypeStat();
                return stats.baseStat.element == selectedElement;
            })
            .ToList();

        // 이미 보유한 스킬 제외
        elementalSkills = elementalSkills
            .Where(skillData => !playerSkills.Any(playerSkill => playerSkill.SkillID == skillData._SkillID))
            .ToList();

        if (elementalSkills.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        // 랜덤하게 3개 선택 (또는 가능한 만큼)
        int choiceCount = Mathf.Min(SKILL_CHOICES, elementalSkills.Count);
        List<SkillData> selectedSkills = new List<SkillData>();

        while (selectedSkills.Count < choiceCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, elementalSkills.Count);
            var selectedSkill = elementalSkills[randomIndex];

            if (!selectedSkills.Contains(selectedSkill))
            {
                selectedSkills.Add(selectedSkill);
                CreateSkillButton(selectedSkill, callback);
            }
        }

        // 선택된 속성 표시
        ShowElementalHeader(selectedElement);
    }

    private void CreateSkillButton(SkillData skillData, Action<Skill> callback)
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);

        skillButton.SetSkillSelectButton(skillData, () =>
        {
            // 스킬 프리팹 생성 및 초기화
            GameObject skillObj = Instantiate(skillData.prefabsByLevel[0], GameManager.Instance.player.transform);
            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                callback(newSkill);
            }
            LevelUpPanelClose();
        });
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

    private void ShowElementalHeader(ElementType element)
    {
        string elementName = element switch
        {
            ElementType.Dark => "어둠",
            ElementType.Water => "물",
            ElementType.Fire => "불",
            ElementType.Earth => "대지",
            _ => "알 수 없음"
        };

        // 헤더 UI 생성 및 설정 (UI 컴포넌트에 맞게 구현 필요)
        Debug.Log($"선택된 속성: {elementName}");
    }

    private void ShowNoSkillsAvailable()
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);
        skillButton.SetDisabledButton("사용 가능한 스킬 없음");
    }

    public void LevelUpPanelClose()
    {
        foreach (Transform button in list)
        {
            Destroy(button.gameObject);
        }
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
