using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SkillDataManager : DataManager
{
    private static SkillDataManager instance;
    public static new SkillDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SkillDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SkillDataManager");
                    instance = go.AddComponent<SkillDataManager>();
                }
            }
            return instance;
        }
    }

    private List<SkillData> skillDatas = new List<SkillData>();
    private const string SKILL_DATA_KEY = "SkillData";
    private const string SKILL_DATA_FILENAME = "SkillData.json";

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        LoadAllSkillData();
    }

    public List<SkillData> GetAllSkillData()
    {
        return new List<SkillData>(skillDatas);
    }

    public void SaveAllSkillData()
    {
        SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillDatas };
        SaveData(SKILL_DATA_FILENAME, wrapper);
    }

    public void LoadAllSkillData()
    {
        SkillDataWrapper wrapper = LoadData<SkillDataWrapper>(SKILL_DATA_FILENAME);
        if (wrapper != null)
        {
            skillDatas = wrapper.skillDatas ?? new List<SkillData>();
        }
        else
        {
            skillDatas = new List<SkillData>();
            CreateDefaultSkillData();
        }
    }

    private void CreateDefaultSkillData()
    {
        // 기본 스킬 데이터 생성 로직
        // 필요한 경우 구현
    }

    public void UpdateSkillData(SkillData updatedSkill)
    {
        int index = skillDatas.FindIndex(x => x._SkillID == updatedSkill._SkillID);
        if (index != -1)
        {
            skillDatas[index] = updatedSkill;
            SaveAllSkillData();
        }
    }

    public SkillData GetSkillData(SkillID skillID)
    {
        return skillDatas.Find(x => x._SkillID == skillID);
    }
}