using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] private GameObject _pauseScreen;
    public override void Enter()
    {
        _pauseScreen.SetActive(true);
    }

    public void Resume()
    {
        StateMachine.RevertState();
        Time.timeScale = 1;
    }
    
    public override void Exit()
    {
        _pauseScreen.SetActive(false);
    }
}
