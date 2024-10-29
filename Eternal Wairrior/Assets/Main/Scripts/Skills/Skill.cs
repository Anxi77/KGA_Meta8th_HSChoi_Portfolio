using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Skill:MonoBehaviour
{
    public enum SkillType 
    {
        Projectile,
        Area,
        Passive
    }

    public enum SkillID 
    {
        DefaultGun = 100000,
        LaserGun,
        MissileLauncher,
        MultiShot,
        Bind,
        GuardianOrb
    }

    [Header("Common Skill Attributes")]
    public SkillID _SkillID;
    public SkillType _SkillType;
    public Image skillImage;
    public GameObject currentSkillObject;
    public float damage;
    public string skillName;
    public int skillLevel;
    public int maxSkillLevel;
    public GameObject[] skillPrefabs;
    [Header("Projectile Skill Atrributes")]
    public float projectileSpeed;
    public float projectileScale;
    public float shotInterval;
    public int pierceCount;
    public float attackRange;
    public float homingRange;
    public bool isHoming;
    public float explosionRad;
    public Vector2 fireDir;
    public GameObject projectile;
    [Tooltip("number of bullets fires in innerinterval")]
    public int projectileCount;
    public float innerInterval;

    public Dictionary<string, List<float>> statsByLevel;
    public List<float> damageByLevel;
    public List<float> projectileSpeedByLevel;
    public List<float> projectileScaleByLevel;
    public List<int> projectileCountByLevel;
    public List<int> pierceCountByLevel;
    public List<float> attackRangeByLevel;
    public List<float> innerIntervalByLevel;
    public List<float> homingRangeByLevel;
    public List<float> explosionRadByLevel;

    public virtual bool SkillLevelUpdate(int skillLevel,Skill skill)
    {
        switch (skill._SkillType) 
        {
            case SkillType.Projectile:
                if (skill.TryGetComponent<ProjectileSkills>(out ProjectileSkills projSkill))
                {
                    projSkill.ProjectileSkillLevelUpdate(skillLevel);
                    return true;
                }
                break;
        }
        return false;
    }


}
