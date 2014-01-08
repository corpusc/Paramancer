using UnityEngine;
using System.Collections;

public class SoundObjScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
		if (audio.time>=audio.clip.length) Destroy(gameObject);
	}
}
