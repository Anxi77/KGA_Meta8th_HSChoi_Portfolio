using UnityEngine;

public class MainMenuStateHandler : IGameStateHandler
{
    public void OnEnter()
    {
        Debug.Log("Entering MainMenu state");
        UIManager.Instance.ShowMainMenu();
        Time.timeScale = 1f;
    }

    public void OnExit()
    {
        Debug.Log("Exiting MainMenu state");
        UIManager.Instance.HideMainMenu();
        UIManager.Instance.ClearUI(); // UI 정리
    }

    public void OnUpdate()
    {
        // 메인 메뉴 업데이트 로직
    }

    public void OnFixedUpdate() { }
}