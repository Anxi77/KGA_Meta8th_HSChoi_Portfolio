using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatData", menuName = "Stats/Player Stat Data")]
public class PlayerStatData : ScriptableObject
{
    [Header("Base Combat Stats")]
    public float baseHp = 100f;
    public float baseDamage = 5f;
    public float baseDefense = 2f;
    public float baseAttackSpeed = 1f;
    public float baseAttackRange = 2f;
    public float baseAttackAngle = 120f;

    [Header("Base Movement Stats")]
    public float baseSpeed = 5f;
    public float baseExpCollectionRadius = 3f;

    [Header("Base Recovery Stats")]
    public float baseHpRegenRate = 1f;

    [Header("Level Up Increases")]
    public float hpIncreasePerLevel = 20f;
    public float damageIncreasePerLevel = 2f;
    public float speedIncreasePerLevel = 0.5f;
    public float defenseIncreasePerLevel = 0.5f;

    [Header("Base Critical Stats")]
    public float baseCriticalChance = 0f;
    public float baseCriticalDamage = 1.5f;

    [Header("Base Gain Rates")]
    public float baseExpGainRate = 1f;
    public float baseGoldGainRate = 1f;

    [Header("Base Resistances")]
    public float baseFireResistance = 0f;
    public float baseIceResistance = 0f;
    public float baseLightningResistance = 0f;
    public float basePoisonResistance = 0f;
    public float baseStunResistance = 0f;
    public float baseSlowResistance = 0f;

    [Header("Base Special Stats")]
    public float baseLuck = 0f;
    public float baseDodgeChance = 0f;
    public float baseReflectDamage = 0f;
    public float baseLifeSteal = 0f;

    [System.Serializable]
    public class PermanentStatsList
    {
        public SourceType sourceType;
        public List<StatContainer> stats = new();
    }

    [SerializeField]
    private List<PermanentStatsList> serializedPermanentStats = new();
    private Dictionary<SourceType, List<StatContainer>> permanentStats = new();

    private void OnEnable()
    {
        InitializePermanentStats();
    }

    private void InitializePermanentStats()
    {
        permanentStats.Clear();
        foreach (var statsList in serializedPermanentStats)
        {
            if (!permanentStats.ContainsKey(statsList.sourceType))
            {
                permanentStats[statsList.sourceType] = new List<StatContainer>();
            }
            permanentStats[statsList.sourceType].AddRange(statsList.stats);
        }
    }

    public void AddPermanentStat(StatContainer stat)
    {
        if (!permanentStats.ContainsKey(stat.buffType))
        {
            permanentStats[stat.buffType] = new List<StatContainer>();
            serializedPermanentStats.Add(new PermanentStatsList
            {
                sourceType = stat.buffType,
                stats = new List<StatContainer>()
            });
        }

        permanentStats[stat.buffType].Add(stat);

        // serialized 리스트도 업데이트
        var serializedList = serializedPermanentStats.Find(x => x.sourceType == stat.buffType);
        if (serializedList != null)
        {
            serializedList.stats.Add(stat);
        }
    }

    public void RemovePermanentStatsBySource(SourceType source)
    {
        if (permanentStats.ContainsKey(source))
        {
            permanentStats[source].Clear();

            // serialized 리스트도 업데이트
            var serializedList = serializedPermanentStats.Find(x => x.sourceType == source);
            if (serializedList != null)
            {
                serializedList.stats.Clear();
            }
        }
    }

    public List<StatContainer> GetPermanentStats(SourceType source)
    {
        if (permanentStats.TryGetValue(source, out var stats))
        {
            return new List<StatContainer>(stats); // 복사본 반환
        }
        return new List<StatContainer>();
    }

    public IEnumerable<StatContainer> GetAllPermanentStats()
    {
        foreach (var statsList in permanentStats.Values)
        {
            foreach (var stat in statsList)
            {
                yield return stat;
            }
        }
    }

    // 에디터에서 수정된 경우를 위한 메서드
    public void SavePermanentStats()
    {
        serializedPermanentStats.Clear();
        foreach (var kvp in permanentStats)
        {
            serializedPermanentStats.Add(new PermanentStatsList
            {
                sourceType = kvp.Key,
                stats = new List<StatContainer>(kvp.Value)
            });
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때마다 Dictionary 업데이트
        InitializePermanentStats();
    }
#endif
}