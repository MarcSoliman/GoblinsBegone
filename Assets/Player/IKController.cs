using System;
using System.Collections;
using System.Collections.Generic;
using DitzelGames.FastIK;
using UnityEngine;

public class IKController : MonoBehaviour
{
    [SerializeField] GameObject _pole;
    [SerializeField] GameObject _legFinalBone;
    [SerializeField] FastIKFabric _ik;

    [SerializeField] LayerMask _layerMask;

    [SerializeField] GameObject _stepTarget;

    private float _stepAnim;

    bool _isStepping = false;

    void Start()
    {

        //raycast down and create a target at the hit point
        RaycastHit hit;
        if (Physics.Raycast(_pole.transform.position, Vector3.down, out hit, 100, _layerMask))
        {
            GameObject target = new GameObject("target");
            target.transform.position = hit.point;
            _ik.Target = target.transform;
            _ik.Pole = _pole.transform;
            _ik.ChainLength = 2;
            _ik.Iterations = 10;
            _ik.Delta = 0.001f;
            _ik.SnapBackStrength = 1f;

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TakeStep();
    }

    private void TakeStep()
    {
        //if sitance between ik target and steptarget is larger than .4f then move ik target towards step target
        if (Vector3.Distance(_ik.Target.position, _stepTarget.transform.position) > 1)
        {
            _isStepping = true;

        }

        if (_isStepping)
        {
            // _ik.Target.position = Vector3.Slerp(_ik.Target.position, _stepTarget.transform.position, 1f * Time.deltaTime);
            // _ik.Target.position = Vector3.Slerp(_ik.Target.position - _stepTarget.transform.position, _stepTarget.transform.position, 3f * Time.deltaTime);
            _stepAnim += Time.deltaTime * 2.5f;
            _stepAnim = _stepAnim % 1f;


            _ik.Target.transform.position = MathParabola.Parabola(_ik.Target.transform.position, _stepTarget.transform.position, .5f, _stepAnim / 1f);

        }

        if (Vector3.Distance(_ik.Target.position, _stepTarget.transform.position) < 0.2f)
        {
            _isStepping = false;
            _stepAnim = 0f;
        }

    }

    private float DistanceToStepTarget()
    {
        return Vector3.Distance(_ik.Target.position, _stepTarget.transform.position);
    }

    private void OnDrawGizmos()
    {

        //draw line from ik target to step target
        Gizmos.color = Color.Lerp(Color.green, Color.red, DistanceToStepTarget() / .4f);
        Gizmos.DrawLine(_ik.Target.position, _stepTarget.transform.position);


    }

}


public class MathParabola
{

    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }

}
