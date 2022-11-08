using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] private GameObject _pauseScreen;
    public override void Enter()
    {
        _pauseScreen.SetActive(true);
        _currentStateText.text = "State: Pause";
    }

    public void Resume()
    {
        Time.timeScale = 1;
        StateMachine.RevertState();
    }

    public override void Exit()
    {
        _pauseScreen.SetActive(false);
    }
}
