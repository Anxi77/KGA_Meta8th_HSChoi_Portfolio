using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public List<InventorySlot> slots = new();
    public int gold;
    public Dictionary<EquipmentSlot, string> equippedItems = new();
}

[System.Serializable]
public class InventorySlot
{
    public string itemId;
    public int amount;
    public bool isEquipped;
}