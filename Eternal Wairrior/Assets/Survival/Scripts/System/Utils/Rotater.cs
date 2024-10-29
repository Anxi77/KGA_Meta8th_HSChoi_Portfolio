using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        if (horizontal != 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = horizontal > 0 ? -1 : 1;
            transform.localScale = newScale;
        }
    }
}
