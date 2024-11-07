using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData itemData;
    private SpriteRenderer spriteRenderer;
    private bool isEquipped = false;
    private EquipmentSlot currentSlot = EquipmentSlot.None;

    private bool isPickupable = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(ItemData data)
    {
        itemData = data;
        if (spriteRenderer != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
        }
        isPickupable = true;
        isEquipped = false;
        currentSlot = EquipmentSlot.None;
    }

    public bool TryEquip(Player player, EquipmentSlot slot)
    {
        if (!CanEquipToSlot(slot))
            return false;

        var inventory = player.GetComponent<Inventory>();
        var playerStat = player.GetComponent<PlayerStat>();

        if (inventory != null && playerStat != null)
        {
            // 같은 슬롯에 장착된 아이템이 있다면 해제
            if (inventory.GetEquippedItem(slot) != null)
            {
                inventory.UnequipFromSlot(slot);
            }

            // 새 아이템 장착
            playerStat.EquipItem(itemData.stats, slot);
            isEquipped = true;
            currentSlot = slot;
            inventory.EquipToSlot(this, slot);

            return true;
        }

        return false;
    }

    public void Unequip(Player player)
    {
        if (!isEquipped)
            return;

        var inventory = player.GetComponent<Inventory>();
        var playerStat = player.GetComponent<PlayerStat>();

        if (inventory != null && playerStat != null)
        {
            playerStat.UnequipItem(currentSlot);
            inventory.UnequipFromSlot(currentSlot);
            isEquipped = false;
            currentSlot = EquipmentSlot.None;
        }
    }

    private bool CanEquipToSlot(EquipmentSlot slot)
    {
        return (itemData.type, slot) switch
        {
            (ItemType.Weapon, EquipmentSlot.Weapon) => true,
            (ItemType.Armor, EquipmentSlot.Armor) => true,
            (ItemType.Accessory, EquipmentSlot.Accessory1) => true,
            (ItemType.Accessory, EquipmentSlot.Accessory2) => true,
            _ => false
        };
    }

    public ItemData GetItemData() => itemData;
    public bool IsEquipped => isEquipped;
    public EquipmentSlot CurrentSlot => currentSlot;

    private void Update()
    {
        if (!isPickupable) return;

        // 플레이어와의 거리 체크
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            float pickupRange = player.GetComponent<PlayerStat>().GetStat(StatType.ExpCollectionRadius);
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= pickupRange)
            {
                OnPickup(player);
            }
        }
    }

    protected virtual void OnPickup(Player player)
    {
        if (!isPickupable) return;

        isPickupable = false;

        // 아이템 타입에 따른 처리
        switch (itemData.type)
        {
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Accessory:
                AddToInventory(player);
                break;

            case ItemType.Consumable:
                ApplyConsumableEffect(player);
                break;

            case ItemType.Material:
                AddToInventory(player);
                break;
        }

        // PoolManager를 통해 오브젝트 반환
        PoolManager.Instance.Despawn(this);
    }

    private void AddToInventory(Player player)
    {
        var inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemData);
        }
    }

    private void ApplyConsumableEffect(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            foreach (var stat in itemData.stats)
            {
                playerStat.AddStatModifier(
                    stat.statType,
                    SourceType.Consumable,
                    stat.incType,
                    stat.amount
                );
            }
        }
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (GameManager.Instance?.player != null)
        {
            float pickupRange = GameManager.Instance.player.GetComponent<PlayerStat>().GetStat(StatType.ExpCollectionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}