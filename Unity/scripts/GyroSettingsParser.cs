using UnityEngine;
using System.IO;

public class GyroSettingsParser {
	private static readonly string GYRO_SETTINGS_FILE = "gyroSettings.json";

	public static GyroSettings parseSettings() {
		return JsonUtility.FromJson<GyroSettings>(File.ReadAllText(GYRO_SETTINGS_FILE));
	}
}

[System.Serializable]
public class GyroSettings : ArduinoSettings {
	[SerializeField]
	private float _rollSensitivity;

	public float RollSensitivity {
		get {return _rollSensitivity;}
	}
}
