using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    #region Variables

    #region Inspector

    [Header("Movement")]
	[Tooltip("Move speed of the character in m/s")]
	[SerializeField] private float _moveSpeed = 2.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	[SerializeField] private float _sprintSpeed = 5.335f;
	[Tooltip("Speed of the character in m/s when on the air")]
	[SerializeField] private float _airSpeed = 7.5f;
	[Tooltip("Dodge speed of the character in m/s")]
	[SerializeField] private float _dodgeSpeed = 8f;
	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	[SerializeField] private float _rotationSmoothTime = 0.12f;
	[Tooltip("Acceleration and deceleration")]
	[SerializeField] private float _speedChangeRate = 10.0f;

	[Header("Jump")]
	[Space(10)]
	[Tooltip("The height the player can jump")]
	[SerializeField] private float _jumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	[SerializeField] private float _gravity = -15.0f;
	[Tooltip("The buffer to jump action so the controls don't feel broken")]
	[SerializeField] private float _jumpBuffer = 0.15f;

	[Header("Events")]
	[SerializeField] private SO_GenericEvent _onDodgeMovementStarted;
	[SerializeField] private SO_GenericEvent _onDodgeMovementEnded;
	[SerializeField] private SO_GenericEvent _onDodgeStarted;
	[SerializeField] private SO_GenericEvent _onDodgeFinished;
	[SerializeField] private SO_GenericEvent _onJumped;
	[SerializeField] private SO_GenericEvent _onDoubleJumped;

	#endregion

	// player
	private float _speed;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;
	private float _targetSpeed;

	//Jump
	private bool _doubleJumped;
	private float _jumpButtonPressedTime;

	//Dodge
	private bool _dodgeMovement;

	private GameObject _mainCamera;
	private CharacterController _characterController = default;

	//References
	private PlayerInputController _inputController = default;
	private PlayerGroundDetector _groundDetector = default;
	private PlayerStateManager _stateManager = default;

	#endregion

	#region Properties

	public float speedChangeRate => _speedChangeRate;
	public float inputMagnitude => InputMagnitude();
	public float targetSpeed => _targetSpeed;

	#endregion

	#region Monobehaviour

	private void Awake()
	{
		GetReferences();
	}

    private void OnEnable()
    {
		SubscribeToEvents();
    }

    private void OnDisable()
    {
		UnsubscribeToEvents();
    }

    private void Update()
	{
		UpdateMovement();
		Gravity();
	}

    #endregion

    #region Methods

    #region Initialization

    private void GetReferences()
    {
		_inputController = GetComponent<PlayerInputController>();
		_stateManager = GetComponent<PlayerStateManager>();
		_groundDetector = GetComponent<PlayerGroundDetector>();

		_mainCamera = Camera.main.gameObject;
		_characterController = GetComponent<CharacterController>();
	}

	private void SubscribeToEvents()
    {
		_inputController.OnJumpPressedEvent += SetJumpInputTime;
		_inputController.OnDodgePressedEvent += StartDodge;

		_onDodgeMovementStarted.AddListener(StartDodgeMovement);
		_onDodgeMovementEnded.AddListener(FinishDodge);
	}

	private void UnsubscribeToEvents()
    {
		_inputController.OnJumpPressedEvent -= SetJumpInputTime;
		_inputController.OnDodgePressedEvent -= StartDodge;

		_onDodgeMovementStarted.RemoveListener(StartDodgeMovement);
		_onDodgeMovementEnded.RemoveListener(FinishDodge);
	}

    #endregion

    #region Update Movement States

    private void UpdateMovement()
    {
        switch (_stateManager.currentState)
        {
			case PlayerState.IDLE:
				TryToMove();
				TryToJump();
				GravityState();
				break;
			case PlayerState.INAIR:
				TryToMove();
				TryToJump();
				GravityState();
				break;
			case PlayerState.DODGING:
				Dodge();
				break;
		}
	}

	private void GravityState()
    {
		switch (_stateManager.currentState)
		{
			case PlayerState.IDLE:
				if (!_groundDetector.isGrounded || _verticalVelocity > 0.0f)
					_stateManager.ChangeState(PlayerState.INAIR);
				break;
			case PlayerState.INAIR:
				if (_groundDetector.isGrounded)
					_stateManager.ChangeState(PlayerState.IDLE);
				break;
		}

	}

    #endregion

    #region Input Movement

    private void TryToMove()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		_targetSpeed = _inputController.sprint && _stateManager.currentState == PlayerState.IDLE ? _sprintSpeed : _moveSpeed;

		if (_stateManager.currentState == PlayerState.INAIR)
			_targetSpeed = _inputController.sprint ? _airSpeed : _moveSpeed;

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (_inputController.move == Vector2.zero) _targetSpeed = 0.0f;

		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (_inputController.move != Vector2.zero)
			_targetRotation = Mathf.Atan2(InputDirection().x, InputDirection().z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;

		MoveCharacter(_targetSpeed * InputMagnitude());
	}

    #endregion

    #region Dodge

    private void StartDodge()
    {
		if (_stateManager.currentState != PlayerState.IDLE) return;

		_targetRotation = Mathf.Atan2(InputDirection().x, InputDirection().z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
		_stateManager.ChangeState(PlayerState.DODGING);

		_onDodgeStarted?.Invoke();
	}

	private void StartDodgeMovement()
    {
		_dodgeMovement = true;
    }

	private void Dodge()
    {
		if (_dodgeMovement)
			MoveCharacter(_dodgeSpeed);
	}

	private void FinishDodge()
    {
		_stateManager.ChangeState(PlayerState.IDLE);
		_dodgeMovement = false;

		_onDodgeFinished?.Invoke();
	}

    #endregion

    #region Jump

    private void SetJumpInputTime()
    {
		_jumpButtonPressedTime = _jumpBuffer;
    }

	private void TryToJump()
    {
		_jumpButtonPressedTime -= Time.deltaTime;

		if (_groundDetector.coyoteTimeCounter > 0f && _jumpButtonPressedTime > 0f && _verticalVelocity <= 0)
		{
			// the square root of H * -2 * G = how much velocity needed to reach desired height
			_verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
			_jumpButtonPressedTime = 0;
			_stateManager.ChangeState(PlayerState.INAIR);

			_onJumped?.Invoke();
		}
		else if (!_doubleJumped && _jumpButtonPressedTime > 0f && _groundDetector.coyoteTimeCounter <= 0f)
        {
			// the square root of H * -2 * G = how much velocity needed to reach desired height
			_verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
			_doubleJumped = true;

			_onDoubleJumped?.Invoke();
		}
	}

    #endregion

	#region General

	private void Gravity()
	{
		if (_groundDetector.isGrounded)
		{
			_doubleJumped = false;

			// stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
				_verticalVelocity = -4f;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
			_verticalVelocity += _gravity * Time.deltaTime;
	}

	private void MoveCharacter(float targetSpeed)
	{
		float currentHorizontalSpeed = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z).magnitude;
		float speedOffset = 0.1f;

		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * _speedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _rotationSmoothTime);

		// rotate to face input direction relative to camera position
		transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		// move the player
		_characterController.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

	private float InputMagnitude()
    {
		return _inputController.analogMovement ? _inputController.move.magnitude : 1f;
	}

	private Vector3 InputDirection()
    {
		return new Vector3(_inputController.move.x, 0.0f, _inputController.move.y).normalized;
	}

	#endregion

	#endregion
}