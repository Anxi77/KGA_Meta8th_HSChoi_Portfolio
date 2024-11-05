using System.Collections;
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
        StartCoroutine(InitializeAfterSkillDataManager());
    }

    private IEnumerator InitializeAfterSkillDataManager()
    {
        while (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            Debug.Log("Initializing SkillManager...");

            availableSkills = SkillDataManager.Instance.GetAllSkillData();

            if (availableSkills == null || availableSkills.Count == 0)
            {
                Debug.LogWarning("No skills loaded from SkillDataManager");
                availableSkills = new List<SkillData>();
            }
            else
            {
                Debug.Log($"Loaded {availableSkills.Count} skills from SkillDataManager");
                foreach (var skill in availableSkills)
                {
                    Debug.Log($"Loaded skill: {skill.metadata.Name} (ID: {skill.metadata.ID})");
                }
            }

            activeSkills = new List<Skill>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillManager: {e.Message}\n{e.StackTrace}");
        }
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;
        Skill existingSkill = player.skills.Find(x => x.SkillID == skillData.metadata.ID);

        if (existingSkill != null)
        {
            int nextLevel = existingSkill.SkillLevel + 1;
            SkillDataManager.Instance.UpdateSkillLevel(skillData.metadata.ID, nextLevel);
            existingSkill.SkillLevelUpdate(nextLevel);
        }
        else
        {
            var level1Stats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID,
                1,
                skillData.metadata.Type
            );

            if (level1Stats != null)
            {
                GameObject skillObj = Instantiate(skillData.metadata.Prefab, player.transform);
                if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
                {
                    player.skills.Add(newSkill);
                    activeSkills.Add(newSkill);
                }
            }
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
}