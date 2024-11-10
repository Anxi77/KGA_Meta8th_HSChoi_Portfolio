using UnityEngine;
using System.Collections;

public abstract class PermanentPassiveSkill : PassiveSkills
{
    private bool effectApplied = false;
    private Coroutine initializeCoroutine;

    protected override void Awake()
    {
        base.Awake();
        StartInitialization();
    }

    protected override void Start()
    {
        // PassiveSkills의 Start에서 시작하는 PassiveEffectCoroutine을 실행하지 않음
    }

    private void StartInitialization()
    {
        if (initializeCoroutine != null)
        {
            StopCoroutine(initializeCoroutine);
        }
        initializeCoroutine = StartCoroutine(WaitForPlayerAndInitialize());
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        while (!GameManager.Instance.player.IsInitialized)
        {
            yield return null;
        }

        if (!effectApplied)
        {
            var playerStat = GameManager.Instance.player.GetComponent<PlayerStat>();
            if (playerStat != null)
            {
                float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);

                ApplyEffectToPlayer(GameManager.Instance.player);
                effectApplied = true;

                float newMaxHp = playerStat.GetStat(StatType.MaxHp);
                float newCurrentHp = Mathf.Max(1f, newMaxHp * currentHpRatio);
                playerStat.SetCurrentHp(newCurrentHp);

                Debug.Log($"Applied permanent effect for {skillData?.metadata?.Name ?? "Unknown Skill"} - HP: {newCurrentHp}/{newMaxHp}");
            }
        }
        initializeCoroutine = null;
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        var playerStat = GameManager.Instance?.player?.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);

            if (effectApplied)
            {
                RemoveEffectFromPlayer(GameManager.Instance.player);
                effectApplied = false;
            }

            base.UpdateInspectorValues(stats);

            ApplyEffectToPlayer(GameManager.Instance.player);
            effectApplied = true;

            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = Mathf.Max(1f, newMaxHp * currentHpRatio);
            playerStat.SetCurrentHp(newCurrentHp);
        }
    }

    protected override void OnDestroy()
    {
        if (effectApplied && GameManager.Instance?.player != null)
        {
            var playerStat = GameManager.Instance.player.GetComponent<PlayerStat>();
            if (playerStat != null)
            {
                float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);
                float currentHp = playerStat.GetStat(StatType.CurrentHp);
                float maxHp = playerStat.GetStat(StatType.MaxHp);

                Debug.Log($"Before destroy - HP: {currentHp}/{maxHp} ({currentHpRatio:F2})");

                // 효과 제거 전에 현재 HP 저장
                RemoveEffectFromPlayer(GameManager.Instance.player);
                effectApplied = false;

                // 새로운 MaxHP에 맞춰 HP 조정
                float newMaxHp = playerStat.GetStat(StatType.MaxHp);
                // 현재 HP와 비율로 계산된 HP 중 더 큰 값 사용
                float newCurrentHp = Mathf.Max(currentHp, newMaxHp * currentHpRatio);
                playerStat.SetCurrentHp(newCurrentHp);

                Debug.Log($"After destroy - HP: {newCurrentHp}/{newMaxHp} ({currentHpRatio:F2})");
            }
        }

        // base.OnDestroy는 호출하지 않음 - PassiveSkills의 OnDestroy에서 추가 HP 조정이 일어나는 것을 방지
        // base.OnDestroy();
    }

    public abstract void ApplyEffectToPlayer(Player player);
    public abstract void RemoveEffectFromPlayer(Player player);
}