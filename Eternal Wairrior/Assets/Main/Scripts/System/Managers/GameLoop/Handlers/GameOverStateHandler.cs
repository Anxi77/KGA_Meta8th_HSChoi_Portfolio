using UnityEngine;

public class GameOverStateHandler : IGameStateHandler
{
    private bool portalSpawned = false;

    public void OnEnter()
    {
        Debug.Log("Entering Game Over state");

        // UI 표시
        UIManager.Instance?.ShowGameOverScreen();

        // 플레이어가 없다면 다시 스폰
        if (GameManager.Instance?.player == null)
        {
            // 죽은 위치 근처에 플레이어 리스폰;
            PlayerUnitManager.Instance.SpawnPlayer(Vector3.zero);

            // 플레이어 상태 복원
            PlayerUnitManager.Instance.LoadGameState();

            Debug.Log("Player respawned at death location");
        }

        // 포탈이 아직 생성되지 않았을 때만 생성
        if (!portalSpawned && GameManager.Instance?.player != null)
        {
            SpawnTownPortal();
            portalSpawned = true;
        }
    }

    private void SpawnTownPortal()
    {
        if (GameManager.Instance?.player != null)
        {
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            Vector3 portalPosition = playerPos + new Vector3(2f, 0f, 0f);
            StageManager.Instance.SpawnTownPortal(portalPosition);
            Debug.Log("Town portal spawned near player's death location");
        }
    }

    public void OnExit()
    {
        Debug.Log("Exiting Game Over state");
        UIManager.Instance?.HideGameOverScreen();
        portalSpawned = false;

        // GameOver 상태를 나갈 때 플레이어 정리
        if (GameManager.Instance?.player != null)
        {
            GameObject.Destroy(GameManager.Instance.player.gameObject);
            GameManager.Instance.player = null;
        }
    }

    public void OnUpdate() { }

    public void OnFixedUpdate() { }
}