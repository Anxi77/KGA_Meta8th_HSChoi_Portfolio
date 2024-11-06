﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : SingletonManager<SkillManager>
{
    private List<SkillData> availableSkills = new List<SkillData>();
    private List<Skill> activeSkills = new List<Skill>();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            Debug.Log("Initializing SkillManager...");
            LoadSkillData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillManager: {e.Message}");
        }
    }

    public void ResetForNewStage()
    {
        Debug.Log("Resetting skills for new stage...");

        // 기존 스킬들 제거
        foreach (var skill in activeSkills.ToList())
        {
            RemoveSkill(skill.SkillID);
        }

        activeSkills.Clear();
    }

    private void LoadSkillData()
    {
        availableSkills = SkillDataManager.Instance.GetAllSkillData();
        Debug.Log($"Loaded {availableSkills.Count} skills from SkillDataManager");
    }

    public Skill GetPlayerSkill(SkillID skillId)
    {
        Debug.Log($"Looking for skill with ID: {skillId}");

        if (GameManager.Instance.player == null)
        {
            Debug.LogError("Player is null");
            return null;
        }

        if (GameManager.Instance.player.skills == null)
        {
            Debug.LogError("Player skills list is null");
            return null;
        }

        Debug.Log($"Player has {GameManager.Instance.player.skills.Count} skills");

        foreach (var skill in GameManager.Instance.player.skills)
        {
            Debug.Log($"Checking skill: {skill.SkillName} (ID: {skill.SkillID})");
        }

        var foundSkill = GameManager.Instance.player.skills.Find(s =>
        {
            Debug.Log($"Comparing {s.SkillID} with {skillId}");
            return s.SkillID == skillId;
        });

        Debug.Log($"Found skill: {(foundSkill != null ? foundSkill.SkillName : "null")}");
        return foundSkill;
    }

    private bool UpdateSkillStats(Skill skill, int targetLevel, out ISkillStat newStats)
    {
        Debug.Log($"Updating stats for skill {skill.SkillName} to level {targetLevel}");

        newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skill.SkillID,
            targetLevel,
            skill.GetSkillData().metadata.Type);

        if (newStats == null)
        {
            Debug.LogError($"Failed to get stats for level {targetLevel}");
            return false;
        }

        Debug.Log($"Got new stats for level {targetLevel}");
        skill.GetSkillData().SetStatsForLevel(targetLevel, newStats);

        bool result = skill.SkillLevelUpdate(targetLevel);
        Debug.Log($"SkillLevelUpdate result: {result}");

        return result;
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        if (GameManager.Instance.player == null || skillData == null) return;

        try
        {
            Debug.Log($"Adding/Upgrading skill: {skillData.metadata.Name} (ID: {skillData.metadata.ID})");

            var existingSkill = GetPlayerSkill(skillData.metadata.ID);
            Debug.Log($"Existing skill check - Found: {existingSkill != null}");

            if (existingSkill != null)
            {
                int nextLevel = existingSkill.SkillLevel + 1;
                Debug.Log($"Current level: {existingSkill.SkillLevel}, Attempting upgrade to level: {nextLevel}");

                GameObject levelPrefab = SkillDataManager.Instance.GetLevelPrefab(skillData.metadata.ID, nextLevel);
                if (levelPrefab != null)
                {
                    Debug.Log($"Found level {nextLevel} prefab, replacing skill");
                    ReplaceSkillWithNewPrefab(existingSkill, levelPrefab, skillData, nextLevel);
                }
                else
                {
                    Debug.Log($"No level {nextLevel} prefab found, updating stats");
                    if (UpdateSkillStats(existingSkill, nextLevel, out _))
                    {
                        Debug.Log($"Successfully upgraded skill to level {nextLevel}");
                    }
                }
            }
            else
            {
                GameObject prefab = SkillDataManager.Instance.GetLevelPrefab(skillData.metadata.ID, 1)
                    ?? skillData.metadata.Prefab;

                if (prefab != null)
                {
                    var tempObj = Instantiate(prefab);
                    tempObj.SetActive(false);

                    if (tempObj.TryGetComponent<Skill>(out var skillComponent))
                    {
                        skillComponent.SetSkillData(skillData);
                        skillComponent.Initialize();

                        tempObj.transform.SetParent(GameManager.Instance.player.transform);
                        tempObj.SetActive(true);
                        GameManager.Instance.player.skills.Add(skillComponent);
                        Debug.Log($"Successfully added new skill: {skillData.metadata.Name}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in AddOrUpgradeSkill: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ReplaceSkillWithNewPrefab(Skill existingSkill, GameObject newPrefab, SkillData skillData, int targetLevel)
    {
        Vector3 position = existingSkill.transform.position;
        Quaternion rotation = existingSkill.transform.rotation;
        Transform parent = existingSkill.transform.parent;

        GameManager.Instance.player.skills.Remove(existingSkill);
        Destroy(existingSkill.gameObject);

        var newObj = Instantiate(newPrefab, position, rotation, parent);
        if (newObj.TryGetComponent<Skill>(out var newSkill))
        {
            skillData.GetCurrentTypeStat().baseStat.skillLevel = targetLevel;
            newSkill.SetSkillData(skillData);
            newSkill.Initialize();
            GameManager.Instance.player.skills.Add(newSkill);
            Debug.Log($"Successfully replaced skill with level {targetLevel} prefab");
        }
    }

    private void CreateNewSkill(SkillData skillData, Transform parent)
    {
        GameObject prefab = SkillDataManager.Instance.GetLevelPrefab(skillData.metadata.ID, 1)
            ?? skillData.metadata.Prefab;

        if (prefab != null)
        {
            GameObject skillObj = Instantiate(prefab, parent);
            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                InitializeNewSkill(newSkill, skillData, 1);
            }
        }
    }

    private void InitializeNewSkill(Skill skill, SkillData skillData, int targetLevel)
    {
        if (UpdateSkillStats(skill, targetLevel, out _))
        {
            GameManager.Instance.player.skills.Add(skill);
            activeSkills.Add(skill);
            Debug.Log($"Successfully initialized skill {skillData.metadata.Name} at level {targetLevel}");
        }
    }

    public void RemoveSkill(SkillID skillID)
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;
        Skill skillToRemove = player.skills.Find(x => x.SkillID == skillID);

        if (skillToRemove != null)
        {
            player.skills.Remove(skillToRemove);
            activeSkills.Remove(skillToRemove);
            Destroy(skillToRemove.gameObject);
        }
    }

    public List<Skill> GetActiveSkills()
    {
        return activeSkills;
    }

    public Skill GetSkillByID(SkillID skillID)
    {
        return activeSkills.Find(x => x.SkillID == skillID);
    }

    public List<SkillData> GetRandomSkills(int count = 3, ElementType? elementType = null)
    {
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogError($"No skills available in SkillManager. Available skills count: {availableSkills?.Count ?? 0}");
            return new List<SkillData>();
        }

        Debug.Log($"Total available skills before filtering: {availableSkills.Count}");
        foreach (var skill in availableSkills)
        {
            Debug.Log($"Available skill: {skill.metadata.Name}, ID: {skill.metadata.ID}, Element: {skill.metadata.Element}");
        }

        var selectedSkills = new List<SkillData>();
        var filteredSkills = availableSkills.Where(skill =>
        {
            if (skill == null || skill.metadata == null)
            {
                Debug.LogError("Found null skill or metadata");
                return false;
            }

            var stats = SkillDataManager.Instance.GetSkillStats(skill.metadata.ID, 1);
            bool hasStats = stats != null;
            bool matchesElement = elementType == null || skill.metadata.Element == elementType;

            Debug.Log($"Checking skill {skill.metadata.Name}:");
            Debug.Log($"  - ID: {skill.metadata.ID}");
            Debug.Log($"  - Element: {skill.metadata.Element}");
            Debug.Log($"  - HasStats: {hasStats}");
            Debug.Log($"  - MatchesElement: {matchesElement}");
            if (!hasStats)
            {
                Debug.LogWarning($"  - No stats found for level 1");
            }

            return hasStats && matchesElement;
        }).ToList();

        if (!filteredSkills.Any())
        {
            Debug.LogWarning("No skills match the criteria");
            return selectedSkills;
        }

        Debug.Log($"Found {filteredSkills.Count} skills matching criteria");

        if (elementType == null)
        {
            var availableElements = filteredSkills
                .Select(s => s.metadata.Element)
                .Distinct()
                .ToList();

            elementType = availableElements[Random.Range(0, availableElements.Count)];
            filteredSkills = filteredSkills.Where(s => s.metadata.Element == elementType).ToList();
            Debug.Log($"Selected element type: {elementType}, remaining skills: {filteredSkills.Count}");
        }

        int possibleCount = Mathf.Min(count, filteredSkills.Count);
        Debug.Log($"Requested {count} skills, possible to select {possibleCount} skills");

        while (selectedSkills.Count < possibleCount && filteredSkills.Any())
        {
            int index = Random.Range(0, filteredSkills.Count);
            selectedSkills.Add(filteredSkills[index]);
            Debug.Log($"Selected skill: {filteredSkills[index].metadata.Name}");
            filteredSkills.RemoveAt(index);
        }

        if (selectedSkills.Count < count)
        {
            Debug.Log($"Returning {selectedSkills.Count} skills instead of requested {count} due to availability");
        }

        return selectedSkills;
    }

    public List<SkillData> GetAvailableSkillChoices(int count)
    {
        var playerSkills = GameManager.Instance.player?.skills ?? new List<Skill>();
        return GetRandomSkills(count)
            .Where(skillData => IsSkillAvailable(skillData, playerSkills))
            .ToList();
    }

    private bool IsSkillAvailable(SkillData skillData, List<Skill> playerSkills)
    {
        if (skillData?.metadata == null) return false;

        var existingSkill = playerSkills.Find(s => s.SkillID == skillData.metadata.ID);
        return existingSkill == null || existingSkill.SkillLevel < existingSkill.MaxSkillLevel;
    }
}