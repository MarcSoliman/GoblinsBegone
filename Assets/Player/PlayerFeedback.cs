using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] private ScriptableArray _detectedEnemy;
    
    [Header("Feedback Particles")]
    [SerializeField] private GameObject _playerBiteAttackParticle;
    [Header("Feedback Audio")]
    [SerializeField] private AudioClip _playerBiteAttackAudio;
    // Start is called before the first frame update
   
    public void PlayerBiteFeedback()
    {
        FeedbackSpawner.Instance.SpawnParticleEffect(_playerBiteAttackParticle, _detectedEnemy.array[0].transform.position);
        FeedbackSpawner.Instance.PlayAudioClip2D(_playerBiteAttackAudio, 1f, -0.5f, 2f, 0.75f, 1.75f);
    }
}
