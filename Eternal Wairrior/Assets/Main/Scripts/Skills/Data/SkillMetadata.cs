using UnityEngine;

[System.Serializable]
public class SkillMetadata
{
    public SkillID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillType Type { get; set; }
    public ElementType Element { get; set; }
    public int Tier { get; set; }  // 스킬 등급
    public string[] Tags { get; set; }  // 스킬 태그 (예: "공격", "방어", "지원" 등)
    public GameObject Prefab { get; set; }
    public Sprite Icon { get; set; }
}