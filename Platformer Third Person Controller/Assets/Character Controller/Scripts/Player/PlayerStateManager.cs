using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enums

public enum PlayerState { IDLE, INAIR, DODGING }

#endregion

public class PlayerStateManager : MonoBehaviour
{
    #region Variables / Components

    private PlayerState _currentState = PlayerState.IDLE;

    #endregion

    #region Properties

    public PlayerState currentState => _currentState;

    #endregion

    #region Methods

    public void ChangeState(PlayerState newState)
    {
        _currentState = newState;
    }

    #endregion

}