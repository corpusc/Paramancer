using UnityEngine;
using System.Collections;

public class LocalUser : MonoBehaviour {
	// freelook
	public bool LookInvert = false;
	public float LookSensitivity = 2f;
	
	
	
	void Start() {
		// load user config
		LookInvert = PlayerPrefs.GetInt("InvertY", 0) == 1;
		LookSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
	}
}
