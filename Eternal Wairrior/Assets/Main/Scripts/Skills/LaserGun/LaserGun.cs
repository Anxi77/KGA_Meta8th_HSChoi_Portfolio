using LaserSystem2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGun : ProjectileSkills
{
    [SerializeField] private Laser _laser;
    [SerializeField] private Transform _laserAnchor;

    //protected override void Start()
    //{
    //    GameManager.Instance.gun = this;
    //}

    protected override void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _laser.Enable(_laserAnchor);
        }
        if (Input.GetMouseButtonUp(0))
        {
            _laser.Disable();
        }
    }

    public void InitializeSkillLevels()
    {
        damageByLevel = new List<float> { 10f, 20f, 30f, 40f, 50f };
    }

    public bool SkillLevelUpdate(int skillLevel)
    {
        if(skillLevel < maxSkillLevel) 
        {
            damage = damageByLevel[skillLevel];
            return true;
        }   
        return false;
    }
}
