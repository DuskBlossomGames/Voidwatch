using System.Collections.Generic;
using Spawnables;
using UnityEditor.Timeline;

namespace Player
{
    public class PlayerDamageType
    {
        public static readonly PlayerDamageType Acidic = new(new Dictionary<EnemyType, float>
        {
            { EnemyType.None, 1 }, { EnemyType.Mechanical, 1.1f }, { EnemyType.Organic, 3 },
            { EnemyType.Worm, 1.2f }, { EnemyType.Carcadon, 1.5f }, { EnemyType.WormBoss, 1.1f }
        });
        public static readonly PlayerDamageType Electric = new(new Dictionary<EnemyType, float>
        {
            { EnemyType.None, 1 }, { EnemyType.Mechanical, 2f }, { EnemyType.Organic, 1.2f },
            { EnemyType.Worm, 1.1f }, { EnemyType.Carcadon, 1.3f }, { EnemyType.WormBoss, 1.3f }
        });

        public readonly Dictionary<EnemyType, float> Modifiers;

        private PlayerDamageType(Dictionary<EnemyType, float> modifiers) { Modifiers = modifiers; }
    }
}