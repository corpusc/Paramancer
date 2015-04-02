// THIS WON'T LOOP 

using UnityEngine;
using System.Collections;



public class SoundObjScript : MonoBehaviour {
	void Start() {
		GetComponent<AudioSource>().Play();
	}
	
	void FixedUpdate() {
		if (GetComponent<AudioSource>().time >= GetComponent<AudioSource>().clip.length) 
			Destroy(gameObject);
	}
}
