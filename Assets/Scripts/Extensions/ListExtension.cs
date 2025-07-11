using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtension
    {
        public static TSource Pop<TSource>(this List<TSource> source, int idx)
        {
            var ret = source[idx];
            source.RemoveAt(idx);
            return ret;
        }
    }
}