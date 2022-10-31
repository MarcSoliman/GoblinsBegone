using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupGoblinsBegoneGameState : GoblinsBegoneState
{

    [SerializeField] private ScriptableArray _enemyArray;
    [SerializeField] private ScriptableArray _detectedEnemyArray;
    bool _activated = false;
    public override void Enter()
    {
        _enemyArray.Clear();
        _detectedEnemyArray.Clear();
        _currentStateText.text = "State: Setup";
        Debug.Log("Setup: ...Entering");
        _activated = false;
    }

    public override void Tick()
    {
        if (_activated == false)
        {
            _activated = true;
            StateMachine.ChangeState<WalkingGoblinBegoneGameState>();
        }
    }

    public override void Exit()
    {
        _activated = false;
        Debug.Log("Setup: Exiting...");
    }
}
