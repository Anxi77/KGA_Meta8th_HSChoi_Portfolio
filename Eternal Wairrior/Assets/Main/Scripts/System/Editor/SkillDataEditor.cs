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
    private bool hasUnsavedChanges = false;

    private Stack<SkillDataState> undoStack = new Stack<SkillDataState>();
    private Stack<SkillDataState> redoStack = new Stack<SkillDataState>();

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
        DrawToolbar();
        EditorGUI.BeginChangeCheck();

        DrawMainContent();

        if (EditorGUI.EndChangeCheck())
        {
            hasUnsavedChanges = true;
        }

        DrawSaveButton();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Undo", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            Undo();
        }

        if (GUILayout.Button("Redo", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            Redo();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSaveButton()
    {
        if (hasUnsavedChanges)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("You have unsaved changes!", MessageType.Warning);

            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                SaveChanges();
                hasUnsavedChanges = false;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void OnLostFocus()
    {
        if (hasUnsavedChanges)
        {
            if (EditorUtility.DisplayDialog("Unsaved Changes",
                "You have unsaved changes. Would you like to save them?",
                "Save", "Discard"))
            {
                SaveChanges();
            }
            hasUnsavedChanges = false;
        }
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
        float listHeight = position.height - 150f; // 상단 여백 고려
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition,
            GUILayout.Width(200), GUILayout.Height(listHeight));

        if (skillList != null)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                DrawSkillListItem(skillList[i], i);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSkillListItem(SkillData skill, int index)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        GUI.backgroundColor = currentSkill == skill ? Color.cyan : Color.white;
        if (GUILayout.Button(skill.metadata.Name, GUILayout.Height(30)))
        {
            currentSkill = skill;
            GUI.FocusControl(null); // 포커스 초기화
        }

        if (GUILayout.Button("⋮", GUILayout.Width(20), GUILayout.Height(30)))
        {
            ShowSkillContextMenu(skill);
        }

        EditorGUILayout.EndHorizontal();
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

    // õ  
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
        stats.isPersistant = EditorGUILayout.Toggle("isPersistant", stats.isPersistant);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);

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
        if (currentSkill == null) return;

        EditorGUILayout.Space(10);
        showLevelStats = EditorGUILayout.Foldout(showLevelStats, "Level Stats");

        if (!showLevelStats) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 레벨 추가 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Level", GUILayout.Width(100)))
        {
            if (EditorUtility.DisplayDialog("Add Level",
                "Are you sure you want to add a new level?", "Add", "Cancel"))
            {
                AddNewLevelStat();
            }
        }

        if (GUILayout.Button("Remove Last Level", GUILayout.Width(120)))
        {
            if (EditorUtility.DisplayDialog("Remove Level",
                "Are you sure you want to remove the last level?", "Remove", "Cancel"))
            {
                RemoveLastLevel();
            }
        }
        EditorGUILayout.EndHorizontal();

        // 현재 스킬의 레벨별 스탯 가져오기
        var skillStats = skillStatsList.ContainsKey(currentSkill.metadata.ID)
            ? skillStatsList[currentSkill.metadata.ID]
            : new List<SkillStatData>();

        // 레벨별 스탯 표시
<<<<<<< HEAD
        for (int i = 0; i < skillStats.Count; i++)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var levelStat = skillStats[i];
            EditorGUILayout.LabelField($"Level {levelStat.level}", EditorStyles.boldLabel);

            // 기본 스탯 표시
            levelStat.damage = EditorGUILayout.FloatField("Damage", levelStat.damage);
            levelStat.elementalPower = EditorGUILayout.FloatField("Elemental Power", levelStat.elementalPower);

            // 스킬 타입별 특수 스탯 표시
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
=======
        statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);

        for (int level = 1; level <= currentSkill.GetMaxLevel(); level++)
        {
            DrawLevelStatGroup(level);
>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawLevelStatGroup(int level)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Level {level}", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Reset Level",
                $"Reset level {level} stats to default?", "Reset", "Cancel"))
            {
                ResetLevelStats(level);
            }
        }
        EditorGUILayout.EndHorizontal();

        var stats = currentSkill.GetStatsForLevel(level);
        if (stats != null)
        {
            EditorGUI.indentLevel++;
            DrawStatsForType(stats, currentSkill.metadata.Type);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    // 레벨별 스탯 표시를 위한 새로운 메서드들
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

    private void DrawPassiveStatsForLevel(SkillStatData stat)
    {
        stat.effectDuration = EditorGUILayout.FloatField("Effect Duration", stat.effectDuration);
        stat.cooldown = EditorGUILayout.FloatField("Cooldown", stat.cooldown);
        stat.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stat.triggerChance);
    }

    // AddNewLevelStat 메서드 수정
    private void AddNewLevelStat()
    {
<<<<<<< HEAD
        if (currentSkill == null) return;

        // 스킬 스탯 리스트가 없으면 초기화
        if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
        {
            skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();
        }

        var levelStats = skillStatsList[currentSkill.metadata.ID];

        // 새로운 스탯 생성
        var newStat = new SkillStatData
        {
            skillID = currentSkill.metadata.ID,
            level = levelStats.Count + 1,
            element = currentSkill.metadata.Element
        };

        // 이전 레벨의 스탯을 기반으로 새 스탯 생성
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

        // 변경사항 저장
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
=======
        try
        {
            if (currentSkill == null)
            {
                EditorUtility.DisplayDialog("Error", "No skill selected", "OK");
                return;
            }

            var levelStats = skillStatsList[currentSkill.metadata.ID];
            int newLevel = levelStats.Count + 1;

            // 최대 레벨 체크
            if (newLevel > currentSkill.GetCurrentTypeStat()?.baseStat?.maxSkillLevel)
            {
                EditorUtility.DisplayDialog("Error", "Cannot exceed max skill level", "OK");
                return;
            }

            var newStat = new SkillStatData
            {
                skillID = currentSkill.metadata.ID,
                level = newLevel
            };

            // 이전 레벨의 스탯을 기반으로 새 스탯 생성
            if (levelStats.Count > 0)
            {
                var prevStat = levelStats[levelStats.Count - 1];
                CopyAndScaleStats(prevStat, newStat);
            }

            levelStats.Add(newStat);
            EditorUtility.SetDirty(currentSkill);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to add new level stat: {e.Message}");
            EditorUtility.DisplayDialog("Error", "Failed to add new level stat", "OK");
        }
    }

    private void CopyAndScaleStats(SkillStatData source, SkillStatData target)
    {
        // 기본 스탯 복사 및 스케일링
        target.damage = source.damage * 1.1f;
        target.maxSkillLevel = source.maxSkillLevel;
        target.element = source.element;
        target.elementalPower = source.elementalPower * 1.05f;

        // 스킬 타입별 스탯 복사
        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                CopyProjectileStats(source, target);
                break;
            case SkillType.Area:
                CopyAreaStats(source, target);
                break;
            case SkillType.Passive:
                CopyPassiveStats(source, target);
                break;
        }
