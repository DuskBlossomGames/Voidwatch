using System;

namespace Util
{
    [Serializable]
    public struct Pair<T1,T2>
    {
        public T1 a;
        public T2 b;
        
        public static implicit operator T1(Pair<T1,T2> p) => p.a; 
        public static implicit operator T2(Pair<T1,T2> p) => p.b; 
    }
}