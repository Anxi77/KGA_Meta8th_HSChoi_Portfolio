using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillTester : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown skillDropdown;
    [SerializeField] private Button addSkillButton;

    private bool isInitialized = false;

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // 필요한 매니저들이 초기화될 때까지 대기
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            SkillDataManager.Instance != null &&
            SkillDataManager.Instance.IsInitialized);

        // UI 요소들이 할당되었는지 확인
        if (!ValidateComponents())
        {
            Debug.LogError("SkillTester: Required UI components are missing!");
            yield break;
        }

        InitializeDropdown();
        SetupButton();
        isInitialized = true;
        Debug.Log("SkillTester initialized successfully");
    }

    private bool ValidateComponents()
    {
        if (skillDropdown == null)
        {
            Debug.LogError("SkillTester: Skill Dropdown is not assigned!");
            return false;
        }

        if (addSkillButton == null)
        {
            Debug.LogError("SkillTester: Add Skill Button is not assigned!");
            return false;
        }

        return true;
    }

    private void InitializeDropdown()
    {
        skillDropdown.ClearOptions();
        var skillDatas = SkillDataManager.Instance.GetAllSkillData();

        foreach (var skillData in skillDatas)
        {
            skillDropdown.options.Add(new TMP_Dropdown.OptionData(
                $"{skillData.metadata.Name} ({skillData.metadata.Type})"
            ));
        }

        skillDropdown.RefreshShownValue();
        Debug.Log($"SkillTester: Initialized dropdown with {skillDatas.Count} skills");
    }

    private void SetupButton()
    {
        addSkillButton.onClick.RemoveAllListeners();
        addSkillButton.onClick.AddListener(AddSelectedSkill);
        Debug.Log("SkillTester: Button setup completed");
    }

    private void AddSelectedSkill()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SkillTester: Not yet initialized!");
            return;
        }

        if (GameManager.Instance?.player == null)
        {
            Debug.LogWarning("SkillTester: Player not found!");
            return;
        }

        var skillDatas = SkillDataManager.Instance.GetAllSkillData();
        if (skillDropdown.value < skillDatas.Count)
        {
            var selectedSkill = skillDatas[skillDropdown.value];
            GameManager.Instance.player.AddOrUpgradeSkill(selectedSkill);
            Debug.Log($"SkillTester: Added/Upgraded skill: {selectedSkill.metadata.Name}");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        // T키로 현재 선택된 스킬 추가
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddSelectedSkill();
        }
    }

    private void OnDisable()
    {
        isInitialized = false;
        if (addSkillButton != null)
        {
            addSkillButton.onClick.RemoveAllListeners();
        }
    }
}
