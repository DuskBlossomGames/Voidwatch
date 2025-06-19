using System;
using UnityEngine;

namespace Util
{
    public class CustomRigidbody2D : MonoBehaviour
    {
        private static float _scaling = 1;

        public static float Scaling
        {
            get => _scaling;
            set
            {
                if (_scaling == value) return;
                
                ScalingChange?.Invoke(_scaling, value);
                _scaling = value;
            }
        }

        private static event Action<float, float> ScalingChange;

        private Rigidbody2D _rigid;
        
        // ReSharper disable once InconsistentNaming
        public Vector2 linearVelocity
        {
            get => _rigid.linearVelocity / _scaling;
            set
            {
                _rigid.linearVelocity = value * _scaling;
            }
        }
        
        // ReSharper disable once InconsistentNaming
        public Vector2 position
        {
            get => _rigid.position;
            set => _rigid.position = value;
        }

        public float angularVelocity
        {
            get => _rigid.angularVelocity;
            set => _rigid.angularVelocity = value;
        }

        [Serializable]
        public class Constraints
        {
            public bool freezeX;
            public bool freezeY;
            public bool freezeRotation;
        }
        
        public float mass = 1;
        public CollisionDetectionMode2D collisionDetection;
        public Constraints constraints;

        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            _rigid.AddForce(force * _scaling, mode);
        }
        
        public void AddRelativeForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            _rigid.AddRelativeForce(force * _scaling, mode);
        }

        private Action<float, float> _eventHandler;

        private void Awake()
        {
            // In the case of things that clone, it may have already added.
            // This does not mean any objects should intentionally have both a CustomRigidbody2D and a normal one.
            _rigid = gameObject.GetComponent<Rigidbody2D>();
            if (_rigid == null) _rigid = gameObject.AddComponent<Rigidbody2D>(); // sadly ?? doesn't work on overridden == ig
            
            _rigid.mass = mass;
            _rigid.gravityScale = 0;
            _rigid.collisionDetectionMode = collisionDetection;
            _rigid.constraints = 
                (constraints.freezeX ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.None) |
                (constraints.freezeY ? RigidbodyConstraints2D.FreezePositionY : RigidbodyConstraints2D.None) |
                (constraints.freezeRotation ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None);
            _rigid.angularDamping = 0;
            
            ScalingChange += _eventHandler = (oldScaling, newScaling) => _rigid.linearVelocity *= newScaling / oldScaling;
        }

        private void OnDestroy()
        {
            ScalingChange -= _eventHandler;
        }
    }
}