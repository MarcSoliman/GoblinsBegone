using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    public State CurrentState => _currentState;
    protected bool InTransition { get; private set; }

    State _currentState;
    protected State _previousState;


    public void ChangeState<T>() where T : State
    {
        T targetState = GetComponent<T>();
        if (targetState == null)
        {
            Debug.LogWarning("Cannot change state as it does not exist on the State Machine Object");
            return;
        }

        InitiateStateChange(targetState);
    }

    public void RevertState()
    {
        if (_previousState == null)
        {
            Debug.LogWarning("Cannot revert state as there is no previous state");
            return;
        }

        InitiateStateChange(_previousState);
    }

    void InitiateStateChange(State targetState)
    {
        if (_currentState != targetState && !InTransition)
        {
            Transition(targetState);
        }
        else
        {
            Debug.LogWarning("Cannot change state as a transition is already in progress or the target state is the same as the current state");
            return;
        }

    }

    void Transition(State newState)
    {
        InTransition = true;

        _currentState?.Exit();
        _previousState = _currentState;
        _currentState = newState;
        _currentState?.Enter();

        InTransition = false;
    }

    private void Update()
    {
        if (CurrentState != null && !InTransition)
        {
            CurrentState.Tick();
        }
    }
}

