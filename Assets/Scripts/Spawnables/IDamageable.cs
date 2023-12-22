using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spawnables
{
    public interface IDamageable
    {
        public enum DmgType
        {
            Physical = 0,  //Generally small kinetic projectiles, weaker to shields, and generally anti material
            Energy = 1,    //Plasma + Heat, very weak to shields, but bleed, and decent against ships, organics resist well
            Concussive = 2,//Blunt force + Select Melee Attacks, strong against sheilds. Organics weak
            Corrosive = 3, //Non-bleed Energy, but extreame anti material, + anti-organic
            Black = 4,     //Most abstract, effectively anti-everything, all is weak, but especially real matter.
        }
        public void Damage(float damage, DmgType dmgType, float reduceMod = 1f);
    }
}
