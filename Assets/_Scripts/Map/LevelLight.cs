using UnityEngine;
using System.Collections;

public class LevelLight : MonoBehaviour {
	public bool litInPitchBlack = false;
	public bool disableIfNotPitchBlack = false;
	

	
	void Start () {
		if (GameObject.Find("Main Program") != null) {
			if (GameObject.Find("Main Program").GetComponent<CcNet>().ModeCfg.pitchBlack) {
				if (!litInPitchBlack) 
					light.enabled = false;
			}else{
				if (disableIfNotPitchBlack) 
					light.enabled = false;
			}
		}
	}
}
