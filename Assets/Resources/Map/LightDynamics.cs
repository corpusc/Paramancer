using UnityEngine;
using System.Collections;



public class LightDynamics : MonoBehaviour {
	public bool litInPitchBlack = false;
	public bool OnlyPitchBlack = false;
	public bool MovingRandomly = false;
	public bool Flickering = false;
	public int FlickerTimer = 30; // also random 
	public int DirChangeTime = 500; // random, actually, that's the average 
	public float MaxSpeed = 2f;
	public float MaxWanderDistance = 80f;
	
	// private 
	Vector3 dir = new Vector3(0f, 0f, 0f);
	Vector3 begin;
	bool dontFlicker = false;
	
	
	
	void Start() {
		var mp = GameObject.Find("Main Program");

		if (mp != null) {
			if (mp.GetComponent<CcNet>().CurrMatch.pitchBlack) {
				if (!litInPitchBlack) 
					light.enabled = false;
			}else{
				if (OnlyPitchBlack){
					light.enabled = false;
					dontFlicker = true;
				}
			}
			
			begin = light.transform.position;
		}
	}
	
	void Update() {
		if (MovingRandomly) {
			if (Random.Range(0, DirChangeTime) == 0) {
				dir.x = Random.Range(-MaxSpeed, MaxSpeed);
				dir.y = Random.Range(-MaxSpeed, MaxSpeed);
				dir.z = Random.Range(-MaxSpeed, MaxSpeed);
			}
			
			light.transform.Translate(dir.x * Time.deltaTime, dir.y * Time.deltaTime, dir.z * Time.deltaTime);
			if (Vector3.Distance(light.transform.position, begin) > MaxWanderDistance) 
				light.transform.position = begin; // revert to start (can't use "start" as var name) 
		}
		
		if (Flickering && !dontFlicker) {
			if (Random.Range(0, FlickerTimer) == 0) 
				light.enabled = !light.enabled;
		}
	}
}
