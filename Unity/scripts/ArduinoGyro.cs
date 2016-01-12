using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;


public class ArduinoGyro : ArduinoBase {
	public const float RAD_TO_DEG = 180f / Mathf.PI;
	public static Vector3 INVALID_VALUE = Vector3.zero;

	private float JETPACK_FORCE = 10f;
	private float AERODYNAMICS_FORCE_SCALER = 0.1f;
	private float YAW_STEER_SCALER = 0.01f;

	public bool _aerodynamics = true;
	public bool _jetpack = false;
	public bool _steerWithYaw = true;
	public GameObject _controlOrientation;
	public GameObject _controlPosition;
		
	private Vector3 _initialOrientation = INVALID_VALUE;
	private Vector3 _cubeRotation = Vector3.zero;

	protected float GetSectionCoefficient (float orientationDegrees) {
		return (Mathf.Sin (orientationDegrees / RAD_TO_DEG)) / (Mathf.Cos (orientationDegrees / RAD_TO_DEG));
	}

	private Vector3 ParseYawPitchRoll(string[] messageParts) {
		Vector3 ret = INVALID_VALUE;

		if (messageParts.Length == 4 && "g".Equals(messageParts[0])) {
			ret = new Vector3 (
				float.Parse (messageParts [2]),
				-float.Parse (messageParts [1]),
				-float.Parse (messageParts [3])
			);

			return ret - _initialOrientation;
		}

		return ret;
	}

	private Vector3 ReadYawPitchRollFromArduino() {
		return ParseYawPitchRoll(RequestDataFromArduino('g'));
	}

	private bool ParseJetPack(string[] messageParts) {
		if (messageParts.Length == 2 && "j".Equals(messageParts[0]))
			return "1".Equals(messageParts[1]);
		return false;
	}

	private Vector3 GetXzVelocity() {
		Vector3 velocity = GetRigidBody().velocity;
		velocity.y = 0;
		velocity = _controlPosition.transform.InverseTransformDirection(velocity);
		return velocity;
	}

	void Start() {
		base.InitSerial();

		//Save initial orientation
		while (_initialOrientation.Equals(INVALID_VALUE))
			_initialOrientation = ReadYawPitchRollFromArduino();
	}

	private void Reset(Vector3 currentYawPitchRoll) {
		_initialOrientation = _initialOrientation + currentYawPitchRoll;
		_controlPosition.transform.eulerAngles = Vector3.zero;
		_cubeRotation = Vector3.zero;
	}

	void FixedUpdate() {
		Vector3 yawPitchRoll = Vector3.zero;
		Vector3 xzVelocity = Vector3.zero;

		// Apply orientation to the object
		yawPitchRoll = ReadYawPitchRollFromArduino();
		if (yawPitchRoll.Equals(INVALID_VALUE))
			return;

		if (Input.GetKeyDown(KeyCode.R)) {
			Reset(yawPitchRoll);
			yawPitchRoll = Vector3.zero;
		} else
			_controlOrientation.transform.localEulerAngles = yawPitchRoll;

		// Rotate container to front-face flight direction in X-Z plane as player doesn't move in it.
		if (_steerWithYaw) {
			_cubeRotation.y += yawPitchRoll.y * YAW_STEER_SCALER;
			_controlPosition.transform.localEulerAngles = _cubeRotation;
		}

		// Always fly frontface
		GetRigidBody().velocity = _controlOrientation.transform.forward * GetRigidBody().velocity.magnitude;

		// Apply forces
		if (_jetpack)
			ApplyJetpackForce(yawPitchRoll);
		if (_aerodynamics)
			ApplyAerodynamicForce(yawPitchRoll);
	}

	protected void ApplyForceRelativeToBodyDirection(Vector3 force, Vector3 yawPitchRoll) {
		Vector3 directional_Force = _controlOrientation.transform.InverseTransformDirection(force);
		this.GetRigidBody().AddForce(directional_Force);
	}

	private void ApplyAerodynamicForce(Vector3 yawPitchRoll) {
		Vector3 force = Vector3.zero;

		// TODO: Get actual wind direction by using velocity
		// Move forward on Pitch
		force.z = -AERODYNAMICS_FORCE_SCALER * GetRigidBody().velocity.y * GetSectionCoefficient(yawPitchRoll.x);
		// Move sideways on Roll
		force.x = AERODYNAMICS_FORCE_SCALER * GetRigidBody().velocity.y * GetSectionCoefficient(yawPitchRoll.z);

		ApplyForceRelativeToBodyDirection(force, yawPitchRoll);
	}

	private void ApplyJetpackForce(Vector3 yawPitchRoll) {
		Vector3 force = new Vector3(0, 0, -JETPACK_FORCE);

		if (ParseJetPack(RequestDataFromArduino('j')) | _jetpack) {
			this.GetRigidBody().AddForce(_controlOrientation.transform.forward * JETPACK_FORCE);
		}
	}
}
