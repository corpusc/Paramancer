using UnityEngine;
using System.Collections;

public class LevelLight : MonoBehaviour {
	public bool litInPitchBlack = false;
	public bool OnlyPitchBlack = false;
	public bool MovingRandomly = false;
	public bool Flickering = false;
	public int DirChangeTime = 500; // random, actually, that's the average
	public float MaxSpeed = 2f;
	public int FlickerTimer = 30; // also random
	public float MaxWanderDistance = 80f;
	
	// private
	Vector3 dir = new Vector3(0f, 0f, 0f);
	Vector3 p_origin;
	bool dontFlicker = false;
	
	
	
	void Start () {
		if (GameObject.Find("Main Program") != null) {
			if (GameObject.Find("Main Program").GetComponent<CcNet>().CurrMatch.pitchBlack) {
				if (!litInPitchBlack) 
					light.enabled = false;
			}else{
				if (OnlyPitchBlack){
					light.enabled = false;
					dontFlicker = true;
				}
			}
			
			p_origin = light.transform.position;
		}
	}
	
	void Update() {
		if (MovingRandomly) {
			if(Random.Range(0, DirChangeTime) == 0) {
				dir.x = Random.Range(-MaxSpeed, MaxSpeed);
				dir.y = Random.Range(-MaxSpeed, MaxSpeed);
				dir.z = Random.Range(-MaxSpeed, MaxSpeed);
			}
			
			light.transform.Translate(dir.x * Time.deltaTime, dir.y * Time.deltaTime, dir.z * Time.deltaTime);
			if (Vector3.Distance(light.transform.position, p_origin) > MaxWanderDistance) 
				light.transform.position = p_origin; //tele to start pos
		}
		
		if (Flickering && !dontFlicker) {
			if (Random.Range(0, FlickerTimer) == 0) 
				light.enabled = !light.enabled;
		}
	}
}
