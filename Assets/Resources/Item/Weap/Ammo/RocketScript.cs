using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	public GameObject particle;
	public int MinPtsPerSecond = 300; // pts == points? 
	public int MaxPtsPerSecond = 500;
	public int ExplosionParticles = 200;
	public float ExplosionSize = 0.4f;
	public float ForwardOffset = 1f; // prevents running into your own rocket when you have lags (doesn't work....prob need to filter out rocket owner's body )
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
	Vector3 oriForward; // the originial forward direction
	Vector3 oriTurn; // the original turning
	float life = 20f;
	
	
	
	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		lastPos = transform.position;
		oriForward = transform.forward;
		oriTurn = Vector3.Normalize(Vector3.Lerp(transform.forward, transform.up, 0.2f));
		transform.position += transform.forward * ForwardOffset;
	}
	
	void Update() {
		if (enabled) {
			if (Turning)
				transform.forward = Quaternion.AngleAxis(life * Turn, oriForward) * oriTurn;

			Vector3 moveForward = transform.forward * Time.deltaTime * 600f / (life < 16f ? 1f : Mathf.Pow(life - 15f, 2f));
			transform.position += moveForward;
			int currentParts = (int)((float)Random.Range(MinPtsPerSecond, MaxPtsPerSecond) * Time.deltaTime); // the amount of particles to spawn in the current frame

			if (Turning) {
				for (int i = 0; i < currentParts; i++) {
					var np = (GameObject)GameObject.Instantiate(particle);
					float offMagn = moveForward.magnitude / 2f;
					Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
					np.transform.position = transform.position + offset;
					np.GetComponent<BeamParticle>().MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					np.GetComponent<BeamParticle>().life = 1.5f;
					np.GetComponent<BeamParticle>().StartColor = Color.Lerp(Color.green, Color.blue, Random.value);
					np.GetComponent<BeamParticle>().UseMidColor = true;
					np.GetComponent<BeamParticle>().MidColor = Color.Lerp(Color.green, Color.blue, Random.value);
					np.GetComponent<BeamParticle>().MidColorPos = Random.Range(0.4f, 0.5f);
					np.GetComponent<BeamParticle>().EndColor = Color.clear;
				}
			}else{
				for (int i = 0; i < currentParts; i++) {
					var np = (GameObject)GameObject.Instantiate(particle);
					float offMagn = moveForward.magnitude / 2f;
					Vector3 offset = new Vector3(Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn), Random.Range(-offMagn, offMagn));
					np.transform.position = transform.position + offset;
					np.GetComponent<BeamParticle>().MoveVec = -transform.forward * Random.Range(MinParticleSpeed, MaxParticleSpeed) + Random.insideUnitSphere * PMR;
					np.GetComponent<BeamParticle>().life = 2.5f;
					np.GetComponent<BeamParticle>().StartColor = Color.Lerp(Color.white, Color.yellow, Random.value);
					np.GetComponent<BeamParticle>().UseMidColor = true;
					np.GetComponent<BeamParticle>().MidColor = Color.Lerp(Color.red, Color.black, Random.Range(0f, 0.5f));
					np.GetComponent<BeamParticle>().MidColorPos = Random.Range(0.4f, 0.6f);
					np.GetComponent<BeamParticle>().EndColor = Color.clear;
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
				np.GetComponent<BeamParticle>().MaxSpeed = ExplosionSpeed;
				np.GetComponent<BeamParticle>().StartColor = Color.Lerp (Color.green, Color.yellow, Random.value);
				np.GetComponent<BeamParticle>().UseMidColor = true;
				np.GetComponent<BeamParticle>().MidColor = Color.Lerp (Color.green, Color.blue, Random.value);
				np.GetComponent<BeamParticle>().MidColorPos = 0.7f;
				np.GetComponent<BeamParticle>().EndColor = Color.clear;
				np.GetComponent<BeamParticle>().life = 2.5f;
				np.GetComponent<BeamParticle>().f = -2f;
				np.GetComponent<BeamParticle>().MinSize = 6f;
				np.GetComponent<BeamParticle>().MaxSize = 8f;
				np.GetComponent<BeamParticle>().acceleration = 0.1f;
				np.GetComponent<BeamParticle>().ParticType = ParticleType.Multiple;
			}
		}else{
			for (int i = 0; i < ExplosionParticles; i++) {
				var np = (GameObject)GameObject.Instantiate(particle);
				np.transform.position = Random.insideUnitSphere * ExplosionSize + lastPos; // do NOT use preciseLocation because particles shouldn't spawn inside a wall (bouncing will mess up) 
				np.GetComponent<BeamParticle>().MaxSpeed = ExplosionSpeed;
				np.GetComponent<BeamParticle>().StartColor = Color.Lerp(Color.red, Color.yellow, Random.value);
				np.GetComponent<BeamParticle>().EndColor = Color.Lerp(Color.Lerp(Color.red, Color.clear, 1f), Color.Lerp(Color.black, Color.clear, 1f), Random.Range(0f, 1f));
				np.GetComponent<BeamParticle>().life = 2.5f;
				np.GetComponent<BeamParticle>().f = -2f;
				np.GetComponent<BeamParticle>().MinSize = 6f;
				np.GetComponent<BeamParticle>().MaxSize = 8f;
				np.GetComponent<BeamParticle>().acceleration = 0.1f;
				np.GetComponent<BeamParticle>().ParticType = ParticleType.Multiple;
			}
		}

		enabled = false;
		if (net.isServer)
			net.Detonate(Item.RocketProjectile, preciseLocation, shooterID, viewID);

		net.DetonateRocket(preciseLocation, hitNorm, viewID);
	}
}
