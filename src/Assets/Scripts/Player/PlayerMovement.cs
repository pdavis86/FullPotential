using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	const float _speed = 5f;
	const float _lookSensitivity = 3f;
	const float _cameraRotationLimit = 85f;

	private Rigidbody _rb;
	private Camera _cam;

	private Vector3 _velocity;
	private Vector3 _rotation;
	private float _cameraRotationX;
	private float _currentCameraRotationX = 0f;

	private void Start()
    {
		_rb = GetComponent<Rigidbody>();
		_cam = transform.Find("PlayerCamera").GetComponent<Camera>();
	}

    void Update()
    {
		var moveX = transform.right * Input.GetAxis("Horizontal");
		var moveZ = transform.forward * Input.GetAxis("Vertical");
		_velocity = (moveX + moveZ) * _speed;

		_rotation = new Vector3(0f, Input.GetAxisRaw("Mouse X"), 0f) * _lookSensitivity;

		_cameraRotationX = Input.GetAxisRaw("Mouse Y") * _lookSensitivity;
	}

	void FixedUpdate()
	{
		PerformMovement();
		PerformRotation();
	}

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
		if (_cam != null)
		{
			_currentCameraRotationX -= _cameraRotationX;
			_currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);
			_cam.transform.localEulerAngles = new Vector3(_currentCameraRotationX, 0f, 0f);
		}
	}

}
