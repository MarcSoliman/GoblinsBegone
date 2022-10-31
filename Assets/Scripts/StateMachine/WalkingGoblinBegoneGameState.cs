using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] PlayerEnemyDetection playerEnemyDetection;
    [SerializeField] PlayerMovement playerMovement;
    public override void Enter()
    {
        _currentStateText.text = "State: Walking";
        Debug.Log("Player Walk State: ...Entering");
        playerEnemyDetection.OnEnemyDetected += combatStart;
        playerMovement.enabled = true;
    }

    public void combatStart()
    {
        //change state to player turn state
        StateMachine.ChangeState<PlayerTurnGoblinBegoneGameState>();
    }

    public override void Exit()
    {
        playerEnemyDetection.OnEnemyDetected -= combatStart;
        Debug.Log("Player Walk State: Exiting...");
        playerMovement.enabled = false;
    }
}
