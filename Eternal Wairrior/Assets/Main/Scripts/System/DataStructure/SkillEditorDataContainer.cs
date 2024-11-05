using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillEditorData", menuName = "SkillSystem/Editor Data Container")]
public class SkillEditorDataContainer : ScriptableObject
{
    [System.Serializable]
    public class SkillLevelStats
    {
        public SkillID skillID;
        public List<SkillStatData> levelStats = new List<SkillStatData>();
    }

    // 에디터에서 관리하는 모든 스킬 데이터
    public List<SkillData> skillList = new List<SkillData>();

    // 스킬별 레벨 스탯
    public List<SkillLevelStats> skillStats = new List<SkillLevelStats>();

    // 마지막 편집 상태 저장
    public SkillID lastSelectedSkillID;
    public Vector2 scrollPosition;
    public bool showBaseStats = true;
    public bool showLevelStats = true;
}