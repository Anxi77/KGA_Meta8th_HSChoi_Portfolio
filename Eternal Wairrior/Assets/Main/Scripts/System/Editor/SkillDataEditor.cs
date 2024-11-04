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
    private Vector2 scrollPosition;
    private SkillData currentSkill;
    private List<SkillData> skillList;
    private Dictionary<SkillID, List<SkillStatData>> skillStatsList;
    private bool showBaseStats = true;
    private bool showLevelStats = true;
    private GUIStyle headerStyle;
    private Vector2 statsScrollPosition;
    private string RESOURCE_PATH => SkillDataManager.ResourcePath;
    private string PREFAB_PATH => SkillDataManager.PrefabPath;
    private string ICON_PATH => SkillDataManager.IconPath;

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
        CreateResourceFolders();
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

    private void OnGUI()
    {
        try
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            // Bottom Panel은 별도의 try-catch로 감싸서 처리
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
            // 모든 GUILayout 상태를 리셋
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

                    // 우클릭 메뉴 처리
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
        if (currentSkill != null)
        {
            DeleteSkill(currentSkill);
            currentSkill = null;
        }
    }

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
                SaveIconToResources(newIcon, currentSkill.metadata.ID);
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
            SavePrefabToResources(newMetadataPrefab, currentSkill.metadata.ID, "Metadata");
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
                SavePrefabToResources(newProjectilePrefab, currentSkill.metadata.ID, "Projectile");
                currentSkill.projectile = newProjectilePrefab;
            }
        }

        // 레벨별 프리팹
        EditorGUILayout.Space(5);
        showBaseStats = EditorGUILayout.Foldout(showBaseStats, "Level Prefabs", true);
        if (showBaseStats)
        {
            EditorGUI.indentLevel++;

            int size = EditorGUILayout.IntField("Size", currentSkill.prefabsByLevel?.Length ?? 0);
            if (size != currentSkill.prefabsByLevel?.Length)
            {
                System.Array.Resize(ref currentSkill.prefabsByLevel, size);
            }

            if (currentSkill.prefabsByLevel != null)
            {
                for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
                {
                    GameObject levelPrefab = (GameObject)EditorGUILayout.ObjectField(
                        $"Level {i + 1}",
                        currentSkill.prefabsByLevel[i],
                        typeof(GameObject),
                        false
                    );

                    if (levelPrefab != currentSkill.prefabsByLevel[i])
                    {
                        SavePrefabToResources(levelPrefab, currentSkill.metadata.ID, $"Level_{i + 1}");
                        currentSkill.prefabsByLevel[i] = levelPrefab;
                    }
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

            // skillStatsList가 null이면 초기화
            if (skillStatsList == null)
            {
                skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
            }

            // 현재 스킬의 스탯 리스트가 없으면 생성
            if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
            {
                skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();

                // 새로운 스킬의 경우 기본 스탯 생성
                var defaultStat = new SkillStatData
                {
                    skillID = currentSkill.metadata.ID,
                    level = 1
                };
                InitializeDefaultStats(defaultStat);
                skillStatsList[currentSkill.metadata.ID].Add(defaultStat);
            }

            // 현재 스킬의 레벨 1 스탯 데이터 가져오기
            var skillStats = skillStatsList[currentSkill.metadata.ID].FirstOrDefault(s => s.level == 1);
            if (skillStats == null)
            {
                Debug.LogError($"Failed to get stats for skill {currentSkill.metadata.ID}");
                return;
            }

            EditorGUI.BeginChangeCheck();

            // Base Stats 그리기
            DrawBaseStats(skillStats);

            // 스킬 타입별 스탯 그리기
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
                // 변경사항이 있을 때만 저장
                SaveSkillData();

                // SkillData에도 변경사항 반영
                var skillStat = skillStats.CreateSkillStat(currentSkill.metadata.Type);
                currentSkill.SetStatsForLevel(1, skillStat);

                // SkillDataManager를 찾아서 SetDirty 호출
                var skillDataManager = FindObjectOfType<SkillDataManager>();
                if (skillDataManager != null)
                {
                    EditorUtility.SetDirty(skillDataManager);
                    skillDataManager.UpdateSkillStatsData(skillStatsList);
                }

                AssetDatabase.SaveAssets();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error drawing skill stats: {e.Message}\n{e.StackTrace}");
        }

        EditorGUI.indentLevel--;
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
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager != null)
            {
                EditorUtility.SetDirty(skillDataManager);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void DrawProjectileStats(SkillStatData stats)
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

    private void DrawAreaStats(SkillStatData stats)
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

    private void DrawPassiveStats(SkillStatData stats)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

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
            AddNewLevelStat();
        }
        EditorGUILayout.EndHorizontal();

        var skillStats = skillStatsList[currentSkill.metadata.ID];
        for (int i = 0; i < skillStats.Count; i++)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Level {skillStats[i].level}", EditorStyles.boldLabel);

            // 레벨 1은 삭제 불가능
            if (skillStats[i].level > 1)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Level",
                        $"Are you sure you want to delete level {skillStats[i].level}?",
                        "Delete", "Cancel"))
                    {
                        skillStats.RemoveAt(i);
                        SaveSkillData();
                        break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 스킬 타입에 따라 다른 스탯 UI 표시
            switch (currentSkill.metadata.Type)
            {
                case SkillType.Projectile:
                    DrawProjectileStatsForLevel(skillStats[i]);
                    break;
                case SkillType.Area:
                    DrawAreaStatsForLevel(skillStats[i]);
                    break;
                case SkillType.Passive:
                    DrawPassiveStatsForLevel(skillStats[i]);
                    break;
            }

            EditorGUILayout.EndVertical();
        }
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
            SaveSkillData();
        }
    }

    private void DrawAreaStatsForLevel(SkillStatData stats)
    {
        EditorGUI.BeginChangeCheck();

        // 기본 스탯
        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

        // 에어리어 스탯
        stats.radius = EditorGUILayout.FloatField("Radius", stats.radius);
        stats.duration = EditorGUILayout.FloatField("Duration", stats.duration);
        stats.tickRate = EditorGUILayout.FloatField("Tick Rate", stats.tickRate);
        stats.isPersistent = EditorGUILayout.Toggle("Is Persistent", stats.isPersistent);
        stats.moveSpeed = EditorGUILayout.FloatField("Move Speed", stats.moveSpeed);

        if (EditorGUI.EndChangeCheck())
        {
            SaveSkillData();
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
            SaveSkillData();
        }
    }

    private void AddNewLevelStat()
    {
        if (currentSkill == null) return;

        if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
        {
            skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();
        }

        var levelStats = skillStatsList[currentSkill.metadata.ID];
        int newLevel = levelStats.Count > 0 ? levelStats.Max(s => s.level) + 1 : 1;

        // 이전 레벨의 스탯을 복사
        var newStat = levelStats.Count > 0
            ? levelStats[levelStats.Count - 1].Clone()
            : new SkillStatData();

        // 새 레의 기본 탯 증가
        if (levelStats.Count > 0)
        {
            newStat.damage *= 1.2f;
            newStat.elementalPower *= 1.1f;
            Debug.Log($"New level stat - Elemental Power: {newStat.elementalPower} (increased by 10%)");
        }

        newStat.level = newLevel;
        newStat.skillID = currentSkill.metadata.ID;

        // 나머지 스탯 처리...

        levelStats.Add(newStat);
        SaveSkillData();
    }

    private void CopyStatsFromPrevious(SkillStatData from, SkillStatData to)
    {
        to.damage = from.damage * 1.1f;
        to.maxSkillLevel = from.maxSkillLevel;
        to.element = currentSkill.metadata.Element;
        to.elementalPower = from.elementalPower * 1.1f;

        // 스킬 타입별로 스탯 복사
        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
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
                break;

            case SkillType.Area:
                to.radius = from.radius;
                to.duration = from.duration;
                to.tickRate = from.tickRate;
                to.isPersistent = from.isPersistent;
                to.moveSpeed = from.moveSpeed;
                break;

            case SkillType.Passive:
                to.effectDuration = from.effectDuration;
                to.cooldown = from.cooldown;
                to.triggerChance = from.triggerChance;
                to.damageIncrease = from.damageIncrease;
                to.defenseIncrease = from.defenseIncrease;
                to.expAreaIncrease = from.expAreaIncrease;
                to.homingActivate = from.homingActivate;
                to.hpIncrease = from.hpIncrease;
                to.moveSpeedIncrease = from.moveSpeedIncrease;
                to.attackSpeedIncrease = from.attackSpeedIncrease;
                to.attackRangeIncrease = from.attackRangeIncrease;
                to.hpRegenIncrease = from.hpRegenIncrease;
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

    private void InitializeDefaultProjectileStats(SkillStatData stats)
    {
        stats.projectileSpeed = 10f;
        stats.projectileScale = 1f;
        stats.shotInterval = 1f;
        stats.pierceCount = 1;
        stats.attackRange = 10f;
        stats.homingRange = 5f;
        stats.isHoming = false;
        stats.explosionRad = 0f;
        stats.projectileCount = 1;
        stats.innerInterval = 0.1f;
    }

    private void InitializeDefaultAreaStats(SkillStatData stats)
    {
        stats.radius = 5f;
        stats.duration = 3f;
        stats.tickRate = 1f;
        stats.isPersistent = false;
        stats.moveSpeed = 0f;
    }

    private void InitializeDefaultPassiveStats(SkillStatData stats)
    {
        stats.effectDuration = 5f;
        stats.cooldown = 10f;
        stats.triggerChance = 100f;
        stats.damageIncrease = 0f;
        stats.defenseIncrease = 0f;
        stats.expAreaIncrease = 0f;
        stats.homingActivate = false;
        stats.hpIncrease = 0f;
        stats.moveSpeedIncrease = 0f;
        stats.attackSpeedIncrease = 0f;
        stats.attackRangeIncrease = 0f;
        stats.hpRegenIncrease = 0f;
    }

    private void CopyProjectileStats(ISkillStat from, SkillStatData to)
    {
        if (from is ProjectileSkillStat projectileStats)
        {
            to.projectileSpeed = projectileStats.projectileSpeed;
            to.projectileScale = projectileStats.projectileScale;
            to.shotInterval = projectileStats.shotInterval;
            to.pierceCount = projectileStats.pierceCount;
            to.attackRange = projectileStats.attackRange;
            to.homingRange = projectileStats.homingRange;
            to.isHoming = projectileStats.isHoming;
            to.explosionRad = projectileStats.explosionRad;
            to.projectileCount = projectileStats.projectileCount;
            to.innerInterval = projectileStats.innerInterval;
        }
    }

    private void CopyAreaStats(ISkillStat from, SkillStatData to)
    {
        if (from is AreaSkillStat areaStats)
        {
            to.radius = areaStats.radius;
            to.duration = areaStats.duration;
            to.tickRate = areaStats.tickRate;
            to.isPersistent = areaStats.isPersistent;
            to.moveSpeed = areaStats.moveSpeed;
        }
    }

    private void CopyPassiveStats(ISkillStat from, SkillStatData to)
    {
        if (from is PassiveSkillStat passiveStats)
        {
            to.effectDuration = passiveStats.effectDuration;
            to.cooldown = passiveStats.cooldown;
            to.triggerChance = passiveStats.triggerChance;
            to.damageIncrease = passiveStats.damageIncrease;
            to.defenseIncrease = passiveStats.defenseIncrease;
            to.expAreaIncrease = passiveStats.expAreaIncrease;
            to.homingActivate = passiveStats.homingActivate;
            to.hpIncrease = passiveStats.hpIncrease;
            to.moveSpeedIncrease = passiveStats.moveSpeedIncrease;
            to.attackSpeedIncrease = passiveStats.attackSpeedIncrease;
            to.attackRangeIncrease = passiveStats.attackRangeIncrease;
            to.hpRegenIncrease = passiveStats.hpRegenIncrease;
        }
    }

    private void LoadSkillStatsData()
    {
        if (skillStatsList == null)
        {
            skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
        }

        try
        {
            string[] fileNames = {
                "ProjectileSkillStats",
                "AreaSkillStats",
                "PassiveSkillStats"
            };

            foreach (var fileName in fileNames)
            {
                TextAsset csvFile = Resources.Load<TextAsset>($"{RESOURCE_PATH}/{fileName}");
                if (csvFile == null)
                {
                    Debug.LogWarning($"Failed to load {fileName}.csv");
                    continue;
                }

                string[] lines = csvFile.text.Split('\n');
                if (lines.Length <= 1) continue;

                string[] headers = lines[0].Trim().Split(',');
                Debug.Log($"Loading {fileName} with headers: {string.Join(", ", headers)}");

                for (int i = 1; i < lines.Length; i++)
                {
                    ProcessCsvLine(fileName, headers, lines[i].Trim());
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill stats: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private void ProcessCsvLine(string fileName, string[] headers, string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        string[] values = line.Split(',');
        if (values.Length != headers.Length)
        {
            Debug.LogWarning($"Incorrect number of values in line");
            return;
        }

        SkillID skillID = SkillID.None;
        int level = 0;

        // SkillID와 Level 먼저 파싱
        for (int j = 0; j < headers.Length; j++)
        {
            string header = headers[j].Trim().ToLower();
            string value = values[j].Trim();

            if (header == "skillid")
            {
                if (System.Enum.TryParse<SkillID>(value, true, out SkillID parsedID))
                {
                    skillID = parsedID;
                }
            }
            else if (header == "level")
            {
                if (int.TryParse(value, out int parsedLevel))
                {
                    level = parsedLevel;
                }
            }
        }

        if (skillID == SkillID.None || level == 0)
        {
            Debug.LogWarning($"Invalid skillID or level in line");
            return;
        }

        var statData = new SkillStatData
        {
            skillID = skillID,
            level = level
        };

        for (int j = 0; j < headers.Length; j++)
        {
            ParseStatValue(headers[j].Trim().ToLower(), values[j].Trim(), statData);
        }

        if (!skillStatsList.ContainsKey(skillID))
        {
            skillStatsList[skillID] = new List<SkillStatData>();
        }

        var existingStat = skillStatsList[skillID].FirstOrDefault(s => s.level == level);
        if (existingStat != null)
        {
            int index = skillStatsList[skillID].IndexOf(existingStat);
            skillStatsList[skillID][index] = statData;
        }
        else
        {
            skillStatsList[skillID].Add(statData);
        }

        if (fileName == "PassiveSkillStats")
        {
            Debug.Log($"Loaded passive skill stats - ID: {skillID}, Level: {level}\n" +
                    $"moveSpeed: {statData.moveSpeedIncrease}\n" +
                    $"attackSpeed: {statData.attackSpeedIncrease}\n" +
                    $"attackRange: {statData.attackRangeIncrease}\n" +
                    $"hpRegen: {statData.hpRegenIncrease}");
        }
    }

    private void ParseStatValue(string header, string value, SkillStatData statData)
    {
        switch (header)
        {
            case "damage":
                if (float.TryParse(value, out float damage))
                    statData.damage = damage;
                break;
            case "maxskilllevel":
                if (int.TryParse(value, out int maxLevel))
                    statData.maxSkillLevel = maxLevel;
                break;
            case "element":
                if (System.Enum.TryParse<ElementType>(value, true, out ElementType element))
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
            case "movespeedincrease":
                if (float.TryParse(value, out float moveSpeedInc))
                    statData.moveSpeedIncrease = moveSpeedInc;
                break;
            case "attackspeedincrease":
                if (float.TryParse(value, out float attackSpeedInc))
                    statData.attackSpeedIncrease = attackSpeedInc;
                break;
            case "attackrangeincrease":
                if (float.TryParse(value, out float attackRangeInc))
                    statData.attackRangeIncrease = attackRangeInc;
                break;
            case "hpregenincrease":
                if (float.TryParse(value, out float hpRegenInc))
                    statData.hpRegenIncrease = hpRegenInc;
                break;
        }
    }

    #region Bottom Panel
    private void DrawBottomPanel()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal(GUILayout.Height(40));

        if (GUILayout.Button("New Skill", GUILayout.Height(30)))
        {
            CreateNewSkill();
        }

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
            // SkillDataManager 초기화
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager == null)
            {
                GameObject go = new GameObject("SkillDataManager");
                skillDataManager = go.AddComponent<SkillDataManager>();
            }

            // 데이터 로드
            skillList = skillDataManager.GetAllSkillData();
            if (skillList == null) skillList = new List<SkillData>();

            // CSV 데이터 로드
            LoadSkillStatsData();

            // 현재 선택된 스킬 복원
            if (currentSkill != null)
            {
                currentSkill = skillList.Find(s => s.metadata.ID == currentSkill.metadata.ID);
            }

            Debug.Log($"Successfully loaded {skillList?.Count ?? 0} skills and their stats");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void LoadSkillResources(SkillData skill)
    {
        if (skill.metadata.ID == SkillID.None) return;

        // 아이콘 로드
        string iconPath = $"{ICON_PATH}/{skill.metadata.ID}_Icon";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        if (icon != null)
        {
            skill.icon = icon;
            skill.metadata.Icon = icon;  // metadata의 Icon 필드도 설정
            Debug.Log($"Loaded icon from: {iconPath}");
        }

        // 메데이터 프리팹 로드
        string metadataPrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Metadata";
        GameObject metadataPrefab = Resources.Load<GameObject>(metadataPrefabPath);
        if (metadataPrefab != null)
        {
            skill.metadata.Prefab = metadataPrefab;
            Debug.Log($"Loaded metadata prefab from: {metadataPrefabPath}");
        }

        // 프로젝타일 타입인 경우 프로젝타일 프리팹 로드
        if (skill.metadata.Type == SkillType.Projectile)
        {
            string projectilePrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Projectile";
            GameObject projectilePrefab = Resources.Load<GameObject>(projectilePrefabPath);
            if (projectilePrefab != null)
            {
                skill.projectile = projectilePrefab;
                Debug.Log($"Loaded projectile prefab from: {projectilePrefabPath}");
            }
        }

        // 레벨별 프리팹 로드
        List<GameObject> levelPrefabs = new List<GameObject>();
        int level = 1;
        while (true)
        {
            string levelPrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Level_{level}";
            GameObject levelPrefab = Resources.Load<GameObject>(levelPrefabPath);
            if (levelPrefab == null) break;
            levelPrefabs.Add(levelPrefab);
            Debug.Log($"Loaded level {level} prefab from: {levelPrefabPath}");
            level++;
        }
        skill.prefabsByLevel = levelPrefabs.ToArray();
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

                        // 각 스킬의 프리팹과 아콘 로드
                        foreach (var skill in skillList)
                        {
                            if (skill.metadata.ID != SkillID.None)
                            {
                                // 기본 프리팹 로드
                                string basePrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Base";
                                GameObject basePrefab = Resources.Load<GameObject>(basePrefabPath);
                                if (basePrefab != null)
                                {
                                    skill.projectile = basePrefab;
                                }

                                // 레벨별 프리팹 로드
                                List<GameObject> levelPrefabs = new List<GameObject>();
                                int level = 1;
                                while (true)
                                {
                                    string levelPrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Level_{level}";
                                    GameObject levelPrefab = Resources.Load<GameObject>(levelPrefabPath);
                                    if (levelPrefab == null) break;
                                    levelPrefabs.Add(levelPrefab);
                                    level++;
                                }
                                skill.prefabsByLevel = levelPrefabs.ToArray();

                                // 아이콘 로드
                                string iconPath = $"{ICON_PATH}/{skill.metadata.ID}_Icon";
                                Sprite icon = Resources.Load<Sprite>(iconPath);
                                if (icon != null)
                                {
                                    skill.icon = icon;
                                }
                            }
                        }

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
            if (currentSkill != null && !ValidateCurrentSkillData())
            {
                Debug.LogError("Current skill data validation failed");
                return;
            }

            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);

            // 임시로 프리팹과 아이콘 참조를 저장
            var tempReferences = new Dictionary<SkillData, (GameObject prefab, Sprite icon, GameObject projectile, GameObject[] levelPrefabs)>();

            foreach (var skill in skillList)
            {
                // 현재 스탯 상태 저장
                var currentStats = skill.GetCurrentTypeStat();
                if (currentStats != null)
                {
                    switch (skill.metadata.Type)
                    {
                        case SkillType.Projectile:
                            var projectileStats = currentStats as ProjectileSkillStat;
                            if (projectileStats != null)
                            {
                                var statData = new SkillStatData
                                {
                                    skillID = skill.metadata.ID,
                                    level = projectileStats.baseStat.skillLevel,
                                    damage = projectileStats.baseStat.damage,
                                    maxSkillLevel = projectileStats.baseStat.maxSkillLevel,
                                    element = projectileStats.baseStat.element,
                                    elementalPower = projectileStats.baseStat.elementalPower,
                                    projectileSpeed = projectileStats.projectileSpeed,
                                    projectileScale = projectileStats.projectileScale,
                                    shotInterval = projectileStats.shotInterval,
                                    pierceCount = projectileStats.pierceCount,
                                    attackRange = projectileStats.attackRange,
                                    homingRange = projectileStats.homingRange,
                                    isHoming = projectileStats.isHoming,
                                    explosionRad = projectileStats.explosionRad,
                                    projectileCount = projectileStats.projectileCount,
                                    innerInterval = projectileStats.innerInterval
                                };
                                UpdateSkillStatsList(skill.metadata.ID, statData);
                            }
                            break;

                        case SkillType.Area:
                            var areaStats = currentStats as AreaSkillStat;
                            if (areaStats != null)
                            {
                                var statData = new SkillStatData
                                {
                                    skillID = skill.metadata.ID,
                                    level = areaStats.baseStat.skillLevel,
                                    damage = areaStats.baseStat.damage,
                                    maxSkillLevel = areaStats.baseStat.maxSkillLevel,
                                    element = areaStats.baseStat.element,
                                    elementalPower = areaStats.baseStat.elementalPower,
                                    radius = areaStats.radius,
                                    duration = areaStats.duration,
                                    tickRate = areaStats.tickRate,
                                    isPersistent = areaStats.isPersistent,
                                    moveSpeed = areaStats.moveSpeed
                                };
                                UpdateSkillStatsList(skill.metadata.ID, statData);
                            }
                            break;

                        case SkillType.Passive:
                            var passiveStats = currentStats as PassiveSkillStat;
                            if (passiveStats != null)
                            {
                                var statData = new SkillStatData
                                {
                                    skillID = skill.metadata.ID,
                                    level = passiveStats.baseStat.skillLevel,
                                    damage = passiveStats.baseStat.damage,
                                    maxSkillLevel = passiveStats.baseStat.maxSkillLevel,
                                    element = passiveStats.baseStat.element,
                                    elementalPower = passiveStats.baseStat.elementalPower,
                                    effectDuration = passiveStats.effectDuration,
                                    cooldown = passiveStats.cooldown,
                                    triggerChance = passiveStats.triggerChance,
                                    damageIncrease = passiveStats.damageIncrease,
                                    defenseIncrease = passiveStats.defenseIncrease,
                                    expAreaIncrease = passiveStats.expAreaIncrease,
                                    homingActivate = passiveStats.homingActivate,
                                    hpIncrease = passiveStats.hpIncrease,
                                    moveSpeedIncrease = passiveStats.moveSpeedIncrease,
                                    attackSpeedIncrease = passiveStats.attackSpeedIncrease,
                                    attackRangeIncrease = passiveStats.attackRangeIncrease,
                                    hpRegenIncrease = passiveStats.hpRegenIncrease
                                };
                                UpdateSkillStatsList(skill.metadata.ID, statData);
                            }
                            break;
                    }
                }

                // 리소스 참조 시 저장
                tempReferences[skill] = (
                    skill.metadata.Prefab,
                    skill.icon,
                    skill.projectile,
                    skill.prefabsByLevel?.ToArray()
                );

                // 직렬화를 위해 참조 제거
                skill.metadata.Prefab = null;
                skill.metadata.Icon = null;
                skill.icon = null;
                skill.projectile = null;
                skill.prefabsByLevel = null;
            }

            // SkillDataWrapper 생성
            var wrapper = new SkillDataWrapper();
            wrapper.skillDatas = skillList;

            // 각 스킬의 리소스 참조를 GUID로 저장
            foreach (var skill in skillList)
            {
                var refs = tempReferences[skill];

                // 메타데이터 프리팹
                if (refs.prefab != null)
                {
                    // 리소스 폴더 내의 프리팹 경로 사용
                    string resourcePath = $"Assets/Resources/{PREFAB_PATH}/{skill.metadata.ID}_Metadata.prefab";
                    SavePrefabToResources(refs.prefab, skill.metadata.ID, "Metadata");
                    string guid = AssetDatabase.AssetPathToGUID(resourcePath);
                    wrapper.resourceReferences.Add($"{skill.metadata.ID}_Metadata", new AssetReference
                    {
                        guid = guid,
                        path = resourcePath
                    });
                }

                // 아이콘
                if (refs.icon != null)
                {
                    string path = AssetDatabase.GetAssetPath(refs.icon);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    wrapper.resourceReferences.Add($"{skill.metadata.ID}_Icon", new AssetReference
                    {
                        guid = guid,
                        path = path
                    });
                }

                // 프로젝타일
                if (skill.metadata.Type == SkillType.Projectile && refs.projectile != null)
                {
                    string path = AssetDatabase.GetAssetPath(refs.projectile);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    wrapper.resourceReferences.Add($"{skill.metadata.ID}_Projectile", new AssetReference
                    {
                        guid = guid,
                        path = path
                    });
                }

                // 레벨별 프리팹
                if (refs.levelPrefabs != null)
                {
                    for (int i = 0; i < refs.levelPrefabs.Length; i++)
                    {
                        if (refs.levelPrefabs[i] != null)
                        {
                            string path = AssetDatabase.GetAssetPath(refs.levelPrefabs[i]);
                            string guid = AssetDatabase.AssetPathToGUID(path);
                            wrapper.resourceReferences.Add($"{skill.metadata.ID}_Level_{i + 1}", new AssetReference
                            {
                                guid = guid,
                                path = path
                            });
                        }
                    }
                }
            }

            // JSON 저장
            string jsonPath = Path.Combine(directory, "SkillData.json");
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // 참조 복원
            foreach (var skill in skillList)
            {
                var refs = tempReferences[skill];
                skill.metadata.Prefab = refs.prefab;
                skill.metadata.Icon = refs.icon;
                skill.icon = refs.icon;
                skill.projectile = refs.projectile;
                skill.prefabsByLevel = refs.levelPrefabs;
            }

            Debug.Log("Successfully saved all skill data");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    private bool ValidateCurrentSkillData()
    {
        if (currentSkill == null || currentSkill.metadata == null)
            return false;

        if (currentSkill.metadata.ID == SkillID.None)
        {
            Debug.LogError("Skill ID is not set");
            return false;
        }

        if (string.IsNullOrEmpty(currentSkill.metadata.Name))
        {
            Debug.LogError("Skill name is not set");
            return false;
        }

        // 킬 타입별 수 데이터 검증
        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                if (currentSkill.projectile == null)
                {
                    Debug.LogError("Projectile prefab is not set");
                    return false;
                }
                break;
            case SkillType.Area:
                // Area 스킬 특정 검증
                break;
            case SkillType.Passive:
                // Passive 스킬 특정 검증
                break;
        }

        return true;
    }

    private void CreateNewSkill()
    {
        try
        {
            // skillStatsList가 null인 경우 초기화
            if (skillStatsList == null)
            {
                skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
            }

            // skillList가 null인 경우 초기화
            if (skillList == null)
            {
                skillList = new List<SkillData>();
            }

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
                    Tags = new string[0]
                }
            };

            // 모든 타입의 스탯 초기화
            newSkill.projectileStat = new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 10f,
                    skillName = newSkill.metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = newSkill.metadata.Element,
                    elementalPower = 1f
                }
            };

            newSkill.areaStat = new AreaSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 10f,
                    skillName = newSkill.metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = newSkill.metadata.Element,
                    elementalPower = 1f
                }
            };

            newSkill.passiveStat = new PassiveSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 10f,
                    skillName = newSkill.metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = newSkill.metadata.Element,
                    elementalPower = 1f
                }
            };

            skillList.Add(newSkill);
            currentSkill = newSkill;

            // 새 스킬의 기본 스탯 데이터 생성
            var defaultStat = new SkillStatData
            {
                skillID = newSkill.metadata.ID,
                level = 1,
                damage = 10f,
                maxSkillLevel = 5,
                element = newSkill.metadata.Element,
                elementalPower = 1f
            };

            // 스탯 리트에 추가
            skillStatsList[newSkill.metadata.ID] = new List<SkillStatData> { defaultStat };

            Debug.Log($"Created new skill: {newSkill.metadata.Name} (ID: {newSkill.metadata.ID})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating new skill: {e.Message}\n{e.StackTrace}");
        }
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
            Debug.Log($"Skill data exported to: {path}");
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
            Debug.Log($"Skill data imported from: {path}");
        }
    }
    #endregion

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

    private void SaveSkillStatsToCSV(string directory)
    {
        try
        {
            // 프로젝타일 스킬 CSV
            string projectilePath = Path.Combine(directory, "ProjectileSkillStats.csv");
            if (!File.Exists(projectilePath) || !skillStatsList.Any())
            {
                CreateDefaultProjectileCSV(projectilePath);
            }
            else
            {
                SaveProjectileSkillStats(projectilePath);
            }

            // 에어리어 스킬 CSV
            string areaPath = Path.Combine(directory, "AreaSkillStats.csv");
            if (!File.Exists(areaPath) || !skillStatsList.Any())
            {
                CreateDefaultAreaCSV(areaPath);
            }
            else
            {
                SaveAreaSkillStats(areaPath);
            }

            // 패시브 스킬 CSV
            string passivePath = Path.Combine(directory, "PassiveSkillStats.csv");
            if (!File.Exists(passivePath) || !skillStatsList.Any())
            {
                CreateDefaultPassiveCSV(passivePath);
            }
            else
            {
                SavePassiveSkillStats(passivePath);
            }

            Debug.Log($"Successfully saved/created skill stats CSV files in: {directory}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill stats to CSV: {e.Message}");
        }
    }

    private void CreateDefaultProjectileCSV(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "projectilespeed,projectilescale,shotinterval,piercecount,attackrange," +
                      "homingrange,ishoming,explosionrad,projectilecount,innerinterval");

        // 예시 데이터 추가
        csv.AppendLine("None,1,10,5,None,1," +
                      "10,1,1,10," +
                      "5,false,0,1,0.1");

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Created default Projectile CSV at: {path}");
    }

    private void CreateDefaultAreaCSV(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "radius,duration,tickrate,ispersistent,movespeed");

        // 예시 데이터 추가
        csv.AppendLine("None,1,10,5,None,1," +
                      "5,3,1,false,0");

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Created default Area CSV at: {path}");
    }

    private void CreateDefaultPassiveCSV(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease,movespeedincrease," +
                      "attackspeedincrease,attackrangeincrease,hpregenincrease");

        // 예시 데이터 추가
        csv.AppendLine("None,1,10,5,None,1," +
                      "5,10,100,0,0," +
                      "0,false,0,0," +
                      "0,0,0");

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Created default Passive CSV at: {path}");
    }

    private void SaveProjectileSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "projectilespeed,projectilescale,shotinterval,piercecount,attackrange," +
                      "homingrange,ishoming,explosionrad,projectilecount,innerinterval");

        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                var skill = skillList.Find(s => s.metadata.ID == stat.skillID);
                if (skill?.metadata.Type == SkillType.Projectile)
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

    private void SaveAreaSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "radius,duration,tickrate,ispersistent,movespeed");

        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                var skill = skillList.Find(s => s.metadata.ID == stat.skillID);
                if (skill?.metadata.Type == SkillType.Area)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.radius}," +
                                 $"{stat.duration},{stat.tickRate},{stat.isPersistent},{stat.moveSpeed}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private void SavePassiveSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease,movespeedincrease," +
                      "attackspeedincrease,attackrangeincrease,hpregenincrease");

        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                var skill = skillList.Find(s => s.metadata.ID == stat.skillID);
                if (skill?.metadata.Type == SkillType.Passive)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +
                                 $"{stat.cooldown},{stat.triggerChance},{stat.damageIncrease}," +
                                 $"{stat.defenseIncrease},{stat.expAreaIncrease},{stat.homingActivate}," +
                                 $"{stat.hpIncrease},{stat.moveSpeedIncrease},{stat.attackSpeedIncrease}," +
                                 $"{stat.attackRangeIncrease},{stat.hpRegenIncrease}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private void SavePrefabToResources(GameObject prefab, SkillID skillId, string suffix)
    {
        if (prefab == null) return;

        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", PREFAB_PATH);
            Directory.CreateDirectory(directory);

            string prefabPath = Path.Combine(directory, $"{skillId}_{suffix}.prefab");
            string assetPath = $"Assets/Resources/{PREFAB_PATH}/{skillId}_{suffix}.prefab";

            // 기존 프리팹이 있다면 삭제
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            // 리팹 복사 및 저장
            GameObject prefabInstance = Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath, out _);
            DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {assetPath}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Failed to save prefab: {assetPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }

    private void SaveIconToResources(Sprite icon, SkillID skillId)
    {
        if (icon == null) return;

        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", ICON_PATH);
            Directory.CreateDirectory(directory);

            string destinationPath = Path.Combine(directory, $"{skillId}_Icon.png");
            string assetPath = $"Assets/Resources/{ICON_PATH}/{skillId}_Icon.png";

            // 소스 텍스처 가져오기
            Texture2D sourceTexture = icon.texture;

            // 읽기 가능한 텍스처인지 확인
            if (!sourceTexture.isReadable)
            {
                string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
                TextureImporter importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                sourceTexture = icon.texture;
            }

            // 새 텍스처 생성 및 복사
            Texture2D newTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
            newTexture.SetPixels(sourceTexture.GetPixels());
            newTexture.Apply();

            // PNG로 저장
            byte[] bytes = newTexture.EncodeToPNG();
            File.WriteAllBytes(destinationPath, bytes);
            Object.DestroyImmediate(newTexture);

            AssetDatabase.Refresh();

            // 임포터 설정
            TextureImporter destinationImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (destinationImporter != null)
            {
                destinationImporter.textureType = TextureImporterType.Sprite;
                destinationImporter.spritePixelsPerUnit = 100;
                destinationImporter.mipmapEnabled = false;
                destinationImporter.SaveAndReimport();
            }

            // 새로운 스프라이트 로드 및 할당
            currentSkill.icon = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            currentSkill.metadata.Icon = currentSkill.icon;

            Debug.Log($"Successfully saved icon to: {assetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save icon: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateSkillStats()
    {
        if (currentSkill == null) return;

        // 현재 스킬의 스탯이 skillStatsList에 없으면 초기화
        if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
        {
            skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();

            // 레벨 1 기본 스탯 추가
            var defaultStat = new SkillStatData
            {
                skillID = currentSkill.metadata.ID,
                level = 1,
                damage = 10f,
                maxSkillLevel = 5,
                element = currentSkill.metadata.Element,
                elementalPower = 1f
            };

            skillStatsList[currentSkill.metadata.ID].Add(defaultStat);
        }

        // SkillDataManager에 변경사항 즉시 반영
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager != null)
        {
            skillDataManager.UpdateSkillStatsData(skillStatsList);
            EditorUtility.SetDirty(skillDataManager);
        }
    }

    [MenuItem("Tools/Print Selected Object InstanceID")]
    static void PrintSelectedObjectInstanceID()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (Object obj in selectedObjects)
        {
            Debug.Log($"Selected Object: {obj.name}, InstanceID: {obj.GetInstanceID()}");

            // 에 경로도 함께 출력
            string assetPath = AssetDatabase.GetAssetPath(obj);
            Debug.Log($"Asset Path: {assetPath}");
        }
    }

    private void InitializeSkillStats(SkillData skill)
    {
        if (skill == null) return;

        var baseStats = new BaseSkillStat
        {
            damage = 10f,
            skillName = skill.metadata.Name,
            skillLevel = 1,
            maxSkillLevel = 5,
            element = skill.metadata.Element,
            elementalPower = 1f
        };

        switch (skill.metadata.Type)
        {
            case SkillType.Projectile:
                skill.projectileStat = new ProjectileSkillStat
                {
                    baseStat = baseStats,
                    projectileSpeed = 10f,
                    projectileScale = 1f,
                    shotInterval = 1f,
                    pierceCount = 1,
                    attackRange = 10f,
                    homingRange = 5f,
                    isHoming = false,
                    explosionRad = 0f,
                    projectileCount = 1,
                    innerInterval = 0.1f
                };
                break;

            case SkillType.Area:
                skill.areaStat = new AreaSkillStat
                {
                    baseStat = baseStats,
                    radius = 5f,
                    duration = 3f,
                    tickRate = 1f,
                    isPersistent = false,
                    moveSpeed = 0f
                };
                break;

            case SkillType.Passive:
                skill.passiveStat = new PassiveSkillStat
                {
                    baseStat = baseStats,
                    effectDuration = 5f,
                    cooldown = 10f,
                    triggerChance = 100f,
                    damageIncrease = 0f,
                    defenseIncrease = 0f,
                    expAreaIncrease = 0f,
                    homingActivate = false,
                    hpIncrease = 0f,
                    moveSpeedIncrease = 0f,
                    attackSpeedIncrease = 0f,
                    attackRangeIncrease = 0f,
                    hpRegenIncrease = 0f
                };
                break;
        }

        // SkillStatData 초기화
        var skillStatData = new SkillStatData
        {
            skillID = skill.metadata.ID,
            level = 1,
            damage = baseStats.damage,
            maxSkillLevel = baseStats.maxSkillLevel,
            element = baseStats.element,
            elementalPower = baseStats.elementalPower
        };

        // 스킬 타입에 따른 추가 데이터 설정
        switch (skill.metadata.Type)
        {
            case SkillType.Projectile:
                CopyProjectileStatsToStatData(skill.projectileStat, skillStatData);
                break;
            case SkillType.Area:
                CopyAreaStatsToStatData(skill.areaStat, skillStatData);
                break;
            case SkillType.Passive:
                CopyPassiveStatsToStatData(skill.passiveStat, skillStatData);
                break;
        }

        // 스탯 리스트에 추가
        if (!skillStatsList.ContainsKey(skill.metadata.ID))
        {
            skillStatsList[skill.metadata.ID] = new List<SkillStatData>();
        }
        skillStatsList[skill.metadata.ID].Add(skillStatData);
    }

    private void CopyProjectileStatsToStatData(ProjectileSkillStat from, SkillStatData to)
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

    private void CopyAreaStatsToStatData(AreaSkillStat from, SkillStatData to)
    {
        to.radius = from.radius;
        to.duration = from.duration;
        to.tickRate = from.tickRate;
        to.isPersistent = from.isPersistent;
        to.moveSpeed = from.moveSpeed;
    }

    private void CopyPassiveStatsToStatData(PassiveSkillStat from, SkillStatData to)
    {
        to.effectDuration = from.effectDuration;
        to.cooldown = from.cooldown;
        to.triggerChance = from.triggerChance;
        to.damageIncrease = from.damageIncrease;
        to.defenseIncrease = from.defenseIncrease;
        to.expAreaIncrease = from.expAreaIncrease;
        to.homingActivate = from.homingActivate;
        to.hpIncrease = from.hpIncrease;
        to.moveSpeedIncrease = from.moveSpeedIncrease;
        to.attackSpeedIncrease = from.attackSpeedIncrease;
        to.attackRangeIncrease = from.attackRangeIncrease;
        to.hpRegenIncrease = from.hpRegenIncrease;
    }

    private void CreateResourceFolders()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string[] folders = new string[]
        {
            Path.Combine(resourcesPath, RESOURCE_PATH),
            Path.Combine(resourcesPath, PREFAB_PATH),
            Path.Combine(resourcesPath, ICON_PATH)
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"Created directory: {folder}");
            }
        }

        AssetDatabase.Refresh();
    }

    private void SaveSkillResources(SkillData skill, ResourceReferenceData references)
    {
        if (skill == null || skill.metadata == null) return;

        // 메타데이터 프리팹 저장
        if (skill.metadata.Prefab != null)
        {
            SavePrefabToResources(skill.metadata.Prefab, skill.metadata.ID, "Metadata");
            string prefabPath = $"Assets/Resources/{PREFAB_PATH}/{skill.metadata.ID}_Metadata.prefab";
            string guid = AssetDatabase.AssetPathToGUID(prefabPath);
            references.Add($"{skill.metadata.ID}_Metadata", new AssetReference
            {
                guid = guid,
                path = prefabPath
            });
        }

        // 아이콘 저장
        if (skill.icon != null)
        {
            SaveIconToResources(skill.icon, skill.metadata.ID);
            string iconPath = $"Assets/Resources/{ICON_PATH}/{skill.metadata.ID}_Icon.png";
            string guid = AssetDatabase.AssetPathToGUID(iconPath);
            references.Add($"{skill.metadata.ID}_Icon", new AssetReference
            {
                guid = guid,
                path = iconPath
            });
        }

        // 프로젝타일 프리팹 저장
        if (skill.metadata.Type == SkillType.Projectile && skill.projectile != null)
        {
            SavePrefabToResources(skill.projectile, skill.metadata.ID, "Projectile");
            string prefabPath = $"Assets/Resources/{PREFAB_PATH}/{skill.metadata.ID}_Projectile.prefab";
            string guid = AssetDatabase.AssetPathToGUID(prefabPath);
            references.Add($"{skill.metadata.ID}_Projectile", new AssetReference
            {
                guid = guid,
                path = prefabPath
            });
        }

        // 레벨별 프리팹 저장
        if (skill.prefabsByLevel != null)
        {
            for (int i = 0; i < skill.prefabsByLevel.Length; i++)
            {
                if (skill.prefabsByLevel[i] != null)
                {
                    SavePrefabToResources(skill.prefabsByLevel[i], skill.metadata.ID, $"Level_{i + 1}");
                    string prefabPath = $"Assets/Resources/{PREFAB_PATH}/{skill.metadata.ID}_Level_{i + 1}.prefab";
                    string guid = AssetDatabase.AssetPathToGUID(prefabPath);
                    references.Add($"{skill.metadata.ID}_Level_{i + 1}", new AssetReference
                    {
                        guid = guid,
                        path = prefabPath
                    });
                }
            }
        }
    }

    private void SaveAllData()
    {
        string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
        Directory.CreateDirectory(directory);

        foreach (var skill in skillList)
        {
            SaveResources(skill, directory);
        }

        SaveSkillData();
        AssetDatabase.Refresh();
    }

    private void SaveResources(SkillData skill, string directory)
    {
        if (skill == null) return;

        var references = new ResourceReferenceData();
        SaveSkillResources(skill, references);
        SaveSkillStatsToCSV(directory);
    }

    private bool ValidateSkill(SkillData skill)
    {
        if (skill == null || skill.metadata == null)
            return false;

        if (skill.metadata.ID == SkillID.None || string.IsNullOrEmpty(skill.metadata.Name))
            return false;

        return ValidateSkillStats(skill);
    }

    private bool ValidateSkillStats(SkillData skill)
    {
        var stats = skill.GetCurrentTypeStat();
        return stats != null && stats.baseStat != null;
    }

    private void UpdateSkillStatsList(SkillID skillID, SkillStatData statData)
    {
        if (!skillStatsList.ContainsKey(skillID))
        {
            skillStatsList[skillID] = new List<SkillStatData>();
        }

        var existingStat = skillStatsList[skillID].FirstOrDefault(s => s.level == statData.level);
        if (existingStat != null)
        {
            int index = skillStatsList[skillID].IndexOf(existingStat);
            skillStatsList[skillID][index] = statData;
        }
        else
        {
            skillStatsList[skillID].Add(statData);
        }
    }

    // 에디터에서만 사용되는 기본값 구조체
    private struct DefaultSkillStats
    {
        // Base Stats
        public const float DEFAULT_DAMAGE = 10f;
        public const int DEFAULT_MAX_LEVEL = 5;
        public const float DEFAULT_ELEMENTAL_POWER = 1f;

        // Projectile Stats
        public const float DEFAULT_PROJECTILE_SPEED = 10f;
        public const float DEFAULT_PROJECTILE_SCALE = 1f;
        public const float DEFAULT_SHOT_INTERVAL = 1f;
        public const int DEFAULT_PIERCE_COUNT = 1;
        public const float DEFAULT_ATTACK_RANGE = 10f;
        public const float DEFAULT_HOMING_RANGE = 5f;
        public const float DEFAULT_EXPLOSION_RADIUS = 0f;
        public const int DEFAULT_PROJECTILE_COUNT = 1;
        public const float DEFAULT_INNER_INTERVAL = 0.1f;

        // Area Stats
        public const float DEFAULT_RADIUS = 5f;
        public const float DEFAULT_DURATION = 3f;
        public const float DEFAULT_TICK_RATE = 1f;
        public const float DEFAULT_MOVE_SPEED = 0f;

        // Passive Stats
        public const float DEFAULT_EFFECT_DURATION = 5f;
        public const float DEFAULT_COOLDOWN = 10f;
        public const float DEFAULT_TRIGGER_CHANCE = 100f;
    }

    private void InitializeNewSkillStats(SkillStatData stat)
    {
        // 에디터에서 새 스킬 생성시에만 사용되는 초기화 로직
        stat.damage = DefaultSkillStats.DEFAULT_DAMAGE;
        stat.maxSkillLevel = DefaultSkillStats.DEFAULT_MAX_LEVEL;
        stat.element = currentSkill.metadata.Element;
        stat.elementalPower = DefaultSkillStats.DEFAULT_ELEMENTAL_POWER;

        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                InitializeNewProjectileStats(stat);
                break;
            case SkillType.Area:
                InitializeNewAreaStats(stat);
                break;
            case SkillType.Passive:
                InitializeNewPassiveStats(stat);
                break;
        }
    }

    private void InitializeNewProjectileStats(SkillStatData stats)
    {
        stats.projectileSpeed = DefaultSkillStats.DEFAULT_PROJECTILE_SPEED;
        stats.projectileScale = DefaultSkillStats.DEFAULT_PROJECTILE_SCALE;
        stats.shotInterval = DefaultSkillStats.DEFAULT_SHOT_INTERVAL;
        stats.pierceCount = DefaultSkillStats.DEFAULT_PIERCE_COUNT;
        stats.attackRange = DefaultSkillStats.DEFAULT_ATTACK_RANGE;
        stats.homingRange = DefaultSkillStats.DEFAULT_HOMING_RANGE;
        stats.isHoming = false;
        stats.explosionRad = DefaultSkillStats.DEFAULT_EXPLOSION_RADIUS;
        stats.projectileCount = DefaultSkillStats.DEFAULT_PROJECTILE_COUNT;
        stats.innerInterval = DefaultSkillStats.DEFAULT_INNER_INTERVAL;
    }

    private void InitializeNewAreaStats(SkillStatData stats)
    {
        stats.radius = DefaultSkillStats.DEFAULT_RADIUS;
        stats.duration = DefaultSkillStats.DEFAULT_DURATION;
        stats.tickRate = DefaultSkillStats.DEFAULT_TICK_RATE;
        stats.isPersistent = false;
        stats.moveSpeed = DefaultSkillStats.DEFAULT_MOVE_SPEED;
    }

    private void InitializeNewPassiveStats(SkillStatData stats)
    {
        stats.effectDuration = DefaultSkillStats.DEFAULT_EFFECT_DURATION;
        stats.cooldown = DefaultSkillStats.DEFAULT_COOLDOWN;
        stats.triggerChance = DefaultSkillStats.DEFAULT_TRIGGER_CHANCE;
        stats.damageIncrease = 0f;
        stats.defenseIncrease = 0f;
        stats.expAreaIncrease = 0f;
        stats.homingActivate = false;
        stats.hpIncrease = 0f;
        stats.moveSpeedIncrease = 0f;
        stats.attackSpeedIncrease = 0f;
        stats.attackRangeIncrease = 0f;
        stats.hpRegenIncrease = 0f;
    }
}