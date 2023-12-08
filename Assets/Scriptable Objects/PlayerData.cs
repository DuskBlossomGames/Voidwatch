using System;
using UnityEngine;

namespace Scriptable_Objects
{
    public class PlayerData : ScriptableObject
    {
        public int playerMaxHealth;
        
        [NonSerialized] public float? Health = null;
        [NonSerialized] public float Scrap;
    }
}