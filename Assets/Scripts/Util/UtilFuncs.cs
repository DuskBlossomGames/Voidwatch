using UnityEngine;

public class UtilFuncs
{
    public static Vector2 LeadShotNorm(Vector2 relPos, Vector2 relVel, float bulletVel)
    {
        float a = bulletVel * bulletVel - relVel.sqrMagnitude;
        float b = 2 * Vector2.Dot(relPos, relVel);
        float c = relPos.sqrMagnitude;

        if (b * b + 4 * a * c < 0 || a == 0)
        {
            return Vector2.zero;
        }

        float colTime = (b + Mathf.Sqrt(b * b + 4 * a * c)) / (2 * a);
        Vector2 colPos = relPos + colTime * relVel;
        return colPos.normalized;
    }
    public static float LeadShot(Vector2 relPos, Vector2 relVel, float bulletVel)
    {
        float a = bulletVel * bulletVel - relVel.sqrMagnitude;
        float b = 2 * Vector2.Dot(relPos, relVel);
        float c = relPos.sqrMagnitude;

        if (b * b + 4 * a * c < 0 || a == 0)
        {
            return 0;
        }

        float colTime = (b + Mathf.Sqrt(b * b + 4 * a * c)) / (2 * a);
        Vector2 colPos = relPos + colTime * relVel;
        return Mathf.Atan2(colPos.y, colPos.x);
    }
    public static Quaternion RotFromNorm(Vector2 vec)
    {
        return Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(vec.y, vec.x));
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

    [System.Serializable]
    public class IRef<T> : ISerializationCallbackReceiver where T : class
    {
        public UnityEngine.Object target;
        public T I { get => target as T; }
        public static implicit operator bool(IRef<T> ir) => ir.target != null;
        void OnValidate()
        {
            if (!(target is T))
            {
                if (target is GameObject go)
                {
                    target = null;
                    foreach (Component c in go.GetComponents<Component>())
                    {
                        if (c is T)
                        {
                            target = c;
                            break;
                        }
                    }
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => this.OnValidate();
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
