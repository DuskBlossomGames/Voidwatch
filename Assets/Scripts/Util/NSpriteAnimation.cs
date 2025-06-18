using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class NSpriteAnimation : MonoBehaviour
    {
        [Serializable]
        public class AnimationState
        { 
            public Sprite[] frames; 
            public string name;
        }

        public AnimationState[] states;

        private SpriteRenderer _sr;
        public int framesPerSecond = 8;
        public bool loop = true;

        private Sprite[] _frames;
        private Dictionary<string, Sprite[]> _states;

        private float _timer;

        private void Start() { Init(); }

        private bool _inited;
        private void Init()
        {
            if (_inited) return;
            _inited = true;
        
            _sr = GetComponent<SpriteRenderer>();
            _states = new Dictionary<string, Sprite[]>();
            foreach (var state in states) _states[state.name] = state.frames;
        
            SwapState(states[0].name);
        }

        public void SwapState(string stateName)
        {
            if (!_inited) Init();

            _frames = _states[stateName];
            _timer = 0;
            UpdateFrame();
        }

        private void Update()
        {

            _timer += Time.deltaTime;

            var maxTime = (float) _frames.Length / framesPerSecond;
            if (loop) _timer %= maxTime;
            else _timer = Mathf.Min(_timer, maxTime);
        
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            _sr.sprite = _frames[Mathf.Clamp((int) (_timer * framesPerSecond), 0, _frames.Length - 1)];
        }

    }
}
