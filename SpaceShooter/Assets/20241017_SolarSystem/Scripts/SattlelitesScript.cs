using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SattlelitesScript : MonoBehaviour
{
    private void Update()
    {
        transform.RotateAround(transform.parent.position, Vector3.up, Time.deltaTime * 25f);        
    }

}
