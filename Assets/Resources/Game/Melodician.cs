using UnityEngine;
using System.Collections;



public class Melodician {
	AudioSource aSrc = new AudioSource();



	public Melodician() {
		Debug.Log("Melodician() constructor");
		aSrc.clip = Sfx.Get("boosh");
		aSrc.volume = 1f;
	}
	
	public void Update() {
		if (CcInput.Holding(UserAction.Activate)) {
			Debug.Log("playin' dat sweet ass muzack");
			aSrc.Play();
		}	
	}
}
