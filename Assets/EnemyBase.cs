using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] EnemyData _enemyData;
    [SerializeField] private ScriptableArray _enemyArray;
    public EnemyData EnemyData => _enemyData;

    public Vector3 PlayerBattlePos {get; private set; }


    private void Awake()
    {
        PlayerBattlePos = transform.position + Vector3.back * 4;
        Invoke(nameof(AddToEnemyArray), 0.2f);
    }
    
    private void AddToEnemyArray()
    {
        _enemyArray.Add(gameObject);
    }
}
