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
    //public TextMeshProUGUI currentKillCountText;
    //public TextMeshProUGUI totalKillCountText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI hpText;
    public Image hpBarImage;
    public Image expBarImage;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        levelupPanel.gameObject.SetActive(false);
        Coroutine playerInfoUI = StartCoroutine(PlayerInfoUpdate());
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
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Player player = GameManager.Instance.player;
            //currentKillCountText.text = player.killCount.ToString();
            //totalKillCountText.text = player.totalKillCount.ToString();
            levelText.text = player.level.ToString();
            expText.text = player.exp.ToString();
            hpText.text = $"{player.hp.ToString()} / {player.maxHp.ToString()}";
            hpBarImage.fillAmount = player.hp / player.maxHp;
            if (player.level >= player._expList.Count)
            {
                expText.text = "MAX LEVEL";
                expBarImage.fillAmount = 1;
            }
            else
            {
                expText.text = $"EXP : {player.CurrentExp()}/{player.GetExpForNextLevel() - player._expList[player.level - 1]}";
                expBarImage.fillAmount = player.ExpAmount;
            }
        }
    }
}
