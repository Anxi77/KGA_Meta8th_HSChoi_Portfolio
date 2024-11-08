using UnityEngine;
using System.Collections;
using static StageManager;

public class TownStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Debug.Log("Entering Town state");

        // UI 초기 정리
        UIManager.Instance.ClearUI();

        // 타운 진입 시 초기화
        if (GameManager.Instance?.player == null)
        {
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
            Debug.Log("Player spawned in Town");

            // 플레이어 생성 후 잠시 대기하여 모든 컴포넌트가 초기화되도록 함
            MonoBehaviour coroutineRunner = GameLoopManager.Instance;
            coroutineRunner.StartCoroutine(InitializeTownAfterPlayerSpawn());
        }
        else
        {
            InitializeTown();
        }
    }

    private IEnumerator InitializeTownAfterPlayerSpawn()
    {
        // 플레이어 컴포넌트들이 완전히 초기화될 때까지 대기
        yield return new WaitForSeconds(0.1f);
        InitializeTown();
    }

    private void InitializeTown()
    {
        // 카메라 설정
        CameraManager.Instance.SetupCamera(SceneType.Town);

        // PathFinding 비활성화
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }

        // 플레이어 UI 초기화
        if (UIManager.Instance?.playerUIManager != null)
        {
            UIManager.Instance.playerUIManager.gameObject.SetActive(true);
            UIManager.Instance.playerUIManager.InitializePlayerUI(GameManager.Instance.player);
            Debug.Log("Player UI initialized");
        }

        // 게임 스테이지 포탈 생성
        StageManager.Instance.SpawnGameStagePortal();
        Debug.Log("Game stage portal spawned");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Town state");
        PlayerUnitManager.Instance.SaveGameState();
    }

    public void OnUpdate()
    {
        // 타운 상태 업데이트 로직
    }

    public void OnFixedUpdate() { }
}