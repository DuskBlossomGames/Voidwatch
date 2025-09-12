using System;
using System.Collections.Generic;

namespace Util
{
    [Serializable]
    public struct Pair<T1,T2>
    {
        public T1 a;
        public T2 b;

        public Pair(T1 a, T2 b)
        {
            this.a = a;
            this.b = b;
        }
        
        public static implicit operator T1(Pair<T1,T2> p) => p.a; 
        public static implicit operator T2(Pair<T1,T2> p) => p.b;
        
        public bool Equals(Pair<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(a, other.a) && EqualityComparer<T2>.Default.Equals(b, other.b);
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(a, b);
        }

    }
}