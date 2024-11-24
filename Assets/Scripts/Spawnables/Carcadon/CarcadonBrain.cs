using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelSelect;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Carcadon
{
    public class CarcadonBrain : MonoBehaviour
    {
        public float stealthOpacity, stackedStealthOpacity;
        public float opacityTime;
        public float accel, maxSpeed;
        
        public float stealthRadius, stealthScaling, stealthScaleMaxRad; // increase in max speed per unit closeness
        public float enterStealthDamage;
        public float minRandStealthTime, maxRandStealthTime, minUnstealthDist;
        public float stealthAcc;
        public float stealthLowAccTime;

        private EnemySpawner.EnemySpawner _enemySpawner;
        private IEnumerable<SpriteRenderer> _baseSpriteRenderers;
        private List<SpriteRenderer> _stackedSpriteRenderers = new();
        private ArmController[] _armControllers;
        private CustomRigidbody2D _rb;

        private enum Mode { Stealth, Rush, Attack }

        private Mode _mode = Mode.Stealth;
        private float _currSpeed;
        private Timer _opacityTimer = new();

        private CustomRigidbody2D _player;


        private int _maxHealthTier;
        private void Start()
        {
            _maxHealthTier = (int) (GetComponent<EnemyDamageable>().maxHealth / enterStealthDamage) - 1;
            _opacityTimer.Value = opacityTime;
            _rb = GetComponent<CustomRigidbody2D>();
            _armControllers = GetComponentsInChildren<ArmController>();
            _enemySpawner = GetComponent<MiniBoss>().enemySpawner;
            _player = _enemySpawner.player.GetComponent<CustomRigidbody2D>();
            StartCoroutine(Init());

            foreach (var ac in _armControllers) ac.Player = _player.gameObject;
        }

        private int _opacityDir;
        private readonly Timer _stealthTimer = new();
        private readonly Timer _stealthAccTimer = new();
        private void Update()
        {
            var vel = _rb.velocity;
            var dir = vel.normalized;
            
            if (_mode == Mode.Stealth)
            {
                var playerDist = (_player.transform.position - transform.position).magnitude;
                
                var distVal = stealthScaleMaxRad - playerDist;
                var trueMax = maxSpeed + (distVal > 0 ? distVal*distVal : 0) * stealthScaling;

                _stealthAccTimer.Update();
                var trueAccel = _stealthAccTimer.IsFinished ? accel : stealthAcc;
                _currSpeed = UtilFuncs.LerpSafe(_currSpeed, Mathf.Min(trueMax, _currSpeed+accel), Time.deltaTime);

                var w = _currSpeed / stealthRadius; // angular velocity
                
                var pointOnCircle = transform.position.normalized * stealthRadius;

                var targPoint = Quaternion.Euler(0, 0, w * Mathf.Rad2Deg * Time.deltaTime) * pointOnCircle;

                dir = targPoint - transform.position;
                
                _stealthTimer.Update();
                if ((_stealthTimer.IsFinished && playerDist >= minUnstealthDist) || _enemySpawner.SpawnedEnemies.All(g => g == gameObject)) _mode = Mode.Rush;
            } else if (_mode == Mode.Rush)
            {
                _currSpeed = Mathf.Min(maxSpeed, _currSpeed + accel * Time.deltaTime);

                var target = (Vector2)_player.transform.position;// + _player.velocity;
                
                var dist = target - (Vector2) transform.position;
                if (dist.magnitude < 19) SetVisualStealth(false);
                
                dir = dist.normalized;
            }
            
            _rb.velocity = UtilFuncs.LerpSafe(vel, dir * _currSpeed, 10 * Time.deltaTime);
            transform.rotation = UtilFuncs.RotFromNorm(_rb.velocity);

            if (_opacityDir != 0)
            {
                _opacityTimer.Update(_opacityDir);
                
                foreach (var sr in _baseSpriteRenderers) sr.color = new Color(1, 1, 1, 1-(1-_opacityTimer.Progress) * (1-stealthOpacity));
                foreach (var sr in _stackedSpriteRenderers) sr.color = new Color(1, 1, 1, 1-(1-_opacityTimer.Progress) * (1-stackedStealthOpacity));
                
                if (_opacityTimer.IsFinished || _opacityTimer.Progress >= 1) _opacityDir = 0;
            }
        }

        public void TakeDamage(float oldHealth, float newHealth)
        {
            if (Math.Min((int)(oldHealth / enterStealthDamage), _maxHealthTier) != (int)(newHealth / enterStealthDamage))
            {
                _mode = Mode.Stealth;
                _rb.velocity = Vector2.zero; _currSpeed = 0;
                _stealthTimer.Value = Random.Range(minRandStealthTime, maxRandStealthTime);
                _stealthAccTimer.Value = stealthLowAccTime;
                SetVisualStealth(true);

                _enemySpawner.SpawnWave();
            }
        }

        private IEnumerator Init()
        {
            yield return new WaitForSeconds(0.01f);

            _stackedSpriteRenderers.AddRange(transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>());
            _stackedSpriteRenderers.AddRange(transform.GetChild(2).GetComponentsInChildren<SpriteRenderer>());
            _stackedSpriteRenderers.AddRange(transform.GetChild(3).GetComponentsInChildren<SpriteRenderer>());
            _stackedSpriteRenderers.AddRange(transform.GetChild(4).GetComponentsInChildren<SpriteRenderer>());
            _stackedSpriteRenderers.AddRange(transform.GetChild(5).GetComponentsInChildren<SpriteRenderer>());
            _baseSpriteRenderers = GetComponentsInChildren<SpriteRenderer>().Where(sr=>!_stackedSpriteRenderers.Contains(sr));

            _mode = Mode.Stealth;
            _stealthTimer.Value = Random.Range(minRandStealthTime, maxRandStealthTime);
            SetVisualStealth(true);
            // TODO: _enemySpawner.SpawnWave();
        }
        
        private void SetVisualStealth(bool stealth)
        {
            var newDir = stealth ? -1 : 1;
            if (_opacityDir == newDir) return;
            _opacityDir = newDir;
            
            foreach (var ac in _armControllers)
            {
                if (stealth) ac.FoldClosed();
                else ac.FoldOpen();
            }
        }
    }
}