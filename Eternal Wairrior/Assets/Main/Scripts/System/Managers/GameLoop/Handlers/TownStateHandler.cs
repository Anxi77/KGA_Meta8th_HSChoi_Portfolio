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

        // 플레이어가 죽은 상태인지 확인
        if (GameManager.Instance?.player?.playerStatus == Player.Status.Dead)
        {
            Debug.Log("Player is dead, respawning...");
            RespawnPlayer();
        }
        // 플레이어가 없는 경우
        else if (GameManager.Instance?.player == null)
        {
            Debug.Log("No player found, spawning new player");
            Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
            PlayerUnitManager.Instance.SpawnPlayer(spawnPos);
        }

        // 플레이어 초기화 및 UI 설정을 코루틴으로 실행
        MonoBehaviour coroutineRunner = GameLoopManager.Instance;
        coroutineRunner.StartCoroutine(InitializeTownAfterPlayerSpawn());
    }

    private void RespawnPlayer()
    {
        // 기존 플레이어 제거
        if (GameManager.Instance.player != null)
        {
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }

        // 새 플레이어 스폰
        Vector3 spawnPos = PlayerUnitManager.Instance.GetSpawnPosition(SceneType.Town);
        PlayerUnitManager.Instance.SpawnPlayer(spawnPos);

        // 플레이어 상태 복구
        if (GameManager.Instance.player != null)
        {
            GameManager.Instance.player.playerStatus = Player.Status.Alive;
            PlayerUnitManager.Instance.LoadGameState();
        }
    }

    private IEnumerator InitializeTownAfterPlayerSpawn()
    {
        // 플레이어가 완전히 초기화될 때까지 대기
        while (GameManager.Instance?.player == null || !GameManager.Instance.player.IsInitialized)
        {
            yield return null;
        }

        InitializeTown();
    }

    private void InitializeTown()
    {
        if (GameManager.Instance?.player == null)
        {
            Debug.LogError("Cannot initialize town: Player is null");
            return;
        }

        // 카메라 설정
        CameraManager.Instance.SetupCamera(SceneType.Town);

        // PathFinding 비활성화
        if (PathFindingManager.Instance != null)
        {
            PathFindingManager.Instance.gameObject.SetActive(false);
        }

        // UI 초기화
        if (UIManager.Instance != null)
        {
            // 플레이어 UI 초기화
            if (UIManager.Instance.playerUIPanel != null)
            {
                UIManager.Instance.playerUIPanel.gameObject.SetActive(true);
                UIManager.Instance.playerUIPanel.InitializePlayerUI(GameManager.Instance.player);
                Debug.Log("Player UI initialized");
            }

            // 인벤토리 데이터 로드
            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var savedData = PlayerDataManager.Instance.CurrentInventoryData;
                if (savedData != null)
                {
                    inventory.LoadInventoryData(savedData);  // 기존 메서드 사용
                    Debug.Log("Inventory data loaded");
                }
            }

            // 인벤토리 UI 초기화 및 활성화
            UIManager.Instance.InitializeInventoryUI();
            UIManager.Instance.SetInventoryAccessible(true);
            UIManager.Instance.UpdateInventoryUI();
            Debug.Log("Inventory UI initialized and enabled");
        }

        // 게임 스테이지 포탈 생성
        StageManager.Instance.SpawnGameStagePortal();
        Debug.Log("Game stage portal spawned");
    }

    public void OnExit()
    {
        Debug.Log("Exiting Town state");

        // 인벤토리 데이터 저장
        if (GameManager.Instance?.player != null)
        {
            var inventory = GameManager.Instance.player.GetComponent<Inventory>();
            if (inventory != null)
            {
                var inventoryData = inventory.GetInventoryData();  // 기존 메서드 사용
                PlayerDataManager.Instance.SaveInventoryData(inventoryData);
                Debug.Log("Inventory data saved");
            }
            PlayerUnitManager.Instance.SaveGameState();
        }

        // 인벤토리 UI 비활성화
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetInventoryAccessible(false);
            UIManager.Instance.HideInventory();
            Debug.Log("Inventory UI disabled");
        }
    }

    public void OnUpdate() { }

    public void OnFixedUpdate() { }
}