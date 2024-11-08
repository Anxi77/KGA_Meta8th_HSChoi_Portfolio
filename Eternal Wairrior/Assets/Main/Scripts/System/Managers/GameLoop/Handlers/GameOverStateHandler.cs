using UnityEngine;

public class GameOverStateHandler : IGameStateHandler
{
    private const float RESPAWN_PORTAL_OFFSET = 2f;
    private const float RETURN_DELAY = 3f;
    private float returnTimer;

    public void OnEnter()
    {
        Time.timeScale = 0.5f;
        UIManager.Instance.ShowGameOverScreen();
        SpawnTownPortalAtPlayer();
        returnTimer = RETURN_DELAY;
    }

    public void OnExit()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideGameOverScreen();
    }

    public void OnUpdate()
    {
        returnTimer -= Time.unscaledDeltaTime;
        UIManager.Instance.UpdateGameOverTimer(returnTimer);

        if (returnTimer <= 0)
        {
            ReturnToTown();
        }
    }

    public void OnFixedUpdate() { }

    private void SpawnTownPortalAtPlayer()
    {
        if (GameManager.Instance.player != null)
        {
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            Vector3 portalPos = playerPos + Vector3.right * RESPAWN_PORTAL_OFFSET;
            StageManager.Instance.SpawnTownPortal(portalPos);
        }
    }

    private void ReturnToTown()
    {
        PlayerUnitManager.Instance.SaveGameState();
        StageManager.Instance.LoadTownScene();
    }
}