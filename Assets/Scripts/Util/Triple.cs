using System;
using System.Collections.Generic;

namespace Util
{
    [Serializable]
    public struct Triple<T1,T2,T3> : IEquatable<Triple<T1, T2, T3>>
    {
        public T1 a;
        public T2 b;
        public T3 c;

        public Triple(T1 a, T2 b, T3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        
        public static implicit operator T1(Triple<T1,T2,T3> t) => t.a; 
        public static implicit operator T2(Triple<T1,T2,T3> t) => t.b; 
        public static implicit operator T3(Triple<T1,T2,T3> t) => t.c;

        public bool Equals(Triple<T1, T2, T3> other)
        {
            return EqualityComparer<T1>.Default.Equals(a, other.a) && EqualityComparer<T2>.Default.Equals(b, other.b) && EqualityComparer<T3>.Default.Equals(c, other.c);
        }

        public override bool Equals(object obj)
        {
            return obj is Triple<T1, T2, T3> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a, b, c);
        }
    }
}