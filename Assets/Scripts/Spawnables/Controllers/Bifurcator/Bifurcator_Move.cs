using Spawnables.Damage;
using UnityEngine;

namespace Spawnables.Controllers.Bifurcator
{
    public class Bifurcator_Move : Stunnable
    {
        public float rotSpeed; // degrees per sec

        private bool _stunned;
        private float _rotMult;
    
        void Start()
        {
            transform.position = new Vector3(0, 0, transform.position.z);
            transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);

            rotSpeed *= Mathf.Sign(Random.value - 0.5f);
            _rotMult = 1;
        }

    
        void FixedUpdate()
        {
            transform.Rotate(0, 0, _rotMult * rotSpeed * Time.fixedDeltaTime);
        }

        public override void Stun()
        {
            // disable laser
            for (var i = 3; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);
        }

        public override void UpdateStun()
        {
            // slow down
            _rotMult *= Mathf.Pow(0.1f, Time.deltaTime);
        }

        public override void UnStun()
        {
            // enable laser
            for (var i = 3; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
            _rotMult = 1;
        }
    }
}
