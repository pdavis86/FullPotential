using Unity.Netcode;
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
public class PlayerMovement : NetworkBehaviour
{
    private readonly NetworkVariable<Vector3> _rigidBodyVelocity = new NetworkVariable<Vector3>();
    private readonly NetworkVariable<Vector3> _rigidBodyRotationDirection = new NetworkVariable<Vector3>();
    private readonly NetworkVariable<Vector3> _cameraRotationDirection = new NetworkVariable<Vector3>();
    //private readonly NetworkVariable<bool> _isJumping = new NetworkVariable<bool>();

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

    private Vector3 _oldVelocity;
    private Vector3 _oldRotation;
    private Vector3 _oldCameraRotation;

    #region Event Handlers 

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnMove(InputValue value)
    {
        _moveVal = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        _lookVal = value.Get<Vector2>();
    }

    private void OnJump()
    {
        _jumpForce = Vector3.up * _jumpForceMultipler;
    }

    private void FixedUpdate()
    {
        //todo: I don't like this. Let's give the client some flexibility but periodically set their position and rotation

        if (_rigidBodyVelocity.Value != Vector3.zero)
        {
            _rb.MovePosition(_rb.position + _rigidBodyVelocity.Value * Time.fixedDeltaTime);
        }

        if (_rigidBodyRotationDirection.Value != Vector3.zero)
        {
            _rb.MoveRotation(_rb.rotation * Quaternion.Euler(_rigidBodyRotationDirection.Value));
        }

        if (_cameraRotationDirection.Value != Vector3.zero)
        {
            _playerCamera.transform.localEulerAngles = _cameraRotationDirection.Value;
        }

        CheckForInput();
    }

    #endregion

    #region Server RPC methods

    [ServerRpc]
    public void UpdatePositionAndRotationServerRpc(Vector3 rigidBodyVelocity, Vector3 rigidBodyRotation, Vector3 cameraRotation)
    {
        _rigidBodyVelocity.Value = rigidBodyVelocity;
        _rigidBodyRotationDirection.Value = rigidBodyRotation;
        _cameraRotationDirection.Value = cameraRotation;
    }

    #endregion

    private void CheckForInput()
    {
        var moveX = transform.right * _moveVal.x;
        var moveZ = transform.forward * _moveVal.y;
        var velocity = (moveX + moveZ) * _speed;

        //todo: fix jumping
        //if (!_isJumping.Value && _jumpForce != Vector3.zero)
        //{
        //    _isJumping.Value = true;
        //    _rb.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        //    _jumpForce = Vector3.zero;
        //}
        //else if (_isJumping.Value && _jumpForce == Vector3.zero)
        //{
        //    _isJumping.Value = false;
        //}

        var rotation = new Vector3(0f, _lookVal.x, 0f) * _lookSensitivity;

        var cameraRotationX = _lookVal.y * _lookSensitivity;
        _currentCameraRotationX -= cameraRotationX;
        _currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
        var cameraRotation = new Vector3(_currentCameraRotationX, 0f, 0f);

        if (velocity != _oldVelocity || rotation != _oldRotation || cameraRotation != _oldCameraRotation)
        {
            UpdatePositionAndRotationServerRpc(velocity, rotation, cameraRotation);
            _oldVelocity = velocity;
            _oldRotation = rotation;
            _oldCameraRotation = cameraRotation;
        }
    }

}
