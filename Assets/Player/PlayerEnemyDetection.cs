using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnemyDetection : MonoBehaviour
{
     public Action OnEnemyDetected = delegate {  };
     [SerializeField] private ScriptableArray _detectedEnemy;
    

     private void OnTriggerEnter(Collider other)
     {
          if (other.CompareTag("Enemy"))
          {
               _detectedEnemy.Add(other.gameObject);
               
               OnEnemyDetected?.Invoke();
              
          }
     }
}
