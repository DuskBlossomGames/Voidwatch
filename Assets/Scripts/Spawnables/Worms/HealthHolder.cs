using UnityEngine;

namespace Spawnables.Worms
{
    public abstract class HealthHolder : MonoBehaviour
    {
        public abstract float Health { get; set; }
        public abstract float MaxHealth { get; }
    }
}