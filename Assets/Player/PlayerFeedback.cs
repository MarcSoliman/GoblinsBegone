using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] private ScriptableArray _detectedEnemy;
    
    [Header("Bite Feedback")]
    [SerializeField] private GameObject _playerBiteAttackParticle;

    [SerializeField] private AudioClip _playerBiteAttackAudio;


    [Header("Pounce Feedback")]
    [SerializeField] private GameObject _playerPounceAttackParticle;

    [SerializeField] private AudioClip _playerPounceAttackAudio;

    [Header("Screech Feedback")]
    [SerializeField] private GameObject _playerScreechAttackParticle;

    [SerializeField] private AudioClip _playerScreechAttackAudio;

    
    [Header("Fatigue Feedback")]
    [SerializeField] private GameObject _playerFatigueAttackParticle;

    [SerializeField] private AudioClip _playerFatigueAttackAudio;

    [Header("Fear Feedback")]
    [SerializeField] private GameObject _playerFearAttackParticle;

    [SerializeField] private AudioClip _playerFearAttackAudio;

    [Header("Hive Feedback")]
    [SerializeField] private GameObject _playerHiveAttackParticle;

    [SerializeField] private AudioClip _playerHiveAttackAudio;

    [Header("Move Icons")]
    [SerializeField] private GameObject _biteIcon;
    [SerializeField] private GameObject _pounceIcon;
    [SerializeField] private GameObject _screechIcon;
    [SerializeField] private GameObject _fatigueIcon;
    [SerializeField] private GameObject _fearIcon;
    [SerializeField] private GameObject _hiveIcon;
    // Start is called before the first frame update
   
    
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
    public void PlayerBiteFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerBiteAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerBiteAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_biteIcon);
    }

    public void PlayerPounceFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerPounceAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerPounceAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_pounceIcon);
    }

    public void PlayerScreechFeedback()
    {

        if (_detectedEnemy.array.Length > 0)
        {
            FeedbackSpawner.Instance.SpawnParticleEffect(_playerScreechAttackParticle,
                _detectedEnemy.array[0].transform.position);
        }
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerScreechAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_screechIcon);
    }

    public void PlayerFatigueFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerFatigueAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerFatigueAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_fatigueIcon);
    }

    public void PlayerFearFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerFearAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerFearAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_fearIcon);
    }

    public void PlayerHiveFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerHiveAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerHiveAttackAudio, 1f, 0.5f, 2f, 0.75f, 1.75f);
        IconTween(_hiveIcon);
    }
}
