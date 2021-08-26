using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lookSensitivity = 0.7f;
    [SerializeField] private float _cameraRotationLimit = 85f;
    [SerializeField] private float _jumpForceMultipler = 10500f;
#pragma warning restore 0649

    private Rigidbody _rb;

    private Vector2 _moveVal;
    private Vector2 _lookVal;
    private Vector3 _jumpForce;
    private float _currentCameraRotationX;
    private bool _isJumping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnMove(InputValue value)
    {
        _moveVal = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        _lookVal = value.Get<Vector2>();
    }

    void OnJump()
    {
        _jumpForce = Vector3.up * _jumpForceMultipler;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    void PerformMovement()
    {
        var moveX = transform.right * _moveVal.x;
        var moveZ = transform.forward * _moveVal.y;
        var velocity = (moveX + moveZ) * _speed;

        if (velocity != Vector3.zero)
        {
            _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);
        }

        if (!_isJumping && _jumpForce != Vector3.zero)
        {
            _isJumping = true;
            _rb.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            _jumpForce = Vector3.zero;
        }
        else if (_isJumping && _jumpForce == Vector3.zero)
        {
            _isJumping = false;
        }
    }

    void PerformRotation()
    {
        var rotation = new Vector3(0f, _lookVal.x, 0f) * _lookSensitivity;
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(rotation));

        var cameraRotationX = _lookVal.y * _lookSensitivity;
        _currentCameraRotationX -= cameraRotationX;
        _currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
        _playerCamera.transform.localEulerAngles = new Vector3(_currentCameraRotationX, 0f, 0f);
    }

}
