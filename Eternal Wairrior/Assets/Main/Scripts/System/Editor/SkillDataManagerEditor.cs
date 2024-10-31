using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

public class SkillDataManagerEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private SkillData currentSkill;
    private List<SkillData> skillList;
    private bool showBaseStats = true;
    private GUIStyle headerStyle;

    [MenuItem("Tools/Skill Data Manager")]
    public static void ShowWindow()
    {
        GetWindow<SkillDataManagerEditor>("Skill Manager");
    }

    private void OnEnable()
    {
        InitializeStyles();
        LoadSkillData();
    }

    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(5, 5, 10, 10)
        };
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLeftPanel();
        DrawRightPanel();
        EditorGUILayout.EndHorizontal();
        DrawBottomPanel();
    }

    #region Left Panel
    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("Skills", headerStyle);
        DrawSkillList();
        EditorGUILayout.EndVertical();
    }

    private void DrawSkillList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (skillList != null)
        {
            foreach (var skill in skillList)
            {
                GUI.backgroundColor = currentSkill == skill ? Color.cyan : Color.white;
                if (GUILayout.Button(skill.Name, GUILayout.Height(30)))
                {
                    currentSkill = skill;
                }
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region Right Panel
    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();

        if (currentSkill == null)
        {
            EditorGUILayout.LabelField("Select a skill to edit", headerStyle);
            EditorGUILayout.EndVertical();
            return;
        }

        DrawBasicInfo();
        DrawPrefabSettings();
        DrawSkillStats();

        EditorGUILayout.EndVertical();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Basic Information", headerStyle);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();
        currentSkill.Name = EditorGUILayout.TextField("Name", currentSkill.Name);
        currentSkill.Description = EditorGUILayout.TextField("Description", currentSkill.Description);
        currentSkill._SkillID = (SkillID)EditorGUILayout.EnumPopup("Skill ID", currentSkill._SkillID);
        currentSkill._SkillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", currentSkill._SkillType);

        EditorGUI.indentLevel--;
    }

    private void DrawPrefabSettings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Settings", headerStyle);
        EditorGUI.indentLevel++;

        currentSkill.icon = (Image)EditorGUILayout.ObjectField("Skill Icon", currentSkill.icon, typeof(Image), false);
        currentSkill.projectile = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", currentSkill.projectile, typeof(GameObject), false);

        showBaseStats = EditorGUILayout.Foldout(showBaseStats, "Level Prefabs");
        if (showBaseStats)
        {
            if (currentSkill.prefabsByLevel == null)
                currentSkill.prefabsByLevel = new GameObject[0];

            EditorGUI.indentLevel++;
            int size = EditorGUILayout.IntField("Size", currentSkill.prefabsByLevel.Length);
            if (size != currentSkill.prefabsByLevel.Length)
            {
                System.Array.Resize(ref currentSkill.prefabsByLevel, size);
            }

            for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
            {
                currentSkill.prefabsByLevel[i] = (GameObject)EditorGUILayout.ObjectField(
                    $"Level {i + 1}", currentSkill.prefabsByLevel[i], typeof(GameObject), false);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }

    private void DrawSkillStats()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Skill Stats", headerStyle);
        EditorGUI.indentLevel++;

        // 기본 스탯 그리기
        DrawBaseStats();

        // 스킬 타입별 스탯 그리기
        switch (currentSkill._SkillType)
        {
            case SkillType.Projectile:
                DrawProjectileStats();
                break;
            case SkillType.Area:
                DrawAreaStats();
                break;
            case SkillType.Passive:
                DrawPassiveStats();
                break;
        }

        EditorGUI.indentLevel--;
    }

    private void DrawBaseStats()
    {
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var baseStat = currentSkill.GetCurrentTypeStat().baseStat;
        baseStat.damage = EditorGUILayout.FloatField("Damage", baseStat.damage);
        baseStat.skillName = EditorGUILayout.TextField("Skill Name", baseStat.skillName);
        baseStat.skillLevel = EditorGUILayout.IntField("Skill Level", baseStat.skillLevel);
        baseStat.maxSkillLevel = EditorGUILayout.IntField("Max Level", baseStat.maxSkillLevel);
        baseStat.element = (ElementType)EditorGUILayout.EnumPopup("Element", baseStat.element);
        baseStat.elementalPower = EditorGUILayout.FloatField("Elemental Power", baseStat.elementalPower);

        EditorGUI.indentLevel--;
    }

    private void DrawProjectileStats()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var stats = currentSkill.projectileStat;
        stats.projectileSpeed = EditorGUILayout.FloatField("Speed", stats.projectileSpeed);
        stats.projectileScale = EditorGUILayout.FloatField("Scale", stats.projectileScale);
        stats.shotInterval = EditorGUILayout.FloatField("Shot Interval", stats.shotInterval);
        stats.pierceCount = EditorGUILayout.IntField("Pierce Count", stats.pierceCount);
        stats.attackRange = EditorGUILayout.FloatField("Attack Range", stats.attackRange);
        stats.homingRange = EditorGUILayout.FloatField("Homing Range", stats.homingRange);
        stats.isHoming = EditorGUILayout.Toggle("Is Homing", stats.isHoming);
        stats.explosionRad = EditorGUILayout.FloatField("Explosion Radius", stats.explosionRad);
        stats.projectileCount = EditorGUILayout.IntField("Projectile Count", stats.projectileCount);
        stats.innerInterval = EditorGUILayout.FloatField("Inner Interval", stats.innerInterval);

        EditorGUI.indentLevel--;
    }

    private void DrawAreaStats()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var stats = currentSkill.areaStat;
        stats.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        stats.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        stats.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        stats.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStats()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var stats = currentSkill.passiveStat;
        stats.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        stats.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        stats.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);

        EditorGUI.indentLevel--;
    }
    #endregion

    #region Bottom Panel
    private void DrawBottomPanel()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("New Skill", GUILayout.Height(30)))
            CreateNewSkill();

        if (GUILayout.Button("Save All", GUILayout.Height(30)))
            SaveSkillData();

        if (GUILayout.Button("Export JSON", GUILayout.Height(30)))
            ExportToJson();

        if (GUILayout.Button("Import JSON", GUILayout.Height(30)))
            ImportFromJson();

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Data Management
    private void LoadSkillData()
    {
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            skillList = skillDataManager.GetAllSkillData();
            if (skillList == null)
            {
                skillList = new List<SkillData>();
            }
            Debug.Log($"Loaded {skillList.Count} skills");
        }
        else
        {
            skillList = new List<SkillData>();
            Debug.LogError("SkillDataManager를 찾을 수 없습니다!");
        }
    }

    private void SaveSkillData()
    {
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            skillDataManager.SaveAllSkillData();
            EditorUtility.SetDirty(skillDataManager);
            AssetDatabase.SaveAssets();
            Debug.Log("스킬 데이터가 저장되었습니다!");
        }
        else
        {
            Debug.LogError("SkillDataManager를 찾을 수 없습니다!");
        }
    }

    private void CreateNewSkill()
    {
        SkillData newSkill = new SkillData
        {
            Name = "New Skill",
            _SkillType = SkillType.Projectile,
            _SkillID = SkillID.DefaultGun,
            prefabsByLevel = new GameObject[1]
        };

        skillList.Add(newSkill);
        currentSkill = newSkill;
        Debug.Log("새로운 스킬이 생성되었습니다!");
    }

    private void ExportToJson()
    {
        string path = EditorUtility.SaveFilePanel("Export Skill Data", "", "SkillData", "json");
        if (!string.IsNullOrEmpty(path))
        {
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"스킬 데이터가 다음 경로로 내보내졌습니다: {path}");
        }
    }

    private void ImportFromJson()
    {
        string path = EditorUtility.OpenFilePanel("Import Skill Data", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = System.IO.File.ReadAllText(path);
            SkillDataWrapper wrapper = JsonUtility.FromJson<SkillDataWrapper>(json);
            skillList = wrapper.skillDatas;
            Debug.Log($"스킬 데이터를 다음 경로에서 불러왔습니다: {path}");
        }
    }
    #endregion
}