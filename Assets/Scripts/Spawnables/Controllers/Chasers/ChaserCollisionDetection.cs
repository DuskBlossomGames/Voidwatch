using System;
using Player;
using Spawnables.Damage;
using UnityEditor;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserCollisionDetection : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerDamageable>() == null) return;

            transform.parent.GetComponent<EnemyDamageable>().varientParent = null; // disable scrap
            transform.parent.GetComponent<EnemyDamageable>().Damage(float.MaxValue, null);
            
            var ea = transform.parent.GetChild(1);
            ea.SetParent(null);
            ea.gameObject.SetActive(true);
            // idk why all of these start disabled :|
            ea.GetComponent<ElectricArea>().enabled = true;
            ea.GetComponent<DestroyAfterX>().enabled = true;
            ea.GetComponent<Collider2D>().enabled = true;
            ea.GetComponent<MiniMapIcon>().enabled = true;
        }
    }
}