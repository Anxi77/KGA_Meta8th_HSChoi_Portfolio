using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemEditorData", menuName = "Game Data/Item Editor Data")]
public class ItemEditorDataContainer : ScriptableObject
{
    // 아이템 데이터
    [SerializeField] private List<ItemData> _itemList = new();
    public List<ItemData> itemList
    {
        get => _itemList;
        set => _itemList = value ?? new List<ItemData>();
    }

    // 드롭테이블 데이터
    [SerializeField] private List<DropTableData> _dropTableList = new();
    public Dictionary<EnemyType, DropTableData> dropTables
    {
        get => _dropTableList.ToDictionary(dt => dt.enemyType);
        set
        {
            _dropTableList = value?.Values.ToList() ?? new List<DropTableData>();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    // 아이콘 데이터
    [SerializeField] private List<IconEntry> _iconEntries = new();
    private Dictionary<string, Sprite> _itemIcons;
    public Dictionary<string, Sprite> itemIcons
    {
        get
        {
            if (_itemIcons == null)
            {
                _itemIcons = _iconEntries.ToDictionary(
                    entry => entry.itemId,
                    entry => entry.icon
                );
            }
            return _itemIcons;
        }
        set
        {
            _itemIcons = value ?? new Dictionary<string, Sprite>();
            _iconEntries = _itemIcons.Select(kvp => new IconEntry
            {
                itemId = kvp.Key,
                icon = kvp.Value
            }).ToList();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    [System.Serializable]
    private class IconEntry
    {
        public string itemId;
        public Sprite icon;
    }

    // 에디터 상태 데이터
#if UNITY_EDITOR
    public string lastSelectedItemId;
    public EditorTab currentTab = EditorTab.Items;
    public Vector2 itemListScrollPosition;
    public Vector2 itemDetailScrollPosition;
    public Vector2 dropTableScrollPosition;
    public Vector2 mainScrollPosition;

    // UI 상태
    public bool showBasicInfo = true;
    public bool showStatRanges = true;
    public bool showEffects = true;
    public bool showResources = true;

    // 드롭테이블 에디터 상태
    public EnemyType selectedEnemyType = EnemyType.Normal;
    public Dictionary<EnemyType, bool> dropTableFoldouts = new();

    // 검색 및 필터링
    public string searchText = "";
    public ItemType typeFilter = ItemType.None;
    public ItemRarity rarityFilter = ItemRarity.Common;

    // 효과 편집 상태
    public Dictionary<string, bool> effectFoldouts = new();
#endif

    public void Initialize()
    {
        // 기본 초기화
        if (_itemList == null) _itemList = new List<ItemData>();
        if (_dropTableList == null) _dropTableList = new List<DropTableData>();
        if (_iconEntries == null) _iconEntries = new List<IconEntry>();

#if UNITY_EDITOR
        // 드롭테이블 폴드아웃 초기화
        dropTableFoldouts = new Dictionary<EnemyType, bool>();
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            if (type != EnemyType.None)
                dropTableFoldouts[type] = false;
        }

        // 효과 폴드아웃 초기화
        effectFoldouts = new Dictionary<string, bool>();

        // UI 상태 초기화
        ResetEditorState();
#endif
    }

    // 아이콘 관리 메서드
    public void SaveIcon(string itemId, Sprite icon)
    {
        if (icon != null)
        {
            itemIcons[itemId] = icon;
        }
    }

    public Sprite GetIcon(string itemId)
    {
        return itemIcons.TryGetValue(itemId, out var icon) ? icon : null;
    }

    public DropTableData GetDropTable(EnemyType enemyType)
    {
        return dropTables.TryGetValue(enemyType, out var table) ? table : null;
    }

#if UNITY_EDITOR
    public void ResetEditorState()
    {
        lastSelectedItemId = null;
        currentTab = EditorTab.Items;
        itemListScrollPosition = Vector2.zero;
        itemDetailScrollPosition = Vector2.zero;
        dropTableScrollPosition = Vector2.zero;
        mainScrollPosition = Vector2.zero;

        showBasicInfo = true;
        showStatRanges = true;
        showEffects = true;
        showResources = true;

        searchText = "";
        typeFilter = ItemType.None;
        rarityFilter = ItemRarity.Common;
    }

    public void SaveItemData(ItemData item)
    {
        if (item == null) return;

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            try
            {
                // 기존 아이템 제거
                _itemList.RemoveAll(x => x.id == item.id);

                // 새 아이템 추가
                var savedItem = item.Clone();
                _itemList.Add(savedItem);

                // 아이콘 저장
                if (item.icon != null)
                {
                    var icons = itemIcons;
                    icons[item.id] = item.icon;
                    itemIcons = icons;
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving item data: {e.Message}\n{e.StackTrace}");
            }
        };
    }

    public void SaveDropTable(EnemyType type, DropTableData dropTable)
    {
        if (dropTable == null) return;

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            try
            {
                // 기존 드롭테이블 제거
                _dropTableList.RemoveAll(x => x.enemyType == type);

                // 새 드롭테이블 추가
                _dropTableList.Add(dropTable);

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving drop table: {e.Message}\n{e.StackTrace}");
            }
        };
    }

    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            try
            {
                if (_dropTableList == null) _dropTableList = new List<DropTableData>();
                if (_iconEntries == null) _iconEntries = new List<IconEntry>();
                if (_itemList == null) _itemList = new List<ItemData>();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in OnValidate: {e.Message}\n{e.StackTrace}");
            }
        };
    }
#endif
}

#if UNITY_EDITOR
public enum EditorTab
{
    Items,
    DropTables,
    Settings
}
#endif
