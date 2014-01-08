using UnityEngine;
using System.Collections;

public class LevelLightScript : MonoBehaviour {
	
	
	public bool litInPitchBlack = false;
	
	public bool disableifNotPitchBlack = false;
	
	// Use this for initialization
	void Start () {
		
		if (GameObject.Find("Main Program") != null){
			if (GameObject.Find("Main Program").GetComponent<CcNet>().ModeCfg.pitchBlack){
				if (!litInPitchBlack) light.enabled = false;
			}else{
				if (disableifNotPitchBlack) light.enabled = false;
			}
		}
	}
	
}
