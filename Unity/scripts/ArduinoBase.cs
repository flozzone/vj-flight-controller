using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

public class BufferedMessage {
	public string[] _msgParts;
	public BufferedMessage(string[] msgParts) {
		_msgParts = msgParts;
	}
}

public class ArduinoBase : MonoBehaviour {	
	private System.Threading.Thread _serialThread = null;
	private SerialPort _serialPort;
	private Rigidbody _rigidBody;

	private Dictionary<string, string[]> _msgBuffer = new Dictionary<string, string[]>();

	protected string[] RequestDataFromArduino(char datatype) {
		string message = "";

		if (_serialPort != null && _serialPort.IsOpen) {
			try {
				_serialPort.Write("" + datatype);
				message = _serialPort.ReadLine();
				Debug.Log("Read: " + message + " from serial");
			} catch (System.IO.IOException) {
				Debug.Log("IOException on ReadLine. About to close?");
			} catch (System.TimeoutException) {
				Debug.Log("Read timed out, resetting.");
				_serialPort.Close();
				InitSerial();
			}
		}
		return message.Split ('\t');
	}

	protected Rigidbody GetRigidBody() {
		return _rigidBody;
	}

	public void Start() {
		InitSerial();

		_rigidBody = GetComponent<Rigidbody>();

		// Spawn thread to read from Arduino
		_serialThread = new Thread(_readSerial);
		_serialThread.Start();
	}

	protected void InitSerial() {
		ArduinoSettings settings = ArduinoSettingsParser.parseSettings();

		Debug.Log("Got settings: " + settings.SerialPort + " " + settings.ReadTimeout);

		// Create a new SerialPort object.
		_serialPort = new SerialPort (settings.SerialPort, 115200);
		_serialPort.ReadTimeout = settings.ReadTimeout;
		_serialPort.Open ();
	}

	private void _readSerial() {
		string[] msg = null;

		while (_serialPort.IsOpen) {
			msg = RequestDataFromArduino('g');
			lock(_msgBuffer) {
				_msgBuffer[msg[0]] = msg;
			}
		}

		Debug.Log("_readSerial ended!");
	}

	protected string[] GetBufferedMessage(string dataType) {
		lock (_msgBuffer) {
			if (_msgBuffer.ContainsKey(dataType))
				return _msgBuffer[dataType];
		}
		return new string[]{"u"};
	}

	public void OnDestroy() {
		Debug.Log("Destroying Arduino");
		if (_serialPort != null && _serialPort.IsOpen) {
			_serialPort.Close();
			_serialThread.Abort();
		}
	}
}

