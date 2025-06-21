using System;
using UnityEngine;
using static Static_Info.Statistics;

namespace Spawnables.Damage
{
    public class DeathTracker : MonoBehaviour
    {
        private void OnDestroy() { StatisticsInstance.enemiesDefeated++; }
    }
}