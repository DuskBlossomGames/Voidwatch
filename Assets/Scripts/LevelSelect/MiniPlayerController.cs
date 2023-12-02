using UnityEngine;

namespace LevelSelect
{
    public class MiniPlayerController : MonoBehaviour
    {
        public float secondsPerOrbit;
        
        private Vector2 _orbitPosition;
        private float _orbitRadius;
        
        public void SetOrbitPosition(Vector2 position)
        {
            _orbitPosition = position;
        }
        
        public void SetOrbitRadius(float radius)
        {
            _orbitRadius = radius + transform.localScale.x / 2;
        }

        private float _orbitAngle;

        private void FixedUpdate()
        {
            _orbitAngle += 2 * Mathf.PI * Time.fixedDeltaTime / secondsPerOrbit % (2 * Mathf.PI);

            transform.SetLocalPositionAndRotation(
                _orbitPosition + new Vector2(
                    _orbitRadius * Mathf.Cos(_orbitAngle), 
                    _orbitRadius * Mathf.Sin(_orbitAngle)),
                Quaternion.Euler(0, 0, Mathf.Rad2Deg * _orbitAngle));
        }
    }
}