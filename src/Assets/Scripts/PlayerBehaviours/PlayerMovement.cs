using Assets.Scripts.Attributes;
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class PlayerMovement : MonoBehaviour
{
    // ReSharper disable InconsistentNaming
    const float _speed = 5f;
    const float _lookSensitivity = 3f;
    const float _cameraRotationLimit = 85f;
    // ReSharper restore InconsistentNaming

    private Rigidbody _rb;
    private PlayerController _playerController;

    private Vector3 _velocity;
    private Vector3 _rotation;
    private float _cameraRotationX;
    private float _currentCameraRotationX;

    public Camera PlayerCamera;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        try
        {
            var moveX = transform.right * Input.GetAxis("Horizontal");
            var moveZ = transform.forward * Input.GetAxis("Vertical");
            _velocity = (moveX + moveZ) * _speed;

            _rotation = new Vector3(0f, Input.GetAxisRaw("Mouse X"), 0f) * _lookSensitivity;

            _cameraRotationX = Input.GetAxisRaw("Mouse Y") * _lookSensitivity;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    void FixedUpdate()
    {
        try
        {
            if (!_playerController.HasMenuOpen)
            {
                PerformMovement();
                PerformRotation();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    [ServerSideOnlyTemp]
    void PerformMovement()
    {
        if (_velocity != Vector3.zero)
        {
            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(_rotation));
        if (PlayerCamera != null)
        {
            _currentCameraRotationX -= _cameraRotationX;
            _currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
            PlayerCamera.transform.localEulerAngles = new Vector3(_currentCameraRotationX, 0f, 0f);
        }
    }

}
