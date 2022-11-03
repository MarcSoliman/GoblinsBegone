using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private ScriptableArray _enemyArray;
    [SerializeField] private Health _playerHealth;
    [SerializeField] private EnemyHealth _enemyHealth;
    [SerializeField] List<EnemyWeightedMoveValues> _weightedValues;

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
    
      
    //0 = Bludgeon, 1 = stab, 2 = heal, 3 = flee

    // public virtual EnemyWeightedMoveValues EnemyMoveDecision()
    // {
    //
    //     if (_playerHealth.HealthValue > 80)
    //     {
    //
    //         if (_enemyHealth.HealthValue < 60)
    //         {
    //             _weightedValues[0].weight = 5;
    //             _weightedValues[1].weight = 2;
    //             _weightedValues[2].weight = 4;
    //             _weightedValues[3].weight = 1;
    //         }
    //         else
    //         {
    //             _weightedValues[0].weight = 6;
    //             _weightedValues[1].weight = 3;
    //             _weightedValues[2].weight = 1;
    //             _weightedValues[3].weight = 0;
    //         }
    //     }
    //     else if (_playerHealth.HealthValue > 50)
    //     {
    //         if (_enemyHealth.HealthValue < 50)
    //         {
    //             _weightedValues[0].weight = 2;
    //             _weightedValues[1].weight = 2;
    //             _weightedValues[2].weight = 7;
    //             _weightedValues[3].weight = 1;
    //         }
    //         else
    //         {
    //             _weightedValues[0].weight = 6;
    //             _weightedValues[1].weight = 2;
    //             _weightedValues[2].weight = 2;
    //             _weightedValues[3].weight = 0;
    //         }
    //     }
    //     else if (_playerHealth.HealthValue > 20)
    //     {
    //         if (_enemyHealth.HealthValue < 40)
    //         {
    //             _weightedValues[0].weight = 2;
    //             _weightedValues[1].weight = 1;
    //             _weightedValues[2].weight = 8;
    //             _weightedValues[3].weight = 4;
    //         }
    //         else
    //         {
    //             _weightedValues[0].weight = 6;
    //             _weightedValues[1].weight = 2;
    //             _weightedValues[2].weight = 2;
    //             _weightedValues[3].weight = 1;
    //         }
    //     }
    //     else
    //     {
    //         if (_enemyHealth.HealthValue < 30)
    //         {
    //             _weightedValues[0].weight = 2;
    //             _weightedValues[1].weight = 1;
    //             _weightedValues[2].weight = 7;
    //             _weightedValues[3].weight = 9;
    //         }
    //         else
    //         {
    //             _weightedValues[0].weight = 6;
    //             _weightedValues[1].weight = 2;
    //             _weightedValues[2].weight = 2;
    //             _weightedValues[3].weight = 1;
    //         }
    //     }
    //
    //     return RandomWeighted(_weightedValues);
    // } 

    // [0] = Bludgeon, [1] = stab, [2] = heal, [3] = rest,  [4] = flee
    public virtual EnemyWeightedMoveValues EnemyMoveDecision()
    {
        foreach (var value in _weightedValues)
        {
            value.weight = 0;
        }

        if (_enemyHealth.HealthValue > 80)
        {
            _weightedValues[0].weight += 6;
            _weightedValues[1].weight += 3;
            _weightedValues[2].weight += 0;
            _weightedValues[4].weight += 0;
        }
        else if (_enemyHealth.HealthValue > 50)
        {
            _weightedValues[0].weight += 4;
            _weightedValues[1].weight += 3;
            _weightedValues[2].weight += 1;
            _weightedValues[4].weight += 0;
        }
        else if (_enemyHealth.HealthValue > 20)
        {
            _weightedValues[0].weight += 2;
            _weightedValues[1].weight += 5;
            _weightedValues[2].weight += 8;
            _weightedValues[4].weight += 4;
        }
        else
        {
            _weightedValues[0].weight += 1;
            _weightedValues[1].weight += 5;
            _weightedValues[2].weight += 7;
            _weightedValues[4].weight += 9;
        }

        if (_enemyHealth.ActionPoints < 15)
        {
            _weightedValues[0].weight = 0;
            _weightedValues[1].weight = 0;
            _weightedValues[3].weight += 7;
        }
        if (_enemyHealth.ActionPoints < 30)
        {
            _weightedValues[0].weight = 0;
            _weightedValues[1].weight += 2;
            _weightedValues[3].weight += 5;
        }
        else if (_enemyHealth.ActionPoints < 50)
        {
            _weightedValues[0].weight += 2;
            _weightedValues[1].weight += 4;
            _weightedValues[3].weight += 4;
        }
        else if (_enemyHealth.ActionPoints < 70)
        {
            _weightedValues[0].weight += 6;
            _weightedValues[1].weight += 3;
            _weightedValues[3].weight += 2;
        }

        if (_enemyHealth.SanityPoints < _weightedValues[2].sanityCost)
        {
            _weightedValues[2].weight = 0;
            _weightedValues[4].weight += 8;
        }
        
        return RandomWeighted(_weightedValues);
    }

    public void EnemyMove(EnemyWeightedMoveValues decidedMove)
    {
        _playerHealth.OnDamage(decidedMove.damage);
        _enemyHealth.OnHeal(decidedMove.healSelf);
        _enemyHealth.OnDamage(decidedMove.damageSelf);
        _enemyHealth.updateActionPoints(decidedMove.attackCost);
        _enemyHealth.updateSanityPoints(decidedMove.sanityCost);
        print("Goblin used " + decidedMove.name);
        print(_playerHealth.HealthValue);
        //turn is over - go back to player turn
    }



    private EnemyWeightedMoveValues RandomWeighted(List<EnemyWeightedMoveValues> weightedValues)
    {
        EnemyWeightedMoveValues output = weightedValues[0];
        float totalWeight = 0;

        foreach (var element in weightedValues)
        {
            totalWeight += element.weight;
        }

        var randWeightValue = Random.Range(1, totalWeight + 1);
        
        var processedWeight = 0;
        foreach (var element in weightedValues)
        {
            processedWeight += element.weight;
            if (randWeightValue <= processedWeight)
            {
                output = element;
                break;
            }
        }

        return output;
        
    }
}
