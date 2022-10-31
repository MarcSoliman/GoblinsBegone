using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnemyDetection : MonoBehaviour
{
     public Action OnEnemyDetected = delegate {  };
     public GameObject _detectedEnemy;
     public EnemyBase _enemyData;

     private void OnTriggerEnter(Collider other)
     {
          if (other.CompareTag("Enemy"))
          {
               _detectedEnemy = other.gameObject;
               _enemyData = other.GetComponent<EnemyBase>();
               OnEnemyDetected?.Invoke();
              
          }
     }
}
