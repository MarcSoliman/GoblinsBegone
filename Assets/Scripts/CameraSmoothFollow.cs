using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmoothFollow : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] float _smoothTime = 0.3f;
    [SerializeField] float yOffset = 20f;
    [SerializeField] float zOffset = -5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SmoothFollow();
    }

    void SmoothFollow()
    {
        //lerp to player position using smooth time and offset
        if (_player == null) return;
        Vector3 targetPosition = new Vector3(_player.transform.position.x , _player.transform.position.y + yOffset, _player.transform.position.z + zOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothTime);
        
    }
}
