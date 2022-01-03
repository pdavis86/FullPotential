using FullPotential.Core.Behaviours.GameManagement;
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
#pragma warning disable 0649
        [SerializeField] private readonly Vector2 _lookSensitivity = new Vector2(0.2f, 0.2f);
        [SerializeField] private readonly Vector2 _lookSmoothness = new Vector2(3f, 3f);
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _cameraRotationLimit = 85f;
        [SerializeField] private float _jumpForceMultiplier = 10500f;
#pragma warning restore 0649

        private Rigidbody _rb;
        private PlayerState _playerState;

        //Variables for passing values
        private Vector2 _moveVal;
        private Vector2 _lookVal;
        private Vector3 _jumpForce;

        //Variables for maintaining state
        private Vector2 _smoothLook;
        private float _currentCameraRotationX;
        private bool _isJumping;
        private bool _isSprinting;

        #region Event Handlers 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerState = GetComponent<PlayerState>();

            InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
        }

        private void OnEnable()
        {
            _smoothLook = Vector2.zero;
            _currentCameraRotationX = 0;
            _isJumping = false;
            _playerCamera.transform.localEulerAngles = Vector3.zero;
        }

        private void OnMove(InputValue value)
        {
            _moveVal = value.Get<Vector2>().normalized;
        }

        private void OnLook(InputValue value)
        {
            _lookVal = value.Get<Vector2>();
        }

        private void OnJump()
        {
            _jumpForce = Vector3.up * _jumpForceMultiplier;
        }

        private void OnSprintStart()
        {
            _isSprinting = true;
        }

        private void OnSprintStop()
        {
            _isSprinting = false;
        }

        private void FixedUpdate()
        {
            MoveAndLook();
        }

        #endregion

        private void MoveAndLook()
        {
            if (_moveVal != Vector2.zero)
            {
                var moveX = transform.right * _moveVal.x;
                var moveZ = transform.forward * _moveVal.y;
                var velocity = (moveX + moveZ) * _speed * (_isSprinting ? 2f : 1);
                _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);
            }

            if (_lookVal != Vector2.zero)
            {
                var lookInput = new Vector2(_lookVal.x, _lookVal.y);
                lookInput = Vector2.Scale(lookInput, new Vector2(_lookSensitivity.x * _lookSmoothness.x, _lookSensitivity.y * _lookSmoothness.y));

                _smoothLook.x = Mathf.Lerp(_smoothLook.x, lookInput.x, 1f / _lookSmoothness.x);
                _smoothLook.y = Mathf.Lerp(_smoothLook.y, lookInput.y, 1f / _lookSmoothness.y);

                var rotation = new Vector3(0f, _smoothLook.x, 0f);
                _rb.MoveRotation(_rb.rotation * Quaternion.Euler(rotation));

                var cameraRotationX = _smoothLook.y;
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

        private void CheckIfOffTheMap()
        {
            if (!_playerState.IsDead && transform.position.y < GameManager.Instance.SceneBehaviour.Attributes.LowestYValue)
            {
                _playerState.HandleDeath();
            }
        }

    }
}
