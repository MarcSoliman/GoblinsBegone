using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinsBegoneSM : StateMachine
{

    void Start()
    {
        ChangeState<SetupGoblinsBegoneGameState>();
    }


}
