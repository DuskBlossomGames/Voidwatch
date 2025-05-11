using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DMG", menuName = "ScriptableObjects/DamageResistances", order = 1)]
public class DamageResistances : ScriptableObject
{
    public float physDmgMod = 1;
    public float physDmgReduce = 0;
    public float physDmgBleed = 0;
    public bool _123456789;
    public float engDmgMod = 1;
    public float engDmgReduce = 0;
    public float engDmgBleed = 0;
    public bool _123456788;
    public float conDmgMod = 1;
    public float conDmgReduce = 0;
    public float conDmgBleed = 0;
    public bool _123456787;
    public float corrDmgMod = 1;
    public float corrDmgReduce = 0;
    public float corrDmgBleed = 0;
    public bool _123456786;
    public float blackDmgMod = 1;
    public float blackDmgReduce = 0;
    public float blackDmgBleed = 0;
    public bool _123456785;

    public List<float> dmgMod;
    public List<float> dmgReduce;
    public List<float> dmgBleed;

    public void Ready()
    {
        dmgMod = new List<float>();
        dmgMod.Add(physDmgMod); dmgMod.Add(engDmgMod); dmgMod.Add(conDmgMod); dmgMod.Add(corrDmgMod); dmgMod.Add(blackDmgMod);
        dmgReduce = new List<float>();
        dmgReduce.Add(physDmgReduce); dmgReduce.Add(engDmgReduce); dmgReduce.Add(conDmgReduce); dmgReduce.Add(corrDmgReduce); dmgReduce.Add(blackDmgReduce);
        dmgBleed = new List<float>();
        dmgBleed.Add(physDmgBleed); dmgBleed.Add(engDmgBleed); dmgBleed.Add(conDmgBleed); dmgBleed.Add(corrDmgBleed); dmgBleed.Add(blackDmgBleed);
    }
}
