using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManager : SingletonManager<SkillManager>
{
    public List<Skill> skillsPrefab;

    public List<Skill> PerkSelect() 
    {
        List<Skill> selectedSkills = new List<Skill>();

        for (int i = 0; i < 3; ++i) 
        {
            int randomSkillNum = Random.Range(0, skillsPrefab.Count - 1);
            foreach (Skill playerSkill in GameManager.Instance.player.skills) 
            {
                if(playerSkill.name == skillsPrefab[randomSkillNum].name) 
                {
                    Skill skill = new Skill();
                    
                    int nextSkillLevel = (playerSkill.skillLevel + 1);

                }

            }
        }

        return selectedSkills;
    }
}
