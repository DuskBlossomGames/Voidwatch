using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    private const float _koffset = 1.5f;
    
    public GameObject healthBarPrefab;
    public int maxHealth;
    public AnimationCurve heathOpacityCurve;
    
    private int _health;
    private GameObject _healthBar;

    private float _barVisibility = 0;
    
    private void Start()
    {
        _health = maxHealth;
        
        _healthBar = Instantiate(healthBarPrefab);
    }

    private void LateUpdate()
    {
        var camAngle = -Camera.main.transform.eulerAngles.z * Mathf.Deg2Rad;
        
        _healthBar.transform.rotation = Camera.main.transform.rotation;
        _healthBar.transform.position = 
            transform.position + new Vector3(_koffset*Mathf.Sin(camAngle), _koffset*Mathf.Cos(camAngle), 0);
        
        foreach (var sprite in _healthBar.GetComponentsInChildren<SpriteRenderer>())
        {
            var color = sprite.color;
            _barVisibility -= .4f * Time.deltaTime;
            color.a = heathOpacityCurve.Evaluate(_barVisibility);
            
            sprite.color = color;
        }
    }

    private void Kill()
    {
        Destroy(_healthBar);
        Destroy(gameObject);
    }
    
    public void Damage(int damage)
    {
        _barVisibility = 1;
        _health -= damage;
        
        if (_health <= 0)
        {
            Kill();
        }
        
        // scale *2 because it extends in both directions
        _healthBar.transform.GetChild(0).localScale = new Vector3(
            2 * (1 - (float)_health / maxHealth), 1, 1);
    }
}