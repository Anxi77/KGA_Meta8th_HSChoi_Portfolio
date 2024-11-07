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

    private IEnumerator InitializeMainStage()
    {
        // 매니저들이 초기화될 때까지 대기
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            SkillDataManager.Instance != null &&
            SkillDataManager.Instance.IsInitialized);

        var player = GameManager.Instance?.player;
        if (player != null)
        {
            PlayerUnitManager.Instance.LoadGameState();
            SkillManager.Instance.ResetForNewStage();
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

        // 4. 새 씬 초기화 - 이벤트 기반으로 변경
        PlayerUnitManager.Instance.OnGameStateLoaded += OnGameStateLoaded;
        PlayerUnitManager.Instance.LoadGameState();
    }

    private void OnGameStateLoaded()
    {
        // 이벤트 구독 해제
        PlayerUnitManager.Instance.OnGameStateLoaded -= OnGameStateLoaded;

        // 씬별 초기화 진행
        switch (SceneManager.GetActiveScene().name)
        {
            case "GameScene":
                InitializeMainStage();
                StartCoroutine(InitializeMonsterSpawn());
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

    public void TransferToMainStage(Player player, Vector3 spawnPosition)
    {
        StartCoroutine(TransferPlayerCoroutine(player, spawnPosition));
    }

    private IEnumerator TransferPlayerCoroutine(Player player, Vector3 spawnPosition)
    {
        if (player == null)
        {
            Debug.LogError("Player is null!");
            yield break;
        }

        // 1. 현재 상태 저장
        PlayerUnitManager.Instance.SaveGameState();

        // 2. 씬 전환 준비
        CleanupStageItems();
        MonsterManager.Instance?.StopSpawning();
        ClearAllPools();

        // 3. 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. 플레이어 위치 설정
        player.transform.position = spawnPosition;

        // 5. 게임 상태 로드 및 초기화
        PlayerUnitManager.Instance.OnGameStateLoaded += OnGameStateLoaded;
        PlayerUnitManager.Instance.LoadGameState();

        // 6. 메인 스테이지 초기화
        yield return StartCoroutine(InitializeMainStage());
        StartCoroutine(InitializeMonsterSpawn());
    }

    // 마을에서 메인 스테이지로 이동할 때 사용할 수 있는 편의 메서드
    public void TransferFromTownToMainStage(Player player)
    {
        // 메인 스테이지의 기본 스폰 위치 설정
        Vector3 mainStageSpawnPosition = new Vector3(0, 0, 0); // 원하는 스폰 위치로 조정
        TransferToMainStage(player, mainStageSpawnPosition);
    }

    private const float STAGE_DURATION = 600f; // 10분
    private Portal bossPortal;
    private Portal townPortal;

    public void StartMainStageLoop()
    {
        StartCoroutine(MainStageLoopCoroutine());
    }

    private IEnumerator MainStageLoopCoroutine()
    {
        // 1. 스테이지 타이머 시작
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);

        // 2. 일반 몬스터 스폰 시작
        MonsterManager.Instance.StartSpawning();

        // 3. 타이머 종료 대기
        yield return new WaitUntil(() => StageTimeManager.Instance.IsStageTimeUp());

        // 4. 보스 스폰 알림
        UIManager.Instance.ShowBossWarning();

        // 5. 보스 스폰
        yield return StartCoroutine(SpawnBossWithDelay());

        // 6. 보스 처치 대기
        yield return new WaitUntil(() => IsBossDefeated());

        // 7. 마을 포탈 생성
        SpawnTownPortal();
    }

    private IEnumerator SpawnBossWithDelay()
    {
        yield return new WaitForSeconds(3f); // 경고 메시지 표시 시간
        MonsterManager.Instance.SpawnStageBoss();
    }

    private bool IsBossDefeated()
    {
        return MonsterManager.Instance.IsBossDefeated;
    }

    private void SpawnTownPortal()
    {
        Vector3 bossPosition = MonsterManager.Instance.LastBossPosition;
        // 프리팹을 직접 참조하도록 수정
        GameObject portalPrefab = Resources.Load<GameObject>("Prefabs/TownPortal");
        if (portalPrefab != null)
        {
            townPortal = PoolManager.Instance.Spawn<Portal>(portalPrefab, bossPosition, Quaternion.identity);
            townPortal.Initialize("Town", OnTownPortalEnter);
        }
        else
        {
            Debug.LogError("TownPortal prefab not found in Resources folder!");
        }
    }

    private void OnTownPortalEnter()
    {
        StartCoroutine(ReturnToTownCoroutine());
    }

    private IEnumerator ReturnToTownCoroutine()
    {
        // 1. 현재 상태 저장
        PlayerUnitManager.Instance.SaveGameState();

        // 2. 보상 지급
        HandleStageClearRewards(StageType.Boss);

        // 3. 씬 전환
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TownScene");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. 플레이어 위치 설정
        GameManager.Instance.player.transform.position = GetTownSpawnPosition();

        // 5. 상태 복원
        PlayerUnitManager.Instance.LoadGameState();
    }

    private Vector3 GetTownSpawnPosition()
    {
        // 마을의 스폰 포인트 반환
        return new Vector3(0, 0, 0); // 실제 마을 스폰 위치로 수정 필요
    }
}

public enum StageType
{
    Normal,
    Boss
}