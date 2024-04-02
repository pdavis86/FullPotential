using FullPotential.Api.Registry.Resources;
using FullPotential.Core.GameManagement;
using FullPotential.Core.GameManagement.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        private Vector2 _lookSensitivity = new Vector2(0.2f, 0.2f);
        private Vector2 _lookSmoothness = new Vector2(3f, 3f);

#pragma warning disable 0649
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _cameraRotationLimit = 85f;
        [SerializeField] private float _jumpForceMultiplier = 10500f;
        [SerializeField] private int _sprintStoppingFactor = 65;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore 0649

        private Rigidbody _rb;
        private PlayerFighter _playerFighter;

        //Variables for capturing input
        private Vector2 _moveVal;
        private Vector2 _lookVal;
        private bool _isTryingToJump;
        private bool _isTryingToSprint;

        //Variables for maintaining state
        private Vector2 _smoothLook;
        private float _currentCameraRotationX;
        private float _maxDistanceToBeStanding;
        private bool _isMidJump;

        //Others
        private UserInterface _userInterface;

        #region Unity Event Handlers 

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerFighter = GetComponent<PlayerFighter>();

            _maxDistanceToBeStanding = gameObject.GetComponent<Collider>().bounds.extents.y + 0.1f;

            _userInterface = GameManager.Instance.UserInterface;

            GameManager.Instance.GameSettingsUpdated += OnGameSettingsUpdated;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _smoothLook = Vector2.zero;
            _currentCameraRotationX = 0;
            _isMidJump = false;
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            ApplyMovementFromInputs();
        }

        #endregion

        #region GameManager Event Handlers

        private void OnGameSettingsUpdated(object sender, GameSettingsUpdatedEventArgs eventArgs)
        {
            _lookSensitivity = new Vector2(eventArgs.UpdatedSettings.LookSensitivity, eventArgs.UpdatedSettings.LookSensitivity);
            _lookSmoothness = new Vector2(eventArgs.UpdatedSettings.LookSmoothness, eventArgs.UpdatedSettings.LookSmoothness);
        }

        #endregion

        #region Input Event Handlers
        // ReSharper disable UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members

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
            if (!_userInterface.IsAnyMenuOpen() && IsOnSolidObject())
            {
                _isTryingToJump = true;
            }
        }

        private void OnSprintStart()
        {
            if (!_userInterface.IsAnyMenuOpen() && IsOnSolidObject())
            {
                _isTryingToSprint = true;
            }
        }

        private void OnSprintStop()
        {
            _isTryingToSprint = false;
        }

        // ReSharper restore UnusedMember.Local
#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region RPC Methods

        [ServerRpc]
        private void UpdateSprintStateServerRpc(bool isTryingToSprint)
        {
            UpdateSprintingState(isTryingToSprint);
        }

        #endregion

        private bool IsOnSolidObject()
        {
            return Physics.Raycast(transform.position, -Vector3.up, _maxDistanceToBeStanding);
        }

        private void UpdateSprintingState(bool isTryingToSprint)
        {
            if (!isTryingToSprint)
            {
                _playerFighter.IsSprinting = false;
                return;
            }

            _playerFighter.IsSprinting = _playerFighter.GetResourceValue(ResourceTypeIds.StaminaId) >= _playerFighter.GetStaminaCost();
            _isTryingToSprint = _playerFighter.IsSprinting;
        }

        private void MoveAndLook(Vector2 moveVal, Vector2 lookVal, bool isTryingToSprint)
        {
            if (!_isMidJump && moveVal != Vector2.zero)
            {
                var moveForwards = transform.forward * moveVal.y;
                var moveSideways = transform.right * moveVal.x;

                UpdateSprintingState(isTryingToSprint);

                if (_playerFighter.IsSprinting)
                {
                    var sprintSpeed = _playerFighter.GetSprintSpeed();
                    moveForwards *= moveVal.y > 0 ? sprintSpeed : sprintSpeed / 2;
                    moveSideways *= sprintSpeed / 2;
                }

                var velocity = _speed * (moveForwards + moveSideways);

                //Move
                _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);

                //Continue after releasing the key
                _rb.AddForce(_sprintStoppingFactor * Time.fixedDeltaTime * velocity, ForceMode.Acceleration);
            }

            if (lookVal != Vector2.zero)
            {
                var lookInput = new Vector2(lookVal.x, lookVal.y);
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
        }

        private void Jump()
        {
            if (_isMidJump)
            {
                if (IsOnSolidObject())
                {
                    _isMidJump = false;
                }

                _isTryingToJump = false;

                return;
            }

            if (IsOnSolidObject()
                && _isTryingToJump)
            {
                _isMidJump = true;
                _rb.AddForce(_jumpForceMultiplier * Time.fixedDeltaTime * Vector3.up, ForceMode.Acceleration);
            }

            _isTryingToJump = false;
        }

        private void ApplyMovementFromInputs()
        {
            if (_userInterface.IsAnyMenuOpen())
            {
                return;
            }

            if (_isTryingToSprint != _playerFighter.IsSprinting)
            {
                UpdateSprintStateServerRpc(_isTryingToSprint);
            }

            MoveAndLook(_moveVal, _lookVal, _isTryingToSprint);

            Jump();
        }

    }
}
