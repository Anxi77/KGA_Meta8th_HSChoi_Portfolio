using UnityEngine;
using System.Collections;
using static StageManager;

public class StageStateHandler : IGameStateHandler
{
    private const float STAGE_DURATION = 600f;
    private bool isBossPhase = false;

    public void OnEnter()
    {
        Debug.Log("Entering Stage state");

        // UI 초기 정리
        UIManager.Instance.ClearUI();

        // 플레이어 스폰 및 초기화
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Game);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Stage");

            // 플레이어 생성 후 잠시 대기하여 모든 컴포넌트가 초기화되도록 함
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
        // 플레이어 컴포넌트들이 완전히 초기화될 때까지 대기
        yield return new WaitForSeconds(0.1f);
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
        if (UIManager.Instance?.playerUIManager != null)
        {
            UIManager.Instance.playerUIManager.gameObject.SetActive(true);
            UIManager.Instance.playerUIManager.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // 플레이어 경험치 체크
        GameManager.Instance.StartLevelCheck();

        // 몬스터 매니저 초기화
        MonsterManager.Instance.StartSpawning();

        // 스테이지 타이머 시작
        StageTimeManager.Instance.StartStageTimer(STAGE_DURATION);

        Debug.Log("Stage initialization complete");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Stage state");
        // 스테이지 종료 시 데이터 저장
        PlayerUnitManager.Instance.SaveGameState();

        MonsterManager.Instance.StopSpawning();
        CameraManager.Instance.ClearCamera();
    }

    public void OnUpdate()
    {
        if (!isBossPhase && StageTimeManager.Instance.IsStageTimeUp())
        {
            StartBossPhase();
        }
    }

    public void OnFixedUpdate() { }

    private void StartBossPhase()
    {
        isBossPhase = true;
        UIManager.Instance.ShowBossWarning();
        MonsterManager.Instance.SpawnStageBoss();
    }

    public void OnBossDefeated(Vector3 position)
    {
        // 보스 처치 위치에 타운 포탈 생성
        StageManager.Instance.SpawnTownPortal(position);
    }
}