using System;
using UnityEngine;
using Util.UI;
using static Singletons.Static_Info.PlayerData;

namespace Player
{
    public class ShieldBarGradientController : MonoBehaviour
    {
        private void Awake()
        {
            var gradient = GetComponent<HorizontalGradientUIMesh>();
            
            var orig = PlayerDataInstance.maxShield.baseValue;
            float cur = PlayerDataInstance.maxShield;

            for (var i = 0; i < gradient.gradientPoints.Length - 1; i++)
            {
                var point = gradient.gradientPoints[i];
                point.normalizedPosition *= orig / cur;
                gradient.gradientPoints[i] = point;

                if (i == gradient.gradientPoints.Length - 2 && point.normalizedPosition < 1)
                {
                    gradient.gradientPoints[i + 1].normalizedPosition = 1;
                }
            }
        }
    }
}