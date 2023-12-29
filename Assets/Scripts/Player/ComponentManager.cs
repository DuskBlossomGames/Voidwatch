using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentManager : MonoBehaviour
{
    public BaseWeapon weapon1;
    public BaseWeapon weapon2;
    public BaseWeapon weapon3;

    public void ElevStartCoroutine(IEnumerator enumerator)
    {
        StartCoroutine(enumerator);
    }

    private void Start()
    {
        weapon1.Start();
        weapon2.Start();
        weapon3.Start();
    }
    private void Update()
    {
        weapon1.Update();
        weapon2.Update();
        weapon3.Update();
    }
}
