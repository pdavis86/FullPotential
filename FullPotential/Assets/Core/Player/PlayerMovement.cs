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
        private PlayerState _playerState;

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
        //private Vector3 _previousPosition;
        //private Quaternion _previousRotation;
        //private Vector3 _previousLook;

        //Services
        //private IRpcService _rpcService;

        //Others
        private UserInterface _userInterface;

        #region Unity Event Handlers 

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerState = GetComponent<PlayerState>();

            _maxDistanceToBeStanding = gameObject.GetComponent<Collider>().bounds.extents.y + 0.1f;

            //_rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();

            _userInterface = GameManager.Instance.UserInterface;

            GameManager.Instance.GameSettingsUpdated += OnGameSettingsUpdated;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _smoothLook = Vector2.zero;
            _currentCameraRotationX = 0;
            _isMidJump = false;
            //_previousPosition = transform.position;
            //_previousRotation = transform.rotation;
            //_previousLook = _playerCamera.transform.localEulerAngles;
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

        //[ServerRpc]
        //private void ApplyMovementServerRpc(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump, bool isTryingToSprint)
        //{
        //    ApplyMovementToLocalObject(position, rotation, lookDirection, isTryingToJump, isTryingToSprint);

        //    //todo: zzz v0.5 - check players are not cheating their movement values

        //    var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, new[] { 0ul, OwnerClientId });
        //    ApplyMovementClientRpc(position, rotation, lookDirection, isTryingToJump, isTryingToSprint, nearbyClients);
        //}

        //// ReSharper disable once UnusedParameter.Local
        //[ClientRpc]
        //private void ApplyMovementClientRpc(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump, bool isTryingToSprint, ClientRpcParams clientRpcParams)
        //{
        //    ApplyMovementToLocalObject(position, rotation, lookDirection, isTryingToJump, isTryingToSprint);
        //}

        #endregion

        //private void ApplyMovementToLocalObject(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump, bool isTryingToSprint)
        //{
        //    var adjustedPosition = _isMidJump
        //        ? new Vector3(position.x, transform.position.y, position.z)
        //        : position;

        //    transform.SetPositionAndRotation(adjustedPosition, rotation);

        //    _playerCamera.transform.localEulerAngles = lookDirection;

        //    UpdateSprintingState(isTryingToSprint);

        //    Jump(isTryingToJump);
        //}

        private bool IsOnSolidObject()
        {
            return Physics.Raycast(transform.position, -Vector3.up, _maxDistanceToBeStanding);
        }

        private void UpdateSprintingState(bool isTryingToSprint)
        {
            if (!isTryingToSprint)
            {
                _playerState.IsSprinting = false;
                return;
            }

            _playerState.IsSprinting = _playerState.GetStamina() >= _playerState.GetStaminaCost();
            _isTryingToSprint = _playerState.IsSprinting;
        }

        private void MoveAndLook(Vector2 moveVal, Vector2 lookVal, bool isTryingToSprint)
        {
            if (!_isMidJump && moveVal != Vector2.zero)
            {
                var moveForwards = transform.forward * moveVal.y;
                var moveSideways = transform.right * moveVal.x;

                UpdateSprintingState(isTryingToSprint);

                if (_playerState.IsSprinting)
                {
                    var sprintSpeed = _playerState.GetSprintSpeed();
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
            //const float positionThreshold = 0.01f;
            //const float rotationThreshold = 0.01f;

            if (_userInterface.IsAnyMenuOpen())
            {
                return;
            }

            if (_isTryingToSprint != _playerState.IsSprinting)
            {
                UpdateSprintStateServerRpc(_isTryingToSprint);
            }

            MoveAndLook(_moveVal, _lookVal, _isTryingToSprint);

            Jump();

            //var positionDiff = Mathf.Abs((_previousPosition - transform.position).magnitude);
            //var rotationDiff = Mathf.Abs((_previousRotation * Quaternion.Inverse(transform.rotation)).y);
            //var lookDiff = Mathf.Abs((_previousLook - _playerCamera.transform.localEulerAngles).magnitude);

            //if (positionDiff > positionThreshold
            //    || rotationDiff > rotationThreshold
            //    || lookDiff > rotationThreshold
            //    || _isTryingToSprint
            //    || initiateAJump)
            //{
            //    ApplyMovementServerRpc(transform.position, transform.rotation, _playerCamera.transform.localEulerAngles, _isTryingToJump, _isTryingToSprint);

            //    _previousPosition = transform.position;
            //    _previousRotation = transform.rotation;
            //    _previousLook = _playerCamera.transform.localEulerAngles;
            //}
        }

    }
}
