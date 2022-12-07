using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private ScriptableArray _enemyArray;
    [SerializeField] private Health _playerHealth;
    [SerializeField] private EnemyHealth _enemyHealth;
    [SerializeField] List<EnemyWeightedMoveValues> _weightedValues;

    [Header("Bludgeon Feedback")]
    [SerializeField] private GameObject _bludgeonAttackParticle;
    [SerializeField] private AudioClip _bludgeonAttackAudio;

    [Header("Stab Feedback")]
    [SerializeField] private GameObject _stabAttackParticle;
    [SerializeField] private AudioClip _stabAttackAudio;

    [Header("Heal Feedback")]
    [SerializeField] private GameObject _healAttackParticle;
    [SerializeField] private AudioClip _healAttackAudio;  

    [Header("Rest Feedback")]
    [SerializeField] private GameObject _restAttackParticle;
    [SerializeField] private AudioClip _restAttackAudio;

    [Header("Flee Feedback")]
    [SerializeField] private GameObject _fleeAttackParticle;
    [SerializeField] private AudioClip _fleeAttackAudio;


    [Header("Move Icons")]
    [SerializeField] private GameObject _bludgeonIcon;
    [SerializeField] private GameObject _stabIcon;
    [SerializeField] private GameObject _healIcon;
    [SerializeField] private GameObject _restIcon;
    

    public Vector3 PlayerBattlePos {get; private set; }


    private void Awake()
    {
        PlayerBattlePos = transform.position + Vector3.back * 4;
        Invoke(nameof(AddToEnemyArray), 0.2f);
    }
    
    private void AddToEnemyArray()
    {
        _enemyArray.Add(gameObject);
    }
    
     private void Update() {
        //look at player
        if (_playerHealth == null) return;
        transform.LookAt(_playerHealth.transform);
    }
      

    // [0] = Bludgeon, [1] = stab, [2] = heal, [3] = rest,  [4] = flee
    public virtual EnemyWeightedMoveValues EnemyMoveDecision()
    {
        foreach (var value in _weightedValues)
        {
            value.weight = 0;
        }

        if (_enemyHealth.HealthValue > 80)
        {
            _weightedValues[0].weight += 6;
            _weightedValues[1].weight += 3;
            _weightedValues[2].weight += 0;
            _weightedValues[4].weight += 0;
        }
        else if (_enemyHealth.HealthValue > 50)
        {
            _weightedValues[0].weight += 4;
            _weightedValues[1].weight += 3;
            _weightedValues[2].weight += 1;
            _weightedValues[4].weight += 0;
        }
        else if (_enemyHealth.HealthValue > 20)
        {
            _weightedValues[0].weight += 2;
            _weightedValues[1].weight += 5;
            _weightedValues[2].weight += 8;
            _weightedValues[4].weight += 4;
        }
        else
        {
            _weightedValues[0].weight += 1;
            _weightedValues[1].weight += 5;
            _weightedValues[2].weight += 7;
            _weightedValues[4].weight += 9;
        }

        if (_enemyHealth.ActionPoints < 15)
        {
            _weightedValues[0].weight = 0;
            _weightedValues[1].weight = 0;
            _weightedValues[3].weight += 7;
        }
        if (_enemyHealth.ActionPoints < 30)
        {
            _weightedValues[0].weight = 0;
            _weightedValues[1].weight += 2;
            _weightedValues[3].weight += 5;
        }
        else if (_enemyHealth.ActionPoints < 50)
        {
            _weightedValues[0].weight += 2;
            _weightedValues[1].weight += 4;
            _weightedValues[3].weight += 4;
        }
        else if (_enemyHealth.ActionPoints < 70)
        {
            _weightedValues[0].weight += 6;
            _weightedValues[1].weight += 3;
            _weightedValues[3].weight += 2;
        }

        if (_enemyHealth.SanityPoints < _weightedValues[2].sanityCost)
        {
            _weightedValues[2].weight = 0;
            _weightedValues[4].weight += 8;
        }
        
        return RandomWeighted(_weightedValues);
    }

     private void IconTween(GameObject icon)
    {
        icon.GetComponent<SpriteRenderer>().DOFade(1, 1f);
        icon.transform.DOScale(Vector3.one * 2, 1f)
        .OnComplete(()=> icon.transform.DOScale(Vector3.one * .8f, .5f));
        icon.transform.DOJump(icon.transform.position, 2,1, 1.2f).OnComplete(()=>
        {
            icon.GetComponent<SpriteRenderer>().DOFade(0, .75f).OnComplete(()=> icon.transform.localScale = Vector3.one*5.6f);
           
        });
    }

     private void PlayerAttackedFeedback()
     {
         var enemyRot = _playerHealth.GetComponent<Transform>().transform.rotation;
         _playerHealth.GetComponent<Transform>().DORotate(enemyRot.eulerAngles + new Vector3(-30, 0, 0), 0.5f)
             .SetLoops(2, LoopType.Yoyo);
     }
    public void EnemyMove(EnemyWeightedMoveValues decidedMove)
    {
        if (decidedMove.name == "Bludgeon")
        {
        FeedbackSpawner.Instance.SpawnParticleEffect(_bludgeonAttackParticle, _playerHealth.transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_bludgeonAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        PlayerAttackedFeedback();
        
        IconTween(_bludgeonIcon);
        }
        else if (decidedMove.name == "Stab")
        {
        FeedbackSpawner.Instance.SpawnParticleEffect(_stabAttackParticle, _playerHealth.transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_stabAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        PlayerAttackedFeedback();
        IconTween(_stabIcon);
        }
        else if (decidedMove.name == "Heal")
        {
        FeedbackSpawner.Instance.SpawnParticleEffect(_healAttackParticle, transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_healAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_healIcon);
        }
        else if (decidedMove.name == "Rest")
        {
        FeedbackSpawner.Instance.SpawnParticleEffect(_restAttackParticle, _playerHealth.transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_restAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_restIcon);
        }
        else if (decidedMove.name == "Flee")
        {
        FeedbackSpawner.Instance.SpawnParticleEffect(_fleeAttackParticle, transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_fleeAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.5f);

        }

        _playerHealth.OnDamage(decidedMove.damage);
        _enemyHealth.OnHeal(decidedMove.healSelf);
        _enemyHealth.OnDamage(decidedMove.damageSelf);
        _enemyHealth.updateActionPoints(decidedMove.attackCost);
        _enemyHealth.updateSanityPoints(decidedMove.sanityCost);
        print("Goblin used " + decidedMove.name);
        print(_playerHealth.HealthValue);
        //turn is over - go back to player turn
    }



    private EnemyWeightedMoveValues RandomWeighted(List<EnemyWeightedMoveValues> weightedValues)
    {
        EnemyWeightedMoveValues output = weightedValues[0];
        float totalWeight = 0;

        foreach (var element in weightedValues)
        {
            totalWeight += element.weight;
        }

        var randWeightValue = Random.Range(1, totalWeight + 1);
        
        var processedWeight = 0;
        foreach (var element in weightedValues)
        {
            processedWeight += element.weight;
            if (randWeightValue <= processedWeight)
            {
                output = element;
                break;
            }
        }

        return output;
        
    }
}
