using Player;
using UnityEngine;

namespace Util
{
    public static class UtilFuncs
    {
        public static float Mod(float a, float b)
        {
            return (a%b + b)%b;
        }
        
        public static float Angle(Vector2 v)
        {
            return Mathf.Atan2(v.y, v.x);
        }

        public static Vector2 GetTargetVel(GameObject target)
        {
            if (target.TryGetComponent<Movement>(out var playerMovement) && playerMovement.Dodging)
            {
                return playerMovement.PreDodgeVel;
            }
            
            return target.GetComponent<CustomRigidbody2D>().linearVelocity;
        }

        public static Vector2 PredictTargetPos(Vector2 pos, Vector2 vel, float time)
        {
            return pos + vel * time;
        }
        
        public static Vector2 LeadShotNorm(Vector2 relPos, Vector2 relVel, float bulletVel)
        {
            var a = relVel.sqrMagnitude - bulletVel * bulletVel;
            var b = 2 * Vector2.Dot(relPos, relVel);
            var c = relPos.sqrMagnitude;

            var disc = b * b - 4 * a * c;
            var colTime = disc < 0 ? 0
                            : a != 0 ? (-b - Mathf.Sqrt(disc)) / (2 * a)
                            : b != 0 ? -c / b
                            : 0;
            colTime = colTime < 0 ? 0 : colTime;
            return PredictTargetPos(relPos, relVel, colTime).normalized;
        }
        public static float LeadShot(Vector2 relPos, Vector2 relVel, float bulletVel)
        {
            return Angle(LeadShotNorm(relPos, relVel, bulletVel));
        }
        public static Quaternion RotFromNorm(Vector2 vec)
        {
            return Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(vec.normalized.y, vec.normalized.x));
        }

        public static float LerpSafe(float a, float b, float t)
        {
            if (t < 0) return a;
            if (t > 1) return b;
            return Lerp(a, b, t);
        }
        public static Vector2 LerpSafe(Vector2 a, Vector2 b, float t)
        {
            if (t < 0) return a;
            if (t > 1) return b;
            return Lerp(a, b, t);
        }
        public static Vector3 LerpSafe(Vector3 a, Vector3 b, float t)
        {
            if (t < 0) return a;
            if (t > 1) return b;
            return Lerp(a, b, t);
        }
        public static float Lerp(float a, float b, float t)
        {
            return ((1 - t) * a + t * b);
        }
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return ((1 - t) * a + t * b);
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return ((1 - t) * a + t * b);
        }

        public static float MinUnsigned(float x, float y)
        {
            return Mathf.Abs(x) < Mathf.Abs(y) ? x : y;
        }
        
        public static Vector2 Rot(Vector2 vec, float angle)
        {
            return new Vector2(vec.x * Mathf.Cos(angle) - vec.y * Mathf.Sin(angle), vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle));
        }
        
        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        

        public class Anim
        {
            public Sprite[] Sprites;
            public int NumFrames;
        }
      
        public static void SetupTexture(Texture2D texture, Anim anim, float widthOverHeight=1)
        {
            var sliceWidth = widthOverHeight * texture.height;

            anim.NumFrames = (int) (texture.width / sliceWidth);
            anim.Sprites = new Sprite[anim.NumFrames];
            for (var i = 0; i < anim.NumFrames; i++) anim.Sprites[i] = Sprite.Create(texture, new Rect(i*sliceWidth, 0, sliceWidth, texture.height), new Vector2(0.5f, 0.5f), texture.height);
        }
    }
}
