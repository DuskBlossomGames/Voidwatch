using Spawnables;

namespace Bosses.Worm
{
    public class WormEyeDamageable : EnemyDamageable
    {
        public WormBrain brain;
        
        protected override void OnDeath()
        {
            base.OnDeath();
            brain.EyeDead();
        }
    }
}