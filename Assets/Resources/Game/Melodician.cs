using UnityEngine;
using System.Collections;



public class Melodician {
	AudioSource aSrc = new AudioSource();



	public Melodician() {
		Debug.Log("Melodician() constructor");
		Debug.Log("aSrc.clip = Sfx.Get(\"boosh\");");
		//aSrc.volume = 1f;
	}
	
	public void Update() {
		if (CcInput.Started(UserAction.Activate)) {
			Debug.Log("aSrc.Play();");
			//aSrc.Play();
		}	
	}
}
