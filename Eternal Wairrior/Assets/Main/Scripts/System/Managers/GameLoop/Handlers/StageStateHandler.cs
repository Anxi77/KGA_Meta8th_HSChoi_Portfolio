using UnityEngine;
using System.Collections;
using static StageManager;

public class StageStateHandler : IGameStateHandler
{
    private const float STAGE_DURATION = 600f;
    private bool isBossPhase = false;
    private bool isInitialized = false;

    public void OnEnter()
    {
        Debug.Log("Entering Stage state");
        isInitialized = false;

        // UI 초기 정리
        UIManager.Instance.ClearUI();

        // 인벤토리 UI 비활성화
        UIManager.Instance.SetInventoryAccessible(false);
        UIManager.Instance.HideInventory();
        Debug.Log("Inventory UI disabled for Stage");

        // 플레이어 스폰 및 초기화
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Game);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Stage");

            MonoBehaviour coroutineRunner = GameLoopManager.Instance;
            coroutineRunner.StartCoroutine(InitializeStageAfterPlayerSpawn());
        }
        else
        {
            InitializeStage();
        }
    }

    private IEnumerator InitializeStageAfterPlayerSpawn()
    {
        // 로딩 스크린이 완전히 사라질 때까지 대기
        while (UIManager.Instance.IsLoadingScreenVisible())
        {
            yield return null;
        }

        // 약간의 지연을 줘서 화면 전환이 완전히 끝나길 기다림
        yield return new WaitForSeconds(0.2f);

        InitializeStage();
    }

    private void InitializeStage()
    {
        // 저장된 데이터 로드
        if (GameManager.Instance.HasSaveData())
        {
            PlayerUnitManager.Instance.LoadGameState();
        }

        // 카메라 설정
        CameraManager.Instance.SetupCamera(SceneType.Game);

        // PathFinding 활성화
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(true);
            PathFindingManager.Instance.InitializeWithNewCamera();
        }

        // 플레이어 UI 초기화
        if (UIManager.Instance?.playerUIPanel != null)
        {
            UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
            UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // 플레이어 경험치 체크
        GameManager.Instance.StartLevelCheck();

        // 스테이지 타이머 시작
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);
        UIManager.Instance.stageTimeUI.gameObject.SetActive(true);
        Debug.Log("Stage timer started");

        Debug.Log("Stage initialization complete");
        isInitialized = true;

        // 로딩이 완료된 후 몬스터 스폰 시작
        GameLoopManager.Instance.StartCoroutine(StartMonsterSpawningWhenReady());
    }

    private IEnumerator StartMonsterSpawningWhenReady()
    {
        // 약간의 지연 시간을 두어 다른 초기화가 완료되도록 함
        yield return new WaitForSeconds(0.5f);

        // 몬스터 매니저 초기화 및 스폰 시작
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.StartSpawning();
            Debug.Log("Monster spawning started");
        }
    }

    public void OnExit()
    {
        Debug.Log("Exiting Stage state");
        isInitialized = false;

        // 플레이어 데이터 저장 및 오브젝트 삭제
        if (GameManager.Instance?.player != null)
        {
            // 인벤토리 상태 저장
            if (GameManager.Instance.player.GetComponent<Inventory>() != null)
            {
                GameManager.Instance.player.GetComponent<Inventory>().SaveInventoryState();
            }

            // 플레이어 상태 저장
            PlayerUnitManager.Instance?.SaveGameState();

            // 플레이어 오브젝트 삭제
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        // 스테이지 관련 시스템 정리
        MonsterManager.Instance?.StopSpawning();
        StageTimeManager.Instance?.PauseTimer();
        StageTimeManager.Instance?.ResetTimer();
        CameraManager.Instance?.ClearCamera();

        // PathFinding 비활성화
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }
    }

    public void OnUpdate()
    {
        if (!isInitialized) return;

        if (!isBossPhase && StageTimeManager.Instance.IsStageTimeUp())
        {
            StartBossPhase();
        }
    }

    public void OnFixedUpdate() { }

    private void StartBossPhase()
    {
        isBossPhase = true;
        UIManager.Instance?.ShowBossWarning();
        MonsterManager.Instance?.SpawnStageBoss();
    }

    public void OnBossDefeated(Vector3 position)
    {
        StageManager.Instance?.SpawnTownPortal(position);
    }
}