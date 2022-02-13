using FullPotential.Api.Gameplay;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.PlayerBehaviours
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
#pragma warning disable 0649
        private readonly Vector2 _lookSensitivity = new Vector2(0.2f, 0.2f);
        private readonly Vector2 _lookSmoothness = new Vector2(3f, 3f);
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
        private ClientNetworkTransform _playerCameraNetworkTransform;
        private Vector2 _smoothLook;
        private float _currentCameraRotationX;
        private float _maxDistanceToBeStanding;
        private bool _isJumping;
        private bool _isSprinting;
        private bool _wasSprinting;

        #region Event Handlers 

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerState = GetComponent<PlayerState>();

            if (IsServer)
            {
                InvokeRepeating(nameof(CheckIfOffTheMap), 1, 1);
            }

            _maxDistanceToBeStanding = gameObject.GetComponent<Collider>().bounds.extents.y + 0.1f;

            _playerCameraNetworkTransform = _playerCamera.GetComponent<ClientNetworkTransform>();

            if (!IsServer)
            {
                //Prevent head spinning
                _playerCameraNetworkTransform.enabled = false;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _smoothLook = Vector2.zero;
            _currentCameraRotationX = 0;
            _isJumping = false;
            _isSprinting = false;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnMove(InputValue value)
        {
            _moveVal = value.Get<Vector2>().normalized;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnLook(InputValue value)
        {
            _lookVal = value.Get<Vector2>();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnJump()
        {
            if (IsOnSolidObject())
            {
                _jumpForce = Vector3.up * _jumpForceMultiplier;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnSprintStart()
        {
            if (_playerState.Stamina.Value >= _playerState.GetStaminaCost())
            {
                _isSprinting = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnSprintStop()
        {
            _isSprinting = false;
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            MoveAndLook();
        }

        #endregion

        private bool IsOnSolidObject()
        {
            return Physics.Raycast(transform.position, -Vector3.up, _maxDistanceToBeStanding);
        }

        private void MoveAndLook()
        {
            if (!_isJumping && _moveVal != Vector2.zero)
            {
                if (_isSprinting && _playerState.Stamina.Value < _playerState.GetStaminaCost())
                {
                    _isSprinting = false;
                }

                var moveForwards = transform.forward * _moveVal.y;
                var moveSideways = transform.right * _moveVal.x;

                if (_isSprinting)
                {
                    var sprintSpeed = _playerState.GetSprintSpeed();
                    moveForwards *= _moveVal.y > 0 ? sprintSpeed : sprintSpeed / 2;
                    moveSideways *= sprintSpeed / 2;
                }

                var velocity = _speed * (moveForwards + moveSideways);

                //Move
                _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);

                //Continue after releasing the key
                _rb.AddForce(100 * Time.fixedDeltaTime * velocity, ForceMode.Acceleration);
            }

            if (_lookVal != Vector2.zero)
            {
                if (!_playerCameraNetworkTransform.isActiveAndEnabled)
                {
                    _playerCameraNetworkTransform.enabled = true;
                }

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

            if (_isJumping)
            {
                if (IsOnSolidObject())
                {
                    _isJumping = false;
                }
            }
            else if (_jumpForce != Vector3.zero)
            {
                if (IsOnSolidObject())
                {
                    _isJumping = true;
                    _rb.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Acceleration);
                    _jumpForce = Vector3.zero;
                }
            }

            if (_wasSprinting != _isSprinting)
            {
                _playerState.UpdateSprintingServerRpc(_isSprinting);
                _wasSprinting = _isSprinting;
            }
        }

        private void CheckIfOffTheMap()
        {
            GameManager.Instance.GetService<IAttackHelper>().CheckIfOffTheMap(_playerState, transform.position.y);
        }

    }
}
