using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseGoblinBegonGameState : GoblinsBegoneState
{
    [SerializeField] private GameObject _loseScreen;
    
    public override void Enter()
    {
        _loseScreen.SetActive(true);
    }
}
