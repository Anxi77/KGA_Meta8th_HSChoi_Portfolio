using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class SkillDataEditor : EditorWindow
{
    private SkillData currentSkill;
    private List<SkillData> skillList;
    private bool showBaseStats = true;
    private bool showLevelStats = true;
    private GUIStyle headerStyle;
    private Vector2 statsScrollPosition;
    private Vector2 scrollPosition;

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
            SkillDataManager.Instance.ClearAllData();
            window.LoadAllData();
        }
    }

    private void OnEnable()
    {
        InitializeStyles();
        LoadAllData();
        EditorApplication.quitting += OnEditorQuitting;
    }

    private void OnDisable()
    {
        EditorApplication.quitting -= OnEditorQuitting;
        SaveAllData();
    }

    private void OnEditorQuitting()
    {
        SaveAllData();
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

    private void LoadAllData()
    {
        if (SkillDataManager.Instance != null)
        {
            skillList = SkillDataManager.Instance.GetAllSkillData();
            Repaint();
        }
    }

    private void SaveAllData()
    {
        if (SkillDataManager.Instance != null)
        {
            SkillDataManager.Instance.SaveAllSkillData();
        }
    }

    private void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            try
            {
                DrawBottomPanel();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawBottomPanel: {e.Message}");
                EditorGUILayout.HelpBox("Error drawing bottom panel", MessageType.Error);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnGUI: {e.Message}");
            EditorGUIUtility.ExitGUI();
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

                    if (Event.current.button == 1)
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

    private void DeleteCurrentSkill()
    {
        if (currentSkill == null) return;
        SkillDataManager.Instance.DeleteSkill(currentSkill);
        currentSkill = null;
        skillList = SkillDataManager.Instance.GetAllSkillData();
    }

    private void DeleteSkill(SkillData skill)
    {
        SkillDataManager.Instance.DeleteSkill(skill);
        currentSkill = null;
        LoadAllData();
    }
    #endregion

    #region Right Panel
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
            SkillDataManager.Instance.UpdateSkillData(currentSkill);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // DrawBasicInfo, DrawPrefabSettings, DrawSkillStats, DrawLevelStats 메서드들은
    // UI 표시 로직만 포함하도록 수정되어야 합니다.
    // 각 메서드 내에서 데이터 변경이 발생할 때마다 SkillDataManager.Instance.UpdateSkillData(currentSkill)를 호출합니다.
    #endregion

    #region Bottom Panel
    private void DrawBottomPanel()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal(GUILayout.Height(40));

        if (GUILayout.Button("New Skill", GUILayout.Height(30)))
        {
            currentSkill = SkillDataManager.Instance.CreateNewSkill();
            LoadAllData();
        }

        if (GUILayout.Button("Save All", GUILayout.Height(30)))
        {
            SkillDataManager.Instance.SaveAllSkillData();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Data",
                "Are you sure you want to clear all skill data? This cannot be undone.",
                "Clear", "Cancel"))
            {
                SkillDataManager.Instance.ClearAllData();
                LoadAllData();
            }
        }

        if (GUILayout.Button("Export JSON", GUILayout.Height(30)))
            SkillDataManager.Instance.ExportToJson();

        if (GUILayout.Button("Import JSON", GUILayout.Height(30)))
        {
            SkillDataManager.Instance.ImportFromJson();
            LoadAllData();
        }

        EditorGUILayout.EndHorizontal();
    }
    #endregion

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
            SkillDataManager.Instance.InitializeSkillStats(currentSkill);
        }

        SkillID oldID = currentSkill.metadata.ID;
        SkillID newID = (SkillID)EditorGUILayout.EnumPopup("Skill ID", currentSkill.metadata.ID);
        if (newID != oldID)
        {
            SkillDataManager.Instance.UpdateSkillID(currentSkill, oldID, newID);
            currentSkill.metadata.ID = newID;
        }

        currentSkill.metadata.Element = (ElementType)EditorGUILayout.EnumPopup("Element Type", currentSkill.metadata.Element);
        currentSkill.metadata.Tier = EditorGUILayout.IntField("Tier", currentSkill.metadata.Tier);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillData(currentSkill);
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

        // 아이콘 프리뷰 및 설정
        if (currentSkill.icon != null)
        {
            Rect previewRect = EditorGUILayout.GetControlRect(false, 64);
            previewRect.width = 64;
            EditorGUI.DrawPreviewTexture(previewRect, currentSkill.icon.texture);

            Rect fieldRect = EditorGUILayout.GetControlRect();
            fieldRect.x += 70;
            fieldRect.width -= 70;

            EditorGUI.BeginChangeCheck();
            Sprite newIcon = (Sprite)EditorGUI.ObjectField(
                fieldRect,
                currentSkill.icon,
                typeof(Sprite),
                false
            );

            if (EditorGUI.EndChangeCheck() && newIcon != currentSkill.icon)
            {
                SkillDataManager.Instance.SaveIconToResources(newIcon, currentSkill.metadata.ID);
                currentSkill.icon = newIcon;
                currentSkill.metadata.Icon = newIcon;
            }
        }
        else
        {
            currentSkill.icon = (Sprite)EditorGUILayout.ObjectField(
                "Icon",
                null,
                typeof(Sprite),
                false
            );
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 프리팹 설정 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);

        // 메타데이터 프리팹
        GameObject newMetadataPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Metadata Prefab",
            currentSkill.metadata.Prefab,
            typeof(GameObject),
            false
        );

        if (newMetadataPrefab != currentSkill.metadata.Prefab)
        {
            SkillDataManager.Instance.SavePrefabToResources(newMetadataPrefab, currentSkill.metadata.ID, "Metadata");
            currentSkill.metadata.Prefab = newMetadataPrefab;
        }

        // 프로젝타일 타입일 때만 프로젝타일 프리팹 필드 표시
        if (currentSkill.metadata.Type == SkillType.Projectile)
        {
            EditorGUILayout.Space(5);
            GameObject newProjectilePrefab = (GameObject)EditorGUILayout.ObjectField(
                "Projectile Prefab",
                currentSkill.projectile,
                typeof(GameObject),
                false
            );

            if (newProjectilePrefab != currentSkill.projectile)
            {
                SkillDataManager.Instance.SavePrefabToResources(newProjectilePrefab, currentSkill.metadata.ID, "Projectile");
                currentSkill.projectile = newProjectilePrefab;
            }
        }

        // 레벨별 프리팹 설정
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);

        // 레벨별 프리팹 배열 크기 설정
        int newSize = EditorGUILayout.IntField("Level Count", currentSkill.prefabsByLevel?.Length ?? 0);
        if (newSize != currentSkill.prefabsByLevel?.Length)
        {
            System.Array.Resize(ref currentSkill.prefabsByLevel, newSize);
        }

        // 각 레벨별 프리팹 설정
        if (currentSkill.prefabsByLevel != null)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GameObject newLevelPrefab = (GameObject)EditorGUILayout.ObjectField(
                    $"Level {i + 1}",
                    currentSkill.prefabsByLevel[i],
                    typeof(GameObject),
                    false
                );

                if (newLevelPrefab != currentSkill.prefabsByLevel[i])
                {
                    SkillDataManager.Instance.SavePrefabToResources(newLevelPrefab, currentSkill.metadata.ID, $"Level_{i + 1}");
                    currentSkill.prefabsByLevel[i] = newLevelPrefab;
                }
                EditorGUILayout.EndHorizontal();
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

            var skillStats = SkillDataManager.Instance.GetSkillStats(currentSkill.metadata.ID)
                .FirstOrDefault(s => s.level == 1);

            if (skillStats == null)
            {
                Debug.LogError($"Failed to get stats for skill {currentSkill.metadata.ID}");
                return;
            }

            EditorGUI.BeginChangeCheck();

            DrawBaseStats(skillStats);
            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    DrawProjectileStats(skillStats);
                    break;
                case SkillType.Area:
                    DrawAreaStats(skillStats);
                    break;
                case SkillType.Passive:
                    DrawPassiveStats(skillStats);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                SkillDataManager.Instance.UpdateSkillData(currentSkill);
                EditorUtility.SetDirty(SkillDataManager.Instance);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error drawing skill stats: {e.Message}");
        }

        EditorGUI.indentLevel--;
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
            SkillDataManager.Instance.AddSkillLevel(currentSkill.metadata.ID);
            LoadAllData();
        }
        EditorGUILayout.EndHorizontal();

        var skillStats = SkillDataManager.Instance.GetSkillStats(currentSkill.metadata.ID);
        foreach (var stat in skillStats)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Level {stat.level}", EditorStyles.boldLabel);

            if (stat.level > 1)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Level",
                        $"Are you sure you want to delete level {stat.level}?",
                        "Delete", "Cancel"))
                    {
                        SkillDataManager.Instance.DeleteSkillLevel(currentSkill.metadata.ID, stat.level);
                        LoadAllData();
                        break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 스킬 타입에 따른 스탯 UI 표시
            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    DrawProjectileStatsForLevel(stat);
                    break;
                case SkillType.Area:
                    DrawAreaStatsForLevel(stat);
                    break;
                case SkillType.Passive:
                    DrawPassiveStatsForLevel(stat);
                    break;
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawBaseStats(SkillStatData stats)
    {
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.maxSkillLevel = EditorGUILayout.IntField("Max Level", stats.maxSkillLevel);
        stats.element = (ElementType)EditorGUILayout.EnumPopup("Element", stats.element);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }

        EditorGUI.indentLevel--;
    }

    private void DrawProjectileStats(SkillStatData stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

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

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }

        EditorGUI.indentLevel--;
    }

    private void DrawAreaStats(SkillStatData stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        stats.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        stats.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        stats.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        stats.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }

        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStats(SkillStatData stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        stats.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        stats.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        stats.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);
        stats.damageIncrease = EditorGUILayout.FloatField("Damage Increase (%)", stats.damageIncrease);
        stats.defenseIncrease = EditorGUILayout.FloatField("Defense Increase (%)", stats.defenseIncrease);
        stats.expAreaIncrease = EditorGUILayout.FloatField("Exp Area Increase (%)", stats.expAreaIncrease);
        stats.homingActivate = EditorGUILayout.Toggle("Homing Activate", stats.homingActivate);
        stats.hpIncrease = EditorGUILayout.FloatField("HP Increase (%)", stats.hpIncrease);
        stats.moveSpeedIncrease = EditorGUILayout.FloatField("Move Speed Increase (%)", stats.moveSpeedIncrease);
        stats.attackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed Increase (%)", stats.attackSpeedIncrease);
        stats.attackRangeIncrease = EditorGUILayout.FloatField("Attack Range Increase (%)", stats.attackRangeIncrease);
        stats.hpRegenIncrease = EditorGUILayout.FloatField("HP Regen Increase (%)", stats.hpRegenIncrease);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }

        EditorGUI.indentLevel--;
    }

    private void DrawProjectileStatsForLevel(SkillStatData stats)
    {
        EditorGUI.BeginChangeCheck();

        // 기본 스탯
        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

        // 프로젝타일 스탯
        stats.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", stats.projectileSpeed);
        stats.projectileScale = EditorGUILayout.FloatField("Projectile Scale", stats.projectileScale);
        stats.shotInterval = EditorGUILayout.FloatField("Shot Interval", stats.shotInterval);
        stats.pierceCount = EditorGUILayout.IntField("Pierce Count", stats.pierceCount);
        stats.attackRange = EditorGUILayout.FloatField("Attack Range", stats.attackRange);
        stats.homingRange = EditorGUILayout.FloatField("Homing Range", stats.homingRange);
        stats.isHoming = EditorGUILayout.Toggle("Is Homing", stats.isHoming);
        stats.explosionRad = EditorGUILayout.FloatField("Explosion Radius", stats.explosionRad);
        stats.projectileCount = EditorGUILayout.IntField("Projectile Count", stats.projectileCount);
        stats.innerInterval = EditorGUILayout.FloatField("Inner Interval", stats.innerInterval);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }
    }

    private void DrawAreaStatsForLevel(SkillStatData stats)
    {
        EditorGUI.BeginChangeCheck();

        // 기본 스탯
        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

        // 에어리어 탯
        stats.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        stats.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        stats.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        stats.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }
    }

    private void DrawPassiveStatsForLevel(SkillStatData stats)
    {
        EditorGUI.BeginChangeCheck();

        // 기본 스탯
        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

        // 패시브 스탯
        stats.effectDuration = EditorGUILayout.FloatField("Effect Duration", stats.effectDuration);
        stats.cooldown = EditorGUILayout.FloatField("Cooldown", stats.cooldown);
        stats.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stats.triggerChance);
        stats.damageIncrease = EditorGUILayout.FloatField("Damage Increase (%)", stats.damageIncrease);
        stats.defenseIncrease = EditorGUILayout.FloatField("Defense Increase (%)", stats.defenseIncrease);
        stats.expAreaIncrease = EditorGUILayout.FloatField("Exp Area Increase (%)", stats.expAreaIncrease);
        stats.homingActivate = EditorGUILayout.Toggle("Homing Activate", stats.homingActivate);
        stats.hpIncrease = EditorGUILayout.FloatField("HP Increase (%)", stats.hpIncrease);
        stats.moveSpeedIncrease = EditorGUILayout.FloatField("Move Speed Increase (%)", stats.moveSpeedIncrease);
        stats.attackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed Increase (%)", stats.attackSpeedIncrease);
        stats.attackRangeIncrease = EditorGUILayout.FloatField("Attack Range Increase (%)", stats.attackRangeIncrease);
        stats.hpRegenIncrease = EditorGUILayout.FloatField("HP Regen Increase (%)", stats.hpRegenIncrease);

        if (EditorGUI.EndChangeCheck())
        {
            SkillDataManager.Instance.UpdateSkillStats(currentSkill.metadata.ID, new List<SkillStatData> { stats });
        }
    }

    // GUI 업데이트 시 자동 저장 추가
    private void OnInspectorUpdate()
    {
        if (GUI.changed)
        {
            SaveAllData();
        }
    }
}