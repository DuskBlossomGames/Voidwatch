namespace Spawnables
{
    public class EnemyDamageable : Damageable
    {
        public int maxHealth;
        
        // have to do this b/c unity doesn't like duplicated properties -_- 
        private float _health;
        protected override float Health { get => _health; set => _health = value; }
        protected override float MaxHealth => maxHealth;

        private new void Start()
        {
            base.Start();
            Health = maxHealth;
        }
    }
}