using UnityEngine;
using System.Collections;


// things only local users need 
public class LocalUser : MonoBehaviour {
	// *** AIMING ***   

	// simplest scenario uses traditional controls (mouse, gamepad thumbstick) 
	// which simultaneously controls: 
	//		* eyes 
	//		* chest 
	//		* wielded-item (such as guns) 

	// ....when wearing VR headsets, your up/down eye aim will be totally independent, 
	// while your side to side eye/head motions will be relative to chest aim. 

	// ....when using motion controller such as a Hydra, your wielded-items can aim 
	// totally independent of your eye OR chest 

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
