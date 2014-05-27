using UnityEngine;
using System.Collections;



// takes care of the light and particles of the bomb 
public class FlashlightScript : MonoBehaviour {
	public bool Visible = true;
	// didn't find where these were set ....so i HAD moved them to private var section 
	public GameObject bombObj;
	public Light otherlight; // dunno what this means 
	public ParticleSystem ps;

	// private 
	float changeTime = 0f;
	float flashTime = 0.3f;
	Arsenal arse;

	
	
	void Start () {
		changeTime = Time.time + flashTime;
		arse = GameObject.Find("Main Program").GetComponent<Arsenal>();
	}
	
	void Update () {
		if (bombObj.renderer.enabled && Visible) {
			if (Time.time > changeTime) {
				changeTime += flashTime;
				light.enabled = !light.enabled;
				
				if (light.enabled) 
					arse.BombBeep(transform.position);
			}
			
			otherlight.enabled = true;
			
			if (!ps.isPlaying) 
				ps.Play();
		}else{
			otherlight.enabled = false;
			light.enabled = false;
			
			if (ps.isPlaying) {
				ps.Clear();
				ps.Stop();
			}
		}
	}
}
