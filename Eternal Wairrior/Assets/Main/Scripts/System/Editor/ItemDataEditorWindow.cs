using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ItemDataEditorWindow : EditorWindow
{
    #region Fields
    private ItemEditorDataContainer editorData;
    private ItemDataManager itemDataManager;
    private GUIStyle headerStyle;
    private GUIStyle tabStyle;
    #endregion

    #region Properties
    private ItemData CurrentItem => editorData?.itemList.Find(i => i.id == editorData.lastSelectedItemId);
    #endregion

    [MenuItem("Tools/Item Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataEditorWindow>("Item Data Editor");
    }

    private void OnEnable()
    {
        InitializeEditorData();
        InitializeStyles();
    }

    private void InitializeEditorData()
    {
        // 에디터 데이터 로드 또는 생성
        editorData = AssetDatabase.LoadAssetAtPath<ItemEditorDataContainer>("Assets/Resources/ItemEditorData.asset");
        if (editorData == null)
        {
            CreateNewEditorData();
        }

        // ItemDataManager 초기화
        itemDataManager = FindObjectOfType<ItemDataManager>();
        if (itemDataManager == null)
        {
            var go = new GameObject("ItemDataManager");
            itemDataManager = go.AddComponent<ItemDataManager>();
            itemDataManager.InitializeDefaultData();
        }
    }

    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(5, 5, 10, 10)
        };

        tabStyle = new GUIStyle(EditorStyles.toolbarButton)
        {
            fixedHeight = 25,
            fontStyle = FontStyle.Bold
        };
    }

    private void CreateNewEditorData()
    {
        string path = "Assets/Resources/ItemEditorData.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        editorData = CreateInstance<ItemEditorDataContainer>();
        editorData.Initialize();
        AssetDatabase.CreateAsset(editorData, path);
        AssetDatabase.SaveAssets();
    }

    private void OnGUI()
    {
        if (editorData == null)
        {
            InitializeEditorData();
            if (editorData == null) return;
        }

        EditorGUILayout.BeginVertical();
        {
            // 헤더
            DrawHeader();
            // ���
            DrawTabs();
            // 탭과 내용 사이 여백 추가
            EditorGUILayout.Space(10);
            // 메인 컨텐츠를 스크롤 뷰로 감싸기
            float contentHeight = position.height - 90f; // 헤더(30) + 탭(25) + 여백(10) + 푸터(25)
            EditorGUILayout.BeginVertical(GUILayout.Height(contentHeight));
            {
                DrawMainContent();
            }
            EditorGUILayout.EndVertical();
            // 푸터
            DrawFooter();
        }
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(editorData);
        }
    }

    private void DrawMainContent()
    {
        // 스크롤 뷰로 감싸기
        editorData.mainScrollPosition = EditorGUILayout.BeginScrollView(editorData.mainScrollPosition);
        {
            switch (editorData.currentTab)
            {
                case EditorTab.Items:
                    DrawItemsTab();
                    break;
                case EditorTab.DropTables:
                    DrawDropTablesTab();
                    break;
                case EditorTab.Settings:
                    DrawSettingsTab();
                    break;
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(30));
        {
            GUILayout.Label("Item Data Editor", headerStyle);
            GUILayout.FlexibleSpace();
            DrawSearchBar();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTabs()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(25));
        {
            if (GUILayout.Toggle(editorData.currentTab == EditorTab.Items, "Items", tabStyle))
                editorData.currentTab = EditorTab.Items;
            if (GUILayout.Toggle(editorData.currentTab == EditorTab.DropTables, "Drop Tables", tabStyle))
                editorData.currentTab = EditorTab.DropTables;
            if (GUILayout.Toggle(editorData.currentTab == EditorTab.Settings, "Settings", tabStyle))
                editorData.currentTab = EditorTab.Settings;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(25));
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                SaveAllData();
            }
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                LoadAllData();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Create Backup", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                itemDataManager.SaveWithBackup();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemsTab()
    {
        // 전체 영역을 좌우로 분할
        EditorGUILayout.BeginHorizontal();
        {
            // 왼쪽 패널 - 아이템 리스트 (고정 너비)
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                DrawItemList();
            }
            EditorGUILayout.EndVertical();
            // 구분선
            EditorGUILayout.Space(5);
            DrawVerticalLine(Color.gray);
            EditorGUILayout.Space(5);
            // 오른쪽 패널 - 아이템 상세 정보 (나머지 영역 차지)
            EditorGUILayout.BeginVertical();
            {
                DrawItemDetails();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemList()
    {
        // 검색 및 필터 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Search & Filter", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            editorData.searchText = EditorGUILayout.TextField("Search", editorData.searchText);
            editorData.typeFilter = (ItemType)EditorGUILayout.EnumPopup("Type", editorData.typeFilter);
            editorData.rarityFilter = (ItemRarity)EditorGUILayout.EnumPopup("Rarity", editorData.rarityFilter);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
        // 아이템 리스트 영역
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            // 스크롤뷰의 높이를 윈도우 크기에 맞게 조정
            float listHeight = position.height - 300; // 상단 UI와 하단 버튼을 위한 여유 공간
            editorData.itemListScrollPosition = EditorGUILayout.BeginScrollView(
                editorData.itemListScrollPosition,
                GUILayout.Height(listHeight)
            );
            {
                var filteredItems = FilterItems();
                foreach (var item in filteredItems)
                {
                    bool isSelected = item.id == editorData.lastSelectedItemId;
                    GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                    if (GUILayout.Button(item.name, GUILayout.Height(25)))
                    {
                        editorData.lastSelectedItemId = item.id;
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
        // 새 아이템 생성 버튼
        if (GUILayout.Button("Create New Item", GUILayout.Height(30)))
        {
            CreateNewItem();
        }
    }

    private void DrawItemDetails()
    {
        try
        {
            if (CurrentItem == null)
            {
                EditorGUILayout.LabelField("Select an item to edit", headerStyle);
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                editorData.itemDetailScrollPosition = EditorGUILayout.BeginScrollView(
                    editorData.itemDetailScrollPosition,
                    GUILayout.Height(position.height - 100)
                );
                try
                {
                    // 기본 정보
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUI.BeginChangeCheck();
                        string newId = EditorGUILayout.TextField("ID", CurrentItem.id);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (!string.IsNullOrEmpty(newId) && newId != CurrentItem.id)
                            {
                                CurrentItem.id = newId;
                                editorData.lastSelectedItemId = newId;
                                EditorUtility.SetDirty(editorData);
                            }
                        }

                        CurrentItem.name = EditorGUILayout.TextField("Name", CurrentItem.name);
                        CurrentItem.description = EditorGUILayout.TextField("Description", CurrentItem.description);
                        CurrentItem.type = (ItemType)EditorGUILayout.EnumPopup("Type", CurrentItem.type);
                        CurrentItem.rarity = (ItemRarity)EditorGUILayout.EnumPopup("Rarity", CurrentItem.rarity);
                        CurrentItem.maxStack = EditorGUILayout.IntField("Max Stack", CurrentItem.maxStack);
                    }
                    EditorGUILayout.EndVertical();

                    // 스탯 범위 설정
                    if (editorData.showStatRanges)
                    {
                        EditorGUILayout.Space(10);
                        DrawStatRanges();
                    }

                    // 효과 설정
                    if (editorData.showEffects)
                    {
                        EditorGUILayout.Space(10);
                        DrawEffects();
                    }

                    // 리소스 설정
                    if (editorData.showResources)
                    {
                        EditorGUILayout.Space(10);
                        DrawResources();
                    }

                    // 삭제 버튼
                    EditorGUILayout.Space(20);
                    DrawDeleteButton();
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndVertical();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in DrawItemDetails: {e.Message}\n{e.StackTrace}");
            // GUI Layout 상태를 리셋
            EditorGUIUtility.ExitGUI();
        }
    }

    private void DrawDropTablesTab()
    {
        EditorGUILayout.BeginVertical();
        {
            // 드롭테이블 헤더
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Enemy Drop Tables", headerStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    itemDataManager.SaveDropTables();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            editorData.dropTableScrollPosition = EditorGUILayout.BeginScrollView(editorData.dropTableScrollPosition);
            {
                // 각 몬스터 타입별 드롭테이블
                foreach (EnemyType enemyType in System.Enum.GetValues(typeof(EnemyType)))
                {
                    if (enemyType == EnemyType.None) continue;
                    if (!editorData.dropTableFoldouts.ContainsKey(enemyType))
                        editorData.dropTableFoldouts[enemyType] = false;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        // 드롭테이블 헤더
                        EditorGUILayout.BeginHorizontal();
                        {
                            var headerStyle = new GUIStyle(EditorStyles.foldout)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 12
                            };
                            editorData.dropTableFoldouts[enemyType] = EditorGUILayout.Foldout(
                                editorData.dropTableFoldouts[enemyType],
                                $"{enemyType} Drop Table",
                                true,
                                headerStyle
                            );
                        }
                        EditorGUILayout.EndHorizontal();
                        // 드롭테이블 내용
                        if (editorData.dropTableFoldouts[enemyType])
                        {
                            EditorGUILayout.Space(5);
                            DrawDropTableSettings(enemyType);
                            EditorGUILayout.Space(5);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawDropTableSettings(EnemyType enemyType)
    {
        var dropTables = itemDataManager.GetDropTables();
        EditorGUI.BeginChangeCheck();
        if (!dropTables.TryGetValue(enemyType, out var dropTable))
        {
            dropTable = new DropTableData
            {
                enemyType = enemyType,
                dropEntries = new List<DropTableEntry>(),
                guaranteedDropRate = 0.1f,
                maxDrops = 3
            };
            dropTables[enemyType] = dropTable;
        }
        // 기본 설정 그룹
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUI.indentLevel++;
            dropTable.guaranteedDropRate = EditorGUILayout.Slider(
                new GUIContent("Guaranteed Drop Rate", "Chance for a guaranteed drop"),
                dropTable.guaranteedDropRate,
                0f,
                1f
            );
            dropTable.maxDrops = EditorGUILayout.IntSlider(
                new GUIContent("Max Drops", "Maximum number of items that can drop"),
                dropTable.maxDrops,
                1,
                10
            );
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
        // 드롭 엔트리 그룹
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Drop Entries", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Entry", GUILayout.Width(80)))
                {
                    var defaultItem = editorData.itemList.FirstOrDefault();
                    if (defaultItem != null)
                    {
                        dropTable.dropEntries.Add(new DropTableEntry
                        {
                            itemId = defaultItem.id,
                            dropRate = 0.1f,
                            rarity = ItemRarity.Common,
                            minAmount = 1,
                            maxAmount = 1
                        });
                        GUI.changed = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            if (dropTable.dropEntries == null)
                dropTable.dropEntries = new List<DropTableEntry>();
            // 엔트리 목록
            for (int i = 0; i < dropTable.dropEntries.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var entry = dropTable.dropEntries[i];
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField($"Entry {i + 1}", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            dropTable.dropEntries.RemoveAt(i);
                            i--;
                            GUI.changed = true;
                            continue;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                    // 아이템 선택
                    var items = editorData.itemList.Select(item => item.name).ToArray();
                    int selectedIndex = System.Array.FindIndex(items, name =>
                        editorData.itemList.Find(item => item.name == name)?.id == entry.itemId
                    );
                    EditorGUI.indentLevel++;
                    int newIndex = EditorGUILayout.Popup("Item", selectedIndex, items);
                    if (newIndex != selectedIndex && newIndex >= 0)
                    {
                        entry.itemId = editorData.itemList[newIndex].id;
                    }
                    entry.dropRate = EditorGUILayout.Slider("Drop Rate", entry.dropRate, 0f, 1f);
                    entry.rarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", entry.rarity);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Amount Range", GUILayout.Width(100));
                        entry.minAmount = EditorGUILayout.IntField(entry.minAmount, GUILayout.Width(50));
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        entry.maxAmount = EditorGUILayout.IntField(entry.maxAmount, GUILayout.Width(50));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck() || GUI.changed)
        {
            itemDataManager.SaveDropTables();
            EditorUtility.SetDirty(editorData);
        }
    }

    private void DrawSettingsTab()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("Editor Settings", headerStyle);
            EditorGUILayout.Space(10);
            // 백업 설정
            EditorGUILayout.LabelField("Backup Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Backup"))
            {
                itemDataManager.SaveWithBackup();
            }
            EditorGUILayout.Space(10);
            // 데이터 초��화
            EditorGUILayout.LabelField("Data Management", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Data",
                    "Are you sure you want to reset all data to default? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    itemDataManager.InitializeDefaultData();
                    InitializeEditorData();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawStatRanges()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        try
        {
            // 스탯 개수 범위 설정
            EditorGUILayout.BeginHorizontal();
            {
                CurrentItem.statRanges.minStatCount = EditorGUILayout.IntField("Stat Count", CurrentItem.statRanges.minStatCount);
                CurrentItem.statRanges.maxStatCount = EditorGUILayout.IntField("to", CurrentItem.statRanges.maxStatCount);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            // 가능한 스탯 목록
            for (int i = 0; i < CurrentItem.statRanges.possibleStats.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var statRange = CurrentItem.statRanges.possibleStats[i];
                    statRange.statType = (StatType)EditorGUILayout.EnumPopup("Stat Type", statRange.statType);
                    EditorGUILayout.BeginHorizontal();
                    {
                        statRange.minValue = EditorGUILayout.FloatField("Value Range", statRange.minValue);
                        statRange.maxValue = EditorGUILayout.FloatField("to", statRange.maxValue);
                    }
                    EditorGUILayout.EndHorizontal();
                    statRange.weight = EditorGUILayout.Slider("Weight", statRange.weight, 0f, 1f);
                    statRange.minRarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", statRange.minRarity);
                    statRange.increaseType = (IncreaseType)EditorGUILayout.EnumPopup("Increase Type", statRange.increaseType);
                    statRange.sourceType = (SourceType)EditorGUILayout.EnumPopup("Source Type", statRange.sourceType);

                    if (GUILayout.Button("Remove Stat Range"))

                    {

                        CurrentItem.statRanges.possibleStats.RemoveAt(i);

                        i--;

                    }

                }

                EditorGUILayout.EndVertical();

            }

            if (GUILayout.Button("Add Stat Range"))

            {

                CurrentItem.statRanges.possibleStats.Add(new ItemStatRange());

            }

        }

        finally

        {

            EditorGUILayout.EndVertical();

        }

    }

    private void DrawResources()

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        try

        {

            // 아이콘만 표시

            EditorGUILayout.BeginHorizontal();

            {

                EditorGUILayout.LabelField("Icon", GUILayout.Width(100));

                CurrentItem.icon = (Sprite)EditorGUILayout.ObjectField(

                    CurrentItem.icon,

                    typeof(Sprite),

                    false

                );

            }

            EditorGUILayout.EndHorizontal();

        }

        finally

        {

            EditorGUILayout.EndVertical();

        }

    }

    private void DrawSearchAndFilter()

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        {

            editorData.searchText = EditorGUILayout.TextField("Search", editorData.searchText);

            editorData.typeFilter = (ItemType)EditorGUILayout.EnumPopup("Type Filter", editorData.typeFilter);

            editorData.rarityFilter = (ItemRarity)EditorGUILayout.EnumPopup("Rarity Filter", editorData.rarityFilter);

        }

        EditorGUILayout.EndVertical();

    }

    private List<ItemData> FilterItems()

    {

        return editorData.itemList.Where(item =>

            (string.IsNullOrEmpty(editorData.searchText) ||

                item.name.ToLower().Contains(editorData.searchText.ToLower())) &&

            (editorData.typeFilter == ItemType.None || item.type == editorData.typeFilter) &&

            (item.rarity >= editorData.rarityFilter)

        ).ToList();

    }

    private void CreateNewItem()

    {

        var newItem = new ItemData

        {

            metadata = new ItemMetadata

            {

                ID = "NEW_ITEM_" + System.Guid.NewGuid().ToString().Substring(0, 8),

                Name = "New Item",

                Description = "New item description",

                Type = ItemType.None,

                Rarity = ItemRarity.Common,

                MaxStack = 1

            }

        };

        editorData.itemList.Add(newItem);

        editorData.lastSelectedItemId = newItem.id;

        EditorUtility.SetDirty(editorData);

    }

    private void DrawDeleteButton()

    {

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Delete Item", GUILayout.Height(30)))

        {

            if (EditorUtility.DisplayDialog("Delete Item",

                $"Are you sure you want to delete '{CurrentItem.name}'?",

                "Delete", "Cancel"))

            {

                editorData.itemList.Remove(CurrentItem);

                editorData.lastSelectedItemId = null;

                EditorUtility.SetDirty(editorData);

            }

        }

    }

    private void DrawSearchBar()

    {

        editorData.searchText = GUILayout.TextField(editorData.searchText, EditorStyles.toolbarSearchField);

    }

    private void DrawVerticalLine(Color color)

    {

        var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(1));

        EditorGUI.DrawRect(rect, color);

    }

    private void SaveAllData()

    {

        if (editorData == null) return;

        EditorUtility.DisplayProgressBar("Saving Data", "Saving items...", 0.3f);

        try

        {

            // 아이템 리스트를 복사하여 사용

            var itemsToSave = new List<ItemData>(editorData.itemList);

            // 복사한 리스트를 사용하여 저장

            foreach (var item in itemsToSave)

            {

                itemDataManager.SaveItemData(item);

            }

            // 드롭테이블도 저장

            itemDataManager.SaveDropTables();

            EditorUtility.SetDirty(editorData);

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log("All data saved successfully!");

        }

        catch (System.Exception e)

        {

            Debug.LogError($"Error saving data: {e.Message}\n{e.StackTrace}");

        }

        finally

        {

            EditorUtility.ClearProgressBar();

        }

    }

    private void LoadAllData()

    {

        if (editorData == null) return;

        EditorUtility.DisplayProgressBar("Loading Data", "Loading items...", 0.3f);

        try

        {

            InitializeEditorData();

            Debug.Log("All data loaded successfully!");

        }

        catch (System.Exception e)

        {

            Debug.LogError($"Error loading data: {e.Message}");

        }

        finally

        {

            EditorUtility.ClearProgressBar();

        }

    }

    private void DrawEffects()

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        try

        {

            // 효과 개수 범위 설정

            EditorGUILayout.BeginHorizontal();

            {

                CurrentItem.effectRanges.minEffectCount = EditorGUILayout.IntField("Effect Count", CurrentItem.effectRanges.minEffectCount);

                CurrentItem.effectRanges.maxEffectCount = EditorGUILayout.IntField("to", CurrentItem.effectRanges.maxEffectCount);

            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 가능한 효과 목록

            EditorGUILayout.LabelField("Possible Effects", EditorStyles.boldLabel);

            for (int i = 0; i < CurrentItem.effectRanges.possibleEffects.Count; i++)

            {

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                {

                    var effectRange = CurrentItem.effectRanges.possibleEffects[i];

                    effectRange.effectId = EditorGUILayout.TextField("Effect ID", effectRange.effectId);

                    effectRange.effectName = EditorGUILayout.TextField("Name", effectRange.effectName);

                    effectRange.description = EditorGUILayout.TextField("Description", effectRange.description);

                    effectRange.effectType = (EffectType)EditorGUILayout.EnumPopup("Type", effectRange.effectType);

                    EditorGUILayout.BeginHorizontal();

                    {

                        effectRange.minValue = EditorGUILayout.FloatField("Value Range", effectRange.minValue);

                        effectRange.maxValue = EditorGUILayout.FloatField("to", effectRange.maxValue);

                    }

                    EditorGUILayout.EndHorizontal();

                    effectRange.weight = EditorGUILayout.Slider("Weight", effectRange.weight, 0f, 1f);

                    effectRange.minRarity = (ItemRarity)EditorGUILayout.EnumPopup("Min Rarity", effectRange.minRarity);

                    // 적용 가능한 아이템 타입

                    DrawApplicableItemTypes(effectRange);

                    // 적용 가능한 스킬 타입

                    DrawApplicableSkillTypes(effectRange);

                    // 적용 가능한 속성

                    DrawApplicableElementTypes(effectRange);

                    if (GUILayout.Button("Remove Effect Range"))

                    {

                        CurrentItem.effectRanges.possibleEffects.RemoveAt(i);

                        i--;

                    }

                }

                EditorGUILayout.EndVertical();

            }

            if (GUILayout.Button("Add Effect Range"))

            {

                CurrentItem.effectRanges.possibleEffects.Add(new ItemEffectRange());

            }

        }

        finally

        {

            EditorGUILayout.EndVertical();

        }

    }

    private void DrawApplicableItemTypes(ItemEffectRange effectRange)

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        {

            EditorGUILayout.LabelField("Applicable Item Types", EditorStyles.boldLabel);

            if (effectRange.applicableTypes == null)

                effectRange.applicableTypes = new ItemType[0];

            var itemTypes = System.Enum.GetValues(typeof(ItemType));

            foreach (ItemType itemType in itemTypes)

            {

                bool isSelected = System.Array.IndexOf(effectRange.applicableTypes, itemType) != -1;

                bool newValue = EditorGUILayout.Toggle(itemType.ToString(), isSelected);

                if (newValue != isSelected)

                {

                    var list = new List<ItemType>(effectRange.applicableTypes);

                    if (newValue)

                        list.Add(itemType);

                    else

                        list.Remove(itemType);

                    effectRange.applicableTypes = list.ToArray();

                }

            }

        }

        EditorGUILayout.EndVertical();

    }

    private void DrawApplicableSkillTypes(ItemEffectRange effectRange)

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        {

            EditorGUILayout.LabelField("Applicable Skill Types", EditorStyles.boldLabel);

            if (effectRange.applicableSkills == null)

                effectRange.applicableSkills = new SkillType[0];

            var skillTypes = System.Enum.GetValues(typeof(SkillType));

            foreach (SkillType skillType in skillTypes)

            {

                bool isSelected = System.Array.IndexOf(effectRange.applicableSkills, skillType) != -1;

                bool newValue = EditorGUILayout.Toggle(skillType.ToString(), isSelected);

                if (newValue != isSelected)

                {

                    var list = new List<SkillType>(effectRange.applicableSkills);

                    if (newValue)

                        list.Add(skillType);

                    else

                        list.Remove(skillType);

                    effectRange.applicableSkills = list.ToArray();

                }

            }

        }

        EditorGUILayout.EndVertical();

    }

    private void DrawApplicableElementTypes(ItemEffectRange effectRange)

    {

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        {

            EditorGUILayout.LabelField("Applicable Element Types", EditorStyles.boldLabel);

            if (effectRange.applicableElements == null)

                effectRange.applicableElements = new ElementType[0];

            var elementTypes = System.Enum.GetValues(typeof(ElementType));

            foreach (ElementType elementType in elementTypes)

            {

                bool isSelected = System.Array.IndexOf(effectRange.applicableElements, elementType) != -1;

                bool newValue = EditorGUILayout.Toggle(elementType.ToString(), isSelected);

                if (newValue != isSelected)

                {

                    var list = new List<ElementType>(effectRange.applicableElements);

                    if (newValue)

                        list.Add(elementType);

                    else

                        list.Remove(elementType);

                    effectRange.applicableElements = list.ToArray();

                }

            }

        }

        EditorGUILayout.EndVertical();

    }

}

