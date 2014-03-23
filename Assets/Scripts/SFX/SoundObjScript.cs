// THIS WON'T LOOP 

using UnityEngine;
using System.Collections;



public class SoundObjScript : MonoBehaviour {
	void Start() {
		audio.Play();
	}
	
	void FixedUpdate() {
		if (audio.time >= audio.clip.length) 
			Destroy(gameObject);
	}
}
