using UnityEngine;
using System.Collections;

public class FlashlightScript : MonoBehaviour {
	public float flashTime = 0.3f;
	private float changeTime = 0f;
	public Light otherlight;
	public GameObject bombObj;
	public ParticleSystem ps;
	public bool visible = true;
	
	// private 
	Arsenal artillery;
	
	
	
	void Start () {
		changeTime = Time.time + flashTime;
		artillery = GameObject.Find("Main Program").GetComponent<Arsenal>();
	}
	
	void Update () {
		if (bombObj.renderer.enabled && visible) {
			if (Time.time > changeTime) {
				changeTime += flashTime;
				light.enabled = !light.enabled;
				
				if (light.enabled) 
					artillery.BombBeep(transform.position);
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