>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
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
        to.duration = from.duration;
        to.isPersistent = from.isPersistent;
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

        // 헤더에 새로운 필드 추가
        csv.AppendLine("SkillID,Level,Damage,MaxSkillLevel,Element,ElementalPower," +
                      "ProjectileSpeed,ProjectileScale,ShotInterval,PierceCount,AttackRange," +
                      "HomingRange,IsHoming,ExplosionRad,ProjectileCount,InnerInterval," +
                      "ProjectilePersistent,ProjectileDuration");

        // 데이터 저장 부분에도 새로운 필드 추가
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
                                 $"{stat.explosionRad},{stat.projectileCount},{stat.innerInterval}," +
                                 $"{stat.projectilePersistent},{stat.projectileDuration}");
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
                      "Radius,AreaDuration,TickRate,AreaPersistent,MoveSpeed");

        // Save area skill data
        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Area)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.radius}," +
                                 $"{stat.areaDuration},{stat.tickRate},{stat.areaPersistent},{stat.moveSpeed}");
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
        LoadSkillData();


        LoadSkillStatsData();

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
            // JSON 파일에서 직접 로드
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
                        // SkillDataManager 업데이트
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

            // 메타데이터 유효성 검사 및 초기화
            foreach (var skill in skillList.ToList()) // ToList()로 복사본 생성
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
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
<<<<<<< HEAD
            // SkillDataManager에 현재 skillList 전달
            skillDataManager.UpdateSkillList(skillList);

            // 에디터에서 저장 실행
            skillDataManager.SaveAllSkillDataInEditor();
            EditorUtility.SetDirty(skillDataManager);

            // JSON 저장
