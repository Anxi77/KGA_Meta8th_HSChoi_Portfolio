using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>
{
    private PlayerStat playerStat;
    private Inventory inventory;
    private Dictionary<SourceType, List<StatContainer>> temporaryEffects = new();
    private Dictionary<SourceType, List<StatContainer>> temporaryEffectsBackup;

    public void Initialize(Player player)
    {
        if (player == null) return;

        playerStat = player.GetComponent<PlayerStat>();
        inventory = player.GetComponent<Inventory>();

        ClearTemporaryEffects();
    }

    public void InitializeTestPlayer()
    {
        if (playerStat != null)
        {
            // 테스트용 기본 스탯 설정
            playerStat.ResetToBase();
            playerStat.level = 10;  // 테스트용 레벨
            playerStat.currentExp = 0;

            // 테스트용 강화된 스탯
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Base, IncreaseType.Add, 500f);
            playerStat.AddStatModifier(StatType.Damage, SourceType.Base, IncreaseType.Add, 50f);
            playerStat.AddStatModifier(StatType.Defense, SourceType.Base, IncreaseType.Add, 20f);
            playerStat.AddStatModifier(StatType.MoveSpeed, SourceType.Base, IncreaseType.Add, 2f);
            playerStat.AddStatModifier(StatType.AttackSpeed, SourceType.Base, IncreaseType.Add, 1f);
            playerStat.AddStatModifier(StatType.ExpCollectionRadius, SourceType.Base, IncreaseType.Add, 3f);

            playerStat.RestoreFullHealth();
        }

        // 테스트용 아이템 지급
        if (inventory != null)
        {
            var testItems = GetTestItems();
            foreach (var item in testItems)
            {
                inventory.AddItem(item);
            }
        }
    }

    private List<ItemData> GetTestItems()
    {
        return new List<ItemData>
        {
            ItemManager.Instance.GetItem("test_sword"),
            ItemManager.Instance.GetItem("test_armor"),
            ItemManager.Instance.GetItem("test_accessory")
        };
    }

    public void InitializeNewPlayer()
    {
        // 새 플레이어 기본 설정
        if (playerStat != null)
        {
            playerStat.ResetToBase();
            playerStat.level = 1;
            playerStat.currentExp = 0;
            playerStat.RestoreFullHealth();
        }

        // 기본 아이템 지급
        if (inventory != null)
        {
            var startingItems = GetStartingItems();
            foreach (var item in startingItems)
            {
                inventory.AddItem(item);
            }
        }
    }

    private List<ItemData> GetStartingItems()
    {
        return new List<ItemData>
        {
            ItemManager.Instance.GetItem("default_sword"),
            ItemManager.Instance.GetItem("basic_armor")
        };
    }

    // 런타임 데이터 관리
    public void SaveRuntimeData()
    {
        SavePlayerState();
        ClearTemporaryEffects(); // 임시 효과는 저장하지 않음

        // PlayerDataManager를 통해 현재 스탯 데이터 저장
        if (playerStat != null)
        {
            GameManager.Instance.playerDataManager.SaveCurrentPlayerStatData();
        }
    }

    public void LoadRuntimeData()
    {
        // PlayerDataManager로부터 모든 데이터 로드
        if (playerStat != null)
        {
            // 스탯 데이터 로드
            var statData = GameManager.Instance.playerDataManager.CurrentPlayerStatData;
            playerStat.LoadStats(statData);

            // 레벨 데이터 로드
            var (level, exp) = GameManager.Instance.playerDataManager.LoadLevelData();
            playerStat.level = level;
            playerStat.currentExp = exp;
        }

        // 인벤토리 데이터 로드
        if (inventory != null)
        {
            var inventoryData = GameManager.Instance.playerDataManager.LoadInventoryData();
            inventory.LoadInventoryData(inventoryData);
        }

        // 런타임 상태 초기화
        ClearTemporaryEffects();
        RestorePlayerState();
    }

    // 런타임 스탯 관리
    public void AddTemporaryEffect(StatContainer effect, float duration = 0f)
    {
        if (!temporaryEffects.ContainsKey(effect.buffType))
        {
            temporaryEffects[effect.buffType] = new List<StatContainer>();
        }

        temporaryEffects[effect.buffType].Add(effect);
        playerStat?.AddStatModifier(effect.statType, effect.buffType, effect.incType, effect.amount);

        if (duration > 0)
        {
            StartCoroutine(RemoveEffectAfterDelay(effect, duration));
        }
    }

    public void RemoveTemporaryEffect(StatContainer effect)
    {
        if (temporaryEffects.TryGetValue(effect.buffType, out var effects))
        {
            effects.Remove(effect);
            playerStat?.RemoveStatModifier(effect.statType, effect.buffType, effect.incType, effect.amount);
        }
    }

    public void ClearTemporaryEffects()
    {
        foreach (var effects in temporaryEffects.Values)
        {
            foreach (var effect in effects)
            {
                playerStat?.RemoveStatModifier(
                    effect.statType,
                    effect.buffType,
                    effect.incType,
                    effect.amount
                );
            }
        }
        temporaryEffects.Clear();
    }

    // 스테이지 전환 관련
    public void SavePlayerState()
    {
        playerStat?.SaveCurrentState();
        inventory?.SaveInventoryState();
    }

    public void RestorePlayerState()
    {
        playerStat?.RestoreState();
        inventory?.RestoreInventoryState();
    }

    // 레벨업 처리
    public void HandleLevelUp()
    {
        if (playerStat != null)
        {
            // 레벨업 시 스탯 증가
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Level, IncreaseType.Add, 10f);
            playerStat.AddStatModifier(StatType.Damage, SourceType.Level, IncreaseType.Add, 2f);
            playerStat.AddStatModifier(StatType.Defense, SourceType.Level, IncreaseType.Add, 1f);

            // HP 회복
            playerStat.RestoreFullHealth();
        }
    }

    // 아이템 관련
    public void EquipItem(ItemData itemData, EquipmentSlot slot)
    {
        inventory?.EquipItem(itemData, slot);
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        inventory?.UnequipFromSlot(slot);
    }

    public void AddItem(ItemData itemData)
    {
        inventory?.AddItem(itemData);
    }

    // 상태 체크
    public bool IsAlive()
    {
        return playerStat != null && playerStat.currentHp > 0;
    }

    public float GetCurrentHpRatio()
    {
        if (playerStat == null) return 0f;
        return playerStat.currentHp / playerStat.GetFinalStatValue(StatType.MaxHp);
    }

    // 게임 저장/로드 연동
    public void SavePlayerData()
    {
        if (playerStat != null)
        {
            // 임시 효과 제외하고 영구적인 데이터만 저장
            ClearTemporaryEffects();

            // 현재 스탯 데이터 저장
            GameManager.Instance.playerDataManager.SaveCurrentPlayerStatData();

            // 인벤토리 데이터 저장
            if (inventory != null)
            {
                var inventoryData = inventory.GetInventoryData();
                GameManager.Instance.playerDataManager.SaveInventoryData(inventoryData);
            }

            // 레벨과 경험치 저장
            GameManager.Instance.playerDataManager.SaveLevelData(
                playerStat.level,
                playerStat.currentExp
            );
        }
    }

    public void LoadPlayerData()
    {
        // 스탯 데이터 로드
        var statData = GameManager.Instance.playerDataManager.CurrentPlayerStatData;
        if (statData != null && playerStat != null)
        {
            playerStat.LoadStats(statData);
        }

        // 인벤토리 데이터 로드
        var inventoryData = GameManager.Instance.playerDataManager.LoadInventoryData();
        if (inventoryData != null && inventory != null)
        {
            inventory.LoadInventoryData(inventoryData);
        }

        // 레벨 데이터 로드
        var (level, exp) = GameManager.Instance.playerDataManager.LoadLevelData();
        if (playerStat != null)
        {
            playerStat.level = level;
            playerStat.currentExp = exp;
        }
    }

    private IEnumerator RemoveEffectAfterDelay(StatContainer effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveTemporaryEffect(effect);
    }

    // 스테이지 전환을 위한 임시 상태 관리
    public void SaveGameState()
    {
        SavePlayerState();        // 현재 스탯 상태 저장
        SaveTemporaryEffects();   // 현재 적용중인 버프/디버프 저장
    }

    public void LoadGameState()
    {
        RestorePlayerState();     // 저장된 스탯 상태 복원
        RestoreTemporaryEffects(); // 저장된 버프/디버프 복원
    }

    private void SaveTemporaryEffects()
    {
        // 현재 적용중인 임시 효과들 저장
        temporaryEffectsBackup = new Dictionary<SourceType, List<StatContainer>>(temporaryEffects);
    }

    private void RestoreTemporaryEffects()
    {
        // 저장된 임시 효과들 복원
        if (temporaryEffectsBackup != null)
        {
            foreach (var kvp in temporaryEffectsBackup)
            {
                foreach (var effect in kvp.Value)
                {
                    AddTemporaryEffect(effect);
                }
            }
        }
    }
}