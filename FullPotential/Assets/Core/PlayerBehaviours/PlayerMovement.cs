using FullPotential.Api.GameManagement;
using FullPotential.Api.Ioc;
using FullPotential.Core.GameManagement;
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
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable ConvertToConstant.Local
        private readonly Vector2 _lookSensitivity = new Vector2(0.2f, 0.2f);
        private readonly Vector2 _lookSmoothness = new Vector2(3f, 3f);
        private readonly int _sprintStoppingFactor = 65;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _cameraRotationLimit = 85f;
        [SerializeField] private float _jumpForceMultiplier = 10500f;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore ConvertToConstant.Local
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
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousLook;

        //Services
        private IRpcService _rpcService;

        //Others
        private UserInterface _userInterface;

        #region Unity Event Handlers 

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerState = GetComponent<PlayerState>();

            _maxDistanceToBeStanding = gameObject.GetComponent<Collider>().bounds.extents.y + 0.1f;

            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();

            _userInterface = GameManager.Instance.UserInterface;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            _smoothLook = Vector2.zero;
            _currentCameraRotationX = 0;
            _isMidJump = false;
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
            _previousLook = _playerCamera.transform.localEulerAngles;
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            ApplyMovementFromInputs();
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
            _isTryingToJump = true;
        }

        private void OnSprintStart()
        {
            _isTryingToSprint = true;
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
        private void ApplyMovementServerRpc(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump)
        {
            ApplyMovementToLocalObject(position, rotation, lookDirection, isTryingToJump);

            //todo: zzz v0.5 - check players are not cheating their movement values

            var nearbyClients = _rpcService.ForNearbyPlayersExcept(transform.position, new[] { 0ul, OwnerClientId });
            ApplyMovementClientRpc(position, rotation, lookDirection, isTryingToJump, nearbyClients);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void ApplyMovementClientRpc(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump, ClientRpcParams clientRpcParams)
        {
            ApplyMovementToLocalObject(position, rotation, lookDirection, isTryingToJump);
        }

        #endregion

        private void ApplyMovementToLocalObject(Vector3 position, Quaternion rotation, Vector3 lookDirection, bool isTryingToJump)
        {
            transform.position = position;
            transform.rotation = rotation;

            _playerCamera.transform.localEulerAngles = lookDirection;

            Jump(isTryingToJump);
        }

        private bool IsOnSolidObject()
        {
            return Physics.Raycast(transform.position, -Vector3.up, _maxDistanceToBeStanding);
        }

        private void MoveAndLook(Vector2 moveVal, Vector2 lookVal, bool isTryingToSprint)
        {
            if (!_isMidJump && moveVal != Vector2.zero)
            {
                if (_playerState.IsSprinting && !isTryingToSprint)
                {
                    _playerState.IsSprinting = false;
                }

                var moveForwards = transform.forward * moveVal.y;
                var moveSideways = transform.right * moveVal.x;

                if (isTryingToSprint && _playerState.GetStamina() >= _playerState.GetStaminaCost())
                {
                    var sprintSpeed = _playerState.GetSprintSpeed();
                    moveForwards *= moveVal.y > 0 ? sprintSpeed : sprintSpeed / 2;
                    moveSideways *= sprintSpeed / 2;
                    _playerState.IsSprinting = true;
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

        private void Jump(bool isTryingToJump)
        {
            if (_isMidJump)
            {
                if (IsOnSolidObject())
                {
                    _isMidJump = false;
                }
            }
            else if (isTryingToJump)
            {
                if (IsOnSolidObject())
                {
                    _isMidJump = true;
                    _rb.AddForce(Vector3.up * _jumpForceMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
                }
            }
        }

        private void ApplyMovementFromInputs()
        {
            const float positionThreshold = 0.01f;
            const float rotationThreshold = 0.01f;

            if (_userInterface.IsAnyMenuOpen())
            {
                return;
            }

            var initiateAJump = !GameManager.Instance.UserInterface.IsAnyMenuOpen()
                && IsOnSolidObject()
                && _isTryingToJump;

            _isTryingToJump = false;

            MoveAndLook(_moveVal, _lookVal, _isTryingToSprint);
            Jump(initiateAJump);

            var positionDiff = Mathf.Abs((_previousPosition - transform.position).magnitude);
            var rotationDiff = Mathf.Abs((_previousRotation * Quaternion.Inverse(transform.rotation)).y);
            var lookDiff = Mathf.Abs((_previousLook - _playerCamera.transform.localEulerAngles).magnitude);

            if (positionDiff > positionThreshold
                || rotationDiff > rotationThreshold
                || lookDiff > rotationThreshold
                || _isTryingToSprint
                || initiateAJump)
            {
                ApplyMovementServerRpc(transform.position, transform.rotation, _playerCamera.transform.localEulerAngles, initiateAJump);

                _previousPosition = transform.position;
                _previousRotation = transform.rotation;
                _previousLook = _playerCamera.transform.localEulerAngles;
            }
        }

    }
}
