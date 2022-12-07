using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, _layerMask))
        {
            if (Input.GetMouseButton(0))
            {
                //lerp to hit.point
                transform.position = Vector3.Lerp(transform.position, hit.point, 8f * Time.deltaTime);
            }
        }
    }
}
