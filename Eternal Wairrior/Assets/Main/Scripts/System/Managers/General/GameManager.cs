using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    internal List<Enemy> enemies = new List<Enemy>();
    internal Player player;

    public PlayerDataManager playerDataManager { get; private set; }
    private const string CURRENT_SAVE_SLOT = "CurrentSave";

    public PlayerStatData PlayerStatData => playerDataManager.CurrentPlayerStatData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 데이터 매니저 초기화
            playerDataManager = new PlayerDataManager();

            // DefaultPlayerStats 로드
            var defaultStatData = Resources.Load<PlayerStatData>("DefaultPlayerStats");
            if (defaultStatData == null)
            {
                Debug.LogError("DefaultPlayerStats not found in Resources folder!");
            }
            else
            {
                playerDataManager.LoadPlayerStatData(Instantiate(defaultStatData));
            }

            playerDataManager.InitializeDefaultData();
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void InitializePlayer(Player newPlayer)
    {
        player = newPlayer;
        if (player != null)
        {
            // PlayerUnitManager 초기화
            PlayerUnitManager.Instance.Initialize(player);

            // 저장된 데이터가 있다면 로드
            var savedData = playerDataManager.LoadPlayerData(CURRENT_SAVE_SLOT);
            if (savedData != null)
            {
                PlayerUnitManager.Instance.LoadPlayerData();
            }
            else
            {
                // 새 게임 시작 시 기본 데이터로 초기화
                var defaultStatData = Resources.Load<PlayerStatData>("DefaultPlayerStats");
                if (defaultStatData != null)
                {
                    playerDataManager.LoadPlayerStatData(Instantiate(defaultStatData));
                }
                PlayerUnitManager.Instance.InitializeNewPlayer();
            }
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            SaveGameData();
            playerDataManager.SaveWithBackup();
            CleanupTemporaryResources();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during application quit: {e.Message}");
        }
    }

    private void CleanupTemporaryResources()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.ClearTemporaryEffects();
        }
        Resources.UnloadUnusedAssets();
    }

    public void SaveGameData()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.SavePlayerData();
        }
    }

    public void LoadGameData()
    {
        if (player != null)
        {
            PlayerUnitManager.Instance.LoadPlayerData();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }
}
