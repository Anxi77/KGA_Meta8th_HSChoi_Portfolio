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

        yield return new WaitForEndOfFrame();

        Initialize();
    }

    private void Initialize()
    {
        availableSkills = SkillDataManager.Instance.GetAllSkillData();
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogError("Failed to load skill data!");
            return;
        }

        availableSkills = availableSkills.Where(skill =>
        {
            bool isValid = SkillDataManager.Instance.ValidateSkillData(skill);
            if (!isValid)
            {
                Debug.LogError($"Invalid skill data found for {skill.metadata?.Name ?? "Unknown Skill"}");
            }
            return isValid;
        }).ToList();

        Debug.Log($"Successfully loaded and validated {availableSkills.Count} skills");
        activeSkills = new List<Skill>();
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
            elementType == null || skill.metadata.Element == elementType).ToList();

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
            existingSkill.SkillLevelUpdate(nextLevel);
        }
        else
        {
            GameObject skillObj = Instantiate(skillData.metadata.Prefab, player.transform);
            skillObj.transform.localPosition = Vector3.zero;

            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                player.skills.Add(newSkill);
                activeSkills.Add(newSkill);
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