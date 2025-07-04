using UnityEngine;

namespace Static_Info
{
    public class GunInfo : MonoBehaviour
    {
        public static GunInfo GunInfoInstance => StaticInfoHolder.Instance.GetCachedComponent<GunInfo>();

        public int ammoCount;
        public float noShootRefillTime;
        public float emptyRefillTime;
        public float timeToRefillFully;
        public int bulletsPerShot;
        public int bulletsPerShotVarience;
        public float fireTime;
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