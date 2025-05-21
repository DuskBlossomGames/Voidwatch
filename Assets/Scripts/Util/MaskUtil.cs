using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class MaskUtil
    {
        public static readonly Dictionary<int, int> COLLISION_MASKS = new(); 
        
        static MaskUtil()
        {
            for (var i = 0; i < 32; i++)
            {
                var mask = 0;
                for (var j = 0; j < 32; j++)
                {
                    if(!Physics2D.GetIgnoreLayerCollision(i, j)) mask |= 1 << j;
                }

                COLLISION_MASKS[i] = mask;
            }
        }
    }
}