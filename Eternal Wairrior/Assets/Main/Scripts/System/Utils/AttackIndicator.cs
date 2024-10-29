using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance.enemies == null)
        {
            Vector2 mouseScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 fireDir = mouseScreenPos - (Vector2)transform.position;
            transform.up = fireDir;
        }
        //else 
        //{
        //    Vector2 Firedir = GetComponentInParent<Skill>().transform.up;
        //    transform.up = Firedir;
        //}
        #region With Angle Calc
        //float angle = Mathf.Atan2(mouseScreenPos.y, mouseScreenPos.x) * Mathf.Rad2Deg;
        //Quaternion targetRotation = Quaternion.Euler(0f, 0f, -angle);
        #endregion
    }

}
