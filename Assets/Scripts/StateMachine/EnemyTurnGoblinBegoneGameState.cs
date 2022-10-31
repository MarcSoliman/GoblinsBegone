using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyTurnGoblinBegoneGameState : GoblinsBegoneState
{
    public UnityEvent  EnemyTurnBegan;
    public UnityEvent EnemyTurnEnded;

    [SerializeField] private float _pauseDuration = 1.5f;
    [SerializeField] private GameObject _playerMoveSet;
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private Health _playerHealth;
    [SerializeField] private Health _enemyHealth;


    public override void Enter()
    {
        Debug.Log("Enemy Turn: ...Enter");
        EnemyTurnBegan?.Invoke();
        _playerMoveSet.SetActive(true);
        StartCoroutine(EnemyThinkingRoutine(_pauseDuration));
    }

    IEnumerator EnemyThinkingRoutine(float pauseDuration)
    {
        Debug.Log("Enemy thinking...");
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log("Enemy performs action");
        EnemyMove();
        EnemyTurnEnded?.Invoke();

        //turn is over - go back to player turn
        StateMachine.ChangeState<PlayerTurnGoblinBegoneGameState>();
    }

    public override void Exit()
    {
        Debug.Log("Enemy Turn: Exit...");
        _playerMoveSet.SetActive(false);
        EnemyTurnEnded?.Invoke();
    }



    
    //0 = Bludgeon, 1 = stab, 2 = heal, 3 = flee
    
    private void EnemyMove()
    {
        float move = 0;
        if (_playerHealth.HealthValue > 80)
        {
            
            if (_enemyHealth.HealthValue < 60)
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 3, 0.7f));
            }
            else
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 1, -0.5f));
            }
        }
        else if (_playerHealth.HealthValue > 50)
        {
            if (_enemyHealth.HealthValue < 60)
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 3, 1f));
            }
            else
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 2, 0.5f));
            }
        }
        else if (_playerHealth.HealthValue > 20)
        {
            if (_enemyHealth.HealthValue < 60)
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 3, 1f));
            }
            else
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 1, -0.2f));
            }
        }
        else
        {
            if (_enemyHealth.HealthValue < 60)
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 3, 1f));
            }
            else
            {
                move = Mathf.Floor(RamdomNumberWeighted(0, 1, -0.2f));
            }
        }
 

        switch (move)
        {
            case 0:
                print("Enemy Uses Bludgeon");
                _playerHealth.OnDamage(30);
                break;
            case 1:
                print("Enemy Uses Stab");
                _playerHealth.OnDamage(10);
                break;
            case 2:
                print("Enemy Uses Heal");
                _enemyHealth.OnHeal(10);
                break;
            case 3:
                print("Enemy Uses Flee");
                _enemyHealth.OnDamage(999);
                StateMachine.ChangeState<WalkingGoblinBegoneGameState>();
                break;
        }
    }

    private float RamdomNumberWeighted(float min, float max, float weight)
    {
        //return random float between min and max, with a bias towards min
        //weight is the bias, 0 is no bias, 1 is all bias towards min
        return Mathf.Lerp(min, max, Mathf.Pow(Random.value, weight));
        
    }

}
