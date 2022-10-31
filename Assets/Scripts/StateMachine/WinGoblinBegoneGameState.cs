using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] private GameObject _winScreen;
    
    public override void Enter()
    {
        _winScreen.SetActive(true);
    }

}
