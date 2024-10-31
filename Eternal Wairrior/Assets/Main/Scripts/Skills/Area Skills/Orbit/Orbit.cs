using UnityEngine;

public class Orbit : AreaSkills
{
    private RotatingOrb orbs;
    private int currentOrbCount = 0;

    private const int ORBS_PER_LEVEL = 2;

    private const int BASE_ORB_COUNT = 1;

    protected override void Awake()
    {
        base.Awake();
        orbs = GetComponentInChildren<RotatingOrb>();
    }

    private void Start()
    {
        if (orbs != null)
        {
            UpdateOrbCount();
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward, MoveSpeed * Time.deltaTime);
    }

    private void UpdateOrbCount()
    {
        int newOrbCount = BASE_ORB_COUNT + (SkillLevel - 1) * ORBS_PER_LEVEL;

        if (currentOrbCount != newOrbCount)
        {
            currentOrbCount = newOrbCount;
            orbs.damage = Damage;
            orbs.InitializeOrbs(currentOrbCount);
        }
    }

    public override bool SkillLevelUpdate(int newLevel)
    {
        bool success = base.SkillLevelUpdate(newLevel);

        if (success)
        {
            UpdateOrbCount();
        }

        return success;
    }
}

