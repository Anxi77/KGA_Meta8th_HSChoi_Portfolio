using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;

public class SkillDataEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private SkillData currentSkill;
    private List<SkillData> skillList;
    private Dictionary<SkillID, List<SkillStatData>> skillStatsList;
    private bool showBaseStats = true;
    private bool showLevelStats = true;
    private GUIStyle headerStyle;
    private Vector2 statsScrollPosition;
    private const string RESOURCE_PATH = "SkillData";

    [MenuItem("Tools/Skill Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillDataEditor>("Skill Data Editor");
    }

    [MenuItem("Tools/Clear Skill Data")]
    public static void ClearSkillData()
    {
        if (EditorUtility.DisplayDialog("Clear Skill Data",
            "Are you sure you want to clear all skill data? This cannot be undone.",
            "Clear", "Cancel"))
        {
            var window = GetWindow<SkillDataEditor>();
            window.ClearAllData();
            window.LoadAllData(); // Reload empty data structures
        }
    }

    private void OnEnable()
    {
        InitializeStyles();
        LoadAllData();
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

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skills", headerStyle);
        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            if (currentSkill != null && EditorUtility.DisplayDialog("Delete Skill",
                $"Are you sure you want to delete '{currentSkill.metadata.Name}'?", "Delete", "Cancel"))
            {
                DeleteCurrentSkill();
            }
        }
        EditorGUILayout.EndHorizontal();

        DrawSkillList();
        EditorGUILayout.EndVertical();
    }

    private void DrawSkillList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (skillList != null)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                var skill = skillList[i];
                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = currentSkill == skill ? Color.cyan : Color.white;
                if (GUILayout.Button(skill.metadata.Name, GUILayout.Height(30)))
                {
                    currentSkill = skill;

                    // Ŭ ޴ ó
                    if (Event.current.button == 1) // Ŭ
                    {
                        ShowSkillContextMenu(skill);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
    }

    // ų Ʈ޴ ǥ
    private void ShowSkillContextMenu(SkillData skill)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete"), false, () => {
            if (EditorUtility.DisplayDialog("Delete Skill",
                $"Are you sure you want to delete '{skill.metadata.Name}'?", "Delete", "Cancel"))
            {
                DeleteSkill(skill);
            }
        });
        menu.ShowAsContext();
    }

    // õ  ų
    private void DeleteCurrentSkill()
    {
        if (currentSkill != null)
        {
            DeleteSkill(currentSkill);
            currentSkill = null;
        }
    }

    // ų 
    private void DeleteSkill(SkillData skill)
    {
        skillList.Remove(skill);

        // 스킬 스탯 제거 (metadata.ID 사용)
        if (skillStatsList.ContainsKey(skill.metadata.ID))
        {
            skillStatsList.Remove(skill.metadata.ID);
        }

        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            EditorUtility.SetDirty(skillDataManager);
        }

        SaveSkillData();
        Debug.Log($"Skill '{skill.metadata.Name}' has been deleted.");
    }
    #endregion


    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();

        if (currentSkill == null)
        {
            EditorGUILayout.LabelField("Select a skill to edit", headerStyle);
            EditorGUILayout.EndVertical();
            return;
        }

        statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);

        EditorGUI.BeginChangeCheck();

        DrawBasicInfo();
        DrawPrefabSettings();
        DrawSkillStats();
        DrawLevelStats();

        if (EditorGUI.EndChangeCheck())
        {
            // SkillDataManager ãƼ dirty ǥ
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager != null)
            {
                EditorUtility.SetDirty(skillDataManager);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Basic Information", headerStyle);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        // 메타데이터 편집
        currentSkill.metadata.Name = EditorGUILayout.TextField("Name", currentSkill.metadata.Name);
        currentSkill.metadata.Description = EditorGUILayout.TextField("Description", currentSkill.metadata.Description);

        SkillType newSkillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", currentSkill.metadata.Type);
        if (newSkillType != currentSkill.metadata.Type)
        {
            currentSkill.metadata.Type = newSkillType;
            InitializeSkillStats(currentSkill);
        }

        currentSkill.metadata.ID = (SkillID)EditorGUILayout.EnumPopup("Skill ID", currentSkill.metadata.ID);
        currentSkill.metadata.Element = (ElementType)EditorGUILayout.EnumPopup("Element Type", currentSkill.metadata.Element);
        currentSkill.metadata.Tier = EditorGUILayout.IntField("Tier", currentSkill.metadata.Tier);

        // 태그 편집
        if (currentSkill.metadata.Tags == null)
            currentSkill.metadata.Tags = new string[0];

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tags");
        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            string[] newTags = new string[currentSkill.metadata.Tags.Length + 1];
            currentSkill.metadata.Tags.CopyTo(newTags, 0);
            newTags[newTags.Length - 1] = "";
            currentSkill.metadata.Tags = newTags;
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < currentSkill.metadata.Tags.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            currentSkill.metadata.Tags[i] = EditorGUILayout.TextField(currentSkill.metadata.Tags[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                string[] newTags = new string[currentSkill.metadata.Tags.Length - 1];
                System.Array.Copy(currentSkill.metadata.Tags, 0, newTags, 0, i);
                System.Array.Copy(currentSkill.metadata.Tags, i + 1, newTags, i, currentSkill.metadata.Tags.Length - i - 1);
                currentSkill.metadata.Tags = newTags;
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

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
        if (currentSkill == null) return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Skill Stats", headerStyle);
        EditorGUI.indentLevel++;

        try
        {
            if (currentSkill.metadata.Type == SkillType.None)
            {
                EditorGUILayout.HelpBox("Please select a skill type first", MessageType.Info);
                return;
            }

            // 기본 스탯
            DrawBaseStats();

            // 스킬 타입별 스탯
            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    DrawProjectileStats(currentSkill.GetStatsForLevel(1) as ProjectileSkillStat);
                    break;
                case SkillType.Area:
                    DrawAreaStats(currentSkill.GetStatsForLevel(1) as AreaSkillStat);
                    break;
                case SkillType.Passive:
                    DrawPassiveStats(currentSkill.GetStatsForLevel(1) as PassiveSkillStat);
                    break;
            }
        }
        finally
        {
            EditorGUI.indentLevel--;
        }
    }

    private void DrawBaseStats()
    {
        if (currentSkill == null) return;

        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        try
        {
            var baseStat = currentSkill.GetCurrentTypeStat()?.baseStat;
            if (baseStat == null)
            {
                EditorGUILayout.HelpBox("Base stat is not initialized", MessageType.Warning);
                return;
            }

            baseStat.damage = EditorGUILayout.FloatField("Damage", baseStat.damage);
            baseStat.skillName = EditorGUILayout.TextField("Skill Name", baseStat.skillName);
            baseStat.skillLevel = EditorGUILayout.IntField("Skill Level", baseStat.skillLevel);
            baseStat.maxSkillLevel = EditorGUILayout.IntField("Max Level", baseStat.maxSkillLevel);
            baseStat.element = (ElementType)EditorGUILayout.EnumPopup("Element", baseStat.element);
            baseStat.elementalPower = EditorGUILayout.FloatField("Elemental Power", baseStat.elementalPower);
        }
        finally
        {
            EditorGUI.indentLevel--;
        }
    }

    private void DrawProjectileStats(ProjectileSkillStat stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

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

    private void DrawAreaStats(AreaSkillStat stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        stats.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        stats.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        stats.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        stats.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStats(PassiveSkillStat stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        stats.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        stats.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        stats.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);

        EditorGUI.indentLevel--;
    }

    private void DrawLevelStats()
    {
        EditorGUILayout.Space(10);
        showLevelStats = EditorGUILayout.Foldout(showLevelStats, "Level Stats");

        if (!showLevelStats) return;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Level", GUILayout.Width(100)))
        {
            AddNewLevelStat();
        }
        EditorGUILayout.EndHorizontal();

        // 레벨별 스탯 표시
        for (int level = 1; level <= currentSkill.GetMaxLevel(); level++)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Level {level}", EditorStyles.boldLabel);
            var stats = currentSkill.GetStatsForLevel(level);

            DrawStatsForType(stats, currentSkill.metadata.Type);

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawStatsForType(ISkillStat stats, SkillType skillType)
    {
        // 기본 스탯 표시
        var baseStat = stats.baseStat;
        baseStat.damage = EditorGUILayout.FloatField("Damage", baseStat.damage);
        baseStat.elementalPower = EditorGUILayout.FloatField("Elemental Power", baseStat.elementalPower);

        // 스킬 타입별 특수 스탯 표시
        switch (skillType)
        {
            case SkillType.Projectile:
                DrawProjectileStats(stats as ProjectileSkillStat);
                break;
            case SkillType.Area:
                DrawAreaStats(stats as AreaSkillStat);
                break;
            case SkillType.Passive:
                DrawPassiveStats(stats as PassiveSkillStat);
                break;
        }
    }

    private void AddNewLevelStat()
    {
        var levelStats = skillStatsList[currentSkill.metadata.ID];
        var newStat = new SkillStatData
        {
            skillID = currentSkill.metadata.ID,
            level = levelStats.Count + 1
        };

        // 이전 레벨의 스탯을 기반으로 새 스탯 생성
        if (levelStats.Count > 0)
        {
            var prevStat = levelStats[levelStats.Count - 1];
            newStat.damage = prevStat.damage * 1.1f; // 10% 증가
            newStat.maxSkillLevel = prevStat.maxSkillLevel;
            newStat.element = currentSkill.metadata.Element;
            newStat.elementalPower = prevStat.elementalPower * 1.1f;

            // 스킬 타입별 스탯 복사
            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    CopyProjectileStats(prevStat, newStat);
                    break;
                case SkillType.Area:
                    CopyAreaStats(prevStat, newStat);
                    break;
                case SkillType.Passive:
                    CopyPassiveStats(prevStat, newStat);
                    break;
            }
        }

        levelStats.Add(newStat);
    }

    private void CopyProjectileStats(SkillStatData from, SkillStatData to)
    {
        to.projectileSpeed = from.projectileSpeed;
        to.projectileScale = from.projectileScale;
        to.shotInterval = from.shotInterval;
        to.pierceCount = from.pierceCount;
        to.attackRange = from.attackRange;
        to.homingRange = from.homingRange;
        to.isHoming = from.isHoming;
        to.explosionRad = from.explosionRad;
        to.projectileCount = from.projectileCount;
        to.innerInterval = from.innerInterval;
    }

    private void CopyAreaStats(SkillStatData from, SkillStatData to)
    {
        to.radius = from.radius;
        to.duration = from.duration;
        to.tickRate = from.tickRate;
        to.isPersistent = from.isPersistent;
        to.moveSpeed = from.moveSpeed;
    }

    private void CopyPassiveStats(SkillStatData from, SkillStatData to)
    {
        to.effectDuration = from.effectDuration;
        to.cooldown = from.cooldown;
        to.triggerChance = from.triggerChance;
    }

    private void LoadSkillStatsData()
    {
        skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();

        LoadSkillStatsFromCSV("SkillData/ProjectileSkillStats");
        LoadSkillStatsFromCSV("SkillData/AreaSkillStats");
        LoadSkillStatsFromCSV("SkillData/PassiveSkillStats");

        Debug.Log($"Loaded {skillStatsList.Count} skill stats from CSV files.");
    }

    private void LoadSkillStatsFromCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split('\n');
            string[] headers = lines[0].Trim().Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                SkillStatData statData = ParseSkillStatLine(headers, values);

                if (!skillStatsList.ContainsKey(statData.skillID))
                {
                    skillStatsList[statData.skillID] = new List<SkillStatData>();
                }

                skillStatsList[statData.skillID].Add(statData);
            }
        }
    }

    private void SaveSkillStatsToCSV()
    {
        string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
        Directory.CreateDirectory(directory);

        // Save CSV files by skill type
        SaveProjectileSkillStats(directory);
        SaveAreaSkillStats(directory);
        SavePassiveSkillStats(directory);

        AssetDatabase.Refresh();
    }

    private void SaveProjectileSkillStats(string directory)
    {
        string path = Path.Combine(directory, "ProjectileSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        // Header for projectile skills
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "ProjectileSpeed,ProjectileScale,ShotInterval,PierceCount,AttackRange," +
                      "HomingRange,IsHoming,ExplosionRad,ProjectileCount,InnerInterval");

        // Save projectile skill data
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Projectile)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.projectileSpeed}," +
                                 $"{stat.projectileScale},{stat.shotInterval},{stat.pierceCount}," +
                                 $"{stat.attackRange},{stat.homingRange},{stat.isHoming}," +
                                 $"{stat.explosionRad},{stat.projectileCount},{stat.innerInterval}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private void SaveAreaSkillStats(string directory)
    {
        string path = Path.Combine(directory, "AreaSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        // Header for area skills
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "Radius,Duration,TickRate,IsPersistent,MoveSpeed");

        // Save area skill data
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Area)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.radius}," +
                                 $"{stat.duration},{stat.tickRate},{stat.isPersistent},{stat.moveSpeed}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private void SavePassiveSkillStats(string directory)
    {
        string path = Path.Combine(directory, "PassiveSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        // Header for passive skills
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "EffectDuration,Cooldown,TriggerChance");

        // Save passive skill data
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Passive)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +
                                 $"{stat.cooldown},{stat.triggerChance}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private SkillType GetSkillType(SkillID skillID)
    {
        var skill = skillList.Find(x => x.metadata.ID == skillID);
        return skill?.metadata.Type ?? SkillType.Projectile;
    }

    #region Bottom Panel
    private void DrawBottomPanel()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("New Skill", GUILayout.Height(30)))
            CreateNewSkill();

        if (GUILayout.Button("Save All", GUILayout.Height(30)))
        {
            SaveSkillData();
            SaveSkillStatsToCSV();
        }

        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Data",
                "Are you sure you want to clear all skill data? This cannot be undone.",
                "Clear", "Cancel"))
            {
                ClearAllData();
                LoadAllData();
            }
        }

        if (GUILayout.Button("Export JSON", GUILayout.Height(30)))
            ExportToJson();

        if (GUILayout.Button("Import JSON", GUILayout.Height(30)))
            ImportFromJson();

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Data Management
    private void LoadAllData()
    {
        // 1. SkillDataManager⺻ ų  ε
        LoadSkillData();

        // 2. CSV ų  ε
        LoadSkillStatsData();

        // 3. JSON Ͽ߰  ε ( )
        LoadJsonData();
    }

    private void LoadJsonData()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "SkillData.json");
        if (File.Exists(jsonPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonPath);
                SkillDataWrapper wrapper = JsonUtility.FromJson<SkillDataWrapper>(jsonContent);

                if (wrapper != null && wrapper.skillDatas != null)
                {
                    foreach (var jsonSkill in wrapper.skillDatas)
                    {
                        var existingSkill = skillList.Find(s => s.metadata.ID == jsonSkill.metadata.ID);
                        if (existingSkill == null)
                        {
                            skillList.Add(jsonSkill);
                        }
                        else
                        {
                            int index = skillList.IndexOf(existingSkill);
                            skillList[index] = jsonSkill;
                        }
                    }
                }

                Debug.Log($"Successfully loaded JSON data from: {jsonPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load JSON data: {e.Message}");
            }
        }
    }

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
            Debug.Log($"Loaded {skillList.Count} skills from SkillDataManager.");
        }
        else
        {
            skillList = new List<SkillData>();
            Debug.LogError("SkillDataManager not found!");
        }
    }

    private void SaveSkillData()
    {
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            // Save base data
            skillDataManager.SaveAllSkillData();
            EditorUtility.SetDirty(skillDataManager);
            AssetDatabase.SaveAssets();

            // Save JSON
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);
            string jsonPath = Path.Combine(directory, "SkillData.json");

            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // Save CSV data
            SaveSkillStatsToCSV();

            AssetDatabase.Refresh();
            Debug.Log($"Skill data saved successfully!\n" +
                     $"JSON Path: {jsonPath}\n" +
                     $"CSV Path: {directory}/[Type]SkillStats.csv");
        }
        else
        {
            Debug.LogError("SkillDataManager not found!");
        }
    }

    private void CreateNewSkill()
    {
        SkillData newSkill = new SkillData
        {
            metadata = new SkillMetadata
            {
                Name = "New Skill",
                Description = "New skill description",
                Type = SkillType.None,
                ID = SkillID.None,
                Element = ElementType.None,
                Tier = 1,
                Tags = new string[0],
                Prefab = null
            }
        };

        // Initialize default stats
        var defaultStats = new ProjectileSkillStat
        {
            baseStat = new BaseSkillStat
            {
                damage = 10f,
                skillName = "New Skill",
                skillLevel = 1,
                maxSkillLevel = 5,
                element = ElementType.None,
                elementalPower = 1f
            }
        };

        // Set initial stats for level 1
        newSkill.SetStatsForLevel(1, defaultStats);

        // Add to list and select
        skillList.Add(newSkill);
        currentSkill = newSkill;

        // Initialize skill stats list if needed
        if (!skillStatsList.ContainsKey(newSkill.metadata.ID))
        {
            skillStatsList[newSkill.metadata.ID] = new List<SkillStatData>();
        }

        Debug.Log("Created new skill with default values");
    }

    private void InitializeSkillStats(SkillData skill)
    {
        // 모든 스킬 타입에 대한 기본 스탯 초기화
        skill.projectileStat = new ProjectileSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.metadata.Name }
        };
        skill.areaStat = new AreaSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.metadata.Name }
        };
        skill.passiveStat = new PassiveSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.metadata.Name }
        };
    }

    private void ExportToJson()
    {
        string defaultPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
        string path = EditorUtility.SaveFilePanel("Export Skill Data", defaultPath, "SkillData", "json");
        if (!string.IsNullOrEmpty(path))
        {
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"ų Ͱ  ηϴ: {path}");
            AssetDatabase.Refresh();
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
            Debug.Log($"ㄴ: {path}");
        }
    }
    #endregion

    private SkillStatData ParseSkillStatLine(string[] headers, string[] values)
    {
        SkillStatData statData = new SkillStatData();

        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string value = values[i].Trim();
            switch (headers[i].Trim().ToLower())
            {
                case "skillid":
                    if (System.Enum.TryParse(value, out SkillID skillID))
                        statData.skillID = skillID;
                    break;
                case "level":
                    if (int.TryParse(value, out int level))
                        statData.level = level;
                    break;
                case "damage":
                    if (float.TryParse(value, out float damage))
                        statData.damage = damage;
                    break;
                case "maxskilllevel":
                    if (int.TryParse(value, out int maxLevel))
                        statData.maxSkillLevel = maxLevel;
                    break;
                case "element":
                    if (System.Enum.TryParse(value, out ElementType element))
                        statData.element = element;
                    break;
                case "elementalpower":
                    if (float.TryParse(value, out float elementalPower))
                        statData.elementalPower = elementalPower;
                    break;
                case "projectilespeed":
                    if (float.TryParse(value, out float speed))
                        statData.projectileSpeed = speed;
                    break;
                case "projectilescale":
                    if (float.TryParse(value, out float scale))
                        statData.projectileScale = scale;
                    break;
                case "shotinterval":
                    if (float.TryParse(value, out float interval))
                        statData.shotInterval = interval;
                    break;
                case "piercecount":
                    if (int.TryParse(value, out int pierce))
                        statData.pierceCount = pierce;
                    break;
                case "attackrange":
                    if (float.TryParse(value, out float attackRange))
                        statData.attackRange = attackRange;
                    break;
                case "homingrange":
                    if (float.TryParse(value, out float homingRange))
                        statData.homingRange = homingRange;
                    break;
                case "ishoming":
                    if (bool.TryParse(value, out bool isHoming))
                        statData.isHoming = isHoming;
                    break;
                case "explosionrad":
                    if (float.TryParse(value, out float explosionRad))
                        statData.explosionRad = explosionRad;
                    break;
                case "projectilecount":
                    if (int.TryParse(value, out int count))
                        statData.projectileCount = count;
                    break;
                case "innerinterval":
                    if (float.TryParse(value, out float innerInterval))
                        statData.innerInterval = innerInterval;
                    break;
            }
        }

        return statData;
    }

    private void ClearAllData()
    {
        // Clear all existing data
        if (skillList != null)
            skillList.Clear();
        if (skillStatsList != null)
            skillStatsList.Clear();

        // Delete all files in the Resources/SkillData folder
        string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
        if (Directory.Exists(directory))
        {
            try
            {
                // Delete JSON file
                string jsonPath = Path.Combine(directory, "SkillData.json");
                if (File.Exists(jsonPath))
                    File.Delete(jsonPath);

                // Delete CSV files
                string[] csvFiles = {
                    "ProjectileSkillStats.csv",
                    "AreaSkillStats.csv",
                    "PassiveSkillStats.csv"
                };

                foreach (var file in csvFiles)
                {
                    string path = Path.Combine(directory, file);
                    if (File.Exists(path))
                        File.Delete(path);
                }

                Debug.Log("All skill data files have been cleared.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to clear data files: {e.Message}");
            }
        }

        // Refresh Unity's asset database
        AssetDatabase.Refresh();
    }
}