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

        currentSkill.metadata.Prefab = (GameObject)EditorGUILayout.ObjectField(
            "Skill Prefab",
            currentSkill.metadata.Prefab,
            typeof(GameObject),
            false
        );

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
                Debug.Log($"Icon changed for skill: {currentSkill.metadata.ID}");
                SaveIconToResources(newIcon, currentSkill.metadata.ID);
                currentSkill.icon = newIcon;
                currentSkill.metadata.Icon = newIcon;  // metadata.Icon도 함께 설정
                EditorUtility.SetDirty(currentSkill.icon);
                EditorUtility.SetDirty(FindObjectOfType<SkillDataManager>());
            }
        }
        else
        {
            // 아이콘이 없는 경우 리소스 폴더에서 로드 시도
            string iconPath = $"{ICON_PATH}/{currentSkill.metadata.ID}_Icon";
            Sprite loadedIcon = Resources.Load<Sprite>(iconPath);
            if (loadedIcon != null)
            {
                currentSkill.icon = loadedIcon;
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

            // 현재 스킬의 스탯 데이터 가져오기
            var skillStats = skillStatsList[currentSkill.metadata.ID].FirstOrDefault(s => s.level == 1);
            if (skillStats == null)
            {
                skillStats = new SkillStatData
                {
                    skillID = currentSkill.metadata.ID,
                    level = 1
                };
                skillStatsList[currentSkill.metadata.ID].Add(skillStats);
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
                // 변경사항 즉시 저장
                SaveSkillData();
                EditorUtility.SetDirty(FindObjectOfType<SkillDataManager>());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error drawing skill stats: {e.Message}");
        }

        EditorGUI.indentLevel--;
    }

    private void DrawBaseStats(SkillStatData stats)
    {
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        stats.damage = EditorGUILayout.FloatField("Damage", stats.damage);
        stats.maxSkillLevel = EditorGUILayout.IntField("Max Level", stats.maxSkillLevel);
        stats.element = (ElementType)EditorGUILayout.EnumPopup("Element", stats.element);
        stats.elementalPower = EditorGUILayout.FloatField("Elemental Power", stats.elementalPower);

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

        // skillStatsList가 null이면 초기화
        if (skillStatsList == null)
        {
            skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
        }

        // 현재 스킬의 스탯 리스트가 없으면 생성
        if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
        {
            skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();

            // 레벨 1 스탯 추가
            var defaultStat = new SkillStatData
            {
                skillID = currentSkill.metadata.ID,
                level = 1
            };

            // 현재 스킬 기본 스탯 복사
            if (currentSkill.metadata.Type == SkillType.Passive && currentSkill.passiveStat != null)
            {
                defaultStat.moveSpeedIncrease = currentSkill.passiveStat.moveSpeedIncrease;
                defaultStat.attackSpeedIncrease = currentSkill.passiveStat.attackSpeedIncrease;
                defaultStat.attackRangeIncrease = currentSkill.passiveStat.attackRangeIncrease;
                defaultStat.hpRegenIncrease = currentSkill.passiveStat.hpRegenIncrease;
            }

            skillStatsList[currentSkill.metadata.ID].Add(defaultStat);
        }

        var skillStats = skillStatsList[currentSkill.metadata.ID];

        // 각 레벨의 스탯 표시
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

    private void DrawProjectileStatsForLevel(SkillStatData stat)
    {
        // 기본 스탯
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        stat.damage = EditorGUILayout.FloatField("Damage", stat.damage);
        stat.elementalPower = EditorGUILayout.FloatField("Elemental Power", stat.elementalPower);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // 프로젝타일 스탯들
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
        EditorGUI.indentLevel--;
    }

    private void DrawAreaStatsForLevel(SkillStatData stat)
    {
        // 기본 스탯
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        stat.damage = EditorGUILayout.FloatField("Damage", stat.damage);
        stat.elementalPower = EditorGUILayout.FloatField("Elemental Power", stat.elementalPower);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // 에어리어 스탯들
        stat.radius = EditorGUILayout.FloatField("Radius", stat.radius);
        stat.duration = EditorGUILayout.FloatField("Duration", stat.duration);
        stat.tickRate = EditorGUILayout.FloatField("Tick Rate", stat.tickRate);
        stat.isPersistent = EditorGUILayout.Toggle("Is Persistent", stat.isPersistent);
        stat.moveSpeed = EditorGUILayout.FloatField("Move Speed", stat.moveSpeed);
        EditorGUI.indentLevel--;
    }

    private void DrawPassiveStatsForLevel(SkillStatData stat)
    {
        if (currentSkill == null) return;

        // SkillID 설정 확인
        if (stat.skillID == SkillID.None)
        {
            stat.skillID = currentSkill.metadata.ID;
        }

        EditorGUI.BeginChangeCheck();

        // 기본 스탯
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        stat.damage = EditorGUILayout.FloatField("Damage", stat.damage);
        stat.elementalPower = EditorGUILayout.FloatField("Elemental Power", stat.elementalPower);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // 기본 패시브 스탯
        stat.effectDuration = EditorGUILayout.FloatField("Effect Duration", stat.effectDuration);
        stat.cooldown = EditorGUILayout.FloatField("Cooldown", stat.cooldown);
        stat.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stat.triggerChance);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Increase Stats", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // 증가 스탯들
        stat.damageIncrease = EditorGUILayout.FloatField("Damage (%)", stat.damageIncrease);
        stat.defenseIncrease = EditorGUILayout.FloatField("Defense (%)", stat.defenseIncrease);
        stat.expAreaIncrease = EditorGUILayout.FloatField("Exp Area (%)", stat.expAreaIncrease);
        stat.hpIncrease = EditorGUILayout.FloatField("HP (%)", stat.hpIncrease);
        stat.moveSpeedIncrease = EditorGUILayout.FloatField("Move Speed (%)", stat.moveSpeedIncrease);
        stat.attackSpeedIncrease = EditorGUILayout.FloatField("Attack Speed (%)", stat.attackSpeedIncrease);
        stat.attackRangeIncrease = EditorGUILayout.FloatField("Attack Range (%)", stat.attackRangeIncrease);
        stat.hpRegenIncrease = EditorGUILayout.FloatField("HP Regen (%)", stat.hpRegenIncrease);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        stat.homingActivate = EditorGUILayout.Toggle("Homing Activate", stat.homingActivate);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(FindObjectOfType<SkillDataManager>());

            // skillStatsList가 null이면 초기화
            if (skillStatsList == null)
            {
                skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
            }

            // 현재 스킬의 스탯 리스트가 없으면 생성
            if (!skillStatsList.ContainsKey(currentSkill.metadata.ID))
            {
                skillStatsList[currentSkill.metadata.ID] = new List<SkillStatData>();
            }

            // 현재 레벨의 스탯이 없으면 추가
            var stats = skillStatsList[currentSkill.metadata.ID];
            var existingStat = stats.FirstOrDefault(s => s.level == stat.level);
            if (existingStat == null)
            {
                stats.Add(stat);
            }
            else
            {
                int index = stats.IndexOf(existingStat);
                stats[index] = stat;
            }

            SaveSkillData();  // 변경사항 즉시 저장

            Debug.Log($"Updated passive skill stats - Level {stat.level}, SkillID: {stat.skillID}:\n" +
                      $"moveSpeed: {stat.moveSpeedIncrease}%\n" +
                      $"attackSpeed: {stat.attackSpeedIncrease}%\n" +
                      $"attackRange: {stat.attackRangeIncrease}%\n" +
                      $"hpRegen: {stat.hpRegenIncrease}%");
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

        // 새 레의 기본 스탯 증가
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
        to.moveSpeedIncrease = from.moveSpeedIncrease;
        to.attackSpeedIncrease = from.attackSpeedIncrease;
        to.attackRangeIncrease = from.attackRangeIncrease;
        to.hpRegenIncrease = from.hpRegenIncrease;
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
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] values = line.Split(',');
                    if (values.Length != headers.Length)
                    {
                        Debug.LogWarning($"Incorrect number of values in line {i}");
                        continue;
                    }

                    // CSV 라인의 실제 내용 출력
                    Debug.Log($"Processing line: {line}");

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
                        Debug.LogWarning($"Invalid skillID or level in line {i}");
                        continue;
                    }

                    // 스킬 스탯 데이터 생성
                    var statData = new SkillStatData
                    {
                        skillID = skillID,
                        level = level
                    };

                    // 나머지 값들 파싱
                    for (int j = 0; j < headers.Length; j++)
                    {
                        string header = headers[j].Trim().ToLower();
                        string value = values[j].Trim();

                        if (string.IsNullOrEmpty(value)) continue;

                        switch (header)
                        {
                            case "movespeedincrease":
                                if (float.TryParse(value, out float moveSpeed))
                                {
                                    statData.moveSpeedIncrease = moveSpeed;
                                    Debug.Log($"Parsed moveSpeedIncrease: {moveSpeed} for skill {skillID}");
                                }
                                break;
                            case "attackspeedincrease":
                                if (float.TryParse(value, out float attackSpeed))
                                {
                                    statData.attackSpeedIncrease = attackSpeed;
                                    Debug.Log($"Parsed attackSpeedIncrease: {attackSpeed} for skill {skillID}");
                                }
                                break;
                            case "attackrangeincrease":
                                if (float.TryParse(value, out float attackRange))
                                {
                                    statData.attackRangeIncrease = attackRange;
                                    Debug.Log($"Parsed attackRangeIncrease: {attackRange} for skill {skillID}");
                                }
                                break;
                            case "hpregenincrease":
                                if (float.TryParse(value, out float hpRegen))
                                {
                                    statData.hpRegenIncrease = hpRegen;
                                    Debug.Log($"Parsed hpRegenIncrease: {hpRegen} for skill {skillID}");
                                }
                                break;
                        }
                    }

                    if (!skillStatsList.ContainsKey(skillID))
                    {
                        skillStatsList[skillID] = new List<SkillStatData>();
                    }

                    // 기존 레벨 스탯이 있다면 업데이트, 없다면 추가
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

                    // 패시브 스킬인 경우 로그 력
                    if (fileName == "PassiveSkillStats")
                    {
                        Debug.Log($"Loaded passive skill stats - ID: {skillID}, Level: {level}\n" +
                                $"moveSpeed: {statData.moveSpeedIncrease}\n" +
                                $"attackSpeed: {statData.attackSpeedIncrease}\n" +
                                $"attackRange: {statData.attackRangeIncrease}\n" +
                                $"hpRegen: {statData.hpRegenIncrease}");
                    }
                }
            }

            Debug.Log($"Successfully loaded {skillStatsList.Count} skill stats from CSV files");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill stats: {e.Message}\nStackTrace: {e.StackTrace}");
        }
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
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager == null)
            {
                Debug.LogError("SkillDataManager not found in scene");
                return;
            }

            // 스킬 데이터 매니저의 데이터 리로드
            skillDataManager.LoadAllSkillData();
            skillDataManager.LoadSkillStatsFromCSV();

            // 에디터의 데이터 동기화
            skillList = skillDataManager.GetAllSkillData();

            // 각 스킬의 스탯 데이터 로드 및 설정
            foreach (var skill in skillList)
            {
                if (skill.metadata.ID == SkillID.None) continue;

                // 레벨 1 스탯 데이터 로드
                var stats = skillDataManager.GetSkillStatsForLevel(skill.metadata.ID, 1, skill.metadata.Type);
                if (stats != null)
                {
                    // 스킬 타입에 따라 스탯 설정
                    switch (skill.metadata.Type)
                    {
                        case SkillType.Projectile:
                            skill.projectileStat = stats as ProjectileSkillStat;
                            break;
                        case SkillType.Area:
                            skill.areaStat = stats as AreaSkillStat;
                            break;
                        case SkillType.Passive:
                            skill.passiveStat = stats as PassiveSkillStat;
                            break;
                    }

                    Debug.Log($"Loaded stats for skill {skill.metadata.Name}:");
                    Debug.Log($"Base Stats - Damage: {stats.baseStat.damage}, ElementalPower: {stats.baseStat.elementalPower}");
                }

                // 리소스 로드 (아이콘, 프리팹)
                LoadSkillResources(skill);
            }

            // 스탯 리스트 초기화
            skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();
            foreach (var skill in skillList)
            {
                if (skill.metadata.ID != SkillID.None)
                {
                    var levelStats = skillDataManager.GetSkillStatsListForID(skill.metadata.ID);
                    if (levelStats != null && levelStats.Any())
                    {
                        skillStatsList[skill.metadata.ID] = levelStats;
                    }
                }
            }

            Debug.Log($"Successfully loaded {skillList.Count} skills and their stats");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data: {e.Message}");
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

        // 메타데이터 프리팹 로드
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

                        // 각 스킬의 프리팹과 아이콘 로드
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
            // 현재 선택된 스킬의 데이터가 유효한지 확인
            if (currentSkill != null && !ValidateCurrentSkillData())
            {
                Debug.LogError("Current skill data validation failed");
                return;
            }

            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            Directory.CreateDirectory(directory);

            // 메타데이터의 Prefab 저장
            if (currentSkill != null)
            {
                // 메타이터 프리팹 저장
                if (currentSkill.metadata.Prefab != null)
                {
                    SavePrefabToResources(currentSkill.metadata.Prefab, currentSkill.metadata.ID, "Metadata");
                }

                // 프로젝타일 타입인 경우 프로젝타일 프리팹도 저장
                if (currentSkill.metadata.Type == SkillType.Projectile)
                {
                    if (currentSkill.projectile != null)
                    {
                        SavePrefabToResources(currentSkill.projectile, currentSkill.metadata.ID, "Projectile");
                    }
                    else
                    {
                        Debug.LogWarning($"Projectile prefab is missing for skill: {currentSkill.metadata.Name}");
                    }
                }

                // 레벨별 프리팹 저장
                if (currentSkill.prefabsByLevel != null)
                {
                    for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
                    {
                        if (currentSkill.prefabsByLevel[i] != null)
                        {
                            SavePrefabToResources(currentSkill.prefabsByLevel[i], currentSkill.metadata.ID, $"Level_{i + 1}");
                        }
                    }
                }
            }

            // JSON 데이터 저장
            SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillList };
            string jsonPath = Path.Combine(directory, "SkillData.json");
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(jsonPath, json);

            // CSV 데이터 저장
            SaveSkillStatsToCSV(directory);

            // SkillDataManager 업데이트
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager != null)
            {
                skillDataManager.UpdateSkillList(skillList);
                skillDataManager.UpdateSkillStatsData(skillStatsList);
                skillDataManager.LoadSkillStatsFromCSV(); // 저장 후 즉시 리로드
                EditorUtility.SetDirty(skillDataManager);
            }

            AssetDatabase.Refresh();
            Debug.Log("Successfully saved all skill data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}\nStackTrace: {e.StackTrace}");
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

        // 스킬 타입별 필수 데이터 검증
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
            Debug.Log($"ų Ͱ  η: {path}");
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

    private void SaveSkillStatsToCSV(string directory)
    {
        try
        {
            // 프로젝타일 스킬 저장
            string path = Path.Combine(directory, "ProjectileSkillStats.csv");
            StringBuilder csv = new StringBuilder();

            // 헤더에 원력 필드 명시적 추가
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
                                     $"{stat.element},{stat.elementalPower},{stat.projectileSpeed}," + // elementalPower 추가
                                     $"{stat.projectileScale},{stat.shotInterval},{stat.pierceCount}," +
                                     $"{stat.attackRange},{stat.homingRange},{stat.isHoming}," +
                                     $"{stat.explosionRad},{stat.projectileCount},{stat.innerInterval}");
                    }
                }
            }

            File.WriteAllText(path, csv.ToString());

            // 에어리어 스킬 저장
            path = Path.Combine(directory, "AreaSkillStats.csv");
            csv.Clear();
            csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +  // elementalpower 추가
                          "radius,duration,tickrate,ispersistent,movespeed");

            foreach (var skillStats in skillStatsList.Values)
            {
                foreach (var stat in skillStats)
                {
                    var skill = skillList.Find(s => s.metadata.ID == stat.skillID);
                    if (skill?.metadata.Type == SkillType.Area)
                    {
                        csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                     $"{stat.element},{stat.elementalPower},{stat.radius}," + // elementalPower 추가
                                     $"{stat.duration},{stat.tickRate},{stat.isPersistent},{stat.moveSpeed}");
                    }
                }
            }

            File.WriteAllText(path, csv.ToString());

            // 패시브 스킬 저장
            path = Path.Combine(directory, "PassiveSkillStats.csv");
            csv.Clear();
            csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +  // elementalpower 추가
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
                                     $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +  // elementalPower 추가
                                     $"{stat.cooldown},{stat.triggerChance},{stat.damageIncrease}," +
                                     $"{stat.defenseIncrease},{stat.expAreaIncrease},{stat.homingActivate}," +
                                     $"{stat.hpIncrease},{stat.moveSpeedIncrease},{stat.attackSpeedIncrease}," +
                                     $"{stat.attackRangeIncrease},{stat.hpRegenIncrease}");
                    }
                }
            }

            File.WriteAllText(path, csv.ToString());
            Debug.Log($"Successfully saved skill stats to CSV files in: {directory}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill stats to CSV: {e.Message}");
        }
    }

    private void SavePrefabToResources(GameObject prefab, SkillID skillId, string suffix)
    {
        if (prefab == null) return;

        string directory = Path.Combine(Application.dataPath, "Resources", PREFAB_PATH);
        Directory.CreateDirectory(directory);

        string prefabPath = Path.Combine(directory, $"{skillId}_{suffix}.prefab");
        string assetPath = $"Assets/Resources/{PREFAB_PATH}/{skillId}_{suffix}.prefab";

        // 프리팹 저장
        bool success = false;
        PrefabUtility.SaveAsPrefabAsset(prefab, assetPath, out success);

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

    private void SaveIconToResources(Sprite icon, SkillID skillId)
    {
        if (icon == null) return;

        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", ICON_PATH);
            Directory.CreateDirectory(directory);

            string assetPath = $"Assets/Resources/{ICON_PATH}/{skillId}_Icon.png";
            string fullPath = Path.Combine(directory, $"{skillId}_Icon.png");

            // 현재 아이콘의 에셋 경로 가져오기
            string sourceAssetPath = AssetDatabase.GetAssetPath(icon);
            Debug.Log($"Source asset path: {sourceAssetPath}");

            // 기존 파일이 있다면 삭제
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }

            if (!string.IsNullOrEmpty(sourceAssetPath))
            {
                // 스프라이트가 이미 에셋인 경우
                FileUtil.CopyFileOrDirectory(sourceAssetPath, assetPath);
                AssetDatabase.Refresh();

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.mipmapEnabled = false;
                    importer.isReadable = true;

                    // 스프라이트 메타 데이터 설정
                    var spritesheet = new SpriteMetaData
                    {
                        name = $"{skillId}_Icon",
                        rect = new Rect(0, 0, icon.texture.width, icon.texture.height),
                        pivot = new Vector2(0.5f, 0.5f),
                        border = Vector4.zero,
                        alignment = 0
                    };
                    importer.spritesheet = new SpriteMetaData[] { spritesheet };

                    importer.SaveAndReimport();
                    Debug.Log($"Saved icon as asset to: {assetPath}");
                }
            }
            else
            {
                // 스프라이트가 런타임 텍스처인 경우
                Texture2D newTexture = new Texture2D(icon.texture.width, icon.texture.height, TextureFormat.RGBA32, false);

                // 스프라이트의 텍스처 데이터 복사
                var pixels = icon.texture.GetPixels();
                newTexture.SetPixels(pixels);
                newTexture.Apply();

                // PNG로 저장
                byte[] bytes = newTexture.EncodeToPNG();
                File.WriteAllBytes(fullPath, bytes);
                Object.DestroyImmediate(newTexture);

                AssetDatabase.Refresh();

                // 저장된 텍스처의 임포터 설정
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.mipmapEnabled = false;
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    Debug.Log($"Created and saved new icon to: {assetPath}");
                }
            }

            // 저장 후 확인
            if (File.Exists(fullPath))
            {
                Debug.Log($"Icon file successfully created at: {fullPath}");
                EditorUtility.SetDirty(currentSkill.icon);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Failed to create icon file at: {fullPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save icon: {e.Message}\nStackTrace: {e.StackTrace}");
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
                // 기본값 설정
                damage = 10f,
                elementalPower = 1f,
                maxSkillLevel = 5
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

            // 에셋 경로도 함께 출력
            string assetPath = AssetDatabase.GetAssetPath(obj);
            Debug.Log($"Asset Path: {assetPath}");
        }
    }
}