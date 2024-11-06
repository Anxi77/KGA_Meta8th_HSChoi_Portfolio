using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRotate : MonoBehaviour
{
    private Vector2 fireDir;

    protected virtual void CalcDirection()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        fireDir = (mouseWorldPos - (Vector2)transform.position).normalized;
        transform.up = fireDir;

        //±¤¼±¹ß½Î
        //Debug.DrawRay(transform.position, fireDir * 5f, Color.red, 0.1f);
    }

    private void Update()
    {
        CalcDirection();
    }
}
