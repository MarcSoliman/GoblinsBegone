using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private bool _lookAt = true;
    [SerializeField] private LayerMask _layerMask;
    private CharacterController controller;
    [SerializeField] private float speedFactor = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private GameObject _wayPoint;

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
        WayPointColor();
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
            
            if (_lookAt)
            { transform.rotation = 
                Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), movingLookAtSpeed * Time.deltaTime); }
            
            if (Vector3.Distance(transform.position, _wayPoint.transform.position) > 0.5f)
            {
                controller.Move((_wayPoint.transform.position - transform.position).normalized * speed * Time.deltaTime);
            }
            else
            {
                controller.Move(Vector3.zero);
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

    void WayPointColor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (Transform child in _wayPoint.transform)
            {
                var material = child.GetComponent<MeshRenderer>().material;
                material.DOColor(Color.magenta * 3f, "_EmissionColor", 2f);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach (Transform child in _wayPoint.transform)
            {
                var material = child.GetComponent<MeshRenderer>().material;
                material.DOColor(Color.magenta * 1f, "_EmissionColor", 2f);
            }
        }
    }
}
