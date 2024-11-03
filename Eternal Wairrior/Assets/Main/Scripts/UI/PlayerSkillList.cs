using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSkillList : MonoBehaviour
{
    public PlayerSkillIcon skillIconPrefab;
    private Player player;

    private void Awake()
    {
        player = GameManager.Instance?.player;
    }

    public void skillListUpdate()
    {
        if (!gameObject.activeInHierarchy) return;

        try
        {
            // 기존 아이콘 제거
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            if (player == null)
            {
                player = GameManager.Instance?.player;
                if (player == null)
                {
                    Debug.LogError("Player reference is null in PlayerSkillList");
                    return;
                }
            }

            if (player.skills != null)
            {
                foreach (Skill skill in player.skills.ToList())
                {
                    if (skill != null)
                    {
                        SkillData skillData = skill.GetSkillData();
                        if (skillData != null)
                        {
                            PlayerSkillIcon icon = Instantiate(skillIconPrefab, transform);
                            icon.SetSkillIcon(skillData.icon, skill);
                        }
                        else
                        {
                            Debug.LogWarning($"Skill data is null for skill: {skill.name}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Player skills list is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating skill list: {e.Message}");
        }
    }
}
