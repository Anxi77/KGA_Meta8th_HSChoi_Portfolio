using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Members

    private static GameManager instance;

    public static GameManager Instance => instance;

    internal List<Enemy> enemies = new List<Enemy>(); //ϴ  ü List

    internal Player player;

    [SerializeField] private PlayerStatData playerStatData;
    public PlayerStat playerStat { get; private set; }

    #endregion

    #region Unity Message Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // PlayerStatData는 Inspector에서 할당
            playerStat = new PlayerStat(player, playerStatData);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        //PlayerStatusCheck();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이나 빌드 인덱스에 따라 다르게 처리
        switch (scene.name)
        {
            case "MainMenu":
                ResetAllStats();
                break;
            case "GameScene":
                LoadPlayerStats();
                break;
            case "BossStage":
                // 보스전에서는 특정 스탯만 초기화하거나 버프 적용
                playerStat.RemoveStatsBySource(SourceType.Buff);
                playerStat.RemoveStatsBySource(SourceType.Debuff);
                break;
        }
    }

    // 모든 스탯 초기화
    public void ResetAllStats()
    {
        playerStat.ResetToBase();
    }

    // 특정 영구 효과만 유지하고 나머지 초기화
    public void ResetTemporaryStats()
    {
        playerStat.RemoveStatsBySource(SourceType.Buff);
        playerStat.RemoveStatsBySource(SourceType.Debuff);
        playerStat.RemoveStatsBySource(SourceType.Active);
    }

    // 장비와 패시브 스킬 효과는 유지하면서 초기화
    public void ResetButKeepPermanentStats()
    {
        var equipment = new List<StatContainer>();
        var passives = new List<StatContainer>();

        // 영구적인 효과들 임시 저장
        foreach (SourceType source in System.Enum.GetValues(typeof(SourceType)))
        {
            if (source.ToString().StartsWith("Equipment_"))
            {
                equipment.AddRange(playerStat.GetActiveEffects(source));
            }
            else if (source == SourceType.Passive)
            {
                passives.AddRange(playerStat.GetActiveEffects(source));
            }
        }

        // 전체 초기화
        playerStat.ResetToBase();

        // 영구적인 효과들 다시 적용
        foreach (var stat in equipment)
        {
            playerStat.AddStatModifier(stat.statType, stat.buffType, stat.incType, stat.amount);
        }
        foreach (var stat in passives)
        {
            playerStat.AddStatModifier(stat.statType, stat.buffType, stat.incType, stat.amount);
        }
    }

    // 게임 저장/로드
    public void SavePlayerStats()
    {
        // PlayerStatData에 현재 영구적인 효과들 저장
        playerStatData.SavePermanentStats();
    }

    public void LoadPlayerStats()
    {
        // PlayerStatData에서 저장된 영구적인 효과들 로드
        playerStat = new PlayerStat(player, playerStatData);
    }

    #endregion

    #region Unit Related Methods
    private void PlayerStatusCheck()
    {
        switch (player.playerStatus)
        {
            case Player.Status.Alive:
                break;
            case Player.Status.Dead:
                Time.timeScale = 0;
                break;
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }

    #endregion

}
