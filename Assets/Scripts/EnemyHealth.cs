using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] ScriptableArray _enemyArray;

    protected override void Die()
    {
        _enemyArray.Remove(gameObject);
        base.Die();
    }
}
