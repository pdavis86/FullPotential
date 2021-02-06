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

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lookSensitivity = 3f;
    [SerializeField] private float _cameraRotationLimit = 85f;
    [SerializeField] private float _jumpForceMultipler = 10500f;

    private Rigidbody _rb;
    private PlayerController _playerController;

    private Vector3 _velocity;
    private Vector3 _rotation;
    private Vector3 _jumpForce;
    private float _cameraRotationX;
    private float _currentCameraRotationX;
    private bool _isJumping;

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

            _jumpForce = Input.GetButton("Jump") ? Vector3.up * _jumpForceMultipler : Vector3.zero;
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

                //todo: if within range of interactable, show "Press E to interact"
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    void PerformMovement()
    {
        if (_velocity != Vector3.zero)
        {
            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
        }

        if (!_isJumping && _jumpForce != Vector3.zero)
        {
            _isJumping = true;
            _rb.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        if (_isJumping && _jumpForce == Vector3.zero)
        {
            _isJumping = false;
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
