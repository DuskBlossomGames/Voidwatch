using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Spawnables;
using Spawnables.Player;
using Util;

public class ElectricArea : MonoBehaviour
{
    public float shieldDamagePerSecond;
    public float stunTime;

    private Movement _player;
    private Timer _stunTimer = new();

    private void Update()
    {
        var wasStunned = !_stunTimer.IsFinished;
        _stunTimer.Update();
        
        // TODO: more than just block input, slow down or make arc or something
        if (wasStunned) _player.SetInputBlocked(!_stunTimer.IsFinished);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!_stunTimer.IsFinished || !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

        if (damageable.TakeEMP(shieldDamagePerSecond * Time.deltaTime))
        {
            _stunTimer.Value = stunTime;
            _player = other.GetComponent<Movement>();
        }
    }
}