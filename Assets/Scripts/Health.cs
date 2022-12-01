using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private AudioClip _deathAudio;
    private float _health = 100;

    public float HealthValue => _health;

    public void OnDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            FeedbackSpawner.Instance.PlayAudioClip2D(_deathAudio, 1f, .5f, 1.5f, .75f, 1.3f);
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
