using System;
using System.Collections.Generic;
using Player;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using UnityEditor;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserCollisionDetection : MonoBehaviour
    {
        public ChaserBehavior chaser;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerDamageable>() == null) return;
            
            var ed = transform.parent.GetComponent<EnemyDamageable>();
            var explosionObj = Instantiate(ed.explosion, null, true);
            explosionObj.SetActive(true);
            explosionObj.transform.position = transform.position;
            explosionObj.transform.localScale = transform.parent.lossyScale * 1.5f;
            explosionObj.GetComponent<ExplosionHandler>().Run(chaser.explosionDamage,
                explosionObj.transform.localScale.x, ed.gameObject);
            
            ed.explosion = null; // we're doing it ourselves
            ed.varientParent = null; // disable scrap
            ed.Damage(float.MaxValue, null);
            
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