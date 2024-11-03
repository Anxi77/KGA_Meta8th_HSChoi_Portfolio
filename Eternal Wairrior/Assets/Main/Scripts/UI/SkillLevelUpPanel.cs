using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;
    public SkillMaxLevelButton maxLevelButtonPrefab;
    private const int SKILL_CHOICES = 3;

    [SerializeField] private GameObject errorPopup;
    [SerializeField] private TextMeshProUGUI errorText;

    public void LevelUpPanelOpen(List<Skill> playerSkills, Action<Skill> callback)
    {
        ClearExistingButtons();
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        var selectedSkills = SkillManager.Instance.GetRandomSkills(SKILL_CHOICES);

        // Filter out max level skills
        selectedSkills = selectedSkills.Where(skillData => {
            var existingSkill = playerSkills.Find(s => s.SkillID == skillData.metadata.ID);
            return existingSkill == null || existingSkill.SkillLevel < existingSkill.MaxSkillLevel;
        }).ToList();

        if (selectedSkills == null || selectedSkills.Count == 0)
        {
            ShowNoSkillsAvailable();
            return;
        }

        foreach (var skillData in selectedSkills)
        {
            if (ValidateSkillData(skillData))
            {
                CreateSkillButton(skillData, callback);
            }
        }

        if (selectedSkills.Count > 0)
        {
            ShowElementalHeader(selectedSkills[0].metadata.Element);
        }
    }

    private void ClearExistingButtons()
    {
        foreach (Transform child in list)
        {
            Destroy(child.gameObject);
        }
    }

    private bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null || skillData.metadata == null)
        {
            Debug.LogError("Invalid skill data");
            return false;
        }

        if (skillData.metadata.Prefab == null)
        {
            Debug.LogError($"Missing prefab for skill: {skillData.metadata.Name}");
            return false;
        }

        return true;
    }

    private void CreateSkillButton(SkillData skillData, Action<Skill> callback)
    {
        try
        {
            var existingSkill = GameManager.Instance.player.skills
                .Find(s => s.SkillID == skillData.metadata.ID);

            SkillLevelUpButton skillButton = Instantiate(buttonPrefab, list);

            if (existingSkill != null)
            {
                string levelInfo = $"Lv.{existingSkill.SkillLevel} → {existingSkill.SkillLevel + 1}";
                var currentStats = existingSkill.GetSkillData().GetCurrentTypeStat();

                skillButton.SetSkillSelectButton(
                    skillData,
                    () =>
                    {
                        if (!existingSkill.SkillLevelUpdate(existingSkill.SkillLevel + 1))
                        {
                            Debug.LogError($"Failed to upgrade skill: {skillData.metadata.Name}");
                            return;
                        }

                        UpdateSkillPrefab(existingSkill, skillData);
                        callback?.Invoke(existingSkill);
                        LevelUpPanelClose();
                    },
                    levelInfo,
                    currentStats
                );
            }
            else
            {
                skillButton.SetSkillSelectButton(
                    skillData,
                    () =>
                    {
                        if (!GameManager.Instance.player.AddOrUpgradeSkill(skillData))
                        {
                            Debug.LogError($"Failed to add new skill: {skillData.metadata.Name}");
                            return;
                        }
                        LevelUpPanelClose();
                    },
                    "New!"
                );
            }
        }
        catch (System.Exception e)
        {
            ShowError($"Failed to create skill button: {e.Message}");
        }
    }

    private void UpdateSkillPrefab(Skill skill, SkillData skillData)
    {
        int newLevel = skill.SkillLevel;

        try
        {
            // 기존 스킬의 상태 백업
            var oldSkillState = new SkillState(skill);

            // 새 스킬 생성 및 설정
            GameObject newSkillObj = CreateNewSkill(skill, skillData, newLevel);
            if (newSkillObj == null) return;

            // 플레이어 스킬 리스트 업데이트
            UpdatePlayerSkillList(skill, newSkillObj.GetComponent<Skill>());

            // 이전 스킬 제거
            Destroy(skill.gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update skill prefab: {e.Message}");
            ShowError("Failed to upgrade skill. Please try again.");
        }
    }

    private GameObject CreateNewSkill(Skill oldSkill, SkillData skillData, int newLevel)
    {
        if (skillData.prefabsByLevel == null ||
            newLevel > skillData.prefabsByLevel.Length ||
            skillData.prefabsByLevel[newLevel - 1] == null)
        {
            Debug.Log($"No level-specific prefab found. Updating stats only.");
            oldSkill.SkillLevelUpdate(newLevel);
            return null;
        }

        GameObject newPrefab = skillData.prefabsByLevel[newLevel - 1];
        GameObject newSkillObj = Instantiate(newPrefab,
            oldSkill.transform.position,
            oldSkill.transform.rotation,
            oldSkill.transform.parent);

        Skill newSkillComponent = newSkillObj.GetComponent<Skill>();
        if (newSkillComponent == null)
        {
            Debug.LogError("New prefab missing Skill component!");
            Destroy(newSkillObj);
            return null;
        }

        // 새 스킬 초기화
        InitializeNewSkill(newSkillComponent, skillData, newLevel);

        return newSkillObj;
    }

    private void InitializeNewSkill(Skill newSkill, SkillData skillData, int newLevel)
    {
        var newSkillData = new SkillData
        {
            metadata = skillData.metadata,
            prefabsByLevel = skillData.prefabsByLevel,
            icon = skillData.icon
        };

        newSkill.SetSkillData(newSkillData);
        newSkill.SkillLevelUpdate(newLevel);
    }

    private void ShowElementalHeader(ElementType element)
    {
        string elementName = element.ToString();
        Debug.Log($"Selected Element: {elementName}");
    }

    private void ShowNoSkillsAvailable()
    {
        ClearExistingButtons();

        if (maxLevelButtonPrefab != null)
        {
            SkillMaxLevelButton maxLevelButton = Instantiate(maxLevelButtonPrefab, list);
            maxLevelButton.Initialize(() => LevelUpPanelClose());
        }
        else
        {
            Debug.LogError("Max Level Button Prefab is not assigned!");
        }
    }

    public void LevelUpPanelClose()
    {
        ClearExistingButtons();
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void ShowError(string message)
    {
        if (errorPopup != null)
        {
            errorPopup.SetActive(true);
            if (errorText != null)
            {
                errorText.text = message;
            }
            StartCoroutine(HideErrorAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    private IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorPopup != null)
        {
            errorPopup.SetActive(false);
        }
    }

    private void UpdatePlayerSkillList(Skill oldSkill, Skill newSkill)
    {
        try
        {
            var player = GameManager.Instance.player;
            if (player != null)
            {
                int index = player.skills.IndexOf(oldSkill);
                if (index != -1)
                {
                    player.skills[index] = newSkill;
                    Debug.Log($"Successfully updated player skill list: {oldSkill.SkillName} -> {newSkill.SkillName}");
                }
                else
                {
                    Debug.LogError($"Could not find skill {oldSkill.SkillName} in player's skill list");
                }
                UIManager.Instance.skillList.skillListUpdate();
            }
            else
            {
                Debug.LogError("Player reference is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating player skill list: {e.Message}");
            ShowError("Failed to update skill list");
        }
    }
}
