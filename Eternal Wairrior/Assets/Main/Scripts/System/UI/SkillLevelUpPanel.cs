using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkillLevelUpPanel : MonoBehaviour
{
    public RectTransform list;
    public SkillLevelUpButton buttonPrefab;

    //플레이어가 레벨업을 하면 패널 활성화 요청
    public void LevelUpPanelOpen(List<Skill> skillList, Action<Skill> callback)
    {
        gameObject.SetActive(true);

        Time.timeScale = 0f;
        //스킬 2개 UI에 표시할 예정
        if (GameManager.Instance.player.skills.Count > 2)
        {
            List<Skill> selectedSkillList = new();
            while (selectedSkillList.Count < 2) //2개의 스킬이 선택될때까지 반복
            {
                int ranNum = Random.Range(0, skillList.Count); //랜덤한 숫자 하나 뽑기

                Skill selectedSkill = skillList[ranNum]; //랜덤하게 선택된 스킬 하나 가져오기.

                if (selectedSkillList.Contains(selectedSkill)) //이미 뽑힌 스킬이 또 뽑혔으면
                {
                    continue; // 밑 라인 을 무시하고 다시 반복문을 돈다.
                }

                selectedSkillList.Add(selectedSkill); //선택한 스킬을 넣어주고

                SkillLevelUpButton skillbutton = Instantiate(buttonPrefab, list); //버티컬 레이아웃 그룹을 가지고 있는 리스트의 자식으로 버튼 생성

                skillbutton.SetSkillSelectButton(selectedSkill.skillName,
                    () =>
                    {
                        callback(selectedSkill);
                        LevelUpPanelClose();
                    });

            }
        }
        else 
        {
           SkillLevelUpButton skillbutton = Instantiate (buttonPrefab, list);
           skillbutton.SetSkillSelectButton("No SKills Left",() => LevelUpPanelClose());
        }
    }

    //레벨업 패널을 닫을 시 플레이어의 LevelUpPanelOpen의 callback을 호출.
    public void LevelUpPanelClose()
    {
        foreach(Transform buttons in list)
        {
            Destroy(buttons.gameObject);
        }
        Time.timeScale = 1f;
        gameObject.SetActive(false);

    }
}
