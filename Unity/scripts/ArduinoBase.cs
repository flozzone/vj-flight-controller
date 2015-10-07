using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class ArduinoBase : MonoBehaviour {	
	public string comPortName;
	public int _SerialReadTimeout = 10;
	
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
		// Create a new SerialPort object.
		_serialPort = new SerialPort (comPortName, 115200);
		_serialPort.ReadTimeout = _SerialReadTimeout;
		_serialPort.Open ();
		_rigidBody = GetComponent<Rigidbody>();
	}
}

