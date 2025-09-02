using System;

namespace Util
{
    [Serializable]
    public struct Triple<T1,T2,T3>
    {
        public T1 a;
        public T2 b;
        public T3 c;
        
        public static implicit operator T1(Triple<T1,T2,T3> t) => t.a; 
        public static implicit operator T2(Triple<T1,T2,T3> t) => t.b; 
        public static implicit operator T3(Triple<T1,T2,T3> t) => t.c; 
    }
}