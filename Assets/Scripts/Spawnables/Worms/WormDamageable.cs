using UnityEngine;

public class WormDamageable : MonoBehaviour, Spawnables.IDamageable
{
    private const float _koffset = 1.5f;
    
    public GameObject healthBarPrefab;
    public int maxHealth;
    public AnimationCurve heathOpacityCurve;
    public GameObject root;
    public DamageResistances dmgRes;

    private GameObject _healthBar;

    private float _barVisibility = 0;
    
    private void Start()
    {
        dmgRes.Ready();
        root.GetComponent<MultiDamageable>().health = maxHealth;
        
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

    public void Kill()
    {
        Destroy(_healthBar);
        Destroy(gameObject);
    }

    public void Damage(float damage, Spawnables.IDamageable.DmgType dmgType, float reduceMod = 1f)
    {
        damage -= reduceMod * dmgRes.dmgReduce[(int)dmgType];
        damage *= dmgRes.dmgMod[(int)dmgType];

        _barVisibility = 1;
        root.GetComponent<MultiDamageable>().health -= damage > 0 ? damage : 0;

        if (root.GetComponent<MultiDamageable>().health <= 0)
        {
            root.GetComponent<MultiDamageable>().Kill();
        }
        
        // scale *2 because it extends in both directions
        _healthBar.transform.GetChild(0).localScale = new Vector3(
            2 * (1 - (float)root.GetComponent<MultiDamageable>().health / maxHealth), 1, 1);
    }
}