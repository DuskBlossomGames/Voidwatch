using Spawnables.Damage;
using UnityEngine;
using static Singletons.Static_Info.Statistics;

namespace Spawnables.Controllers.Worms
{
    public class WormDamageable : EnemyDamageable
    {
        public override float MaxHealth { get => _rootDamageable.MaxHealth; }
        public override float Health { get => _rootDamageable.Health; set => _rootDamageable.Health = value; }
    
        public GameObject root;
        public float dmgMod = 1;
    
        private HealthHolder _rootDamageable;
    
        private new void Start()
        {
            _rootDamageable = root.GetComponent<HealthHolder>();
            base.Start();
        }

        public override bool Damage(float damage, GameObject source)
        {
            return base.Damage(dmgMod * damage, source);
        }

        private void SpawnBits(GameObject source)
        {
            base.OnDeath(source);
        }

        protected override void OnDeath(GameObject source)
        {
            foreach (var dmg in root.GetComponentsInChildren<WormDamageable>()) dmg.SpawnBits(source);
            Destroy(root);
        }
    }
}