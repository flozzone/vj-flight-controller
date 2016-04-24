using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;


public class ArduinoGyro : ArduinoBase {
	public const float RAD_TO_DEG = 180f / Mathf.PI;
	public static Vector3 INVALID_VALUE = Vector3.zero;

	public float LEFT_RIGHT_SCALER = 0.05f;
	public float SERVO_POSITION_SCALER = 30f;

	private float JETPACK_FORCE = 10f;
	private float AERODYNAMICS_FORCE_SCALER = 0.1f;
	private float YAW_STEER_SCALER = 0.01f;

	public bool _aerodynamics = true;
	public bool _jetpack = false;
	public bool _steerWithYaw = true;
	public GameObject _controlOrientation;
	public GameObject _controlPosition;

	private ServoControllerClient _servoController = null;

	private bool _isInverted = false;
	private Vector3 _initialOrientation = INVALID_VALUE;
	private Vector3 _cubeRotation = Vector3.zero;

	private Quaternion _initialQuaternion = Quaternion.identity;

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

	private Quaternion ParseQuaternion(string[] messageParts) {
		Quaternion ret = Quaternion.identity;

		if (messageParts.Length == 5 && "q".Equals(messageParts[0])) {
			ret = new Quaternion (
				float.Parse (messageParts [2]),
				float.Parse (messageParts [3]),
				float.Parse (messageParts [4]),
				float.Parse (messageParts [1])
				);

			return ret * _initialQuaternion;
		}

		return ret;
	}

	private Vector3 ReadYawPitchRollFromArduino() {
		return ParseYawPitchRoll(GetBufferedMessage("g"));
	}

	private Quaternion ReadQuaternionFromArduino() {
		return ParseQuaternion(GetBufferedMessage("q"));
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

	new void Start() {
		base.Start();

		GyroSettings gyroSettings = GyroSettingsParser.parseSettings();
		this.LEFT_RIGHT_SCALER = gyroSettings.RollSensitivity;

		base.InitSerial(gyroSettings);

		this._servoController = new ServoControllerClient();
		this.SetInverted(false);

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
			ApplyLeftRightForce(yawPitchRoll.z);

		/*else
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
		*/
	}

	new public void OnDestroy() {
		base.OnDestroy();
		this._servoController.Close();
	}

	public void SetInverted(bool inverted) {
		this._isInverted = inverted;
	}

	protected void ApplyLeftRightForce(float rollAngle) {
		float magnitude = rollAngle * LEFT_RIGHT_SCALER;
		int servoPosition = (int)Mathf.Round(512 + magnitude * SERVO_POSITION_SCALER);

		Debug.Log(magnitude);

		if (this._isInverted)
			magnitude = -magnitude;

		/*
		if (magnitude < 0)
			_servoController.PullToLeft();
		else
			_servoController.PullToRight();
		*/

		_servoController.EventSetServoPosition(servoPosition);

		Vector3 pos = _controlPosition.transform.position;
		pos.z += magnitude;
		_controlPosition.transform.position = pos;
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

		if (ParseJetPack(GetBufferedMessage("j")) | _jetpack) {
			this.GetRigidBody().AddForce(_controlOrientation.transform.forward * JETPACK_FORCE);
		}
	}
}
