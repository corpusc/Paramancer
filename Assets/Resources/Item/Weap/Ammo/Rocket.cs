using UnityEngine;
using System.Collections;



public class Rocket : MonoBehaviour {
	public GameObject particle;
	public float FlightSpeed = 120f; // accelerates to this 
	public float BaseFlightSpeed = 40f; // starts at this speed 
	public int MinPtsPerSecond = 75; // pts == particles 
	public int MaxPtsPerSecond = 125;
	public int ExplosionParticles = 200;
	public float ExplosionSize = 0.4f;
	public float ForwardOffset = 1f; // prevents running into your own rocket when you have lags (FIXME: doesn't work....might need to filter out rocket owner's body ) 
	public float Turn = 300f;
	public bool Turning = false;
	public float MinParticleSpeed = 5f;
	public float MaxParticleSpeed = 8f;
	public float PMR = 1f; // Particle Movement Randomness 
	public float ExplosionSpeed = 8f;
	public NetworkViewID viewID;
	public NetworkViewID shooterID;

	// private 
	CcNet net;
	Vector3 lastPos;
	Vector3 origFwd; // the original forward direction 
	Vector3 origUp; // the original up direction 
	float life = 10f;
	float maxLife;

	
	
	void Start() {
		maxLife = life;
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		lastPos = transform.position;
		origFwd = transform.forward;
		origUp = transform.up;
		transform.position += transform.forward * ForwardOffset;
	}
	
	void Update() {
		if (enabled) {
			if (Turning) {
				var turnVec = Vector3.Normalize(Vector3.Lerp(origFwd, origUp, Mathf.Lerp(0.3f, 0.8f, (maxLife - life) / maxLife)));
				transform.forward = Quaternion.AngleAxis(life * Turn, origFwd) * turnVec;
			}

			Vector3 moveForward = (transform.forward * Time.deltaTime) * (FlightSpeed * (maxLife - life) / maxLife + BaseFlightSpeed);
			transform.position += moveForward;
			int num = (int)((float)Random.Range(MinPtsPerSecond, MaxPtsPerSecond) * Time.deltaTime); // number of particles this frame 

			if (Turning) {
				for (int i = 0; i < num; i++) {
					var np = (GameObject)GameObject.Instantiate(particle);
					float offMagn = moveForward.magnitude / 2f;
					Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
					np.transform.position = transform.position + offset;
					var p = np.GetComponent<CcParticle>();
					p.MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					p.life = 1.5f;
					p.StartColor = Color.Lerp(Color.green, Color.blue, Random.value);
					p.UseMidColor = true;
					p.MidColor = Color.Lerp(Color.green, Color.blue, Random.value);
					p.MidColorPos = Random.Range(0.4f, 0.5f);
					p.EndColor = Color.clear;
				}
			}else{
				for (int i = 0; i < num; i++) {
					var np = (GameObject)GameObject.Instantiate(particle);
					float offMagn = moveForward.magnitude / 2f;
					Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
					np.transform.position = transform.position + offset;
					var p = np.GetComponent<CcParticle>();
					p.MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					p.life = 2.5f;
					p.StartColor = Color.Lerp(S.Orange, Color.yellow, Random.value);
					p.UseMidColor = true;
					p.MidColor = Color.Lerp(S.Orange, Color.black, Random.Range(0f, 0.5f));
					p.MidColorPos = Random.Range(0.4f, 0.6f);
					p.EndColor = Color.clear;
				}
			}

			var hitInfo = new RaycastHit();
			int layerMask = 1;   //(1<<0);
			var rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				detonateMaybe(hitInfo.point, hitInfo.normal);
			}
			layerMask = 1<<8;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				detonateMaybe(hitInfo.point, Vector3.zero);
			}
			
			lastPos = transform.position;
			
			life -= Time.deltaTime;
			if (life <= 0f) {
				detonateMaybe(transform.position, Vector3.zero);
			}
		}
	}
	
	private void detonateMaybe(Vector3 preciseLocation, Vector3 hitNorm) {
		if (Turning) {
			for (int i = 0; i < ExplosionParticles; i++) {
				var np = (GameObject)GameObject.Instantiate(particle);
				np.transform.position = Random.insideUnitSphere * ExplosionSize + lastPos;
				var p = np.GetComponent<CcParticle>();
				p.MaxSpeed = ExplosionSpeed;
				p.StartColor = Color.Lerp (Color.green, Color.yellow, Random.value);
				p.UseMidColor = true;
				p.MidColor = Color.Lerp (Color.green, Color.blue, Random.value);
				p.MidColorPos = 0.7f;
				p.EndColor = Color.clear;
				p.life = 2.5f;
				p.f = -2f;
				p.MinSize = 6f;
				p.MaxSize = 8f;
				p.acceleration = 0.1f;
				p.ParticType = ParticleType.Multiple;
			}
		}else{
			for (int i = 0; i < ExplosionParticles; i++) {
				var np = (GameObject)GameObject.Instantiate(particle);
				np.transform.position = Random.insideUnitSphere * ExplosionSize + lastPos; // do NOT use preciseLocation because particles shouldn't spawn inside a wall (bouncing will mess up) 
				var p = np.GetComponent<CcParticle>();
				p.MaxSpeed = ExplosionSpeed;
				p.StartColor = Color.Lerp(Color.red, Color.yellow, Random.value);
				p.EndColor = Color.Lerp(Color.Lerp(Color.red, Color.clear, 1f), Color.Lerp(Color.black, Color.clear, 1f), Random.Range(0f, 1f));
				p.life = 2.5f;
				p.f = -2f;
				p.MinSize = 6f;
				p.MaxSize = 8f;
				p.acceleration = 0.1f;
				p.ParticType = ParticleType.Multiple;
			}
		}

		enabled = false;
		if (net.isServer)
			net.Detonate(Gun.RocketLauncher, preciseLocation, shooterID, viewID);

		net.DetonateRocket(preciseLocation, hitNorm, viewID);
	}
}
