using UnityEngine;

namespace Spawnables
{
    public abstract class Stunnable : MonoBehaviour
    {
        public abstract void Stun();
        public abstract void UpdateStun();
        public abstract void UnStun();
    }
}