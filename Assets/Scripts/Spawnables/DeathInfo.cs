using UnityEngine;

namespace Spawnables
{
    public class DeathInfo : MonoBehaviour
    {
        public Sprite icon;
        public string title;
        public Transform additionalChildren;

        public Vector2 offsetMin;
        public Vector2 offsetMax;
    }
}