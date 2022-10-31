using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PlayerTurnGoblinBegoneGameState : GoblinsBegoneState
{
    [SerializeField] TextMeshProUGUI _playerTurnTextUI = null;
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _playerMoveSet;
    [SerializeField] private Button[] _playerMoveSetButtons;
    [SerializeField] private ScriptableArray _enemyArray;

    [SerializeField] private ScriptableArray _detectedEnemyArray;
    private GameObject _detectedEnemy;
    private Vector3 _playerBattlePos;
    

    int _playerTurnCount = 0;

    public override void Enter()
    {
        _playerMoveSet.SetActive(true);
        foreach (var button in _playerMoveSetButtons)
        {
            button.interactable = true;
        }
        
        if (_detectedEnemyArray.array.Length > 0)
            _detectedEnemy = _detectedEnemyArray.array[0];
        else
            _detectedEnemy = null;

        if (_detectedEnemy != null) _playerBattlePos = _detectedEnemy.GetComponent<EnemyBase>().PlayerBattlePos;
        _currentStateText.text = "State: Player Turn";
        Debug.Log("Player Turn: ...Entering");
        _playerTurnTextUI.gameObject.SetActive(true);

        _playerTurnCount++;
        _playerTurnTextUI.text = "Player Turn: " + _playerTurnCount.ToString();
    }

    public override void Tick()
    {
        //detecting if enemy is still present, edge case for when enemy flees from battle and inflicts self damage
        if (_detectedEnemyArray.array.Length < 1)
        {
            if (_enemyArray.IsEmpty())
            {
                _detectedEnemyArray.Clear();
                _detectedEnemy = null;
                StateMachine.ChangeState<WinGoblinBegoneGameState>();
            }
            else
            {
                EnemyArrayDebug();
                _detectedEnemyArray.Clear();
                _detectedEnemy = null;
                StateMachine.ChangeState<WalkingGoblinBegoneGameState>();
            }
        }

        if (_player.IsDestroyed())
        {
            StateMachine.ChangeState<LoseGoblinBegonGameState>();
            return;
        }
        //player look at enemy
        if (_detectedEnemy != null)
        {
            _player.transform.LookAt(_detectedEnemy.transform);
        }
       
        //lerp current player location to battle location
        //if player is at battle location, then change state to player battle state
        
        if (Vector3.Distance(_player.transform.position, _playerBattlePos) > 0.025f)
        {
            _player.transform.position = Vector3.Lerp(_player.transform.position, _playerBattlePos, 2f * Time.deltaTime);
        }
        else
        {
            _player.transform.position = _playerBattlePos;
        }
        
    }

    public override void Exit()
    {
        _playerTurnTextUI.gameObject.SetActive(false);
        Debug.Log("Player Turn: Exiting...");
        foreach (var button in _playerMoveSetButtons)
        {
            button.interactable = false;
        }
        _playerMoveSet.SetActive(false);
    }


    public void Bite()
    {
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(25);
        print("Bite!");
        PlayerBattleStateChange();
    }
    
    public void Pounce()
    {
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(50);
        print("Pounce!");
        PlayerBattleStateChange();
    }
    
    public void Screech()
    {
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(50);
        print("Screeeech!");
        PlayerBattleStateChange();
    }

    public void FearArrow()
    {
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(50);
        print("Fear Arrow Fired!");
        PlayerBattleStateChange();
    }
    
    public void Venom()
    {
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(50);
        print("Venom!");
        PlayerBattleStateChange();
    }
    
    public void HiveMind(){
        if (_detectedEnemy == null) return;
        _detectedEnemy.GetComponent<EnemyHealth>().OnDamage(50);
        print("Hive Mind!");
        PlayerBattleStateChange();
    }

    //checks if enemy is dead and/or if there are any enemies left then decides on what state to change to
    private void PlayerBattleStateChange()
    {
        if (_detectedEnemy.GetComponent<Health>().HealthValue <= 0 )
        {
            if (_enemyArray.IsEmpty())
            {
                _detectedEnemyArray.Clear();
                _detectedEnemy = null;
                StateMachine.ChangeState<WinGoblinBegoneGameState>();
            }
            else
            {
                EnemyArrayDebug();
                _detectedEnemyArray.Clear();
                _detectedEnemy = null;
                StateMachine.ChangeState<WalkingGoblinBegoneGameState>();
            }
        }
        else
        {
            StateMachine.ChangeState<EnemyTurnGoblinBegoneGameState>();
        }
    }
    
    //debug.log elements in enemy array
    private void EnemyArrayDebug()
    {
        foreach (var enemy in _enemyArray.array)
        {
            Debug.Log(enemy);
        }
    }
}
