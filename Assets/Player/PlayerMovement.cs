using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    private CharacterController controller;
    [SerializeField] private float speedFactor = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float movingLookAtSpeed = 15f;
    [SerializeField] private float stationaryLookAtSpeed = 5f;

    private Vector3 target = Vector3.zero;
    private float speed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

    }

    private void OnEnable()
    {
        controller.Move(Vector3.zero);
        target = Vector3.zero;
    }

    void Update()
    {
        SpeedByDistance();
        Gravity();
        ClickToMove();
    }

    void ClickToMove()
    {

        if (Input.GetMouseButtonUp(0))
        {
            controller.Move(Vector3.zero);
            target = Vector3.zero;
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, 999, _layerMask))
        {

            target = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), movingLookAtSpeed * Time.deltaTime);
            if (Input.GetMouseButton(0))
            {

                if (Vector3.Distance(transform.position, target) < 0.5f)
                {
                    controller.Move(Vector3.zero);
                    //smoothly rotate towards target
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), stationaryLookAtSpeed * Time.deltaTime);

                    return;
                }
                controller.Move(transform.forward * speed * Time.deltaTime);
            }
        }

    }

    void Gravity()
    {
        Vector3 velocity = controller.velocity;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void SpeedByDistance()
    {
        //Interpolate speed based on distance to target
        float distance = Vector3.Distance(transform.position, target);
        speed = Mathf.Lerp(0, 12, distance / speedFactor);
        speed = Mathf.Clamp(speed, 0, maxSpeed);
    }
}
