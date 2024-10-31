using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : SingletonManager<SkillManager>
{
    private List<SkillData> availableSkills;
    private List<Skill> activeSkills;

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        availableSkills = SkillDataManager.Instance.GetAllSkillData();
        activeSkills = new List<Skill>();
    }

    public List<SkillData> GetRandomSkills(int count = 3)
    {
        List<SkillData> selectedSkills = new List<SkillData>();

        List<ElementType> availableElements = new List<ElementType>();
        foreach (SkillData skill in availableSkills)
        {
            ElementType skillElement = skill.GetCurrentTypeStat().baseStat.element;
            if (!availableElements.Contains(skillElement))
            {
                availableElements.Add(skillElement);
            }
        }

        if (availableElements.Count == 0) return selectedSkills;

        int randomElementIndex = Random.Range(0, availableElements.Count);
        ElementType selectedElement = availableElements[randomElementIndex];

        List<SkillData> elementalSkills = availableSkills
            .Where(skill => skill.GetCurrentTypeStat().baseStat.element == selectedElement)
            .ToList();

        int skillsToSelect = Mathf.Min(count, elementalSkills.Count);

        while (selectedSkills.Count < skillsToSelect && elementalSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, elementalSkills.Count);
            selectedSkills.Add(elementalSkills[randomIndex]);
            elementalSkills.RemoveAt(randomIndex);
        }

        return selectedSkills;
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;
        Skill existingSkill = player.skills.Find(x => x.SkillID == skillData._SkillID);

        if (existingSkill != null)
        {
            int nextLevel = existingSkill.SkillLevel + 1;
            existingSkill.SkillLevelUpdate(nextLevel);
        }
        else
        {
            GameObject skillObj = Instantiate(skillData.prefabsByLevel[0], player.transform);
            skillObj.transform.localPosition = Vector3.zero;

            if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
            {
                player.skills.Add(newSkill);
                activeSkills.Add(newSkill);
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
}