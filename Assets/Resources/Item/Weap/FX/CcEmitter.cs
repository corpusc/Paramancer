using UnityEngine;
using System.Collections;



public class CcEmitter {
	public int MinPerSec = 75; // min particles per second 
	public int MaxPerSec = 125; // max " " " 
	public float MinParticleSpeed = 5f;
	public float MaxParticleSpeed = 8f;
	// explosion 
	public float ExplosionSize = 0.4f;
	public float ExplosionSpeed = 8f;

	// private 
	Transform tr;
	Vector3 aim;
	Vector3 pos;



	public void Update(Transform t, bool spiralling) {
		tr = t;
		//aim = -tr.forward;
		//pos = tr.position;

		if (spiralling)
			makeCluster(Color.green, Color.blue, Random.value, 1.5f);
		else
			makeCluster(S.Orange, Color.yellow, Random.Range(0f, 0.5f), 2.5f);
	}


	private void makeCluster(Color a, Color b, float rnd, float dura) {
		int num = (int)((float)Random.Range(MinPerSec, MaxPerSec) * Time.deltaTime); // number of particles this frame 
		num *= 2;

		for (int i = 0; i < num; i++) {
			var has = 45f; // half angle span 
			//var q = Quaternion.Euler(Random.Range(-has, has), Random.Range(-has, has), 0f);
			tr.Rotate(
				new Vector3(
					Random.Range(-has, has), 
					Random.Range(-has, has), 
					0f
				)
			);
			var o = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
			o.transform.position = tr.position;
			o.transform.rotation = tr.rotation;
			//o.transform.position = tr.position;//pos;
			//o.transform.rotation = q;
			var p = o.GetComponent<CcParticle>();
			p.MinSize = 0.5f; // atm, just the 2 types of rockets use this 
			p.MaxSize = 0.8f; // atm, just the 2 types of rockets use this 
			p.MoveVec = -tr.forward /*aim*/*5f;// * Random.Range(MinParticleSpeed, MaxParticleSpeed);// + Random.insideUnitSphere;
			p.Dura = dura;
			p.StartColor = Color.Lerp(a, b, Random.value);
			//p.MidColor = Color.Lerp(Color.green, Color.blue, rnd);
			p.MidColor = Color.Lerp(S.Orange, Color.black, rnd);
			p.MidColorPos = Random.Range(0.4f, 0.5f);
			p.EndColor = Color.clear;
		}
	}


	// FIXME?  maybe this should be consolidated with the bullet impact puffs 
	public void Explode(Vector3 prevPos, Color start, Color end, bool useMidColor = true) {
		int num = 25; // ...num particles per frame 

		for (int i = 0; i < num; i++) {
			var np = (GameObject)GameObject.Instantiate(GOs.Get("CcParticle"));
			// FIXME: so its only a half of sphere, in direction of hit surface normal 
			np.transform.position = prevPos + Random.insideUnitSphere * ExplosionSize; // do NOT use preciseLocation because particles shouldn't spawn inside a wall (bouncing will mess up) 
			var p = np.GetComponent<CcParticle>();
			p.UseMidColor = useMidColor;
			p.MaxSpeed = ExplosionSpeed;
			p.StartColor = Color.Lerp(start, Color.yellow, Random.value);
			p.EndColor = end;
			p.Dura = 2.5f;
			p.OneDScale = -2f;
			p.MinSize = .1f;
			p.MaxSize = .8f;
			p.acceleration = 0.4f;
			p.ParticType = ParticleType.Circle;
		}
	}
}
