using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyTurnGoblinBegoneGameState : GoblinsBegoneState
{
    public UnityEvent EnemyTurnBegan;
    public UnityEvent EnemyTurnEnded;

    [SerializeField] private float _pauseDuration = 1.5f;
    [SerializeField] private GameObject _playerMoveSet;
    [SerializeField] private ScriptableArray _detectedEnemyArray;

    private EnemyBase _enemy;

    public override void Enter()
    {
        _enemy = _detectedEnemyArray.array[0]?.gameObject.GetComponent<EnemyBase>();
        Debug.Log("Enemy Turn: ...Enter");
        EnemyTurnBegan?.Invoke();
        _playerMoveSet.SetActive(true);
        _currentStateText.text = "State: Enemy Turn";
        StartCoroutine(EnemyThinkingRoutine(_pauseDuration));
    }




    IEnumerator EnemyThinkingRoutine(float pauseDuration)
    {
        Debug.Log("Enemy thinking...");
        yield return new WaitForSeconds(pauseDuration);

        Debug.Log("Enemy performs action");
        _enemy.EnemyMove(_enemy.EnemyMoveDecision());
        StateMachine.ChangeState<PlayerTurnGoblinBegoneGameState>();
        EnemyTurnEnded?.Invoke();


    }

    public override void Exit()
    {
        Debug.Log("Enemy Turn: Exit...");
        _playerMoveSet.SetActive(false);
        EnemyTurnEnded?.Invoke();
    }





}

