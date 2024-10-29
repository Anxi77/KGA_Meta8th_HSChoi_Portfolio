using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour, IContactable
{
    public virtual void Contact() 
    {
        Destroy(gameObject);
    }

}
