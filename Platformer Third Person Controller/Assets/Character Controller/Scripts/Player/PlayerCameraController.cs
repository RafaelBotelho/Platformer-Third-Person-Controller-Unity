using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    #region Variables

    [Header("Settings")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject _cinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] private float _topClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] private float _bottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] private float _cameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] private bool _lockCameraPosition = false;
    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] private float _sensitivity = 1;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private const float _threshold = 0.01f;

    private PlayerInputController _inputController = default;

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        GetReferences();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    #endregion

    #region Methods

    private void GetReferences()
    {
        _inputController = GetComponent<PlayerInputController>();
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_inputController.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            _cinemachineTargetYaw += _inputController.look.x * Time.deltaTime * _sensitivity;
            _cinemachineTargetPitch += _inputController.look.y * Time.deltaTime * _sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        // Cinemachine will follow this target
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    #endregion
}
