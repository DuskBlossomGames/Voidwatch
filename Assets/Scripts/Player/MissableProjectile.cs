using System;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;
using static Singletons.Static_Info.PlayerData;

namespace Player
{
    [RequireComponent(typeof(Collider2D))]
    public class MissableProjectile : MonoBehaviour
    {
        private class TriggerHandler : MonoBehaviour
        {
            private const float IMMUNE_TIME = 1f;
            
            private Movement _movement;
            private bool _willMiss;
            private float _timeEntered;

            private Collider2D _myCollider, _playerCollider;

            private void Awake()
            {
                _movement = FindAnyObjectByType<Movement>();
                _playerCollider = _movement.GetComponent<Collider2D>();
                _myCollider = transform.parent.GetComponent<Collider2D>();

                CheckMiss();
            }

            // disabling loses all `IgnoreCollision`s
            private void OnEnable()
            {
                Physics2D.IgnoreCollision(_myCollider, _playerCollider, _willMiss);
            }

            private void CheckMiss()
            {
                _willMiss = Random.value < PlayerDataInstance.missChance;

                Physics2D.IgnoreCollision(_myCollider, _playerCollider, _willMiss);
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                if (!_willMiss || other != _playerCollider) return;
                
                _movement.ShowBillboard(BillboardMessage.Missed, transform.position);
                _timeEntered = Time.time;
            }

            private void OnTriggerStay2D(Collider2D other)
            {
                if (!_willMiss || other != _playerCollider || Time.time - _timeEntered <= IMMUNE_TIME) return;

                _willMiss = false;
                Physics2D.IgnoreCollision(_myCollider, _playerCollider, false);
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                if (!_willMiss || other != _playerCollider) return;

                CheckMiss(); // recalculate
            }
        }
        
        private void Awake()
        {
            var myCollider = GetComponent<Collider2D>();

            var trigger = new GameObject("MissTrigger")
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                },
                layer = gameObject.layer
            };
            trigger.AddComponent<TriggerHandler>();
            trigger.AddComponent<Rigidbody2D>();
            var triggerColl = (Collider2D) trigger.AddComponent(myCollider.GetType());
            triggerColl.isTrigger = true;
            switch (myCollider)
            {
                case BoxCollider2D bc:
                    ((BoxCollider2D)triggerColl).size = bc.size;
                    ((BoxCollider2D)triggerColl).offset = bc.offset;
                    break;
                case CircleCollider2D cc:
                    ((CircleCollider2D)triggerColl).radius = cc.radius;
                    ((CircleCollider2D)triggerColl).offset = cc.offset;
                    break;
                default:
                    throw new Exception("Unsupported collider type '"+myCollider.GetType()+"' for missable projectile");
            }
        }
    }
}