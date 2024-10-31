using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;
    private const int SKILL_CHOICES = 3;

    public void LevelUpPanelOpen(List<Skill> playerSkills, Action<Skill> callback)
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        // Get random skills using SkillManager
        var selectedSkills = SkillManager.Instance.GetRandomSkills(SKILL_CHOICES);

        if (selectedSkills.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        // Create buttons for selected skills
        foreach (var skillData in selectedSkills)
        {
            CreateSkillButton(skillData, callback);
        }

        // Show header for the element type
        if (selectedSkills.Count > 0)
        {
            ShowElementalHeader(selectedSkills[0].metadata.Element);
        }
    }

    private void CreateSkillButton(SkillData skillData, Action<Skill> callback)
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);
        skillButton.SetSkillSelectButton(skillData, () =>
        {
            GameObject skillObj = Instantiate(skillData.metadata.Prefab, GameManager.Instance.player.transform);
            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                callback(newSkill);
            }
            LevelUpPanelClose();
        });
    }

    private void ShowElementalHeader(ElementType element)
    {
        string elementName = element switch
        {
            ElementType.Dark => "Dark",
            ElementType.Water => "Water",
            ElementType.Fire => "Fire",
            ElementType.Earth => "Earth",
            _ => "Unknown"
        };

        Debug.Log($"Selected Element: {elementName}");
    }

    private void ShowNoSkillsAvailable()
    {
        SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);
        skillButton.SetDisabledButton("No skills available");
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
