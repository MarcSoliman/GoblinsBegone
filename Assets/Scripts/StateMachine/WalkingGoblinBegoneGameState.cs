using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WalkingGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] PlayerEnemyDetection playerEnemyDetection;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject _wayPoint;
    public override void Enter()
    {
        _currentStateText.text = "State: Walking";
        Debug.Log("Player Walk State: ...Entering");
        playerEnemyDetection.OnEnemyDetected += combatStart;
        playerMovement.enabled = true;
        foreach (Transform child in _wayPoint.transform)
        {
            var material = child.GetComponent<MeshRenderer>().material;
            material.DOColor(Color.magenta * 3f, "_EmissionColor", 3f);
        }
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
