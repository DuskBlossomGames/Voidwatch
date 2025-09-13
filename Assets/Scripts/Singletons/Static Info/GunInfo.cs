using Player.Upgrades;
using UnityEngine;
using UnityEngine.Serialization;

namespace Singletons.Static_Info
{
    public class GunInfo : MonoBehaviour
    {
        public static GunInfo GunInfoInstance => StaticInfoHolder.Instance.GetCachedComponent<GunInfo>();

        public BoostableStat<int> ammoCount;
        public float noShootRefillTime;
        public float emptyRefillTime;
        public float timeToRefillFully;
        public UpgradableStat<int> bulletsPerShot;
        public int bulletsPerShotVarience;
        public UpgradableStat<float> fireTime;
        public UpgradableStat<float> shotForce;
        public float forceVarience;
        public UpgradableStat<float> lateralSeparation;
        public float verticalSeperation;
        public float misfireChance;
        public int repeats;
        public float repeatSeperation;

        public BoostableStat<float> dmgMod;

        [Space(10)] [Header("Upgrade Values (uninitialized)")]
        public bool shieldAsAmmo;
        public UpgradableStat<int> bulletChains;
        public UpgradableStat<int> bulletPierce;
    }
}