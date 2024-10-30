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
    private SerializedObject serializedSkill;
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
                if (GUILayout.Button(skill.Name, GUILayout.Height(30)))
                {
                    currentSkill = skill;
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region Right Panel
    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();

        if (currentSkill.Equals(default(SkillData)))
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

    private void DrawProjectileStats()
    {
        var stats = currentSkill.projectileStat;
        DrawBaseStats(stats.baseStat);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        currentSkill.projectileStat.projectileSpeed = EditorGUILayout.FloatField("Speed", stats.projectileSpeed);
        currentSkill.projectileStat.projectileScale = EditorGUILayout.FloatField("Scale", stats.projectileScale);
        currentSkill.projectileStat.shotInterval = EditorGUILayout.FloatField("Shot Interval", stats.shotInterval);
        currentSkill.projectileStat.pierceCount = EditorGUILayout.IntField("Pierce Count", stats.pierceCount);
        currentSkill.projectileStat.attackRange = EditorGUILayout.FloatField("Attack Range", stats.attackRange);
        currentSkill.projectileStat.homingRange = EditorGUILayout.FloatField("Homing Range", stats.homingRange);
        currentSkill.projectileStat.isHoming = EditorGUILayout.Toggle("Is Homing", stats.isHoming);
        currentSkill.projectileStat.explosionRad = EditorGUILayout.FloatField("Explosion Radius", stats.explosionRad);
        currentSkill.projectileStat.projectileCount = EditorGUILayout.IntField("Projectile Count", stats.projectileCount);
        currentSkill.projectileStat.innerInterval = EditorGUILayout.FloatField("Inner Interval", stats.innerInterval);

        EditorGUI.indentLevel--;
    }

    private void DrawAreaStats()
    {
        var stats = currentSkill.areaStat;
        DrawBaseStats(stats.baseStat);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        currentSkill.areaStat.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        currentSkill.areaStat.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        currentSkill.areaStat.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        currentSkill.areaStat.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        currentSkill.areaStat.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStats()
    {
        var stats = currentSkill.passiveStat;
        DrawBaseStats(stats.baseStat);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        currentSkill.passiveStat.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        currentSkill.passiveStat.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        currentSkill.passiveStat.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);

        EditorGUI.indentLevel--;
    }

    private void DrawBaseStats(BaseSkillStat baseStat)
    {
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        baseStat.damage = EditorGUILayout.FloatField("Damage", baseStat.damage);
        baseStat.skillName = EditorGUILayout.TextField("Skill Name", baseStat.skillName);
        baseStat.skillLevel = EditorGUILayout.IntField("Skill Level", baseStat.skillLevel);
        baseStat.maxSkillLevel = EditorGUILayout.IntField("Max Level", baseStat.maxSkillLevel);
        baseStat.element = (ElementType)EditorGUILayout.EnumPopup("Element", baseStat.element);
        baseStat.elementalPower = EditorGUILayout.FloatField("Element Power", baseStat.elementalPower);

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
        if (SkillDataManager.Instance != null)
        {
            skillList = SkillDataManager.Instance.GetAllSkillData();
        }
        else
        {
            skillList = new List<SkillData>();
            Debug.LogWarning("SkillDataManager instance not found!");
        }
    }

    private void SaveSkillData()
    {
        if (SkillDataManager.Instance != null)
        {
            SkillDataManager.Instance.SaveAllSkillData();
            EditorUtility.SetDirty(SkillDataManager.Instance);
            AssetDatabase.SaveAssets();
            Debug.Log("Skill data saved successfully!");
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
        Debug.Log("New skill created!");
    }

    private void ExportToJson()
    {
        string path = EditorUtility.SaveFilePanel("Export Skill Data", "", "SkillData", "json");
        if (!string.IsNullOrEmpty(path))
        {
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"Skill data exported to: {path}");
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
            Debug.Log($"Skill data imported from: {path}");
        }
    }
    #endregion
}
