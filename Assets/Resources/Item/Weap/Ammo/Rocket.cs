using UnityEngine;
using System.Collections;



public class Rocket : MonoBehaviour {
	public float FlightSpeed = 120f; // accelerates to this 
	public float BaseFlightSpeed = 40f; // starts at this speed 
	public float ForwardOffset = 1f; // prevents running into your own rocket when you have lags (FIXME: doesn't work....might need to filter out rocket owner's body ) 
	public NetworkViewID viewID;
	public NetworkViewID shooterID;

	// private 
	CcNet net;
	Vector3 prevPos;
	float life = 10f;
	CcEmitter em = new CcEmitter();

	
	
	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		prevPos = transform.position;
		transform.position += transform.forward * ForwardOffset;
	}
	
	void Update() {
		if (enabled) {
			transform.position += transform.forward * Time.deltaTime * 30f;

			em.Update(transform);

			var hitInfo = new RaycastHit();
			int layerMask = 1;   //(1<<0);
			var rayDirection = (transform.position - prevPos).normalized;
			if (Physics.SphereCast(prevPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, prevPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				detonateMaybe(hitInfo.point, hitInfo.normal);
			}
			layerMask = 1<<8;
			if (Physics.SphereCast(prevPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, prevPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				detonateMaybe(hitInfo.point, Vector3.zero);
			}
			
			prevPos = transform.position;
			
			life -= Time.deltaTime;
			if (life <= 0f) {
				detonateMaybe(transform.position, Vector3.zero);
			}
		}
	}

	private void detonateMaybe(Vector3 pos, Vector3 hitNorm) {
		em.Explode(prevPos, Color.red, Color.Lerp(
			Color.Lerp(Color.red, Color.clear, 1f), 
			Color.Lerp(Color.black, Color.clear, 1f), 
			Random.Range(0f, 1f)), 
			false);

		enabled = false;
		if (net.InServerMode)
			net.Detonate(Gun.NapalmLauncher, pos, shooterID, viewID);

		// FIXME?  originally we only did the above conditional, and not all the stuff below 
		net.RemoveRocketWithExplosionEffects(pos, hitNorm, viewID);
	}
}
