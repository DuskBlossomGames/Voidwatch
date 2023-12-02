using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDamageable : MonoBehaviour
{
    public float health;

    public void Kill()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.GetComponent<WormDamageable>().Kill();
        }
    }
}
