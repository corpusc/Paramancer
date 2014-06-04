using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	public GameObject particle;
	public float FlightSpeed = 120f; // accelerates to this 
	public float BaseFlightSpeed = 40f; // starts at this speed 
	public int MinPtsPerSecond = 300; // pts == particles 
	public int MaxPtsPerSecond = 500;
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
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		lastPos = transform.position;
		origFwd = transform.forward;
		origUp = transform.up;
		transform.position += transform.forward * ForwardOffset;
		maxLife = life;
	}
	
	void Update() {
		if (enabled) {
			if (Turning) {
				Vector3 turnVec = Vector3.Normalize(Vector3.Lerp(origFwd, origUp, Mathf.Lerp(0.3f, 0.8f, (maxLife - life) / maxLife)));
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
					np.GetComponent<CcParticle>().MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					np.GetComponent<CcParticle>().life = 1.5f;
					np.GetComponent<CcParticle>().StartColor = Color.Lerp(Color.green, Color.blue, Random.value);
					np.GetComponent<CcParticle>().UseMidColor = true;
					np.GetComponent<CcParticle>().MidColor = Color.Lerp(Color.green, Color.blue, Random.value);
					np.GetComponent<CcParticle>().MidColorPos = Random.Range(0.4f, 0.5f);
					np.GetComponent<CcParticle>().EndColor = Color.clear;
				}
			}else{
				for (int i = 0; i < num; i++) {
					var np = (GameObject)GameObject.Instantiate(particle);
					float offMagn = moveForward.magnitude / 2f;
					Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
					np.transform.position = transform.position + offset;
					np.GetComponent<CcParticle>().MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					np.GetComponent<CcParticle>().life = 2.5f;
					np.GetComponent<CcParticle>().StartColor = Color.Lerp(Color.white, Color.yellow, Random.value);
					np.GetComponent<CcParticle>().UseMidColor = true;
					np.GetComponent<CcParticle>().MidColor = Color.Lerp(Color.red, Color.black, Random.Range(0f, 0.5f));
					np.GetComponent<CcParticle>().MidColorPos = Random.Range(0.4f, 0.6f);
					np.GetComponent<CcParticle>().EndColor = Color.clear;
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
				np.GetComponent<CcParticle>().MaxSpeed = ExplosionSpeed;
				np.GetComponent<CcParticle>().StartColor = Color.Lerp (Color.green, Color.yellow, Random.value);
				np.GetComponent<CcParticle>().UseMidColor = true;
				np.GetComponent<CcParticle>().MidColor = Color.Lerp (Color.green, Color.blue, Random.value);
				np.GetComponent<CcParticle>().MidColorPos = 0.7f;
				np.GetComponent<CcParticle>().EndColor = Color.clear;
				np.GetComponent<CcParticle>().life = 2.5f;
				np.GetComponent<CcParticle>().f = -2f;
				np.GetComponent<CcParticle>().MinSize = 6f;
				np.GetComponent<CcParticle>().MaxSize = 8f;
				np.GetComponent<CcParticle>().acceleration = 0.1f;
				np.GetComponent<CcParticle>().ParticType = ParticleType.Multiple;
			}
		}else{
			for (int i = 0; i < ExplosionParticles; i++) {
				var np = (GameObject)GameObject.Instantiate(particle);
				np.transform.position = Random.insideUnitSphere * ExplosionSize + lastPos; // do NOT use preciseLocation because particles shouldn't spawn inside a wall (bouncing will mess up) 
				np.GetComponent<CcParticle>().MaxSpeed = ExplosionSpeed;
				np.GetComponent<CcParticle>().StartColor = Color.Lerp(Color.red, Color.yellow, Random.value);
				np.GetComponent<CcParticle>().EndColor = Color.Lerp(Color.Lerp(Color.red, Color.clear, 1f), Color.Lerp(Color.black, Color.clear, 1f), Random.Range(0f, 1f));
				np.GetComponent<CcParticle>().life = 2.5f;
				np.GetComponent<CcParticle>().f = -2f;
				np.GetComponent<CcParticle>().MinSize = 6f;
				np.GetComponent<CcParticle>().MaxSize = 8f;
				np.GetComponent<CcParticle>().acceleration = 0.1f;
				np.GetComponent<CcParticle>().ParticType = ParticleType.Multiple;
			}
		}

		enabled = false;
		if (net.isServer)
			net.Detonate(Gun.RocketProjectile, preciseLocation, shooterID, viewID);

		net.DetonateRocket(preciseLocation, hitNorm, viewID);
	}
}
