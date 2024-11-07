using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>
{
    private PlayerStat playerStat;
    private Inventory inventory;
    private Dictionary<SourceType, List<StatContainer>> temporaryEffects = new();
    private Dictionary<SourceType, List<StatContainer>> temporaryEffectsBackup;

    public event System.Action OnPlayerInitialized;
    public event System.Action OnGameStateLoaded;

    public void Initialize(Player player)
    {
        if (player == null) return;

        playerStat = player.GetComponent<PlayerStat>();
        inventory = player.GetComponent<Inventory>();

        ValidateReferences();
        OnPlayerInitialized?.Invoke();
    }

    public void LoadGameState()
    {
        var saveData = GameManager.Instance.playerDataManager.LoadPlayerData("CurrentSave");
        if (saveData != null)
        {
            if (playerStat != null)
            {
                playerStat.LoadStats(saveData.stats);
                playerStat.level = saveData.levelData.level;
                playerStat.currentExp = saveData.levelData.exp;
            }

            if (inventory != null && saveData.inventory != null)
            {
                inventory.LoadInventoryData(saveData.inventory);
            }
        }

        ClearTemporaryEffects();
        RestorePlayerState();
        RestoreTemporaryEffects();

        OnGameStateLoaded?.Invoke();
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
        if (playerStat != null)
        {
            // 기존 효과 모두 제거
            ClearTemporaryEffects();

            // 기본 스탯으로 초기화
            playerStat.ResetToBase();
            playerStat.level = 1;
            playerStat.currentExp = 0;

            // 체력을 최대치로 설정
            playerStat.RestoreFullHealth();
        }

        // 기본 아이템 지급
        if (inventory != null)
        {
            inventory.ClearInventory();  // 인벤토리 비우기
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
        ClearTemporaryEffects();

        var saveData = new PlayerDataManager.PlayerSaveData
        {
            stats = playerStat?.GetStatData(),
            inventory = inventory?.GetInventoryData(),
            levelData = new LevelData
            {
                level = playerStat?.level ?? 1,
                exp = playerStat?.currentExp ?? 0f
            }
        };

        GameManager.Instance.playerDataManager.SavePlayerData("CurrentSave", saveData);
    }

    public void LoadRuntimeData()
    {
        var saveData = GameManager.Instance.playerDataManager.LoadPlayerData("CurrentSave");
        if (saveData != null)
        {
            if (playerStat != null)
            {
                playerStat.LoadStats(saveData.stats);
                playerStat.level = saveData.levelData.level;
                playerStat.currentExp = saveData.levelData.exp;
            }

            if (inventory != null && saveData.inventory != null)
            {
                inventory.LoadInventoryData(saveData.inventory);
            }
        }

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
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Level, IncreaseType.Add, 10f);
            playerStat.AddStatModifier(StatType.Damage, SourceType.Level, IncreaseType.Add, 2f);
            playerStat.AddStatModifier(StatType.Defense, SourceType.Level, IncreaseType.Add, 1f);

            playerStat.RestoreFullHealth();

            // 레벨업 후 자동 저장
            SavePlayerData();
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
            ClearTemporaryEffects();

            // PlayerSaveData 객체 생성
            var saveData = new PlayerDataManager.PlayerSaveData
            {
                stats = playerStat.GetStatData(),
                inventory = inventory?.GetInventoryData(),
                levelData = new LevelData
                {
                    level = playerStat.level,
                    exp = playerStat.currentExp
                }
            };

            // 현재 슬롯에 저장
            GameManager.Instance.playerDataManager.SavePlayerData("CurrentSave", saveData);
        }
    }

    public void LoadPlayerData()
    {
        var saveData = GameManager.Instance.playerDataManager.LoadPlayerData("CurrentSave");
        if (saveData != null)
        {
            if (playerStat != null)
            {
                playerStat.LoadStats(saveData.stats);
                playerStat.level = saveData.levelData.level;
                playerStat.currentExp = saveData.levelData.exp;
            }

            if (inventory != null && saveData.inventory != null)
            {
                inventory.LoadInventoryData(saveData.inventory);
            }
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

    private void SaveTemporaryEffects()
    {
        temporaryEffectsBackup = new Dictionary<SourceType, List<StatContainer>>(temporaryEffects);
    }

    private void RestoreTemporaryEffects()
    {
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

    private void ValidateReferences()
    {
        if (playerStat == null)
        {
            Debug.LogError("PlayerStat reference is missing!");
        }
        if (inventory == null)
        {
            Debug.LogError("Inventory reference is missing!");
        }
    }
}