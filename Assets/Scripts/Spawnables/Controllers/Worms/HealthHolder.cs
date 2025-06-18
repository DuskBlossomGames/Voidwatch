using UnityEngine;

namespace Spawnables.Controllers.Worms
{
    public abstract class HealthHolder : MonoBehaviour
    {
        public abstract float Health { get; set; }
        public abstract float MaxHealth { get; }
    }
}