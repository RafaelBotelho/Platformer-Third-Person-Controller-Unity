using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
	#region Variables

    [Header("Character Input Values")]
	private Vector2 _move;
	private Vector2 _look;
	private bool _sprint;
	private bool _scope;
	private bool _defend;

	[Header("Movement Settings")]
	private bool _analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID

	[Header("Mouse Cursor Settings")]
	private bool _cursorLocked = true;
	private bool _cursorInputForLook = true;

#endif

	#endregion

	#region Properties

	public Vector2 move => _move;
	public Vector2 look => _look;
	public bool sprint => _sprint;
	public bool scope => _scope;
	public bool defend => _defend;

	public bool analogMovement => _analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID

	public bool cursorLocked => _cursorLocked;
	public bool cursorInputForLook => _cursorInputForLook;

#endif

	#endregion

	#region Events

	public delegate void JumpPressedEvent();
	public event JumpPressedEvent OnJumpPressedEvent;

	public delegate void AttackPressedEvent();
	public event AttackPressedEvent OnAttackPressedEvent;

	public delegate void DodgePressedEvent();
	public event DodgePressedEvent OnDodgePressedEvent;

	#endregion

	#region Methods

	#region Events

	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if (_cursorInputForLook)
		{
			LookInput(value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		JumpInput();
	}

	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}

	public void OnScope(InputValue value)
	{
		ScopeInput(value.isPressed);
	}

	public void OnDefend(InputValue value)
	{
		DefendInput(value.isPressed);
	}

	public void OnAttack(InputValue value)
	{
		AttackInput();
	}

	public void OnDodge(InputValue value)
    {
		DodgeInput();
    }

	#endregion

	#region Change States

	public void MoveInput(Vector2 newMoveDirection)
	{
		_move = newMoveDirection;
	}

	public void LookInput(Vector2 newLookDirection)
	{
		_look = newLookDirection;
	}

	public void JumpInput()
	{
		OnJumpPressedEvent?.Invoke();
	}

	public void SprintInput(bool newSprintState)
	{
		_sprint = newSprintState;
	}

	public void ScopeInput(bool newScopeState)
	{
		_scope = newScopeState;
	}

	public void DefendInput(bool newDefendState)
	{
		_defend = newDefendState;
	}

	public void AttackInput()
	{
		OnAttackPressedEvent?.Invoke();
	}

	public void DodgeInput()
    {
		OnDodgePressedEvent?.Invoke();
    }

#if !UNITY_IOS || !UNITY_ANDROID

	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(_cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}

#endif

    #endregion

    #endregion
}
