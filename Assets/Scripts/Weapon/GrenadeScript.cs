using UnityEngine;
using System.Collections;

public class GrenadeScript : MonoBehaviour {
	public Vector3 start;
	public Vector3 direction;
	public double startTime;
	public float detonationTime;
	public NetworkViewID viewID;
	public NetworkViewID shooterID;
	public AudioClip sfx_bounce;
	
	// private
	bool alive = true;
	CcNet net;
	Vector3 lastPos;
	Vector3 moveVector = Vector3.zero;
	
	
	
	void Start () {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		transform.position = start;
		lastPos = start;
		detonationTime += Time.time;
		moveVector = direction * 20f;
	}
	
	void Update () {
		if (alive) {
			transform.position += moveVector * Time.deltaTime;
			moveVector.y -= Time.deltaTime * 10f;
			int layerMask = (1<<8) | 1;    //(1<<0);
			Vector3 rayDirection = (transform.position - lastPos).normalized;
			
			RaycastHit hitInfo = new RaycastHit();
			if (Physics.SphereCast(
				lastPos, 
				0.15f, 
				rayDirection, 
				out hitInfo, 
				Vector3.Distance(transform.position, lastPos), 
				layerMask)
			) {
				if (hitInfo.collider.GetType() == typeof(CharacterController)) {
					// i like players being able to hurt themselves with careless grenade tosses,
					// but some kinda FIXME needed for insta self kills while throwing and sprinting forward?
					maybeDetonate();
				}else{
					transform.position = hitInfo.point + (hitInfo.normal*0.15f);
					moveVector = Vector3.Reflect(moveVector, hitInfo.normal);
					moveVector *= 0.8f;
					audio.clip = sfx_bounce;
					audio.Play();
				}
			}
			
			lastPos = transform.position;
			
			if (Time.time > detonationTime) {
				maybeDetonate();
			}
		}
	}

	void maybeDetonate() {
		alive = false;

		if (net.isServer) {
			net.Detonate (Item.Grenade, transform.position, shooterID, viewID);
		}
	}
}
