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

        public List<SkillStatData> GetSkillStatDataList()
        {
            return levelStats;
        }
    }

    // 데이터베이스에서 관리하는 모든 스킬 데이터
    public List<SkillData> skillList = new List<SkillData>();

    // 스킬의 레벨 정보
    public List<SkillLevelStats> skillStats = new List<SkillLevelStats>();

    // 에디터 상태 저장
    public SkillID lastSelectedSkillID;
    public Vector2 scrollPosition;
    public bool showBaseStats = true;
    public bool showLevelStats = true;

    // OnEnable에서 데이터 복원
    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    // OnDisable에서 데이터 저장
    private void OnDisable()
    {
        UnityEditor.EditorUtility.SetDirty(this);
    }
}