using System.Collections.Generic;
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
        List<SkillData> tempSkills = new List<SkillData>(availableSkills);

        while (selectedSkills.Count < count && tempSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, tempSkills.Count);
            selectedSkills.Add(tempSkills[randomIndex]);
            tempSkills.RemoveAt(randomIndex);
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