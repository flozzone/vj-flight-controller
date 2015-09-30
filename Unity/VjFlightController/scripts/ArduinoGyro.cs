using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;


public class ArduinoGyro : ArduinoBase {
	public const float RAD_TO_DEG = 180f / Mathf.PI;

	private float orientationForceScaler = 1f;

	public bool _aerodynamics = true;
	public bool _steerWithYaw = true;
	public GameObject _controlOrientation;
	public GameObject _controlPosition;
	public GameObject _frontFace;
		
	private Vector3 _initialOrientation = Vector3.zero;

	protected float GetSectionCoefficient (float orientationDegrees) {
		return (Mathf.Sin (orientationDegrees / RAD_TO_DEG)) / (Mathf.Cos (orientationDegrees / RAD_TO_DEG));
	}

	private Vector3 ParseYawPitchRoll(string[] messageParts) {
		Vector3 ret = Vector3.zero;

		if (messageParts.Length == 4 && "g".Equals(messageParts[0]))
			ret = new Vector3 (
				float.Parse (messageParts [2]),
				float.Parse (messageParts [1]),
				float.Parse (messageParts [3])
			);

		return ret - _initialOrientation;
	}

	private Vector3 GetXzVelocity() {
		Vector3 velocity = GetRigidBody().velocity;
		velocity.y = 0;
		return velocity;
	}

	void Start() {
		base.InitSerial();

		//Save initial orientation
		while (_initialOrientation.Equals(Vector3.zero))
			_initialOrientation = ParseYawPitchRoll(RequestDataFromArduino('g'));

	}

	void FixedUpdate() {
		Vector3 yawPitchRoll = Vector3.zero;
		Vector3 xzVelocity = GetXzVelocity();
		float yAngleRad = 0f;

		// Apply orientation to the object
		yawPitchRoll = ParseYawPitchRoll(RequestDataFromArduino('g'));
		_controlOrientation.transform.localEulerAngles = yawPitchRoll;

		// Rotate container to front-face flight direction in X-Z plane as player doesn't move in it.
		if (!xzVelocity.Equals(Vector3.zero))
		yAngleRad = Mathf.Sign(xzVelocity.x) * Mathf.Acos(Vector3.Dot(Vector3.forward, xzVelocity.normalized));
		_controlPosition.transform.eulerAngles = new Vector3(0, yAngleRad * RAD_TO_DEG, 0);

		// Apply forces
		if (_steerWithYaw)
			ApplySteerWithYaw(yawPitchRoll);
		ApplyJetpackForce(yawPitchRoll);
		if (_aerodynamics)
			ApplyAerodynamicForce(yawPitchRoll);
	}

	protected void ApplyForceRelativeToBodyDirection(Vector3 force, Vector3 yawPitchRoll) {
		Vector3 frontFace = Quaternion.AngleAxis(yawPitchRoll.y, Vector3.up) * Vector3.forward;
		Vector3 absoluteForce = _controlOrientation.transform.InverseTransformVector(force);
		absoluteForce.z *= -1;
		this.GetRigidBody().AddForce(absoluteForce);
	}

	private void ApplySteerWithYaw(Vector3 yawPitchRoll) {
		Vector3 force = Vector3.zero;
		Vector3 xzVelocity = GetXzVelocity();

		// Fly curve on Yaw
		force.x += orientationForceScaler * xzVelocity.magnitude * Mathf.Sin(yawPitchRoll.y / RAD_TO_DEG);
		
		ApplyForceRelativeToBodyDirection(force, yawPitchRoll);
	}

	private void ApplyAerodynamicForce(Vector3 yawPitchRoll) {
		Vector3 force = Vector3.zero;

		// Move forward on Pitch
		force.z = orientationForceScaler * GetRigidBody().velocity.y * GetSectionCoefficient(yawPitchRoll.x);
		// Move sideways on Roll
		force.x = orientationForceScaler * GetRigidBody().velocity.y * GetSectionCoefficient(yawPitchRoll.z);
	

		ApplyForceRelativeToBodyDirection(force, yawPitchRoll);
	}

	private void ApplyJetpackForce(Vector3 yawPitchRoll) {
		Vector3 force = Vector3.zero;

		if (Input.GetKey(KeyCode.Space))
			force.z = -10f;

		ApplyForceRelativeToBodyDirection(force, yawPitchRoll);
	}
}
