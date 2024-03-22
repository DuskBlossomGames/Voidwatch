using UnityEngine;

namespace Util
{
    public class Timer
    {
        private float _maxValue;
        private float _value;
        
        public float Value
        {
            get => _value;
            set => _maxValue = _value = value;
        }

        public float Progress => _value / _maxValue;
        public bool IsActive => _value < _maxValue;
        public bool IsFinished => _value == 0;
        

        public void Update(int direction = -1)
        {
            _value = Mathf.Clamp(_value + direction * Time.deltaTime * CustomRigidbody2D.Scaling, 0, _maxValue);
        }

        public void FixedUpdate(int direction = -1)
        {
            _value = Mathf.Clamp(_value + direction * Time.fixedDeltaTime * CustomRigidbody2D.Scaling, 0, _maxValue);
        }
    }
}