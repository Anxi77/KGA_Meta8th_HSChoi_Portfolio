using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    private Dictionary<string, ItemData> availableItems = new Dictionary<string, ItemData>();
    private Dictionary<string, Item> activeItems = new Dictionary<string, Item>();
    private Dictionary<EnemyType, DropTableData> dropTables = new Dictionary<EnemyType, DropTableData>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!ItemDataManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for ItemDataManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing ItemManager...");
            LoadItemData();
            LoadDropTables();
            IsInitialized = true;
            Debug.Log("ItemManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing ItemManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void LoadItemData()
    {
        var items = ItemDataManager.Instance.GetAllItemData();
        foreach (var item in items)
        {
            availableItems[item.id] = item;
        }
    }

    private void LoadDropTables()
    {
        dropTables = ItemDataManager.Instance.GetDropTables();
    }

    #region Item Management
    public void EquipItem(string itemId, EquipmentSlot slot)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            var player = GameManager.Instance?.player;
            if (player != null)
            {
                var playerStat = player.GetComponent<PlayerStat>();
                playerStat.EquipItem(itemData.stats, slot);
            }
        }
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            var playerStat = player.GetComponent<PlayerStat>();
            playerStat.UnequipItem(slot);
        }
    }

    public ItemData GetItem(string itemId)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return itemData.Clone();
        }
        return null;
    }
    #endregion

    #region Item Drop System
    public void DropItem(ItemData itemData, Vector3 position)
    {
        if (itemData?.prefab == null) return;

        var droppedItem = PoolManager.Instance.Spawn<Item>(
            itemData.prefab,
            position + Random.insideUnitSphere * 1f,
            Quaternion.identity
        );
        droppedItem.Initialize(itemData);
    }

    public void DropItems(List<ItemData> items, Vector3 position, float spreadRadius = 1f)
    {
        foreach (var item in items)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spreadRadius;
            randomOffset.z = 0;
            DropItem(item, position + randomOffset);
        }
    }

    public List<ItemData> GetDropsForEnemy(EnemyType enemyType, float luckMultiplier = 1f)
    {
        if (dropTables.TryGetValue(enemyType, out var dropTable))
        {
            var drops = new List<ItemData>();
            int dropCount = 0;

            // 보장 드랍
            if (Random.value < dropTable.guaranteedDropRate * luckMultiplier)
            {
                var guaranteedDrop = GetRandomDropFromTable(dropTable, luckMultiplier);
                if (guaranteedDrop != null)
                {
                    drops.Add(guaranteedDrop);
                    dropCount++;
                }
            }

            // 추가 드랍
            foreach (var entry in dropTable.dropEntries)
            {
                if (dropCount >= dropTable.maxDrops) break;

                if (Random.value < entry.dropRate * luckMultiplier)
                {
                    if (availableItems.TryGetValue(entry.itemId, out var itemData))
                    {
                        var drop = itemData.Clone();
                        drop.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                        drops.Add(drop);
                        dropCount++;
                    }
                }
            }

            return drops;
        }

        return new List<ItemData>();
    }

    private ItemData GetRandomDropFromTable(DropTableData dropTable, float luckMultiplier)
    {
        float totalWeight = dropTable.dropEntries.Sum(entry => entry.dropRate * luckMultiplier);
        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var entry in dropTable.dropEntries)
        {
            currentWeight += entry.dropRate * luckMultiplier;
            if (random <= currentWeight)
            {
                if (availableItems.TryGetValue(entry.itemId, out var itemData))
                {
                    var drop = itemData.Clone();
                    drop.amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    return drop;
                }
            }
        }

        return null;
    }
    #endregion

    #region Item Utilities
    public List<ItemData> GetRandomItems(int count, ItemType type = ItemType.None)
    {
        var filteredItems = availableItems.Values
            .Where(item => type == ItemType.None || item.type == type)
            .ToList();

        var result = new List<ItemData>();
        while (result.Count < count && filteredItems.Any())
        {
            int index = Random.Range(0, filteredItems.Count);
            result.Add(filteredItems[index].Clone());
            filteredItems.RemoveAt(index);
        }

        return result;
    }

    public bool CanStackMore(string itemId, int currentAmount)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return currentAmount < itemData.maxStack;
        }
        return false;
    }

    public int GetRemainingStackSpace(string itemId, int currentAmount)
    {
        if (availableItems.TryGetValue(itemId, out var itemData))
        {
            return itemData.maxStack - currentAmount;
        }
        return 0;
    }
    #endregion

    public List<GameObject> GetAllItemPrefabs()
    {
        return availableItems.Values
            .Where(item => item.prefab != null)
            .Select(item => item.prefab)
            .Distinct()
            .ToList();
    }
}