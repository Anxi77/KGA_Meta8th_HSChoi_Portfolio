using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        switch (scene.name)
        {
            case "MainMenu":
                ResetAllData();
                break;
            case "GameScene":
                InitializeMainStage();
                StartCoroutine(InitializeMonsterSpawn());
                break;
            case "BossStage":
                InitializeBossStage();
                break;
            case "TestScene":
                InitializeTestStage();
                break;
        }
    }

    private void ResetAllData()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            PlayerUnitManager.Instance.InitializeNewPlayer();
            SkillManager.Instance.ResetForNewStage();
        }

        // 몬스터 정리
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();
    }

    private void InitializeMainStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // 스탯 초기화
            PlayerUnitManager.Instance.LoadGameState();

            // 스킬 초기화
            SkillManager.Instance.ResetForNewStage();

            // 아이템 초기화
            InitializeStageItems();
        }
    }

    private void InitializeBossStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // 임시 효과만 제거
            PlayerUnitManager.Instance.ClearTemporaryEffects();
        }
    }

    private void InitializeTestStage()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            // 테스트용 스탯 설정
            PlayerUnitManager.Instance.InitializeTestPlayer();

            // 스킬 초기화
            SkillManager.Instance.ResetForNewStage();

            // 아이템 초기화
            InitializeStageItems();
        }
    }

    private IEnumerator InitializeMonsterSpawn()
    {
        // 플레이어가 완전히 초기화될 때까지 대기
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        // 풀 초기화
        ClearAllPools();
        InitializeObjectPools();

        // 몬스터 매니저 초기화 및 스폰 시작
        MonsterManager.Instance.StartSpawning();
    }

    private void ClearAllPools()
    {
        PoolManager.Instance.ClearAllPools();
    }

    private void InitializeObjectPools()
    {
        PoolManager.Instance.InitializePool();
    }

    private void InitializeStageItems()
    {
        // 드롭테이블 초기화
        ItemManager.Instance.LoadDropTables();
    }

    private void CleanupStageItems()
    {
        var droppedItems = FindObjectsOfType<Item>();
        foreach (var item in droppedItems)
        {
            PoolManager.Instance.Despawn<Item>(item);
        }
    }

    public IEnumerator LoadStageAsync(string sceneName)
    {
        // 1. 현재 상태 저장
        GameManager.Instance.SaveGameData();
        PlayerUnitManager.Instance.SaveGameState();

        // 2. 씬 전환 준비
        CleanupStageItems();
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();

        // 3. 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. 새 씬 초기화
        switch (sceneName)
        {
            case "GameScene":
                InitializeMainStage();
                break;
            case "BossStage":
                InitializeBossStage();
                break;
        }
    }

    public void HandleStageClearRewards(StageType stageType)
    {
        var rewards = GenerateStageRewards(stageType);
        foreach (var reward in rewards)
        {
            PlayerUnitManager.Instance.AddItem(reward);
        }
    }

    private List<ItemData> GenerateStageRewards(StageType stageType)
    {
        var rewards = new List<ItemData>();
        switch (stageType)
        {
            case StageType.Normal:
                rewards.AddRange(ItemManager.Instance.GetRandomItems(3));
                break;
            case StageType.Boss:
                rewards.AddRange(ItemManager.Instance.GetRandomItems(1, ItemType.Weapon));
                rewards.AddRange(ItemManager.Instance.GetRandomItems(2, ItemType.Armor));
                break;
        }
        return rewards;
    }

    public void LoadTestScene()
    {
        StartCoroutine(LoadStageAsync("TestScene"));
    }

    public void ReturnToMainStage()
    {
        StartCoroutine(LoadStageAsync("GameScene"));
    }
}

public enum StageType
{
    Normal,
    Boss
}