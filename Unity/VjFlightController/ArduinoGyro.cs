using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class ArduinoGyro : MonoBehaviour {
	private static float RAD_TO_DEG = 180f / Mathf.PI;

	public string comPortName;
	public GameObject ControlOrientation;
	public GameObject ControlPosition;

	private SerialPort _serialPort;
	private Vector3 _initialOrientation = Vector3.zero;
	private Rigidbody _rigidBody;

	private Vector3 ParseYawPitchRoll(float yaw, float pitch, float roll) {
		return new Vector3 (pitch, yaw, roll) - _initialOrientation;
	}

	private float GetSectionCoefficient (float orientationDegrees) {
		return (Mathf.Sin (orientationDegrees / RAD_TO_DEG)) / (Mathf.Cos (orientationDegrees / RAD_TO_DEG));
	}

	private Vector3 GetCurrentOrientation() {
		Vector3 ret = Vector3.zero;

		_serialPort.Write ("g");
		string message = _serialPort.ReadLine();
		string[] messageParts = message.Split ('\t');

		if (messageParts.Length == 4 && messageParts[0].Equals("ypr")) {
			// Orientation on debug object
			ret = ParseYawPitchRoll(
				float.Parse (messageParts [1]),
				float.Parse (messageParts [2]),
				float.Parse (messageParts [3]));
		}

		return ret;
	}
	// Use this for initialization
	void Start () {
		bool initialized = false;

		_rigidBody = GetComponent<Rigidbody>();

		// Create a new SerialPort object.
		_serialPort = new SerialPort (comPortName, 115200, Parity.None, 8, StopBits.One);
		_serialPort.Open ();
		while (!initialized) {
			_serialPort.Write ("g");
			string message = _serialPort.ReadLine();
			string[] messageParts = message.Split ('\t');

			if (messageParts.Length == 4 && messageParts[0].Equals("ypr")) {
				_initialOrientation = ParseYawPitchRoll(
					float.Parse (messageParts [1]),
					float.Parse (messageParts [2]),
					float.Parse (messageParts [3]));
				initialized = true;
			}
		}
	}

	// Update is called once per frame
	void Update() {
		Vector3 velocity = Vector3.zero;
		float direction = 0;

		velocity = _rigidBody.velocity;
		velocity.y = 0;

		// Apply orientation
		ControlOrientation.transform.eulerAngles = GetCurrentOrientation();

		// Rotate container to front-face flight direction in X-Z plane as player doesn't move in it.
		direction = Mathf.Sign(velocity.x) * Mathf.Acos(Vector3.Dot(Vector3.forward, velocity.normalized));
		ControlPosition.transform.eulerAngles = new Vector3(0, direction * RAD_TO_DEG, 0);
	}

	// FixedUpdate to simulate aerodynamics
	void FixedUpdate() {
		float orientationForceScaler = 0.1f;
		Vector3 force = Vector3.zero;

		Vector3 currentOrientation;
		Vector3 frontFace;
		Vector3 velocity;

		currentOrientation = GetCurrentOrientation();

		// Get velocity in X-Z Plane
		velocity = _rigidBody.velocity;
		velocity.y = 0;

		// TODO: Calcultae all forces acting on the player
		force.z = - orientationForceScaler * _rigidBody.velocity.y * GetSectionCoefficient(currentOrientation.x);
		force.x = orientationForceScaler * _rigidBody.velocity.y * GetSectionCoefficient(currentOrientation.z);
		force.x += orientationForceScaler * velocity.magnitude * Mathf.Sin(currentOrientation.y / RAD_TO_DEG);

		// Apply forces
		frontFace = velocity.normalized + (Quaternion.AngleAxis(currentOrientation.y, Vector3.up) * Vector3.forward);
		_rigidBody.AddForce(frontFace.normalized * force.magnitude);
	}
}
