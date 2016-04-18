using UnityEngine;
using System.IO;

public class ArduinoSettingsParser {
	private static readonly string ARDUINO_SETTINGS_FILE = "gyroSettings.json";

	public static ArduinoSettings parseSettings() {
		return JsonUtility.FromJson<ArduinoSettings>(File.ReadAllText(ARDUINO_SETTINGS_FILE));
	}
}

[System.Serializable]
public struct ArduinoSettings {
	[SerializeField]
	private string _serialPort;
	[SerializeField]
	private int _readTimeout;

	public string SerialPort {
		get {return _serialPort;}
	}

	public int ReadTimeout {
		get{return _readTimeout;}
	}
}
