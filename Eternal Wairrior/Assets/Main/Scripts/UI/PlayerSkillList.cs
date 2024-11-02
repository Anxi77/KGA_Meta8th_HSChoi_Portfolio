using System.Collections;
using System.Collections.Generic;
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
        if(player != null) 
        {
            if(player.skills.Count > 0) 
            {
                foreach(Skill skill in player.skills) 
                {
                    SkillData skillData = skill.GetSkillData();
                    PlayerSkillIcon icon = Instantiate(skillIconPrefab, transform);
                    icon.SetSkillIcon(skillData,skill);
                }
            }
        }
    }
}
