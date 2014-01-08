using UnityEngine;
using System.Collections;

public class GrenadeScript : MonoBehaviour {

	
	public Vector3 start;
	public Vector3 direction;
	public double startTime;
	
	public float detonationTime;
	private Vector3 moveVector = Vector3.zero;
	
	public NetworkViewID viewID;
	public NetworkViewID shooterID;
	
	private CcNet theNetwork;
	
	private Vector3 lastPos;
	
	private bool active = true;
	
	public AudioClip sfx_bounce;
	
	// Use this for initialization
	void Start () {
		theNetwork = GameObject.Find("Main Program").GetComponent<CcNet>();
		
		
		transform.position = start;
		lastPos = start;
		detonationTime += Time.time;
		
		moveVector = direction * 20f;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (active){
			transform.position += moveVector * Time.deltaTime;
			
			moveVector.y -= Time.deltaTime * 10f;
			
			RaycastHit hitInfo = new RaycastHit();
			int layerMask = (1<<0);
			Vector3 rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.15f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)){
				transform.position = hitInfo.point + (hitInfo.normal*0.15f);
				moveVector = Vector3.Reflect(moveVector, hitInfo.normal);
				moveVector *= 0.8f;
				
				
				audio.clip = sfx_bounce;
				audio.Play();
			}
			lastPos = transform.position;
			
			if (Time.time>detonationTime){
				active = false;
				if (theNetwork.isServer){
					//detonate now
					theNetwork.Detonate("grenade", transform.position, shooterID, viewID);
				}
			}
		}else{
			
		}
		
		
	}
}
