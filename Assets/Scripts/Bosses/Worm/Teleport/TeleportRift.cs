using System;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Util;

namespace Bosses.Worm.Teleport
{
    public class TeleportRift : MonoBehaviour
    {
        public float timeToLive;
        public bool isEntrance;
        [NonSerialized] public GameObject pair;
        
        private Vector3 _posDiff;
        private Vector3 _rotDiff;
        private readonly Timer _ttlTimer = new();

        private void Start()
        {
            _posDiff = pair.transform.position - transform.position;
            _rotDiff = pair.transform.rotation.eulerAngles - transform.rotation.eulerAngles;
            _ttlTimer.Value = timeToLive;
        }

        private void Update()
        {
            _ttlTimer.Update();

            if (_ttlTimer.IsFinished)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D coll)
        {
            if (!isEntrance || coll.GetComponent<Teleportable>() == null) return;

            var other = coll.transform;
            var newObj = Instantiate(other.gameObject, other.position + _posDiff, Quaternion.Euler(other.rotation.eulerAngles + _rotDiff));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var teleportable = other.GetComponent<Teleportable>();
            if (teleportable == null) return;
            
            if (isEntrance)
            {
                Destroy(other.gameObject);
            }
            else
            {
                teleportable.Exit();
            }
        }
    }
}