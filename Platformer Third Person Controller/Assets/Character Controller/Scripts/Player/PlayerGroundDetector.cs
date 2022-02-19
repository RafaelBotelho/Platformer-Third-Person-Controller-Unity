using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundDetector : MonoBehaviour
{
    #region Variables / Components

    [Header("Player Grounded")]
    [Tooltip("Useful for rough ground")]
    [SerializeField] private float _groundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float _groundedRadius = 0.28f;
    [Tooltip("Time Used for more responsive controls")]
    [SerializeField] private float _coyoteTime = 0.15f;
    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask _groundLayers;

    private bool _isGrounded = false;
    private float _coyoteTimeCounter;

    #endregion

    #region Properties

    public bool isGrounded => _isGrounded;
    public float coyoteTimeCounter => _coyoteTimeCounter;

    #endregion

    #region Monobehaviour

    private void Update()
    {
        CheckGrounded();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGroundedGizmos();
    }

    #endregion

    #region Methods

    private void CheckGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        _isGrounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        if (_isGrounded)
            _coyoteTimeCounter = _coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;
    }

    private void DrawGroundedGizmos()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z), _groundedRadius);
    }

    #endregion
}