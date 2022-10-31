using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] ScriptableArray _enemyArray;
    [SerializeField] private ScriptableArray _detectedEnemyArray;

    protected override void Die()
    {
        _enemyArray.Remove(gameObject);
        _detectedEnemyArray.Remove(gameObject);
        
        
        base.Die();
    }
}
