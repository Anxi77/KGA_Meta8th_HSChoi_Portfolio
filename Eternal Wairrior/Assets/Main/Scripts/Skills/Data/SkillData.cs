using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct SkillData
{
    public SkillID _SkillID;
    public SkillType _SkillType;
    public string Name;
    public string Description;
    public int id;
    public GameObject[] prefabsByLevel;
    public GameObject projectile;
    public Image icon;

    // 타입별 스탯
    public ProjectileSkillStat projectileStat;
    public AreaSkillStat areaStat;
    public PassiveSkillStat passiveStat;

    // 현재 스킬 타입에 맞는 스탯 반환
    public ISkillStat GetCurrentTypeStat()
    {
        return _SkillType switch
        {
            SkillType.Projectile => projectileStat,
            SkillType.Area => areaStat,
            SkillType.Passive => passiveStat,
            _ => throw new System.ArgumentException("Invalid skill type")
        };
    }
}

[System.Serializable]
public class SkillDataWrapper
{
    public List<SkillData> skillDatas;
}