using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeMovementAnimBehaviour : StateMachineBehaviour
{
    [SerializeField] private SO_GenericEvent _onMovementStarEvent = default;
    [SerializeField] private SO_GenericEvent _onMovementEndEvent = default;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _onMovementStarEvent?.Invoke();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _onMovementEndEvent?.Invoke();
    }
}
