using System.Collections.Generic;

using System.Linq;

using UnityEngine;



public class Inventory : MonoBehaviour, IInitializable

{

    private List<InventorySlot> slots = new();

    private Dictionary<EquipmentSlot, Item> equippedItems = new();

    private int gold;

    private InventoryData savedState;

    private PlayerStat playerStat;

    public const int MAX_SLOTS = 20;

    public bool IsInitialized { get; private set; }

    public int MaxSlots => MAX_SLOTS;



    private void Awake()

    {

        playerStat = GetComponent<PlayerStat>();

    }



    public void Initialize()

    {

        if (!IsInitialized)

        {

            slots.Clear();

            equippedItems.Clear();

            gold = 0;

            LoadSavedInventory();

            IsInitialized = true;

        }

    }



    private void LoadSavedInventory()

    {

        var savedData = PlayerDataManager.Instance.CurrentInventoryData;

        if (savedData != null)

        {

            LoadInventoryData(savedData);

        }

    }



    public List<InventorySlot> GetSlots()

    {

        return new List<InventorySlot>(slots);

    }



    public InventoryData GetInventoryData()

    {

        return new InventoryData

        {

            slots = new List<InventorySlot>(slots),

            equippedItems = equippedItems.ToDictionary(

                kvp => kvp.Key,

                kvp => kvp.Value.GetItemData().id

            ),

            gold = this.gold

        };

    }



    public void LoadInventoryData(InventoryData data)

    {

        if (data == null) return;

        slots = new List<InventorySlot>(data.slots);

        gold = data.gold;

        foreach (var kvp in data.equippedItems)

        {

            var itemData = ItemManager.Instance.GetItem(kvp.Value);

            if (itemData != null)

            {

                EquipItem(itemData, kvp.Key);

            }

        }

    }



    public void AddItem(ItemData itemData)

    {

        if (itemData == null) return;



        if (itemData.maxStack > 1)

        {

            var existingSlot = slots.Find(slot =>

                slot.itemId == itemData.id &&

                slot.amount < itemData.maxStack);



            if (existingSlot != null)

            {

                existingSlot.amount++;

                return;

            }

        }



        if (slots.Count < MAX_SLOTS)

        {

            slots.Add(new InventorySlot

            {

                itemId = itemData.id,

                amount = 1,

                isEquipped = false

            });

        }

        else

        {

            Debug.LogWarning("Inventory is full!");

        }

    }



    public Item GetEquippedItem(EquipmentSlot slot)

    {

        if (equippedItems.TryGetValue(slot, out var item))

        {

            if (item != null && item.GetItemData() != null)

            {

                return item;

            }

            else

            {

                Debug.LogWarning($"Found null item or ItemData in slot {slot}");

                equippedItems.Remove(slot);

            }

        }

        return null;

    }



    public void EquipToSlot(Item item, EquipmentSlot slot)

    {

        if (equippedItems.ContainsKey(slot))

        {

            UnequipFromSlot(slot);

        }

        equippedItems[slot] = item;

        var inventorySlot = slots.Find(s => s.itemId == item.GetItemData().id);

        if (inventorySlot != null)

        {

            inventorySlot.isEquipped = true;

        }

    }



    public void UnequipFromSlot(EquipmentSlot slot)

    {

        if (equippedItems.TryGetValue(slot, out var item))

        {

            var inventorySlot = slots.Find(s => s.itemId == item.GetItemData().id);

            if (inventorySlot != null)

            {

                inventorySlot.isEquipped = false;

            }

            equippedItems.Remove(slot);

        }

    }



    public void EquipItem(ItemData itemData, EquipmentSlot slot)

    {

        if (itemData == null)

        {

            Debug.LogError("Attempted to equip null ItemData");

            return;

        }

        Debug.Log($"Attempting to equip {itemData.name} to slot {slot}");

        // 기존 장비 해제

        if (equippedItems.ContainsKey(slot))

        {

            UnequipFromSlot(slot);

        }



        // 새 아이템 생성 및 장착

        Item newItem = CreateEquipmentItem(itemData);

        if (newItem != null)

        {

            newItem.Initialize(itemData);

            equippedItems[slot] = newItem;



            // 인벤토리 슬롯 상태 업데이트

            var inventorySlot = slots.Find(s => s.itemId == itemData.id);

            if (inventorySlot != null)

            {

                inventorySlot.isEquipped = true;

            }



            // 스탯 적용

            if (playerStat != null)

            {

                playerStat.EquipItem(itemData.stats, slot);

            }



            Debug.Log($"Successfully equipped {itemData.name} to slot {slot}");

        }

        else

        {

            Debug.LogError($"Failed to create equipment item for {itemData.name}");

        }

    }



    private Item CreateEquipmentItem(ItemData itemData)

    {

        try

        {

            Item newItem = itemData.type switch

            {

                ItemType.Weapon => new WeaponItem(itemData),

                ItemType.Armor => new ArmorItem(itemData),

                ItemType.Accessory => new AccessoryItem(itemData),

                _ => null

            };



            if (newItem == null)

            {

                Debug.LogError($"Failed to create item of type: {itemData.type}");

            }

            else

            {

                Debug.Log($"Successfully created {itemData.type} item: {itemData.name}");

            }



            return newItem;

        }

        catch (System.Exception e)

        {

            Debug.LogError($"Error creating equipment item: {e.Message}");

            return null;

        }

    }



    public void SaveInventoryState()

    {

        savedState = GetInventoryData();

    }



    public void SaveEquippedItems()

    {

        if (savedState == null)

        {

            savedState = new InventoryData();

        }

        savedState.equippedItems = equippedItems.ToDictionary(

            kvp => kvp.Key,

            kvp => kvp.Value.GetItemData().id

        );

    }



    public void RestoreEquippedItems()

    {

        if (savedState?.equippedItems == null) return;

        foreach (var slot in equippedItems.Keys.ToList())

        {

            UnequipFromSlot(slot);

        }

        foreach (var kvp in savedState.equippedItems)

        {

            var itemData = ItemManager.Instance.GetItem(kvp.Value);

            if (itemData != null)

            {

                EquipItem(itemData, kvp.Key);

            }

        }

    }



    public void RestoreInventoryState()

    {

        if (savedState != null)

        {

            LoadInventoryData(savedState);

            savedState = null;

        }

    }



    public void ClearInventory()

    {

        foreach (var slot in equippedItems.Keys.ToList())

        {

            UnequipFromSlot(slot);

        }

        slots.Clear();

        equippedItems.Clear();

        gold = 0;

        savedState = null;

    }



    public void SwapItems(int fromSlot, int toSlot)

    {

        if (fromSlot < 0 || fromSlot >= slots.Count ||

            toSlot < 0 || toSlot >= slots.Count) return;

        var temp = slots[toSlot];

        slots[toSlot] = slots[fromSlot];

        slots[fromSlot] = temp;

    }



    public void UpdateStats()

    {

        if (playerStat == null) return;

        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))

        {

            playerStat.UnequipItem(slot);

        }

        foreach (var kvp in equippedItems)

        {

            var itemData = kvp.Value.GetItemData();

            if (itemData != null)

            {

                playerStat.EquipItem(itemData.stats, kvp.Key);

            }

        }

    }



    public void RemoveItem(string itemId)

    {

        var slot = slots.Find(s => s.itemId == itemId);

        if (slot != null)

        {

            slots.Remove(slot);

        }

    }

}


