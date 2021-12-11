using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.PlayerBehaviours
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        //private readonly NetworkVariable<Vector3> _rigidBodyVelocity = new NetworkVariable<Vector3>();
        //private readonly NetworkVariable<Vector3> _rigidBodyRotationDirection = new NetworkVariable<Vector3>();
        //private readonly NetworkVariable<Vector3> _cameraRotationDirection = new NetworkVariable<Vector3>();
        //private readonly NetworkVariable<bool> _isJumping = new NetworkVariable<bool>();

#pragma warning disable 0649
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _lookSensitivity = 0.7f;
        [SerializeField] private float _cameraRotationLimit = 85f;
        [SerializeField] private float _jumpForceMultiplier = 10500f;
#pragma warning restore 0649

        private Rigidbody _rb;
        private PlayerState _playerState;
        private Vector2 _moveVal;
        private Vector2 _lookVal;
        private Vector3 _jumpForce;
        private float _currentCameraRotationX;
        private bool _isJumping;

        #region Event Handlers 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerState = GetComponent<PlayerState>();
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
            _jumpForce = Vector3.up * _jumpForceMultiplier;
        }

        private void FixedUpdate()
        {
            CheckForInput();
        }

        #endregion

        private void CheckForInput()
        {
            if (_moveVal != Vector2.zero)
            {
                var moveX = transform.right * _moveVal.x;
                var moveZ = transform.forward * _moveVal.y;
                var velocity = (moveX + moveZ) * _speed;
                _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);
            }

            if (_lookVal != Vector2.zero)
            {
                var rotation = new Vector3(0f, _lookVal.x, 0f) * _lookSensitivity;
                _rb.MoveRotation(_rb.rotation * Quaternion.Euler(rotation));

                var cameraRotationX = _lookVal.y * _lookSensitivity;
                _currentCameraRotationX -= cameraRotationX;
                _currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
                var cameraRotation = new Vector3(_currentCameraRotationX, 0f, 0f);
                _playerCamera.transform.localEulerAngles = cameraRotation;
            }

            switch (_isJumping)
            {
                case false when _jumpForce != Vector3.zero:
                    _isJumping = true;
                    _rb.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
                    _jumpForce = Vector3.zero;
                    break;

                case true when _jumpForce == Vector3.zero:
                    _isJumping = false;
                    break;
            }

            if (_moveVal != Vector2.zero || _lookVal != Vector2.zero || _rb.velocity != Vector3.zero)
            {
                _playerState.UpdatePositionsAndRotationsServerRpc(_rb.position, _rb.rotation, _rb.velocity, _playerCamera.transform.rotation);
            }
        }

    }
}
