using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] ScriptableArray _enemyArray;
    [SerializeField] private ScriptableArray _detectedEnemyArray;
    [SerializeField] private float _actionPoints = 100;
    [SerializeField] private float _sanityPoints = 100;


        

    public float ActionPoints => _actionPoints;
    public float SanityPoints => _sanityPoints;
    
    protected override void Die()
    {
        
        _enemyArray.Remove(gameObject);
        _detectedEnemyArray.Remove(gameObject);
        base.Die();
    }
    
    public void updateActionPoints(float amount)
    {
        _actionPoints -= amount;
        if (_actionPoints <= 0)
        {
            _actionPoints = 0;
        }
    }
    
    public void updateSanityPoints(float amount)
    {
        _sanityPoints -= amount;
        if (_sanityPoints <= 0)
        {
            _sanityPoints = 0;
        }
    }
    
    
}
