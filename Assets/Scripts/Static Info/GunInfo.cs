using UnityEngine;

namespace Static_Info
{
    public class GunInfo : MonoBehaviour
    {
        public static GunInfo GunInfoInstance => StaticInfoHolder.instance.GetCachedComponent<GunInfo>(); 
        
        public int clipCount;
        public int clipCap;
        public int bulletsPerShot;
        public int bulletsPerShotVarience;
        public float fireTime;
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
}