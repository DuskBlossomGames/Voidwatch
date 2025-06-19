using System;
using Spawnables.Controllers.Bullets;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class SpikeRiftController : MonoBehaviour
    {
        public float numSpikes;
        public float arcAngle;
        public float arcDist;
        public float leadTime;
        public CustomRigidbody2D player;

        private GameObject _spike;
        public GameObject Spike
        {
            get => _spike;
            set
            {
                _spike = value;
                value.GetComponent<DestroyCallback>().Destroyed += () =>
                {
                    Destroy(gameObject);
                    if (_exit.GetComponent<SpikeExitRiftController>().Spike == null) Destroy(_exit);
                };
            }
        }
        [NonSerialized] public int Index;

        private GameObject _exit;

        private void Awake()
        {
            _exit = transform.GetChild(1).gameObject;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject != Spike) return;

            var playerDirection = Mathf.Atan2(player.linearVelocity.y, player.linearVelocity.x) * Mathf.Rad2Deg;
            
            var angle = playerDirection - 180 + arcAngle / 2 - arcAngle / numSpikes * Index;
            var radAngle = angle * Mathf.Deg2Rad;
            
            _exit.transform.position = (Vector3)(Vector2) player.transform.position + new Vector3(
                arcDist * Mathf.Cos(radAngle), 
                arcDist * Mathf.Sin(radAngle),
                -10);

            var vel = Spike.GetComponent<CustomRigidbody2D>().linearVelocity;
            var rot = UtilFuncs.LeadShot(player.transform.position - _exit.transform.position, player.linearVelocity, vel.magnitude);
            _exit.transform.rotation = Quaternion.Euler(0, 0, rot*Mathf.Rad2Deg);
            
            var newSpike = Instantiate(Spike);
            newSpike.GetComponent<DestroyOffScreen>().enabled = true;
            newSpike.transform.position = _exit.transform.position -
                                          _exit.transform.localRotation *
                                          (Spike.transform.position - transform.position) -
                                          _exit.transform.rotation * (vel.magnitude * leadTime * Vector3.right);
            newSpike.transform.rotation = _exit.transform.localRotation * Spike.transform.rotation * Quaternion.Euler(0, 0, 180);
            newSpike.GetComponent<CustomRigidbody2D>().linearVelocity = _exit.transform.localRotation * -vel;
            
            _exit.GetComponent<SpikeExitRiftController>().Spike = newSpike;
            
            _exit.transform.parent = null;
            _exit.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject != Spike) return;

            Destroy(Spike);
            Destroy(gameObject);
        }
    }
}