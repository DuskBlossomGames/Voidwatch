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

        public float LastStepProgress => _lastStep / _maxValue;

        private float _lastStep;
        

        public void Update(int direction = -1)
        {
            _value = Mathf.Clamp(_value + (_lastStep = direction * Time.deltaTime * CustomRigidbody2D.Scaling), 0, _maxValue);
        }

        public void FixedUpdate(int direction = -1)
        {
            _value = Mathf.Clamp(_value + (_lastStep = direction * Time.fixedDeltaTime * CustomRigidbody2D.Scaling), 0, _maxValue);
        }
    }
}