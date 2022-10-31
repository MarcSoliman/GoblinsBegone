using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
   [SerializeField] private float gravity = -9.81f;
   [SerializeField] CharacterController controller;
    // Update is called once per frame
    void Update()
    {
        Gravity();
    }
    
    void Gravity()
    {
        Vector3 velocity = controller.velocity;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
