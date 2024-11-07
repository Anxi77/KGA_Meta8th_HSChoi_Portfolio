﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : SingletonManager<UIManager>
{
    [Header("UI Panels")]
    public Canvas mainCanvas;
    public GameObject pausePanel;
    public SkillLevelUpPanel levelupPanel;
    public PlayerSkillList skillList;

    [Header("Player Info Texts")]
    [SerializeField] private PlayerUIManager playerUIManager;

    [Header("UI Bars")]
    [SerializeField] private Slider hpBarImage;
    [SerializeField] private Slider expBarImage;

    private bool isPaused = false;
    private Coroutine levelCheckCoroutine;

    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                SetupMainMenuUI();
                break;
            case "GameScene":
                StartCoroutine(SetupGameSceneUI());
                break;
            case "BossStage":
                SetupBossStageUI();
                break;
            case "TestScene":
                StartCoroutine(SetupGameSceneUI());
                break;
        }
    }

    private void InitializeUI()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
    }

    private void SetupMainMenuUI()
    {
        StopAllCoroutines();
    }

    private IEnumerator SetupGameSceneUI()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }
        playerUIManager.Initialize(GameManager.Instance.player);
        StartLevelCheck();
    }

    private void SetupBossStageUI()
    {
        // 보스 스테이지 UI 설정
    }

    private void StartLevelCheck()
    {
        if (levelCheckCoroutine != null)
            StopCoroutine(levelCheckCoroutine);

        levelCheckCoroutine = StartCoroutine(CheckLevelUp());
    }

    private void Update()
    {
        CheckPause();
    }

    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel) pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
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

    // UI 요소 초기화/정리 메서드
    public void ClearUI()
    {
        StopAllCoroutines();
        if (pausePanel) pausePanel.SetActive(false);
        if (levelupPanel) levelupPanel.gameObject.SetActive(false);
        playerUIManager.Clear();
        Time.timeScale = 1f;
    }
}