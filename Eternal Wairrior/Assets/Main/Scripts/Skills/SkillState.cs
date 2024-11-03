using UnityEngine;

public class SkillState
{
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Transform Parent { get; private set; }
    public SkillData SkillData { get; private set; }
    public int Level { get; private set; }

    public SkillState(Skill skill)
    {
        Position = skill.transform.position;
        Rotation = skill.transform.rotation;
        Parent = skill.transform.parent;
        SkillData = skill.GetSkillData();
        Level = skill.SkillLevel;
    }

    public void ApplyTo(Skill skill)
    {
        skill.transform.position = Position;
        skill.transform.rotation = Rotation;
        skill.transform.SetParent(Parent);
        skill.SetSkillData(SkillData);
        skill.SkillLevelUpdate(Level);
    }
}