=======
            skillDataManager.SaveAllSkillData();
            EditorUtility.SetDirty(skillDataManager);
            AssetDatabase.SaveAssets();

>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);

<<<<<<< HEAD
            // 메타데이터를 명시적으로 포함하여 저장
            var skillsToSave = skillList.Select(skill => new SkillData
            {
                metadata = new SkillMetadata
                {
                    Name = skill.metadata.Name,
                    Description = skill.metadata.Description,
                    Type = skill.metadata.Type,
                    ID = skill.metadata.ID,
                    Element = skill.metadata.Element,
                    Tier = skill.metadata.Tier,
                    Tags = skill.metadata.Tags?.ToArray() ?? new string[0]
                },
                icon = skill.icon,
                projectile = skill.projectile,
                prefabsByLevel = skill.prefabsByLevel,
                projectileStat = skill.projectileStat,
                areaStat = skill.areaStat,
                passiveStat = skill.passiveStat
            }).ToList();

            SkillDataWrapper wrapper = new SkillDataWrapper
            {
                skillDatas = skillsToSave
            };

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            AssetDatabase.SaveAssets();
=======
            // JSON 저장
            string jsonPath = Path.Combine(directory, "SkillData.json");
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // CSV 저장
            SaveSkillStatsToCSV();

>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
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

        SaveSkillData(); // 새 스킬 생성 즉시 저장
        Debug.Log($"Created new skill: {newSkill.metadata.Name} (ID: {newSkill.metadata.ID})");
    }

    // 새로운 메서드 추가: 사용 가능한 다음 SkillID 찾기
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
        return SkillID.None; // 모든 ID가 사용 중인 경우
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
                case "duration":
                    if (float.TryParse(value, out float duration))
                        statData.duration = duration;
                    break;
                case "ispersistant":
                    if (bool.TryParse(value, out bool isPersistent))
                        statData.isPersistent = isPersistent;
                    break;
                case "projectilepersistent":
                    if (bool.TryParse(value, out bool projPersistent))
                        statData.projectilePersistent = projPersistent;
                    break;
                case "projectileduration":
                    if (float.TryParse(value, out float projDuration))
                        statData.projectileDuration = projDuration;
                    break;
                case "areapersistent":
                    if (bool.TryParse(value, out bool areaPersistent))
                        statData.areaPersistent = areaPersistent;
                    break;
                case "areaduration":
                    if (float.TryParse(value, out float areaDuration))
                        statData.areaDuration = areaDuration;
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

    private void DrawPersistenceSettings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Persistence Settings", headerStyle);
        EditorGUI.indentLevel++;

        if (currentSkill?.GetCurrentTypeStat() is ProjectileSkillStat projectileStats)
        {
            var persistenceData = projectileStats.persistenceData;
            if (persistenceData == null)
            {
                persistenceData = new ProjectilePersistenceData();
                projectileStats.persistenceData = persistenceData;
            }

            EditorGUI.BeginChangeCheck();
            persistenceData.isPersistent = EditorGUILayout.Toggle("Is Persistent", persistenceData.isPersistent);

            if (persistenceData.isPersistent)
            {
                EditorGUI.indentLevel++;
                persistenceData.duration = EditorGUILayout.FloatField("Duration", persistenceData.duration);
                persistenceData.effectInterval = EditorGUILayout.FloatField("Effect Interval", persistenceData.effectInterval);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentSkill);
            }
        }

        EditorGUI.indentLevel--;
    }

    // 에러 처리 강화
    private void ValidateSkillData()
    {
        if (currentSkill == null) return;

        var stats = currentSkill.GetCurrentTypeStat();
        if (stats == null)
        {
            EditorGUILayout.HelpBox("Invalid skill stats!", MessageType.Error);
            return;
        }

        // 값 유효성 검사
        if (stats is ProjectileSkillStat projectileStats)
        {
            if (projectileStats.projectileSpeed <= 0)
                EditorGUILayout.HelpBox("Projectile speed must be greater than 0", MessageType.Warning);

            if (projectileStats.persistenceData?.duration < 0)
                EditorGUILayout.HelpBox("Duration cannot be negative", MessageType.Warning);
        }
    }

    private class SkillDataState
    {
        public SkillData skillData;
        public Dictionary<SkillID, List<SkillStatData>> skillStats;

        public SkillDataState(SkillData data, Dictionary<SkillID, List<SkillStatData>> stats)
        {
            skillData = JsonUtility.FromJson<SkillData>(JsonUtility.ToJson(data));
            skillStats = new Dictionary<SkillID, List<SkillStatData>>();
            foreach (var pair in stats)
            {
                skillStats[pair.Key] = new List<SkillStatData>(pair.Value);
            }
        }
    }

    private void SaveState()
    {
        undoStack.Push(new SkillDataState(currentSkill, skillStatsList));
        redoStack.Clear();
    }

    private void Undo()
    {
        if (undoStack.Count > 0)
        {
            redoStack.Push(new SkillDataState(currentSkill, skillStatsList));
            var state = undoStack.Pop();
            RestoreState(state);
        }
    }

    private void Redo()
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(new SkillDataState(currentSkill, skillStatsList));
            var state = redoStack.Pop();
            RestoreState(state);
        }
    }

    private void RestoreState(SkillDataState state)
    {
        currentSkill = state.skillData;
        skillStatsList = state.skillStats;
        Repaint();
    }

    private void DrawBulkEditTools()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Bulk Edit Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scale All Damage"))
        {
            float scale = EditorUtility.DisplayDialog("Scale Damage",
                "Enter scale factor (e.g., 1.1 for 10% increase):", "Scale", "Cancel");
            if (scale > 0)
            {
                ScaleAllDamage(scale);
            }
        }

        if (GUILayout.Button("Adjust All Levels"))
        {
            BulkLevelAdjustment();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Stats to Level"))
        {
            CopyStatsToLevel();
        }

        if (GUILayout.Button("Reset All to Default"))
        {
            if (EditorUtility.DisplayDialog("Reset All",
                "Are you sure you want to reset all skills to default values?",
                "Reset", "Cancel"))
            {
                ResetAllToDefault();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void ScaleAllDamage(float scale)
    {
        SaveState(); // Undo 지원

        foreach (var skillData in skillList)
        {
            var stats = skillData.GetCurrentTypeStat();
            if (stats?.baseStat != null)
            {
                stats.baseStat.damage *= scale;
            }
        }

        EditorUtility.SetDirty(FindObjectOfType<SkillDataManager>());
    }

    private void BulkLevelAdjustment()
    {
        SaveState();

        int newMaxLevel = EditorUtility.DisplayDialogComplex("Adjust Levels",
            "Choose level adjustment option:",
            "Set Max Level",
            "Add Level to All",
            "Remove Last Level");

        switch (newMaxLevel)
        {
            case 0: // Set Max Level
                SetMaxLevelForAll();
                break;
            case 1: // Add Level
                AddLevelToAll();
                break;
            case 2: // Remove Level
                RemoveLastLevelFromAll();
                break;
        }
    }

    private void DrawValidationFeedback()
    {
        if (currentSkill == null) return;

        var validationResult = SkillDataValidator.ValidateSkillData(currentSkill);

        if (validationResult.HasErrors || validationResult.HasWarnings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (validationResult.HasErrors)
            {
                EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                foreach (var error in validationResult.Errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }

            if (validationResult.HasWarnings)
            {
                EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);
                foreach (var warning in validationResult.Warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }

            if (GUILayout.Button("Fix All Auto-fixable Issues"))
            {
                AutoFixIssues(validationResult);
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void AutoFixIssues(ValidationResult result)
    {
        SaveState();

        if (currentSkill != null)
        {
            // 자동 수정 가능한 문제들 처리
            if (string.IsNullOrEmpty(currentSkill.metadata.Name))
                currentSkill.metadata.Name = "New Skill";

            if (currentSkill.metadata.Tier <= 0)
                currentSkill.metadata.Tier = 1;

            var stats = currentSkill.GetCurrentTypeStat();
            if (stats is ProjectileSkillStat projectileStats)
            {
                if (projectileStats.projectileSpeed <= 0)
                    projectileStats.projectileSpeed = 10f;

                if (projectileStats.shotInterval <= 0)
                    projectileStats.shotInterval = 0.5f;
            }

            EditorUtility.SetDirty(currentSkill);
        }
    }
}