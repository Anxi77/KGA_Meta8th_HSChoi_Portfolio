using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class SkillData
{
    public SkillID _SkillID;
    public SkillType _SkillType;
    public string Name;
    public string Description;
    public GameObject[] prefabsByLevel;
    public GameObject projectile;
    public Image icon;
    public ProjectileSkillStat projectileStat;
    public AreaSkillStat areaStat;
    public PassiveSkillStat passiveStat;

    public SkillData()
    {
        projectileStat = new ProjectileSkillStat();
        areaStat = new AreaSkillStat();
        passiveStat = new PassiveSkillStat();
    }

    public ISkillStat GetCurrentTypeStat()
    {
        switch (_SkillType)
        {
            case SkillType.Projectile:
                return projectileStat;
            case SkillType.Area:
                return areaStat;
            case SkillType.Passive:
                return passiveStat;
            default:
                throw new System.ArgumentException("Invalid skill type");
        }
    }
}

[System.Serializable]
public class SkillDataWrapper
{
    public List<SkillData> skillDatas;
}
