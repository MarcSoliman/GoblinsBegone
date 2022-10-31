using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create scriptable object
[CreateAssetMenu(fileName = "Scriptable_Array", menuName = "ScriptableGenerics/Array")]
public class ScriptableArray : ScriptableObject
{
   
   public GameObject[] array;
   
   //add to the array
   public void Add(GameObject obj)
   {
      List<GameObject> list = new List<GameObject>(array);
      list.Add(obj);
      array = list.ToArray();
   }
   
   //remove object from array
   public void Remove(GameObject obj)
   {
      List<GameObject> list = new List<GameObject>(array);
      list.Remove(obj);
      array = list.ToArray();
   }
   
   //check if array is empty
   public bool IsEmpty()
   {
      return array.Length == 0;
   }

   public void Clear()
   {
      array = Array.Empty<GameObject>();
   }
  
}
