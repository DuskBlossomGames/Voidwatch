using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    private const float Offset = 1.5f;
    
    public GameObject healthBarPrefab;
    public int maxHealth;
    
    private int _health;
    private GameObject _healthBar;
    
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
            transform.position + new Vector3(Offset*Mathf.Sin(camAngle), Offset*Mathf.Cos(camAngle), 0);
    }

    private void Kill()
    {
        // Destroy(gameObject);
    }
    
    public void Damage(int damage)
    {
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