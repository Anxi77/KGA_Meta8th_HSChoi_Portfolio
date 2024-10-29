using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultGun : ProjectileSkills
{

    protected override void Start()
    {
        base.Start();
        InitializeSkillLevels();
    }

    protected override void Update()
    {
        base.Update();
    }

    public void InitializeSkillLevels()
    {
        damageByLevel = new List<float> { 10f, 20f, 30f, 40f, 50f };
        projectileSpeedByLevel = new List<float> { 25f, 35f, 45f, 50f, 60f };
        projectileCountByLevel = new List<int> { 1, 2, 3, 4, 5 };
        pierceCountByLevel = new List<int> { 1, 2, 3, 4, 5 };
        attackRangeByLevel = new List<float> { 6f, 9f, 12f, 15f, 18f };
        innerIntervalByLevel = new List<float> { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
        projectileScaleByLevel = new List<float> { 1f, 1.2f, 1.5f, 1.8f, 2.1f };
        homingRangeByLevel = new List<float> { 3.5f, 6.2f, 8.1f , 3.2f , 8.5f };
    }

}
