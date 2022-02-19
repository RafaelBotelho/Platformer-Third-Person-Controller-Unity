using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    #region Variables

    [Header("Settings")]
    [SerializeField] private float _fallTimeout = 0.15f;

    [Header("Events")]
    [SerializeField] private SO_GenericEvent _onPlayerJumped = default;
    [SerializeField] private SO_GenericEvent _onPlayerDoubleJumped = default;
    [SerializeField] private SO_GenericEvent _onPlayerDodged = default;

    private Animator _animator = default;

    private float _fallTimeoutDelta;
    private float _animationBlend = 0;

    private int _animIDSpeed = 0;
    private int _animIDGrounded = 0;
    private int _animIDJump = 0;
    private int _animIDDoubleJump = 0;
    private int _animIDFreeFall = 0;
    private int _animIDMotionSpeed = 0;
    private int _animIDDodge = 0;

    private PlayerMovementController _movementController = default;
    private PlayerGroundDetector _groundDetector = default;

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        GetReferences();
    }

    private void Start()
    {
        Initialize();
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
        GravityAnimator();
        GroundedCheckAnimator();
        MoveAnimator();
    }

    #endregion

    #region Methods

    private void GetReferences()
    {
        _animator = GetComponent<Animator>();
        _movementController = GetComponent<PlayerMovementController>();
        _groundDetector = GetComponent<PlayerGroundDetector>();
    }

    private void Initialize()
    {
        AssignAnimationIDs();
    }

    private void SubscribeToEvents()
    {
        _onPlayerJumped.AddListener(JumpAnimator);
        _onPlayerDoubleJumped.AddListener(DoubleJumpAnimator);
        _onPlayerDodged.AddListener(DodgeAnimator);
    }

    private void UnsubscribeToEvents()
    {
        _onPlayerJumped.RemoveListener(JumpAnimator);
        _onPlayerDoubleJumped.RemoveListener(DoubleJumpAnimator);
        _onPlayerDodged.RemoveListener(DodgeAnimator);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDDoubleJump = Animator.StringToHash("Double Jump");
        _animIDDodge = Animator.StringToHash("Dodge");
    }

    private void GroundedCheckAnimator()
    {
        _animator.SetBool(_animIDGrounded, _groundDetector.isGrounded);
    }

    private void MoveAnimator()
    {
        _animationBlend = Mathf.Lerp(_animationBlend, _movementController.targetSpeed, Time.deltaTime * _movementController.speedChangeRate);

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, _movementController.inputMagnitude);
    }

    private void JumpAnimator()
    {
        _animator.SetTrigger(_animIDJump);
    }

    private void DoubleJumpAnimator()
    {
        _animator.SetTrigger(_animIDDoubleJump);
    }

    private void DodgeAnimator()
    {
        _animator.SetTrigger(_animIDDodge);
    }

    private void GravityAnimator()
    {
        if (_groundDetector.isGrounded)
        {
            _fallTimeoutDelta = _fallTimeout;

            _animator.SetBool(_animIDFreeFall, false);
        }
        else
        {
            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;

            if (_fallTimeoutDelta < 0.0f)
                _animator.SetBool(_animIDFreeFall, true);
        }
    }

    #endregion
}