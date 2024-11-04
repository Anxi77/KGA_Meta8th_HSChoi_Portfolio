using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : SingletonManager<UIManager>
{
    public Canvas mainCanvas;
    public GameObject pausePanel;
    public SkillLevelUpPanel levelupPanel;
    public PlayerSkillList skillList;
    public TextMeshProUGUI playerDefText;
    public TextMeshProUGUI playerAtkText;
    public TextMeshProUGUI playerMSText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI expCollectRadText;
    public TextMeshProUGUI playerHpRegenText;
    public TextMeshProUGUI playerAttackRangeText;
    public TextMeshProUGUI playerAttackSpeedText;
    public Slider hpBarImage;
    public Slider expBarImage;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(InitializeAfterGameManager());
    }

    private IEnumerator InitializeAfterGameManager()
    {
        // GameManager ʱȭ Ϸ UI ʱȭ
        while (GameManager.Instance == null || GameManager.Instance.player == null)
        {
            yield return null;
        }

        // GameManager ʱȭ
        InitializeUI();
    }

    private void InitializeUI()
    {
        pausePanel.SetActive(false);
        levelupPanel.gameObject.SetActive(false);
        StartCoroutine(PlayerInfoUpdate());
        StartCoroutine(CheckLevelUp());
    }

    bool isPaused = false;

    private void Update()
    {
        CheckPause();
    }

    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    private void Reset()
    {
        mainCanvas = GetComponent<Canvas>();
        pausePanel = transform.Find("PausePanel")?.gameObject;
        levelupPanel = transform.Find("LevelupPanel")?.GetComponent<SkillLevelUpPanel>();
    }

    private IEnumerator PlayerInfoUpdate()
    {
        Player player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("Player reference is null in UIManager");
            yield break;
        }

        while (true)
        {
            yield return new WaitForEndOfFrame();
            UpdatePlayerInfo(player);
        }
    }

    private void UpdatePlayerInfo(Player player)
    {
        try
        {
            playerAtkText.text = $"ATK : {player.damage:F1}";
            playerDefText.text = $"DEF : {player.defense:F1}";
            levelText.text = $"LEVEL : {player.level}";
            playerMSText.text = $"MoveSpeed : {player.moveSpeed:F1}";
            hpText.text = $"{player.hp:F0} / {player.maxHp:F0}";
            hpBarImage.value = player.hp / player.maxHp;
            expCollectRadText.text = $"ExpRad : {player.expCollectionRadius:F1}";
            playerHpRegenText.text = $"HPRegen : {player.hpRegenMultiplier:F1}x";
            playerAttackRangeText.text = $"AR : {player.attackRange:F1}";
            playerAttackSpeedText.text = $"AS : {player.attackSpeed:F1}/s";

            if (player.level >= player._expList.Count)
            {
                expText.text = "MAX LEVEL";
                expBarImage.value = 1;
            }
            else
            {
                float currentExp = player.CurrentExp();
                float requiredExp = player.GetExpForNextLevel();

                expText.text = $"EXP : {currentExp:F0}/{requiredExp:F0}";
                expBarImage.value = currentExp / requiredExp;

                Debug.Log($"Level: {player.level}, Total Exp: {player.exp}, Current Level Exp: {currentExp}, Required Exp: {requiredExp}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating player info: {e.Message}\n{e.StackTrace}");
        }
    }

    private IEnumerator CheckLevelUp()
    {
        Player player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("Player reference is null in UIManager");
            yield break;
        }

        int lastLevel = player.level;

        while (true)
        {
            if (player.level > lastLevel)
            {
                Debug.Log($"Level Up detected: {lastLevel} -> {player.level}");
                lastLevel = player.level;
                ShowLevelUpPanel();
                Time.timeScale = 0f;
            }

            if (levelupPanel.gameObject.activeSelf)
            {
                Time.timeScale = 0f;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShowLevelUpPanel()
    {
        if (levelupPanel != null)
        {
            levelupPanel.gameObject.SetActive(true);
            Time.timeScale = 0f;
            levelupPanel.LevelUpPanelOpen(GameManager.Instance.player.skills, OnSkillSelected);
        }
        else
        {
            Debug.LogError("LevelUpPanel is not assigned in UIManager");
        }
    }

    private void OnSkillSelected(Skill skill)
    {
        if (skill != null)
        {
            skillList.skillListUpdate();
            Time.timeScale = 1f;
            levelupPanel.gameObject.SetActive(false);
        }
    }
}
