using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBop : MonoBehaviour
{

    private Vector3 _startPosition;
    

    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        IdleBopAnimation();
    }

    void IdleBopAnimation()
    {
        transform.localPosition = _startPosition + new Vector3(0, Mathf.Sin(Time.time *.75f)*.3f,0);
        
        //rotate up and down the same way
        transform.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.2f) * 15, 0, 0);
     
        
    }
}
