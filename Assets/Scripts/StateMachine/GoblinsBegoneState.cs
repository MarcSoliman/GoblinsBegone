using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GoblinsBegoneSM))]
public class GoblinsBegoneState : State
{
    protected GoblinsBegoneSM StateMachine { get; private set; }

    [SerializeField] protected TextMeshProUGUI _currentStateText;
    void Awake()
    {
        StateMachine = GetComponent<GoblinsBegoneSM>();
    }

    public override void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
            StateMachine.ChangeState<PauseGoblinBegoneGameState>();

        }
    }
}
