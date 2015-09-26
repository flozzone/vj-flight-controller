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

	private string[] RequestDataFromArduino(char datatype) {
		string[] ret = {};
		string message = "";
		string datatypeStr = datatype + "";

		if (!_serialPort.IsOpen)
			return ret;

		_serialPort.Write (datatypeStr);
		message = _serialPort.ReadLine();

		Debug.Log("Requested: " + datatype + " Got message: " + message);

		return message.Split ('\t');
	}

	private Vector3 GetArduinoData(char datatype) {
		Vector3 ret = Vector3.zero;

		string[] messageParts = RequestDataFromArduino(datatype);

		if (messageParts.Length == 4 && messageParts[0].Equals(datatype.ToString())) {
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
		_rigidBody = GetComponent<Rigidbody>();

		// Create a new SerialPort object.
		_serialPort = new SerialPort (comPortName, 115200, Parity.None, 8, StopBits.One);
		_serialPort.Open ();
	}

	// FixedUpdate to simulate aerodynamics
	void FixedUpdate() {
		float orientationForceScaler = 0.1f;
		float accelerationScaler = 0.001f;
		Vector3 force = Vector3.zero;
		float direction = 0;

		Vector3 currentOrientation;
		Vector3 frontFace;
		Vector3 velocity;

		currentOrientation = GetArduinoData('g');
		ControlOrientation.transform.eulerAngles = currentOrientation;

		// Get velocity in X-Z Plane
		velocity = _rigidBody.velocity;
		velocity.y = 0;

		// Rotate container to front-face flight direction in X-Z plane as player doesn't move in it.
		direction = Mathf.Sign(velocity.x) * Mathf.Acos(Vector3.Dot(Vector3.forward, velocity.normalized));
		ControlPosition.transform.eulerAngles = new Vector3(0, direction * RAD_TO_DEG, 0);

		// Apply air resistance forces
		force.z = - orientationForceScaler * _rigidBody.velocity.y * GetSectionCoefficient(currentOrientation.x);
		force.x = orientationForceScaler * _rigidBody.velocity.y * GetSectionCoefficient(currentOrientation.z);
		force.x += orientationForceScaler * velocity.magnitude * Mathf.Sin(currentOrientation.y / RAD_TO_DEG);
		frontFace = velocity.normalized + (Quaternion.AngleAxis(currentOrientation.y, Vector3.up) * Vector3.forward);
		_rigidBody.AddForce(frontFace.normalized * force.magnitude);

		// TODO: Calcultae all forces acting on the player

		// Apply acceleration forces
		_rigidBody.AddForce(GetArduinoData('a') * accelerationScaler);
	}
}
