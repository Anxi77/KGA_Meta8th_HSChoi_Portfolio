using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Linq;

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
        if (currentSkill == null)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Select a skill to edit", headerStyle);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginVertical();
        statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);

        EditorGUI.BeginChangeCheck();

        DrawBasicInfo();
        DrawPrefabSettings();
        DrawSkillStats();
        DrawLevelStats();

        if (EditorGUI.EndChangeCheck())
        {
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
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Prefab Settings", headerStyle);
        EditorGUI.indentLevel++;

        // 아이콘 설정 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Skill Icon", EditorStyles.boldLabel);

        // 아이콘 프리뷰
        if (currentSkill.icon != null)
        {
            Rect previewRect = EditorGUILayout.GetControlRect(false, 64);
            previewRect.width = 64;
            EditorGUI.DrawPreviewTexture(previewRect, currentSkill.icon.texture);

            // 아이콘 필드를 프리뷰 옆에 배치
            Rect fieldRect = EditorGUILayout.GetControlRect();
            fieldRect.x += 70;
            fieldRect.width -= 70;
            currentSkill.icon = (Sprite)EditorGUI.ObjectField(
                fieldRect,
                currentSkill.icon,
                typeof(Sprite),
                false
            );
        }
        else
        {
            currentSkill.icon = (Sprite)EditorGUILayout.ObjectField(
                "Icon",
                currentSkill.icon,
                typeof(Sprite),
                false
            );
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 프리팹 설정 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);

        currentSkill.projectile = (GameObject)EditorGUILayout.ObjectField(
            "Projectile Prefab",
            currentSkill.projectile,
            typeof(GameObject),
            false
        );

        showBaseStats = EditorGUILayout.Foldout(showBaseStats, "Level Prefabs", true);
        if (showBaseStats)
        {
            EditorGUI.indentLevel++;

            // 배열 크기 조절
            int size = EditorGUILayout.IntField("Size", currentSkill.prefabsByLevel?.Length ?? 0);
            if (size != currentSkill.prefabsByLevel?.Length)
            {
                System.Array.Resize(ref currentSkill.prefabsByLevel, size);
            }

            // 레벨별 프리팹 설정
            if (currentSkill.prefabsByLevel != null)
            {
                for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
                {
                    currentSkill.prefabsByLevel[i] = (GameObject)EditorGUILayout.ObjectField(
                        $"Level {i + 1}",
                        currentSkill.prefabsByLevel[i],
                        typeof(GameObject),
                        false
                    );
                }
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

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

            DrawBaseStats();

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
        if (stats == null) return;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // 기존 필드들
        stats.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        stats.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        stats.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);
        stats.damageIncrease = EditorGUILayout.FloatField("Damage Increase (%)", stats.damageIncrease);
        stats.defenseIncrease = EditorGUILayout.FloatField("Defense Increase (%)", stats.defenseIncrease);
        stats.expAreaIncrease = EditorGUILayout.FloatField("Exp Area Increase (%)", stats.expAreaIncrease);
        stats.homingActivate = EditorGUILayout.Toggle("Homing Activate", stats.homingActivate);
        stats.hpIncrease = EditorGUILayout.FloatField("HP Increase (%)", stats.hpIncrease);

        // 새로운 필드들
        stats.moveSpeedIncrease = EditorGUILayout.FloatField("Move Speed Increase (%)", stats.moveSpeedIncrease);
        stats.attackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed Increase (%)", stats.attackSpeedIncrease);
        stats.attackRangeIncrease = EditorGUILayout.FloatField("Attack Range Increase (%)", stats.attackRangeIncrease);
        stats.hpRegenIncrease = EditorGUILayout.FloatField("HP Regen Increase (%)", stats.hpRegenIncrease);

        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStatsForLevel(SkillStatData stat)
    {
        stat.effectDuration = EditorGUILayout.FloatField("Effect Duration", stat.effectDuration);
        stat.cooldown = EditorGUILayout.FloatField("Cooldown", stat.cooldown);
        stat.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stat.triggerChance);
        stat.damageIncrease = EditorGUILayout.FloatField("Damage Increase (%)", stat.damageIncrease);
        stat.defenseIncrease = EditorGUILayout.FloatField("Defense Increase (%)", stat.defenseIncrease);
        stat.expAreaIncrease = EditorGUILayout.FloatField("Exp Area Increase (%)", stat.expAreaIncrease);
        stat.homingActivate = EditorGUILayout.Toggle("Homing Activate", stat.homingActivate);
        stat.hpIncrease = EditorGUILayout.FloatField("HP Increase (%)", stat.hpIncrease);
        stat.moveSpeedIncrease = EditorGUILayout.FloatField("Move Speed Increase (%)", stat.moveSpeedIncrease);
        stat.attackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed Increase (%)", stat.attackSpeedIncrease);
        stat.attackRangeIncrease = EditorGUILayout.FloatField("Attack Range Increase (%)", stat.attackRangeIncrease);
        stat.hpRegenIncrease = EditorGUILayout.FloatField("HP Regen Increase (%)", stat.hpRegenIncrease);
    }

    private void DrawLevelStats()
    {
        if (currentSkill == null) return;

        EditorGUILayout.Space(10);
        showLevelStats = EditorGUILayout.Foldout(showLevelStats, "Level Stats");

        if (!showLevelStats) return;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Level", GUILayout.Width(100)))
        {
            AddNewLevelStat();
        }
        EditorGUILayout.EndHorizontal();

        var skillStats = skillStatsList.ContainsKey(currentSkill.metadata.ID)
            ? skillStatsList[currentSkill.metadata.ID]
            : new List<SkillStatData>();

        for (int i = 0; i < skillStats.Count; i++)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var levelStat = skillStats[i];
            EditorGUILayout.LabelField($"Level {levelStat.level}", EditorStyles.boldLabel);

            levelStat.damage = EditorGUILayout.FloatField("Damage", levelStat.damage);
            levelStat.elementalPower = EditorGUILayout.FloatField("Elemental Power", levelStat.elementalPower);

            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    DrawProjectileStatsForLevel(levelStat);
                    break;
                case SkillType.Area:
                    DrawAreaStatsForLevel(levelStat);
                    break;
                case SkillType.Passive:
                    DrawPassiveStatsForLevel(levelStat);
                    break;
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawProjectileStatsForLevel(SkillStatData stat)
    {
        stat.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", stat.projectileSpeed);
        stat.projectileScale = EditorGUILayout.FloatField("Projectile Scale", stat.projectileScale);
        stat.shotInterval = EditorGUILayout.FloatField("Shot Interval", stat.shotInterval);
        stat.pierceCount = EditorGUILayout.IntField("Pierce Count", stat.pierceCount);
        stat.attackRange = EditorGUILayout.FloatField("Attack Range", stat.attackRange);
        stat.homingRange = EditorGUILayout.FloatField("Homing Range", stat.homingRange);
        stat.isHoming = EditorGUILayout.Toggle("Is Homing", stat.isHoming);
        stat.explosionRad = EditorGUILayout.FloatField("Explosion Radius", stat.explosionRad);
        stat.projectileCount = EditorGUILayout.IntField("Projectile Count", stat.projectileCount);
        stat.innerInterval = EditorGUILayout.FloatField("Inner Interval", stat.innerInterval);
    }

    private void DrawAreaStatsForLevel(SkillStatData stat)
    {
        stat.radius = EditorGUILayout.FloatField("Radius", stat.radius);
        stat.duration = EditorGUILayout.FloatField("Duration", stat.duration);
        stat.tickRate = EditorGUILayout.FloatField("Tick Rate", stat.tickRate);
        stat.isPersistent = EditorGUILayout.Toggle("Is Persistent", stat.isPersistent);
        stat.moveSpeed = EditorGUILayout.FloatField("Move Speed", stat.moveSpeed);
    }

    private void AddNewLevelStat()
    {
        if (currentSkill == null) return;

        if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
        {
            skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();
        }

        var levelStats = skillStatsList[currentSkill.metadata.ID];

        var newStat = new SkillStatData
        {
            skillID = currentSkill.metadata.ID,
            level = levelStats.Count + 1,
            element = currentSkill.metadata.Element
        };

        if (levelStats.Count > 0)
        {
            var prevStat = levelStats[levelStats.Count - 1];
            CopyStatsFromPrevious(prevStat, newStat);
        }
        else
        {
            InitializeDefaultStats(newStat);
        }

        levelStats.Add(newStat);

        EditorUtility.SetDirty(FindObjectOfType<SkillDataManager>());
        SaveSkillData();
    }

    private void CopyStatsFromPrevious(SkillStatData from, SkillStatData to)
    {
        to.damage = from.damage * 1.1f;
        to.maxSkillLevel = from.maxSkillLevel;
        to.element = currentSkill.metadata.Element;
        to.elementalPower = from.elementalPower * 1.1f;

        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                CopyProjectileStats(from, to);
                break;
            case SkillType.Area:
                CopyAreaStats(from, to);
                break;
            case SkillType.Passive:
                CopyPassiveStats(from, to);
                break;
        }
    }

    private void InitializeDefaultStats(SkillStatData stat)
    {
        stat.damage = 10f;
        stat.maxSkillLevel = 5;
        stat.element = currentSkill.metadata.Element;
        stat.elementalPower = 1f;

        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                InitializeDefaultProjectileStats(stat);
                break;
            case SkillType.Area:
                InitializeDefaultAreaStats(stat);
                break;
            case SkillType.Passive:
                InitializeDefaultPassiveStats(stat);
                break;
        }
    }

    private void InitializeDefaultProjectileStats(SkillStatData stat)
    {
        stat.projectileSpeed = 10f;
        stat.projectileScale = 1f;
        stat.shotInterval = 1f;
        stat.pierceCount = 1;
        stat.attackRange = 10f;
        stat.homingRange = 5f;
        stat.isHoming = false;
        stat.explosionRad = 0f;
        stat.projectileCount = 1;
        stat.innerInterval = 0.1f;
    }

    private void InitializeDefaultAreaStats(SkillStatData stat)
    {
        stat.radius = 5f;
        stat.duration = 3f;
        stat.tickRate = 1f;
        stat.isPersistent = false;
        stat.moveSpeed = 0f;
    }

    private void InitializeDefaultPassiveStats(SkillStatData stat)
    {
        stat.effectDuration = 5f;
        stat.cooldown = 10f;
        stat.triggerChance = 100f;
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
        to.damageIncrease = from.damageIncrease;
        to.defenseIncrease = from.defenseIncrease;
        to.expAreaIncrease = from.expAreaIncrease;
        to.homingActivate = from.homingActivate;
        to.hpIncrease = from.hpIncrease;
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

    private void SaveSkillStatsToCSV(string directory)
    {
        // 스킬 타입별로 CSV 파일 생성
        SaveProjectileSkillStats(directory);
        SaveAreaSkillStats(directory);
        SavePassiveSkillStats(directory);
    }

    private void SaveProjectileSkillStats(string directory)
    {
        string path = Path.Combine(directory, "ProjectileSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "projectilespeed,projectilescale,shotinterval,piercecount,attackrange," +
                      "homingrange,ishoming,explosionrad,projectilecount,innerinterval");

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

        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "radius,duration,tickrate,ispersistent,movespeed");

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

        // 헤더 작성
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease," +
                      "movespeedincrease,attackspeedincrease,attackrangeincrease,hpregenincrease");

        // 각 스킬의 레벨별 스탯 데이터 저장
        foreach (var skillPair in skillStatsList)
        {
            var skillData = skillList.Find(s => s.metadata.ID == skillPair.Key);
            if (skillData?.metadata.Type == SkillType.Passive)
            {
                foreach (var stat in skillPair.Value)
                {
                    csv.AppendLine(CreatePassiveStatLine(stat));
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Saved passive skill stats to: {path}");
    }

    private string CreatePassiveStatLine(SkillStatData stat)
    {
        return $"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
               $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +
               $"{stat.cooldown},{stat.triggerChance},{stat.damageIncrease}," +
               $"{stat.defenseIncrease},{stat.expAreaIncrease},{stat.homingActivate}," +
               $"{stat.hpIncrease},{stat.moveSpeedIncrease},{stat.attackSpeedIncrease}," +
               $"{stat.attackRangeIncrease},{stat.hpRegenIncrease}";
    }

    private SkillType GetSkillType(SkillID skillID)
    {
        if (skillID == SkillID.None)
        {
            Debug.LogError("Cannot get type for SkillID.None");
            return SkillType.None;
        }

        var skill = skillList.Find(x => x.metadata.ID == skillID);
        if (skill == null)
        {
            Debug.LogError($"Could not find skill with ID: {skillID}");
            return SkillType.None;
        }
        return skill.metadata.Type;
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
            if (ValidateSkillData())
            {
                SaveSkillData();
                string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
                SaveSkillStatsToCSV(directory);

                var skillDataManager = FindObjectOfType<SkillDataManager>();
                if (skillDataManager != null)
                {
                    skillDataManager.LoadSkillStatsFromCSV();
                    skillDataManager.LoadAllSkillData();
                    skillDataManager.UpdateSkillStatsData(skillStatsList);
                }
                Debug.Log("All skill data saved and reloaded successfully.");
            }
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

    private bool ValidateSkillData()
    {
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (stat.level <= 0)
                {
                    Debug.LogError($"Invalid level value for skill {stat.skillID}: {stat.level}");
                    return false;
                }
                if (stat.damage < 0)
                {
                    Debug.LogError($"Invalid damage value for skill {stat.skillID}: {stat.damage}");
                    return false;
                }
                if (stat.maxSkillLevel <= 0)
                {
                    Debug.LogError($"Invalid max skill level for skill {stat.skillID}: {stat.maxSkillLevel}");
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    #region Data Management
    private void LoadAllData()
    {
        try
        {
            // JSON 데이터 로드
            LoadSkillData();

            // CSV 데이터 로드
            LoadSkillStatsData();

            Debug.Log($"Successfully loaded {skillList.Count} skills and their stats");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data: {e.Message}");
        }
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
            string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "SkillData.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    SkillDataWrapper wrapper = JsonUtility.FromJson<SkillDataWrapper>(jsonContent);
                    if (wrapper != null && wrapper.skillDatas != null)
                    {
                        skillList = wrapper.skillDatas;
                        skillDataManager.UpdateSkillList(skillList);
                    }
                    else
                    {
                        skillList = new List<SkillData>();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load JSON data: {e.Message}");
                    skillList = new List<SkillData>();
                }
            }
            else
            {
                skillList = skillDataManager.GetAllSkillData();
            }

            foreach (var skill in skillList.ToList())
            {
                if (skill == null)
                {
                    skillList.Remove(skill);
                    continue;
                }

                if (skill.metadata == null)
                {
                    skill.metadata = new SkillMetadata
                    {
                        Name = "Unnamed Skill",
                        Description = "",
                        Type = SkillType.None,
                        ID = SkillID.None,
                        Element = ElementType.None,
                        Tier = 1,
                        Tags = new string[0]
                    };
                }
            }

            Debug.Log($"Loaded {skillList.Count} skills from JSON/SkillDataManager.");
        }
        else
        {
            skillList = new List<SkillData>();
            Debug.LogError("SkillDataManager not found!");
        }
    }

    private void SaveSkillData()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);

            // 스프라이트 경로 저장
            foreach (var skill in skillList)
            {
                if (skill.icon != null)
                {
                    // 에셋 경로를 Resources 폴더 기준 상대 경로로 저장
                    string assetPath = AssetDatabase.GetAssetPath(skill.icon);
                    string resourcePath = assetPath.Replace("Assets/Resources/", "").Replace(".png", "");
                    skill.metadata.Icon = skill.icon;  // 메타데이터에도 스프라이트 저장
                }
            }

            // JSON 저장
            string jsonPath = Path.Combine(directory, "SkillData.json");
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // CSV 파일에도 스프라이트 경로 포함
            SaveSkillStatsToCSV(directory);

            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}");
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
                ID = GetNextAvailableSkillID(),
                Element = ElementType.None,
                Tier = 1,
                Tags = new string[0]
            }
        };

        InitializeSkillStats(newSkill);
        skillList.Add(newSkill);
        currentSkill = newSkill;

        if (!skillStatsList.ContainsKey(newSkill.metadata.ID))
        {
            skillStatsList[newSkill.metadata.ID] = new List<SkillStatData>();
        }

        SaveSkillData();
        Debug.Log($"Created new skill: {newSkill.metadata.Name} (ID: {newSkill.metadata.ID})");
    }

    private SkillID GetNextAvailableSkillID()
    {
        var usedIDs = skillList.Select(s => s.metadata.ID).ToHashSet();
        foreach (SkillID id in System.Enum.GetValues(typeof(SkillID)))
        {
            if (id != SkillID.None && !usedIDs.Contains(id))
            {
                return id;
            }
        }
        return SkillID.None;
    }

    private void InitializeSkillStats(SkillData skill)
    {
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
                case "radius":
                    if (float.TryParse(value, out float radius))
                        statData.radius = radius;
                    break;
                case "duration":
                    if (float.TryParse(value, out float duration))
                        statData.duration = duration;
                    break;
                case "tickrate":
                    if (float.TryParse(value, out float tickRate))
                        statData.tickRate = tickRate;
                    break;
                case "ispersistent":
                    if (bool.TryParse(value, out bool isPersistent))
                        statData.isPersistent = isPersistent;
                    break;
                case "movespeed":
                    if (float.TryParse(value, out float moveSpeed))
                        statData.moveSpeed = moveSpeed;
                    break;
                case "effectduration":
                    if (float.TryParse(value, out float effectDuration))
                        statData.effectDuration = effectDuration;
                    break;
                case "cooldown":
                    if (float.TryParse(value, out float cooldown))
                        statData.cooldown = cooldown;
                    break;
                case "triggerchance":
                    if (float.TryParse(value, out float triggerChance))
                        statData.triggerChance = triggerChance;
                    break;
                case "homingactivate":
                    if (bool.TryParse(value, out bool homingActivate))
                        statData.homingActivate = homingActivate;
                    break;
                case "damageincrease":
                    if (float.TryParse(value, out float damageIncrease))
                        statData.damageIncrease = damageIncrease;
                    break;
                case "defenseincrease":
                    if (float.TryParse(value, out float defenseIncrease))
                        statData.defenseIncrease = defenseIncrease;
                    break;
                case "expareaincrease":
                    if (float.TryParse(value, out float expAreaIncrease))
                        statData.expAreaIncrease = expAreaIncrease;
                    break;
                case "hpincrease":
                    if (float.TryParse(value, out float hpIncrease))
                        statData.hpIncrease = hpIncrease;
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