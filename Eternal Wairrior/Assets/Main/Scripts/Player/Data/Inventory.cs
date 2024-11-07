using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<InventorySlot> slots = new();
    private Dictionary<EquipmentSlot, Item> equippedItems = new();
    private int gold;

    private InventoryData savedState;

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

        //  ItemManager 에서 Item 가져오기
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
        var existingSlot = slots.Find(slot => slot.itemId == itemData.id && slot.amount < itemData.maxStack);

        if (existingSlot != null)
        {
            existingSlot.amount++;
        }
        else
        {
            slots.Add(new InventorySlot
            {
                itemId = itemData.id,
                amount = 1,
                isEquipped = false
            });
        }
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems.TryGetValue(slot, out var item) ? item : null;
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
        var playerStat = GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.EquipItem(itemData.stats, slot);
        }
    }

    public void SaveInventoryState()
    {
        // 저장된 상태 저장
        savedState = GetInventoryData();
    }

    public void SaveEquippedItems()
    {
        // 장착된 아이템 저장
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

        // 장착된 아이템 해제
        foreach (var slot in equippedItems.Keys.ToList())
        {
            UnequipFromSlot(slot);
        }

        // 장착된 아이템 재설정
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
            savedState = null; // 저장된 상태 초기화
        }
    }

    public void ClearInventory()
    {
        // 모든 장착 아이템 해제
        foreach (var slot in equippedItems.Keys.ToList())
        {
            UnequipFromSlot(slot);
        }

        // 인벤토리 슬롯 초기화
        slots.Clear();
        equippedItems.Clear();
        gold = 0;

        // 저장된 상태도 초기화
        savedState = null;
    }
}