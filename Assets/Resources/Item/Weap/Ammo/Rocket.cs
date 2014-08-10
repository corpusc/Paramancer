using UnityEngine;
using System.Collections;



public class Rocket : MonoBehaviour {
	public float FlightSpeed = 120f; // accelerates to this 
	public float BaseFlightSpeed = 40f; // starts at this speed 
	public float ForwardOffset = 1f; // prevents running into your own rocket when you have lags (FIXME: doesn't work....might need to filter out rocket owner's body ) 
	public float Turn = 300f;
	public bool Spiralling = false;
	public NetworkViewID viewID;
	public NetworkViewID shooterID;

	// private 
	CcNet net;
	Vector3 lastPos;
	Vector3 origFwd; // the original forward direction 
	Vector3 origUp; // the original up direction 
	float life = 10f;
	float maxLife;
	CcEmitter em = new CcEmitter();

	
	
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
			if (Spiralling) {
				var turnVec = Vector3.Normalize(Vector3.Lerp(origFwd, origUp, Mathf.Lerp(0.3f, 0.8f, (maxLife - life) / maxLife)));
				transform.forward = Quaternion.AngleAxis(life * Turn, origFwd) * turnVec;
			}

			Vector3 moveForward = (transform.forward * Time.deltaTime) * (FlightSpeed * (maxLife - life) / maxLife + BaseFlightSpeed);
			transform.position += moveForward;

			em.Update(-transform.forward, lastPos, Spiralling);

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
		if (Spiralling)
			em.Explode(lastPos, Color.green, Color.clear);
		else
			em.Explode(lastPos, Color.red, Color.Lerp(
				Color.Lerp(Color.red, Color.clear, 1f), 
				Color.Lerp(Color.black, Color.clear, 1f), 
				Random.Range(0f, 1f)), 
				false);

		enabled = false;
		if (net.InServerMode)
			net.Detonate(Gun.RocketLauncher, preciseLocation, shooterID, viewID);

		net.DetonateRocket(preciseLocation, hitNorm, viewID);
	}
}
