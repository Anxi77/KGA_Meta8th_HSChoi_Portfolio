using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : SingletonManager<SkillManager>
{
    private List<SkillData> availableSkills = new List<SkillData>();
    private List<Skill> activeSkills = new List<Skill>();

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(InitializeAfterSkillDataManager());
    }

    private IEnumerator InitializeAfterSkillDataManager()
    {
        Debug.Log("Waiting for SkillDataManager initialization...");

        float timeoutDuration = 5f; // 5초 타임아웃
        float elapsedTime = 0f;

        while (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > timeoutDuration)
            {
                Debug.LogError("SkillDataManager initialization timeout!");
                yield break;
            }
            Debug.Log($"Waiting... Time elapsed: {elapsedTime:F1}s");
            yield return null;
        }

        Debug.Log("SkillDataManager initialized, proceeding with initialization...");
        yield return new WaitForSeconds(0.1f);

        Initialize();
    }

    private void Initialize()
    {
        try
        {
            Debug.Log("Initializing SkillManager...");

            availableSkills = SkillDataManager.Instance.GetAllSkillData();

            if (availableSkills == null || availableSkills.Count == 0)
            {
                Debug.LogWarning("No skills loaded from SkillDataManager");
                availableSkills = new List<SkillData>();
            }
            else
            {
                Debug.Log($"Loaded {availableSkills.Count} skills from SkillDataManager");
                foreach (var skill in availableSkills)
                {
                    Debug.Log($"Loaded skill: {skill.metadata.Name} (ID: {skill.metadata.ID})");
                }
            }

            activeSkills = new List<Skill>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillManager: {e.Message}\n{e.StackTrace}");
        }
    }

    public void AddOrUpgradeSkill(SkillData skillData)
    {
        if (GameManager.Instance.player == null || skillData == null) return;

        Player player = GameManager.Instance.player;
        Skill existingSkill = player.skills.Find(x => x.SkillID == skillData.metadata.ID);

        try
        {
            if (existingSkill != null)
            {
                int nextLevel = existingSkill.SkillLevel + 1;
                if (nextLevel > skillData.GetMaxLevel())
                {
                    Debug.LogWarning($"Skill {skillData.metadata.Name} is already at max level");
                    return;
                }

                // 레벨별 프리팹 확인
                GameObject newPrefab = null;
                if (skillData.prefabsByLevel != null && nextLevel <= skillData.prefabsByLevel.Length)
                {
                    newPrefab = skillData.prefabsByLevel[nextLevel - 1];
                    Debug.Log($"Found level {nextLevel} prefab for {skillData.metadata.Name}");
                }

                if (newPrefab != null)
                {
                    // 기존 스킬의 Transform 정보 저장
                    Vector3 position = existingSkill.transform.position;
                    Quaternion rotation = existingSkill.transform.rotation;
                    Transform parent = existingSkill.transform.parent;

                    // 기존 스킬 제거
                    player.skills.Remove(existingSkill);
                    activeSkills.Remove(existingSkill);
                    Destroy(existingSkill.gameObject);

                    // 새 레벨의 프리팹으로 스킬 생성
                    GameObject newSkillObj = Instantiate(newPrefab, position, rotation, parent);
                    if (newSkillObj.TryGetComponent<Skill>(out Skill newSkill))
                    {
                        newSkill.SetSkillData(skillData);
                        player.skills.Add(newSkill);
                        activeSkills.Add(newSkill);
                        bool success = newSkill.SkillLevelUpdate(nextLevel);
                        Debug.Log($"Upgraded {skillData.metadata.Name} to level {nextLevel}, success: {success}");
                    }
                }
                else
                {
                    // 프리팹이 없으면 기존 스킬의 레벨만 업데이트
                    bool success = existingSkill.SkillLevelUpdate(nextLevel);
                    Debug.Log($"Updated {skillData.metadata.Name} to level {nextLevel} without prefab change, success: {success}");
                }
            }
            else
            {
                // 새 스킬 추가
                GameObject prefabToUse = (skillData.prefabsByLevel != null && skillData.prefabsByLevel.Length > 0)
                    ? skillData.prefabsByLevel[0]
                    : skillData.metadata.Prefab;

                if (prefabToUse != null)
                {
                    GameObject skillObj = Instantiate(prefabToUse, player.transform);
                    if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
                    {
                        newSkill.SetSkillData(skillData);
                        player.skills.Add(newSkill);
                        activeSkills.Add(newSkill);
                        Debug.Log($"Added new skill: {skillData.metadata.Name}");
                    }
                }
                else
                {
                    Debug.LogError($"No prefab found for skill: {skillData.metadata.Name}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in AddOrUpgradeSkill: {e.Message}\n{e.StackTrace}");
        }
    }

    public void RemoveSkill(SkillID skillID)
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;
        Skill skillToRemove = player.skills.Find(x => x.SkillID == skillID);

        if (skillToRemove != null)
        {
            player.skills.Remove(skillToRemove);
            activeSkills.Remove(skillToRemove);
            Destroy(skillToRemove.gameObject);
        }
    }

    public List<Skill> GetActiveSkills()
    {
        return activeSkills;
    }

    public Skill GetSkillByID(SkillID skillID)
    {
        return activeSkills.Find(x => x.SkillID == skillID);
    }

    public List<SkillData> GetRandomSkills(int count = 3, ElementType? elementType = null)
    {
        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogError($"No skills available in SkillManager. Available skills count: {availableSkills?.Count ?? 0}");
            return new List<SkillData>();
        }

        Debug.Log($"Total available skills before filtering: {availableSkills.Count}");
        foreach (var skill in availableSkills)
        {
            Debug.Log($"Available skill: {skill.metadata.Name}, ID: {skill.metadata.ID}, Element: {skill.metadata.Element}");
        }

        var selectedSkills = new List<SkillData>();
        var filteredSkills = availableSkills.Where(skill =>
        {
            if (skill == null || skill.metadata == null)
            {
                Debug.LogError("Found null skill or metadata");
                return false;
            }

            var stats = SkillDataManager.Instance.GetSkillStats(skill.metadata.ID, 1);
            bool hasStats = stats != null;
            bool matchesElement = elementType == null || skill.metadata.Element == elementType;

            Debug.Log($"Checking skill {skill.metadata.Name}:");
            Debug.Log($"  - ID: {skill.metadata.ID}");
            Debug.Log($"  - Element: {skill.metadata.Element}");
            Debug.Log($"  - HasStats: {hasStats}");
            Debug.Log($"  - MatchesElement: {matchesElement}");
            if (!hasStats)
            {
                Debug.LogWarning($"  - No stats found for level 1");
            }

            return hasStats && matchesElement;
        }).ToList();

        if (!filteredSkills.Any())
        {
            Debug.LogWarning("No skills match the criteria");
            return selectedSkills;
        }

        Debug.Log($"Found {filteredSkills.Count} skills matching criteria");

        if (elementType == null)
        {
            var availableElements = filteredSkills
                .Select(s => s.metadata.Element)
                .Distinct()
                .ToList();

            elementType = availableElements[Random.Range(0, availableElements.Count)];
            filteredSkills = filteredSkills.Where(s => s.metadata.Element == elementType).ToList();
            Debug.Log($"Selected element type: {elementType}, remaining skills: {filteredSkills.Count}");
        }

        int possibleCount = Mathf.Min(count, filteredSkills.Count);
        Debug.Log($"Requested {count} skills, possible to select {possibleCount} skills");

        while (selectedSkills.Count < possibleCount && filteredSkills.Any())
        {
            int index = Random.Range(0, filteredSkills.Count);
            selectedSkills.Add(filteredSkills[index]);
            Debug.Log($"Selected skill: {filteredSkills[index].metadata.Name}");
            filteredSkills.RemoveAt(index);
        }

        if (selectedSkills.Count < count)
        {
            Debug.Log($"Returning {selectedSkills.Count} skills instead of requested {count} due to availability");
        }

        return selectedSkills;
    }
}