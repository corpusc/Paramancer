using UnityEngine;
using System.Collections;



public class LocalUser : MonoBehaviour {
	// *** FREELOOK / AIMING ***
	// could be...
	// (A) mouse movement
	// (B) gamepad thumbstick
	// (C) Hydra aiming/angle
	// (D) God knows what the future brings!!!
	// this setting is irrelevant for VR HMDs.  they need to precisely match head movements 
	public bool LookInvert = false;
	public float LookSensitivity = 2f;
	public float FOV;
	
	
	
	void Start() {
		// load user config 
		LookInvert = PlayerPrefs.GetInt("InvertY", 0) == 1;
		LookSensitivity = PlayerPrefs.GetFloat("LookSensitivity", 2f);
		FOV = PlayerPrefs.GetFloat("FOV", 90f);
	}
}
