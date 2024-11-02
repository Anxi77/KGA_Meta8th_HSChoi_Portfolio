using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerSkillIcon : MonoBehaviour
{
    public Image skillIcon;
    public TextMeshProUGUI skillTierText;
    public void SetSkillIcon(SkillData skillData,Skill skill)
    {
        if (skillIcon != null)
        {
            skillIcon.sprite = skillData.icon?.sprite;
            skillIcon.gameObject.SetActive(skillData.icon != null);
        }        
        if(skillTierText != null) 
        {
            skillTierText.text = skill.SkillLevel.ToString(); 
        }
    }
}
