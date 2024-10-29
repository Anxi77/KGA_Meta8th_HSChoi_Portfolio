using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SkillLevelUpButton : MonoBehaviour
{
    public TextMeshProUGUI skillNameText;
    public Button button;
    
    public void SetSkillSelectButton(string skillName, UnityAction onClick) 
    {
        skillNameText.text = skillName;
        button.onClick.AddListener(onClick);
    }
}
