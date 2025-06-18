using Spawnables;
using Spawnables.Damage;
using UnityEngine;

namespace Bosses.Worm
{
    public class WormEyeDamageable : EnemyDamageable
    {
        public WormBrain brain;
        
        protected override void OnDeath(GameObject source)
        {
            base.OnDeath(source);
            brain.EyeDead();
        }
    }
}