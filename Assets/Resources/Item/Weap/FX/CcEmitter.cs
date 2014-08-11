using UnityEngine;
using System.Collections;



public class CcEmitter {
	public int NumPerSplo = 25; // ...per per frame.  splo/splosion/explosion 
	public int MinPerSec = 75; // min particles per second 
	public int MaxPerSec = 125; // max " " " 
	public float MinParticleSpeed = 5f;
	public float MaxParticleSpeed = 8f;
	public float PMR = 1f; // Particle Movement Randomness 
	// explosion 
	public float ExplosionSize = 0.4f;
	public float ExplosionSpeed = 8f;

	// private 
	Vector3 aim;
	Vector3 currPos;



	public CcEmitter() {}
	
	public void Update(Transform tr, bool spiralling) {
		aim =    -tr.forward;
		currPos = tr.position;

		if (spiralling)
			makeCluster(Color.green, Color.blue, Random.value, 1.5f);
		else
			makeCluster(S.Orange, Color.yellow, Random.Range(0f, 0.5f), 2.5f);
	}
	
	private void makeCluster(Color a, Color b, float rnd, float dura) {
		int num = (int)((float)Random.Range(MinPerSec, MaxPerSec) * Time.deltaTime); // number of particles this frame 

		for (int i = 0; i < num; i++) {
			var np = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
			np.transform.position = currPos;
			var p = np.GetComponent<CcParticle>();
			p.MinSize = 0.5f; // atm, just the 2 types of rockets use this 
			p.MaxSize = 0.8f; // atm, just the 2 types of rockets use this 
			p.MoveVec = currPos + aim * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
			p.Dura = dura;
			p.StartColor = Color.Lerp(a, b, Random.value);
			//p.MidColor = Color.Lerp(Color.green, Color.blue, rnd);
			p.MidColor = Color.Lerp(S.Orange, Color.black, rnd);
			p.MidColorPos = Random.Range(0.4f, 0.5f);
			p.EndColor = Color.clear;
		}
	}
	
	// explosion 
	// FIXME?  maybe this should be consolidated with the bullet impact puffs 
	public void Explode(Vector3 prevPos, Color start, Color end, bool useMidColor = true) {
		for (int i = 0; i < NumPerSplo; i++) {
			var np = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
			// FIXME: so its only a half of sphere, in direction of hit surface normal 
			np.transform.position = prevPos + Random.insideUnitSphere * ExplosionSize; // do NOT use preciseLocation because particles shouldn't spawn inside a wall (bouncing will mess up) 
			var p = np.GetComponent<CcParticle>();
			p.UseMidColor = useMidColor;
			p.MaxSpeed = ExplosionSpeed;
			p.StartColor = Color.Lerp(start, Color.yellow, Random.value);
			p.EndColor = end;
			p.Dura = 2.5f;
			p.f = -2f;
			p.MinSize = .1f;
			p.MaxSize = .3f;
			p.acceleration = 0.1f;
			p.ParticType = ParticleType.Circle;
		}
	}
}
