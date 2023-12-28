using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KineticSettings", menuName = "Components/Weapons/KineticSettings")]
public class KineticSettings : ScriptableObject
{
    public GameObject bulletPrefab;
    public float playRadius;

    public int clipCount;
    public int clipCap;
    public int bulletsPerShot;
    public int bulletsPerShotVarience;
    public float reloadTime;
    public float refillTime;
    public float shotForce;
    public float forceVarience;
    public float lateralSeperation;
    public float verticalSeperation;
    public float misfireChance;
    public int repeats;
    public float repeatSeperation;

    public float dmgMod;
}
