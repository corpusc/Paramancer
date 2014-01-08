using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	public GameObject dissPrefab;
	public NetworkViewID viewID;
	public NetworkViewID shooterID;
	
	private CcNet net;
	private Vector3 lastPos;
	private float life = 20f;
	
	
	
	void Start () {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		lastPos = transform.position;
	}
	
	void Update () {
		if (enabled){
			transform.position += transform.forward * Time.deltaTime * 30f;
			var newDiss = (GameObject)GameObject.Instantiate(dissPrefab);
			newDiss.transform.position = transform.position;
			var hitInfo = new RaycastHit();
			int layerMask = (1<<0);
			var rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)) {
				//Debug.Log(hitInfo.collider.gameObject.name);
				detonateMaybe();
			}
			
			lastPos = transform.position;
			
			life -= Time.deltaTime;
			if (life <= 0f) {
				detonateMaybe();
			}
		}
	}
	
	private void detonateMaybe() {
		enabled = false;
		if (net.isServer)
			net.Detonate("rocket", transform.position, shooterID, viewID);
	}
}
