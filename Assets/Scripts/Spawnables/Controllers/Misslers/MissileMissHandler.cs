using System;
using Player;
using TMPro.EditorUtilities;
using UnityEngine;

namespace Spawnables.Controllers.Misslers
{
    public class MissileMissHandler : MonoBehaviour
    {
        private Collider2D _missile;
        private void Awake()
        {
            _missile = transform.parent.GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerDamageable>(out var pd) || !pd.Missed(transform.parent.gameObject)) return;

            Physics2D.IgnoreCollision(_missile, other, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerDamageable>() == null) return;

            Physics2D.IgnoreCollision(_missile, other, false);
        }
    }
}