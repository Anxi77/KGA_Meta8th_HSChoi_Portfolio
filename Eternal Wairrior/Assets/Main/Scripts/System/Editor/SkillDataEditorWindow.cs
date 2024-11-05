using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SkillDataEditorWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Vector2 statsScrollPosition;
    private SkillData currentSkill;
    private GUIStyle headerStyle;
    private SkillEditorDataContainer editorData;

    [MenuItem("Tools/NEW Skill Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillDataEditorWindow>("Skill Data Editor");
    }

    private void OnEnable()
    {
        LoadEditorData();
        InitializeStyles();
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

    private void LoadEditorData()
    {
        // ���� SkillDataManager �ʱ�ȭ
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager == null)
        {
            var go = new GameObject("SkillDataManager");
            skillDataManager = go.AddComponent<SkillDataManager>();
            skillDataManager.InitializeDefaultData();  // �ʱ� ������ ���� ����
            Debug.Log("Created and initialized new SkillDataManager");
        }

        // ������ ������ �ε�
        editorData = AssetDatabase.LoadAssetAtPath<SkillEditorDataContainer>(
            "Assets/Resources/SkillEditorData.asset"
        );

        if (editorData == null)
        {
            string resourcesPath = Path.Combine(Application.dataPath, "Resources");
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
                Debug.Log($"Created Resources directory at: {resourcesPath}");
            }

            editorData = CreateInstance<SkillEditorDataContainer>();
            AssetDatabase.CreateAsset(editorData, "Assets/Resources/SkillEditorData.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Created new SkillEditorData asset");
        }

        scrollPosition = editorData.scrollPosition;
        statsScrollPosition = Vector2.zero;

        if (editorData.lastSelectedSkillID != SkillID.None)
        {
            currentSkill = editorData.skillList.Find(s => s.metadata.ID == editorData.lastSelectedSkillID);
        }
    }

    private void OnGUI()
    {
        if (editorData == null)
        {
            LoadEditorData();
            if (editorData == null) return;
        }

        // ��ü �����츦 �������� ����
        EditorGUILayout.BeginVertical();
        {
            // ���� ���� (���)
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                // ���� �г� (���� �ʺ�)
                EditorGUILayout.BeginVertical(GUILayout.Width(250));
                DrawLeftPanel();
                EditorGUILayout.EndVertical();

                // ���м�
                EditorGUILayout.Space(2);
                DrawVerticalLine(Color.gray);
                EditorGUILayout.Space(2);

                // ������ �г� (������ ����)
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                DrawRightPanel();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // ���м�
            DrawHorizontalLine(Color.gray);

            // �ϴ� ��ư ����
            EditorGUILayout.BeginVertical(GUILayout.Height(40));
            DrawBottomPanel();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(editorData);
        }
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Skills", headerStyle);
                if (currentSkill != null)
                {
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        DeleteCurrentSkill();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                if (editorData.skillList != null)
                {
                    foreach (var skill in editorData.skillList)
                    {
                        if (skill?.metadata == null) continue;

                        GUI.backgroundColor = currentSkill == skill ? Color.cyan : Color.white;
                        if (GUILayout.Button(skill.metadata.Name, GUILayout.Height(30)))
                        {
                            currentSkill = skill;
                            editorData.lastSelectedSkillID = skill.metadata.ID;
                            EditorUtility.SetDirty(editorData);
                        }
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create New Skill", GUILayout.Height(30)))
            {
                CreateNewSkill();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        if (currentSkill == null)
        {
            EditorGUILayout.LabelField("Select a skill to edit", headerStyle);
            return;
        }

        statsScrollPosition = EditorGUILayout.BeginScrollView(statsScrollPosition);
        {
            DrawBasicInfo();
            EditorGUILayout.Space(10);
            DrawPrefabSettings();
            EditorGUILayout.Space(10);
            DrawSkillStats();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawBottomPanel()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SaveAllData();
            }

            if (GUILayout.Button("Load All", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                LoadAllData();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create Backup", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                var skillDataManager = FindObjectOfType<SkillDataManager>();
                skillDataManager?.SaveWithBackup();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawVerticalLine(Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(1));
        EditorGUI.DrawRect(rect, color);
    }

    private void DrawHorizontalLine(Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, color);
    }

    private void CreateNewSkill()
    {
        // SkillDataManager�� ������ �����ϰ� �ʱ�ȭ
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager == null)
        {
            var go = new GameObject("SkillDataManager");
            skillDataManager = go.AddComponent<SkillDataManager>();
            skillDataManager.InitializeDefaultData();
        }

        var newSkill = new SkillData
        {
            metadata = new SkillMetadata
            {
                Name = "New Skill",
                Description = "New skill description",
                Type = SkillType.None,
                ID = GetNextAvailableSkillID(),
                Element = ElementType.None
            }
        };

        if (editorData.skillList == null)
        {
            editorData.skillList = new List<SkillData>();
        }

        editorData.skillList.Add(newSkill);
        currentSkill = newSkill;
        editorData.lastSelectedSkillID = newSkill.metadata.ID;

        // �⺻ ���� 1 ���� ����
        var defaultStat = new SkillStatData
        {
            skillID = newSkill.metadata.ID,
            level = 1,
            damage = 10f,
            maxSkillLevel = 5,
            element = newSkill.metadata.Element,
            elementalPower = 1f
        };

        var stats = new SkillEditorDataContainer.SkillLevelStats
        {
            skillID = newSkill.metadata.ID,
            levelStats = new List<SkillStatData> { defaultStat }
        };

        if (editorData.skillStats == null)
        {
            editorData.skillStats = new List<SkillEditorDataContainer.SkillLevelStats>();
        }

        editorData.skillStats.Add(stats);
        EditorUtility.SetDirty(editorData);
    }

    private SkillID GetNextAvailableSkillID()
    {
        var existingIDs = editorData.skillList
            .Select(s => (int)s.metadata.ID)
            .ToList();

        var allIDs = System.Enum.GetValues(typeof(SkillID))
            .Cast<int>()
            .Where(id => id != (int)SkillID.None)
            .ToList();

        return (SkillID)allIDs.FirstOrDefault(id => !existingIDs.Contains(id));
    }

    private void DeleteCurrentSkill()
    {
        if (currentSkill == null) return;

        if (EditorUtility.DisplayDialog("Delete Skill",
            $"Are you sure you want to delete '{currentSkill.metadata.Name}'?",
            "Delete", "Cancel"))
        {
            editorData.skillList.Remove(currentSkill);
            var stats = editorData.skillStats.Find(s => s.skillID == currentSkill.metadata.ID);
            if (stats != null)
            {
                editorData.skillStats.Remove(stats);
            }

            currentSkill = null;
            editorData.lastSelectedSkillID = SkillID.None;
            EditorUtility.SetDirty(editorData);
        }
    }

    private void AddNewLevel()
    {
        if (currentSkill == null) return;

        var stats = editorData.skillStats.Find(s => s.skillID == currentSkill.metadata.ID);
        if (stats == null)
        {
            stats = new SkillEditorDataContainer.SkillLevelStats
            {
                skillID = currentSkill.metadata.ID,
                levelStats = new List<SkillStatData>()
            };
            editorData.skillStats.Add(stats);
        }

        int newLevel = stats.levelStats.Count + 1;

        // ���� ������ ������ �����Ͽ� �� ���� ����
        var prevStat = stats.levelStats.LastOrDefault()?.Clone() ?? new SkillStatData();
        prevStat.level = newLevel;
        prevStat.skillID = currentSkill.metadata.ID;

        // �⺻���� ���� ����
        prevStat.damage *= 1.1f;
        prevStat.elementalPower *= 1.1f;

        stats.levelStats.Add(prevStat);
        EditorUtility.SetDirty(editorData);
    }

    private void SaveAllData()
    {
        if (editorData == null)
        {
            Debug.LogError("EditorData is null");
            return;
        }

        if (editorData.skillList == null || editorData.skillList.Count == 0)
        {
            Debug.LogError("No skills to save in EditorData");
            return;
        }

        EditorUtility.DisplayProgressBar("Saving Data", "Initializing...", 0f);

        try
        {
            Debug.Log($"Attempting to save {editorData.skillList.Count} skills and {editorData.skillStats?.Count ?? 0} stat entries");

            // SkillDataManager ã�� �Ǵ� ����
            var skillDataManager = FindObjectOfType<SkillDataManager>();
            if (skillDataManager == null)
            {
                var go = new GameObject("SkillDataManager");
                skillDataManager = go.AddComponent<SkillDataManager>();
            }

            // �ʱ�ȭ �õ�
            EditorUtility.DisplayProgressBar("Saving Data", "Initializing SkillDataManager...", 0.2f);
            skillDataManager.InitializeDefaultData();

            // �ʱ�ȭ Ȯ��
            if (!skillDataManager.IsInitialized)
            {
                throw new System.Exception("Failed to initialize SkillDataManager");
            }

            EditorUtility.DisplayProgressBar("Saving Data", "Saving skill data...", 0.4f);

            // ������ ��ȿ�� �˻�
            foreach (var skill in editorData.skillList)
            {
                if (skill?.metadata == null)
                {
                    Debug.LogError($"Invalid skill data found");
                    continue;
                }
                Debug.Log($"Preparing to save skill: {skill.metadata.Name} (ID: {skill.metadata.ID})");
            }

            // ���� ������ ����
            EditorUtility.DisplayProgressBar("Saving Data", "Writing files...", 0.6f);
            skillDataManager.SaveAllData(editorData.skillList, editorData.skillStats);

            // ������ ������ ����
            EditorUtility.DisplayProgressBar("Saving Data", "Saving editor data...", 0.8f);
            EditorUtility.SetDirty(editorData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("All skill data saved successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            // ������ ��忡���� SkillDataManager�� ����
            if (Application.isEditor && !Application.isPlaying)
            {
                var manager = FindObjectOfType<SkillDataManager>();
                if (manager != null)
                {
                    DestroyImmediate(manager.gameObject);
                }
            }
        }
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Basic Information", headerStyle);

        // ��Ÿ������ ����
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            currentSkill.metadata.Name = EditorGUILayout.TextField("Name", currentSkill.metadata.Name);
            currentSkill.metadata.Description = EditorGUILayout.TextField("Description", currentSkill.metadata.Description);

            // ��ų Ÿ���� ����Ǹ� ���� �ʱ�ȭ
            SkillType newType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", currentSkill.metadata.Type);
            if (newType != currentSkill.metadata.Type)
            {
                currentSkill.metadata.Type = newType;
                InitializeSkillStats();
            }

            currentSkill.metadata.ID = (SkillID)EditorGUILayout.EnumPopup("Skill ID", currentSkill.metadata.ID);
            currentSkill.metadata.Element = (ElementType)EditorGUILayout.EnumPopup("Element", currentSkill.metadata.Element);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        // �⺻ ���� ����
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (currentSkill.baseStats == null)
            {
                currentSkill.baseStats = new BaseSkillStat();
            }

            currentSkill.baseStats.damage = EditorGUILayout.FloatField("Base Damage", currentSkill.baseStats.damage);
            currentSkill.baseStats.skillLevel = EditorGUILayout.IntField("Current Level", currentSkill.baseStats.skillLevel);
            currentSkill.baseStats.maxSkillLevel = EditorGUILayout.IntField("Max Level", currentSkill.baseStats.maxSkillLevel);
            currentSkill.baseStats.element = (ElementType)EditorGUILayout.EnumPopup("Base Element", currentSkill.baseStats.element);
            currentSkill.baseStats.elementalPower = EditorGUILayout.FloatField("Elemental Power", currentSkill.baseStats.elementalPower);

            // ��Ÿ�����Ϳ� ���̽� ������ ������Ʈ ����ȭ
            if (currentSkill.baseStats.element != currentSkill.metadata.Element)
            {
                if (EditorUtility.DisplayDialog("Element Mismatch",
                    "Base Stats element differs from Metadata element. Would you like to sync them?",
                    "Yes", "No"))
                {
                    currentSkill.baseStats.element = currentSkill.metadata.Element;
                }
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(editorData);
        }
    }

    private void DrawPrefabSettings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Settings", headerStyle);

        // ������ ����
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Icon", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            currentSkill.icon = (Sprite)EditorGUILayout.ObjectField(
                "Icon Sprite",
                currentSkill.icon,
                typeof(Sprite),
                false
            );
            if (EditorGUI.EndChangeCheck() && currentSkill.icon != null)
            {
                currentSkill.metadata.Icon = currentSkill.icon;
                EditorUtility.SetDirty(editorData);
            }

            if (currentSkill.icon != null)
            {
                var rect = EditorGUILayout.GetControlRect(false, 64);
                rect.width = 64;
                EditorGUI.DrawPreviewTexture(rect, currentSkill.icon.texture);
            }
        }
        EditorGUILayout.EndVertical();

        // ��Ÿ������ ������
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Metadata Prefab", EditorStyles.boldLabel);
            currentSkill.metadata.Prefab = (GameObject)EditorGUILayout.ObjectField(
                "Base Prefab",
                currentSkill.metadata.Prefab,
                typeof(GameObject),
                false
            );
        }
        EditorGUILayout.EndVertical();

        // ������Ÿ�� ������ (�ش��ϴ� ���)
        if (currentSkill.metadata.Type == SkillType.Projectile)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Projectile Prefab", EditorStyles.boldLabel);
                currentSkill.projectile = (GameObject)EditorGUILayout.ObjectField(
                    "Projectile",
                    currentSkill.projectile,
                    typeof(GameObject),
                    false
                );
            }
            EditorGUILayout.EndVertical();
        }

        // ������ ������
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Level Prefabs", EditorStyles.boldLabel);

            // ������ ������ �迭 ũ�� ����
            int newSize = EditorGUILayout.IntField("Level Count", currentSkill.prefabsByLevel?.Length ?? 0);
            if (newSize != currentSkill.prefabsByLevel?.Length)
            {
                System.Array.Resize(ref currentSkill.prefabsByLevel, newSize);
                EditorUtility.SetDirty(editorData);
            }

            if (currentSkill.prefabsByLevel != null)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentSkill.prefabsByLevel.Length; i++)
                {
                    currentSkill.prefabsByLevel[i] = (GameObject)EditorGUILayout.ObjectField(
                        $"Level {i + 1}",
                        currentSkill.prefabsByLevel[i],
                        typeof(GameObject),
                        false
                    );
                }
                EditorGUI.indentLevel--;
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void InitializeSkillStats()
    {
        var stats = editorData.skillStats.Find(s => s.skillID == currentSkill.metadata.ID);
        if (stats == null)
        {
            stats = new SkillEditorDataContainer.SkillLevelStats
            {
                skillID = currentSkill.metadata.ID,
                levelStats = new List<SkillStatData>()
            };
            editorData.skillStats.Add(stats);
        }

        // ���� ���� �ʱ�ȭ
        stats.levelStats.Clear();

        // ���� 1 �⺻ ���� ����
        var defaultStat = new SkillStatData
        {
            skillID = currentSkill.metadata.ID,
            level = 1,
            damage = 10f,
            maxSkillLevel = 5,
            element = currentSkill.metadata.Element,
            elementalPower = 1f
        };

        stats.levelStats.Add(defaultStat);
        EditorUtility.SetDirty(editorData);
    }

    private void DrawSkillStats()
    {
        EditorGUILayout.LabelField("Skill Stats", headerStyle);
        EditorGUI.indentLevel++;

        var stats = editorData.skillStats.Find(s => s.skillID == currentSkill.metadata.ID);
        if (stats == null)
        {
            stats = new SkillEditorDataContainer.SkillLevelStats
            {
                skillID = currentSkill.metadata.ID,
                levelStats = new List<SkillStatData>()
            };
            editorData.skillStats.Add(stats);
        }

        foreach (var stat in stats.levelStats)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField($"Level {stat.level}", EditorStyles.boldLabel);
                DrawStatFields(stat);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add Level"))
        {
            AddNewLevel(stats);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawStatFields(SkillStatData stat)
    {
        // �⺻ ����
        stat.damage = EditorGUILayout.FloatField("Damage", stat.damage);
        stat.maxSkillLevel = EditorGUILayout.IntField("Max Level", stat.maxSkillLevel);
        stat.elementalPower = EditorGUILayout.FloatField("Elemental Power", stat.elementalPower);

        // ��ų Ÿ�Ժ� Ư�� ����
        switch (currentSkill.metadata.Type)
        {
            case SkillType.Projectile:
                DrawProjectileStats(stat);
                break;
            case SkillType.Area:
                DrawAreaStats(stat);
                break;
            case SkillType.Passive:
                DrawPassiveStats(stat);
                break;
        }
    }

    private void AddNewLevel(SkillEditorDataContainer.SkillLevelStats stats)
    {
        var newLevel = stats.levelStats.Count + 1;
        var newStat = new SkillStatData
        {
            skillID = currentSkill.metadata.ID,
            level = newLevel,
            damage = 10f,
            maxSkillLevel = 5,
            element = currentSkill.metadata.Element,
            elementalPower = 1f
        };
        stats.levelStats.Add(newStat);
        EditorUtility.SetDirty(editorData);
    }

    private void DrawProjectileStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Projectile Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
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
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawAreaStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Area Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.radius = EditorGUILayout.FloatField("Radius", stat.radius);
            stat.duration = EditorGUILayout.FloatField("Duration", stat.duration);
            stat.tickRate = EditorGUILayout.FloatField("Tick Rate", stat.tickRate);
            stat.isPersistent = EditorGUILayout.Toggle("Is Persistent", stat.isPersistent);
            stat.moveSpeed = EditorGUILayout.FloatField("Move Speed", stat.moveSpeed);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawPassiveStats(SkillStatData stat)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Passive Stats", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            stat.effectDuration = EditorGUILayout.FloatField("Effect Duration", stat.effectDuration);
            stat.cooldown = EditorGUILayout.FloatField("Cooldown", stat.cooldown);
            stat.triggerChance = EditorGUILayout.FloatField("Trigger Chance", stat.triggerChance);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Passive Effects", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
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
        EditorGUILayout.EndVertical();
    }

    private void LoadAllData()
    {
        var skillDataManager = FindObjectOfType<SkillDataManager>();
        if (skillDataManager == null)
        {
            var go = new GameObject("SkillDataManager");
            skillDataManager = go.AddComponent<SkillDataManager>();
        }

        // ������ ������ ���ε�
        LoadEditorData();

        // ������� ����
        EditorUtility.SetDirty(editorData);
        AssetDatabase.SaveAssets();
    }
}