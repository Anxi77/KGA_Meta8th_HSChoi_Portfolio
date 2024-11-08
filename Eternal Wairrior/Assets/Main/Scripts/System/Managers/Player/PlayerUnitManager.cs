using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using static StageManager;

public class PlayerUnitManager : SingletonManager<PlayerUnitManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private PlayerStat playerStat;
    private Inventory inventory;
    private Dictionary<SourceType, List<StatContainer>> temporaryEffects = new();
    private Dictionary<SourceType, List<StatContainer>> temporaryEffectsBackup;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        if (!PlayerDataManager.Instance.IsInitialized)
        {
            Debug.LogWarning("Waiting for PlayerDataManager to initialize...");
            return;
        }

        try
        {
            Debug.Log("Initializing PlayerUnitManager...");
            IsInitialized = true;
            Debug.Log("PlayerUnitManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PlayerUnitManager: {e.Message}");
            IsInitialized = false;
        }
    }

    #region Player Initialization
    public void InitializePlayer(Player player)
    {
        if (player == null) return;

        playerStat = player.GetComponent<PlayerStat>();
        inventory = player.GetComponent<Inventory>();
        ValidateReferences();
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

    public void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        try
        {
            if (GameManager.Instance.player != null)
            {
                GameManager.Instance.player.CleanupPlayer();
                SaveGameState();
                Destroy(GameManager.Instance.player.gameObject);
            }

            GameObject playerObj = Instantiate(playerPrefab, position, Quaternion.identity);
            DontDestroyOnLoad(playerObj);

            if (playerObj.TryGetComponent<Player>(out var player))
            {
                GameManager.Instance.InitializePlayer(player);
                InitializePlayer(player);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SpawnPlayer: {e.Message}");
        }
    }

    public Vector3 GetSpawnPosition(SceneType sceneType)
    {
        return sceneType switch
        {
            SceneType.Town => new Vector3(0, 0, 0),
            SceneType.Game => new Vector3(0, 0, 0),
            SceneType.Test => new Vector3(0, 0, 0),
            _ => Vector3.zero
        };
    }
    #endregion

    #region State Management
    public void SaveGameState()
    {
        SavePlayerState();
        SaveTemporaryEffects();
    }

    public void LoadGameState()
    {
        if (GameManager.Instance?.player == null)
        {
            Debug.LogError("Player is null during LoadGameState!");
            return;
        }

        try
        {
            GameManager.Instance.player.CleanupPlayer();
            ClearTemporaryEffects();

            var saveData = PlayerDataManager.Instance.LoadPlayerData("CurrentSave");
            if (saveData != null)
            {
                LoadPlayerData(saveData);
            }

            RestorePlayerState();
            RestoreTemporaryEffects();

            if (GameManager.Instance?.player != null)
            {
                GameManager.Instance.player.InitializePlayerSystems();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LoadGameState: {e.Message}");
        }
    }

    private void LoadPlayerData(PlayerDataManager.PlayerSaveData saveData)
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
    #endregion

    #region Effect Management
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

    private IEnumerator RemoveEffectAfterDelay(StatContainer effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveTemporaryEffect(effect);
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
    #endregion

    #region Player State
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
    #endregion
}