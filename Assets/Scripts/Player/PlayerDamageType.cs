using System.Collections.Generic;
using Spawnables;
using UnityEditor.Timeline;

namespace Player
{
    public class PlayerDamageType
    {
        public static readonly PlayerDamageType Base = new(new Dictionary<EnemyType, float>
        {
            { EnemyType.Mechanical, 1 }, { EnemyType.Organic, 1 }
        });
        public static readonly PlayerDamageType Acidic = new(new Dictionary<EnemyType, float>
        {
            { EnemyType.Mechanical, 1 }, { EnemyType.Organic, 1.5f }
        });
        public static readonly PlayerDamageType Electric = new(new Dictionary<EnemyType, float>
        {
            { EnemyType.Mechanical, 1.25f }, { EnemyType.Organic, 1 }
        });

        public readonly Dictionary<EnemyType, float> Modifiers;

        private PlayerDamageType(Dictionary<EnemyType, float> modifiers) { Modifiers = modifiers; }
    }
}