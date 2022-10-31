using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create scriptable object
[CreateAssetMenu(fileName = "Scriptable_EnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    
    public float attackPoints = 100;
    public float sanityPoints = 100;

    public string[] enemyMoves = { "Bludgeon", "Stab", "Heal", "Flee"};
    

}





