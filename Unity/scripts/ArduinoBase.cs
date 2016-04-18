using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class ArduinoBase : MonoBehaviour {	
	private SerialPort _serialPort;
	private Rigidbody _rigidBody;

	protected string[] RequestDataFromArduino(char datatype) {
		string[] ret = {};
		string message = "";
		
		if (_serialPort == null || !_serialPort.IsOpen)
			return ret;

		lock (_serialPort) {
			_serialPort.Write (datatype + "");
			message = _serialPort.ReadLine();
		}
		
		Debug.Log("Requested: " + datatype + " Got message: " + message);
		
		return message.Split ('\t');
	}

	protected Rigidbody GetRigidBody() {
		return _rigidBody;
	}

	protected void InitSerial() {
		ArduinoSettings settings = ArduinoSettingsParser.parseSettings();

		Debug.Log("Got settings: " + settings.SerialPort + " " + settings.ReadTimeout);

		// Create a new SerialPort object.
		_serialPort = new SerialPort (settings.SerialPort, 115200);
		_serialPort.ReadTimeout = settings.ReadTimeout;
		_serialPort.Open ();
		_rigidBody = GetComponent<Rigidbody>();
	}
}

