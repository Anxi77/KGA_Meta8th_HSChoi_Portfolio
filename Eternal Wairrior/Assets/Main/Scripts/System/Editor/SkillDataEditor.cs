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
                $"Are you sure you want to delete '{currentSkill.Name}'?", "Delete", "Cancel"))
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
                if (GUILayout.Button(skill.Name, GUILayout.Height(30)))
                {
                    currentSkill = skill;

                    // 오른쪽 클릭 메뉴 처리
                    if (Event.current.button == 1) // 오른쪽 클릭
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

    // 스킬 컨텍스트 메뉴 표시
    private void ShowSkillContextMenu(SkillData skill)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete"), false, () => {
            if (EditorUtility.DisplayDialog("Delete Skill",
                $"Are you sure you want to delete '{skill.Name}'?", "Delete", "Cancel"))
            {
                DeleteSkill(skill);
            }
        });
        menu.ShowAsContext();
    }

    // 현재 선택된 스킬 삭제
    private void DeleteCurrentSkill()
    {
        if (currentSkill != null)
        {
            DeleteSkill(currentSkill);
            currentSkill = null;
        }
    }

    // 스킬 삭제 로직
    private void DeleteSkill(SkillData skill)
    {
        // 스킬 리스트에서 제거
        skillList.Remove(skill);

        // 스킬 스탯 데이터 제거
        if (skillStatsList.ContainsKey(skill._SkillID))
        {
            skillStatsList.Remove(skill._SkillID);
        }

        // SkillDataManager 업데이트
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            EditorUtility.SetDirty(skillDataManager);
        }

        // 저장
        SaveSkillData();

        Debug.Log($"Skill '{skill.Name}' has been deleted.");
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
            // SkillDataManager를 찾아서 dirty로 표시
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
        currentSkill.Name = EditorGUILayout.TextField("Name", currentSkill.Name);
        currentSkill.Description = EditorGUILayout.TextField("Description", currentSkill.Description);

        // SkillType이 변경될 때 스탯 초기화
        SkillType newSkillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", currentSkill._SkillType);
        if (newSkillType != currentSkill._SkillType)
        {
            currentSkill._SkillType = newSkillType;
            InitializeSkillStats(currentSkill);
        }

        currentSkill._SkillID = (SkillID)EditorGUILayout.EnumPopup("Skill ID", currentSkill._SkillID);

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
            if (currentSkill._SkillType == SkillType.None)
            {
                EditorGUILayout.HelpBox("Please select a skill type first", MessageType.Info);
                return;
            }

            // 기본 스탯 그리기
            DrawBaseStats();

            // 스킬 타입별 스탯만 그리기
            switch (currentSkill._SkillType)
            {
                case SkillType.Projectile:
                    if (currentSkill.projectileStat == null)
                        currentSkill.projectileStat = new ProjectileSkillStat();
                    DrawProjectileStats();
                    break;
                case SkillType.Area:
                    if (currentSkill.areaStat == null)
                        currentSkill.areaStat = new AreaSkillStat();
                    DrawAreaStats();
                    break;
                case SkillType.Passive:
                    if (currentSkill.passiveStat == null)
                        currentSkill.passiveStat = new PassiveSkillStat();
                    DrawPassiveStats();
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

    private void DrawLevelStats()
    {
        EditorGUILayout.Space(10);
        showLevelStats = EditorGUILayout.Foldout(showLevelStats, "Level Stats");

        if (!showLevelStats) return;

        if (!skillStatsList.ContainsKey(currentSkill._SkillID))
        {
            skillStatsList[currentSkill._SkillID] = new List<SkillStatData>();
        }

        var levelStats = skillStatsList[currentSkill._SkillID];

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Level", GUILayout.Width(100)))
        {
            AddNewLevelStat();
        }
        if (GUILayout.Button("Remove Last Level", GUILayout.Width(120)))
        {
            if (levelStats.Count > 0)
                levelStats.RemoveAt(levelStats.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < levelStats.Count; i++)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Level {i + 1}", EditorStyles.boldLabel);
            var stat = levelStats[i];

            // 기본 스탯
            stat.damage = EditorGUILayout.FloatField("Damage", stat.damage);
            stat.maxSkillLevel = EditorGUILayout.IntField("Max Level", stat.maxSkillLevel);
            stat.element = (ElementType)EditorGUILayout.EnumPopup("Element", stat.element);
            stat.elementalPower = EditorGUILayout.FloatField("Elemental Power", stat.elementalPower);

            // 스킬 타입별 스탯만 표시
            switch (currentSkill._SkillType)
            {
                case SkillType.Projectile:
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
                    stat.projectileSpeed = EditorGUILayout.FloatField("Speed", stat.projectileSpeed);
                    stat.projectileScale = EditorGUILayout.FloatField("Scale", stat.projectileScale);
                    stat.shotInterval = EditorGUILayout.FloatField("Shot Interval", stat.shotInterval);
                    stat.pierceCount = EditorGUILayout.IntField("Pierce Count", stat.pierceCount);
                    stat.attackRange = EditorGUILayout.FloatField("Attack Range", stat.attackRange);
                    stat.homingRange = EditorGUILayout.FloatField("Homing Range", stat.homingRange);
                    stat.isHoming = EditorGUILayout.Toggle("Is Homing", stat.isHoming);
                    stat.explosionRad = EditorGUILayout.FloatField("Explosion Radius", stat.explosionRad);
                    stat.projectileCount = EditorGUILayout.IntField("Projectile Count", stat.projectileCount);
                    stat.innerInterval = EditorGUILayout.FloatField("Inner Interval", stat.innerInterval);
                    break;
                case SkillType.Area:
                    // Area 스킬 스탯
                    break;
                case SkillType.Passive:
                    // Passive 스킬 스탯
                    break;
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void AddNewLevelStat()
    {
        var levelStats = skillStatsList[currentSkill._SkillID];
        var newStat = new SkillStatData
        {
            skillID = currentSkill._SkillID,
            level = levelStats.Count + 1
        };

        // 이전 레벨의 스탯을 복사 (있는 경우)
        if (levelStats.Count > 0)
        {
            var prevStat = levelStats[levelStats.Count - 1];
            newStat.damage = prevStat.damage * 1.1f; // 10% 증가
            newStat.maxSkillLevel = prevStat.maxSkillLevel;
            newStat.element = prevStat.element;
            newStat.elementalPower = prevStat.elementalPower * 1.1f;

            // 발사체 스킬 스탯
            newStat.projectileSpeed = prevStat.projectileSpeed;
            newStat.projectileScale = prevStat.projectileScale;
            newStat.shotInterval = prevStat.shotInterval;
            newStat.pierceCount = prevStat.pierceCount;
            newStat.attackRange = prevStat.attackRange;
            newStat.homingRange = prevStat.homingRange;
            newStat.isHoming = prevStat.isHoming;
            newStat.explosionRad = prevStat.explosionRad;
            newStat.projectileCount = prevStat.projectileCount;
            newStat.innerInterval = prevStat.innerInterval;
        }

        levelStats.Add(newStat);
    }

    private void LoadSkillStatsData()
    {
        skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();

        LoadSkillStatsFromCSV("SkillData/ProjectileSkillStats");
        LoadSkillStatsFromCSV("SkillData/AreaSkillStats");
        LoadSkillStatsFromCSV("SkillData/PassiveSkillStats");

        Debug.Log($"CSV 파일에서 {skillStatsList.Count}개의 스킬 스탯을 로드했습니다.");
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

        // 스킬 타입별로 CSV 파일 생성
        SaveProjectileSkillStats(directory);
        SaveAreaSkillStats(directory);
        SavePassiveSkillStats(directory);

        AssetDatabase.Refresh();
    }

    private void SaveProjectileSkillStats(string directory)
    {
        string path = Path.Combine(directory, "ProjectileSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        // 프로젝타일 스킬 헤더
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "ProjectileSpeed,ProjectileScale,ShotInterval,PierceCount,AttackRange," +
                      "HomingRange,IsHoming,ExplosionRad,ProjectileCount,InnerInterval");

        // 프로젝타일 스킬 데이터만 저장
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

        // 영역 스킬 헤더
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "Radius,Duration,TickRate,IsPersistent,MoveSpeed");

        // 영역 스킬 데이터만 저장
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Area)
                {
                    // 영역 스킬 데이터 포맷
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

        // 패시브 스킬 헤더
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "EffectDuration,Cooldown,TriggerChance");

        // 패시브 스킬 데이터만 저장
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Passive)
                {
                    // 패시브 스킬 데이터 포맷
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
        var skill = skillList.Find(x => x._SkillID == skillID);
        return skill?._SkillType ?? SkillType.Projectile;
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
        // 1. SkillDataManager에서 기본 스킬 데이터 로드
        LoadSkillData();

        // 2. CSV에서 스킬 스탯 데이터 로드
        LoadSkillStatsData();

        // 3. JSON 파일에서 추가 데이터 로드 (백업용)
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

                // JSON 데이터와 현재 데이터 병합
                if (wrapper != null && wrapper.skillDatas != null)
                {
                    foreach (var jsonSkill in wrapper.skillDatas)
                    {
                        // 기존 스킬 찾기
                        var existingSkill = skillList.Find(s => s._SkillID == jsonSkill._SkillID);
                        if (existingSkill == null)
                        {
                            // 새로운 스킬이면 추가
                            skillList.Add(jsonSkill);
                        }
                        else
                        {
                            // 기존 스킬이면 데이터 업데이트
                            int index = skillList.IndexOf(existingSkill);
                            skillList[index] = jsonSkill;
                        }
                    }
                }

                Debug.Log($"JSON 데이터를 성공적으로 로드했습니다: {jsonPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON 데이터 로드 중 오류 발생: {e.Message}");
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
            Debug.Log($"SkillDataManager에서 {skillList.Count}개의 스킬을 로드했습니다.");
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
            // 기존 데이터 저장
            skillDataManager.SaveAllSkillData();
            EditorUtility.SetDirty(skillDataManager);
            AssetDatabase.SaveAssets();

            // JSON 저장
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);
            string jsonPath = Path.Combine(directory, "SkillData.json");

            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // CSV 데이터도 저장
            SaveSkillStatsToCSV();

            AssetDatabase.Refresh();
            Debug.Log($"스킬 데이터가 저장되었습니다!\n" +
                     $"JSON 파일: {jsonPath}\n" +
                     $"CSV 파일: {directory}/[Type]SkillStats.csv");
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
            _SkillType = SkillType.None,
            _SkillID = SkillID.None,
            prefabsByLevel = new GameObject[1]
        };

        // 모든 스탯 타입 초기화
        InitializeSkillStats(newSkill);

        skillList.Add(newSkill);
        currentSkill = newSkill;

        if (!skillStatsList.ContainsKey(newSkill._SkillID))
        {
            skillStatsList[newSkill._SkillID] = new List<SkillStatData>();
        }

        Debug.Log("새로운 스킬이 생성되었습니다!");
    }

    private void InitializeSkillStats(SkillData skill)
    {
        // 모든 스탯 타입에 대한 기본값 초기화
        skill.projectileStat = new ProjectileSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.Name }
        };
        skill.areaStat = new AreaSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.Name }
        };
        skill.passiveStat = new PassiveSkillStat
        {
            baseStat = new BaseSkillStat { skillName = skill.Name }
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
            Debug.Log($"스킬 데이터가 다음 경로로 내보내졌습니다: {path}");
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
            Debug.Log($"스킬 데이터를 다음 경로에서 불러왔습니다: {path}");
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
}