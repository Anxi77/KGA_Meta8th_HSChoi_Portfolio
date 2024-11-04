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
        while (SkillDataManager.Instance == null)
        {
            yield return null;
        }

        while (!SkillDataManager.Instance.IsInitialized)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            availableSkills = SkillDataManager.Instance.GetAllSkillData();
            if (availableSkills == null || availableSkills.Count == 0)
            {
                Debug.LogError("Failed to load skill data!");
                return;
            }

            availableSkills = availableSkills.Where(skill =>
            {
                if (skill == null || skill.metadata == null)
                {
                    Debug.LogError("Null skill data found");
                    return false;
                }

                bool isValid = SkillDataManager.Instance.ValidateSkillData(skill);
                if (!isValid)
                {
                    Debug.LogError($"Invalid skill data found for {skill.metadata.Name}");
                    return false;
                }

                var stats = SkillDataManager.Instance.GetSkillStatsForLevel(skill.metadata.ID, 1, skill.metadata.Type);
                if (stats == null)
                {
                    Debug.LogError($"No level 1 stats found for {skill.metadata.Name}");
                    return false;
                }

                if (!ValidateSkillResources(skill))
                {
                    return false;
                }

                return true;
            }).ToList();

            Debug.Log($"Successfully loaded and validated {availableSkills.Count} skills");
            activeSkills = new List<Skill>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillManager: {e.Message}\n{e.StackTrace}");
        }
    }

    private bool ValidateSkillResources(SkillData skill)
    {
        if (skill.metadata.Prefab == null)
        {
            string metadataPrefabPath = $"{SkillDataManager.PrefabPath}/{skill.metadata.ID}_Metadata";
            skill.metadata.Prefab = Resources.Load<GameObject>(metadataPrefabPath);
            if (skill.metadata.Prefab == null)
            {
                Debug.LogError($"Missing metadata prefab for skill {skill.metadata.Name} at {metadataPrefabPath}");
                return false;
            }
        }

        if (skill.metadata.Type == SkillType.Projectile)
        {
            if (skill.projectile == null)
            {
                string projectilePrefabPath = $"{SkillDataManager.PrefabPath}/{skill.metadata.ID}_Projectile";
                skill.projectile = Resources.Load<GameObject>(projectilePrefabPath);
                if (skill.projectile == null)
                {
                    Debug.LogError($"Missing projectile prefab for skill {skill.metadata.Name} at {projectilePrefabPath}");
                    return false;
                }
            }
        }

        if (skill.icon == null)
        {
            string iconPath = $"{SkillDataManager.IconPath}/{skill.metadata.ID}_Icon";
            skill.icon = Resources.Load<Sprite>(iconPath);
            if (skill.icon == null)
            {
                Debug.LogWarning($"Missing icon for skill {skill.metadata.Name} at {iconPath}");
            }
        }

        return true;
    }

    public List<SkillData> GetRandomSkills(int count = 3, ElementType? elementType = null)
    {
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogError("No skills available in SkillManager");
            return new List<SkillData>();
        }

        var selectedSkills = new List<SkillData>();
        var filteredSkills = availableSkills.Where(skill =>
        {
            var stats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skill.metadata.ID,
                1,
                skill.metadata.Type
            );
            return stats != null && (elementType == null || skill.metadata.Element == elementType);
        }).ToList();

        if (!filteredSkills.Any())
            return selectedSkills;

        if (elementType == null)
        {
            var availableElements = filteredSkills
                .Select(s => s.metadata.Element)
                .Distinct()
                .ToList();

            elementType = availableElements[Random.Range(0, availableElements.Count)];
            filteredSkills = filteredSkills.Where(s => s.metadata.Element == elementType).ToList();
        }

        while (selectedSkills.Count < count && filteredSkills.Any())
        {
            int index = Random.Range(0, filteredSkills.Count);
            selectedSkills.Add(filteredSkills[index]);
            filteredSkills.RemoveAt(index);
        }

        return selectedSkills;
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;
        Skill existingSkill = player.skills.Find(x => x.SkillID == skillData.metadata.ID);

        if (existingSkill != null)
        {
            int nextLevel = existingSkill.SkillLevel + 1;
            var nextLevelStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID,
                nextLevel,
                skillData.metadata.Type
            );

            if (nextLevelStats != null)
            {
                existingSkill.SkillLevelUpdate(nextLevel);
                Debug.Log($"Upgraded skill {skillData.metadata.Name} to level {nextLevel}");
            }
            else
            {
                Debug.LogWarning($"No stats found for {skillData.metadata.Name} level {nextLevel}");
            }
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
                skillObj.transform.localPosition = Vector3.zero;

                if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
                {
                    player.skills.Add(newSkill);
                    activeSkills.Add(newSkill);
                    Debug.Log($"Added new skill {skillData.metadata.Name}");
                }
            }
            else
            {
                Debug.LogError($"Failed to add skill {skillData.metadata.Name}: No level 1 stats found");
            }
        }
        UIManager.instance.skillList.skillListUpdate();
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
}