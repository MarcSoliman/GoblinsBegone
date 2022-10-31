using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private float _health = 100;

    public float HealthValue => _health;

    public void OnDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Die();
        }
    }

    public void OnHeal(float heal)
    {
        _health += heal;
        if (_health > 100)
        {
            _health = 100;
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
    
}
