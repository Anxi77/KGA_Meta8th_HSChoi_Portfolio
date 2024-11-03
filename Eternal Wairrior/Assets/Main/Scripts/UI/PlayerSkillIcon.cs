using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerSkillIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private Skill skill;

    public void SetSkillIcon(Sprite iconSprite, Skill skill)
    {
        if (iconImage != null)
        {
            if (iconSprite == null)
            {
                iconSprite = Resources.Load<Sprite>("DefaultSkillIcon");
                Debug.LogWarning($"Using default icon for skill: {skill?.GetType().Name}");
            }
            iconImage.sprite = iconSprite;
            iconImage.gameObject.SetActive(iconSprite != null);
        }
        else
        {
            Debug.LogError("Icon Image component is missing!");
        }
        this.skill = skill;
    }

    private void OnValidate()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null)
            {
                Debug.LogError("PlayerSkillIcon requires an Image component!");
            }
        }
    }
}
