using UnityEngine;

namespace LevelPlay
{
    public class GravityEmitter : MonoBehaviour
    {
        public static float gravitationalStrength = 1_000_000;
        public float mass;

        public Vector3 CalcGravAccel(Vector2 origin)
        {
            //A = GM/(r**2)
            Vector2 v2Pos = new Vector2(transform.position.x, transform.position.y); // Dont want to be pulling along z now do we
            Vector2 u = (v2Pos - origin).normalized; //point from origin to self
            float sqrDist = (v2Pos - origin).sqrMagnitude;
            return gravitationalStrength * mass / sqrDist * u;
        }
    }
}
