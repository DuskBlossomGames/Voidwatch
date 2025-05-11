using Spawnables;
using UnityEngine;

public class AreaDamager : MonoBehaviour
{
    public float damagePerSecond;
    public float entryDamage;
    public bool canOnlyHurtPlayer = false;
    public IDamageable.DmgType dmgType;

    void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.name);

        IDamageable dmg;
        if ((dmg = other.gameObject.GetComponent<IDamageable>()) != null && !canOnlyHurtPlayer || canOnlyHurtPlayer && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            dmg.Damage(damagePerSecond * Time.deltaTime, dmgType, gameObject, 0);
            //Debug.LogFormat("{0} Component found as {1}", other.gameObject.name, dmg.ToString());
        } else
        {
            //Debug.LogFormat("{0} Does not have Interface Damageable", other.gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.name);

        IDamageable dmg;
        if ((dmg = other.gameObject.GetComponent<IDamageable>()) != null && !canOnlyHurtPlayer || canOnlyHurtPlayer && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            dmg.Damage(entryDamage, dmgType, gameObject, 0);
            //Debug.LogFormat("{0} Component found as {1}", other.gameObject.name, dmg.ToString());
        }
        else
        {
            //Debug.LogFormat("{0} Does not have Interface Damageable", other.gameObject.name);
        }
    }
}