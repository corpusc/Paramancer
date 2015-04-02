using UnityEngine;
using System.Collections;



// takes care of the light and particles of the bomb 
public class FlashingLight : MonoBehaviour {
	public bool Visible = true;
	// didn't find where these were set ....so i HAD moved them to private var section 
	public GameObject bombObj;
	public Light Sparks;
	public ParticleSystem ps;

	// private 
	float changeTime = 0f;
	float flashTime = 0.3f;
	Arsenal arse;

	
	
	void Start() {
		changeTime = Time.time + flashTime;
		arse = GameObject.Find("Main Program").GetComponent<Arsenal>();
	}
	
	void Update() {
		if (bombObj.GetComponent<Renderer>().enabled && Visible) {
			if (Time.time > changeTime) {
				changeTime += flashTime;
				GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
				
				if (GetComponent<Light>().enabled) 
					arse.BombBeep(transform.position);
			}
			
			Sparks.enabled = true;
			
			if (!ps.isPlaying) 
				ps.Play();
		}else{
			Sparks.enabled = false;
			GetComponent<Light>().enabled = false;
			
			if (ps.isPlaying) {
				ps.Clear();
				ps.Stop();
			}
		}
	}
}
