using System;
using Player;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class SpikeRiftController : MonoBehaviour
    {
        public float numSpikes;
        public float arcAngle;
        public float arcDist;
        public CustomRigidbody2D player;
        
        [NonSerialized] public GameObject Spike;
        [NonSerialized] public int Index;

        private GameObject _exit;

        private void Awake()
        {
            _exit = transform.GetChild(1).gameObject;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject != Spike) return;

            var playerDirection = Mathf.Atan2(player.velocity.y, player.velocity.x) * Mathf.Rad2Deg;
            
            var angle = playerDirection - 180 + arcAngle / 2 - arcAngle / numSpikes * Index;
            var radAngle = angle * Mathf.Deg2Rad;
            
            var pos = _exit.transform.position = (Vector3)(Vector2) player.transform.position + new Vector3(
                arcDist * Mathf.Cos(radAngle), 
                arcDist * Mathf.Sin(radAngle),
                -10);
            
            var rot = UtilFuncs.LeadShot(player.transform.position - pos, player.velocity,
                Spike.GetComponent<CustomRigidbody2D>().velocity.magnitude);
            _exit.transform.rotation = Quaternion.Euler(0, 0, rot*Mathf.Rad2Deg);
            
            
            _exit.SetActive(true);
            
            var newSpike = Instantiate(Spike);
            newSpike.transform.position = _exit.transform.position - _exit.transform.localRotation * (Spike.transform.position - transform.position);
            newSpike.transform.rotation = _exit.transform.localRotation * Spike.transform.rotation * Quaternion.Euler(0, 0, 180);
            newSpike.GetComponent<CustomRigidbody2D>().velocity =
                _exit.transform.localRotation * -Spike.GetComponent<CustomRigidbody2D>().velocity;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject != Spike) return;

            Destroy(Spike);
            Destroy(gameObject);
        }
    }
}