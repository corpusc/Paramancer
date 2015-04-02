using UnityEngine;
using System.Collections;



public class BasketballScript : MonoBehaviour {
	public NetworkViewID throwerID; // set this when the ball is picked up 
	public bool held = false; // ball held 
	public Vector3 moveVector = Vector3.zero;
	public AudioClip sfx_bounce;

	// private 
	CcNet net;
	Vector3 lastPos;
	float throwTime = 0f;



	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		ResetBall();
	}
	
	public void ResetBall() {
		for (int i=0; i<net.Entities.Count; i++) {
			net.Entities[i].hasBall = false;
		}

		transform.parent = null;
		transform.position = GameObject.Find("_BasketballStart").transform.position;
		lastPos = transform.position;
		moveVector = Vector3.zero;
	}
	
	public void Throw(Vector3 fromPos, Vector3 direction, float strength) {
		throwTime = Time.time;
		for (int i=0; i<net.Entities.Count; i++) {
			net.Entities[i].hasBall = false;
		}
		transform.parent = null;
		transform.position = fromPos;
		lastPos = transform.position;
		moveVector = direction * strength;
		held = false;
	}
	
	public void HoldBall(NetworkViewID viewID) {
		throwerID = viewID;
		moveVector = Vector3.zero;
		held = true;
		
		for (int i=0; i<net.Entities.Count; i++) {
			if (net.Entities[i].viewID == throwerID){
				net.Entities[i].hasBall = true;
				
				transform.parent = net.Entities[i].Actor.MeshInHand.transform.parent;
				transform.localPosition = (-Vector3.right * 0.7f) + (Vector3.forward * 0.2f);
				
				net.Entities[i].Actor.PlaySound("Catch");
			}
		}
	}
	
	void Update() {
		var col = S.ColToVec(new Color(1f, 0.5f, 0f, 1f));

		if (!held) {
			transform.position += moveVector * Time.deltaTime;
			moveVector.y -= Time.deltaTime * 10f;
			RaycastHit hitInfo = new RaycastHit();
			int layerMask = (1<<0)|(1<<10)|(1<<11)|(1<<12);
			Vector3 rayDirection = (transform.position - lastPos).normalized;

			if (Physics.SphereCast(lastPos, 0.5f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)) {
				
				if (hitInfo.collider.gameObject.layer == 11) {
					// blue scores 
					if (net.InServerMode && !net.gameOver) {
						ResetBall();
						net.team2Score++;
						net.netView.RPC("AnnounceTeamScores", RPCMode.Others, net.team1Score, net.team2Score);
						net.netView.RPC("AddEntry",RPCMode.All, "BASKETBALL: ", "TEAM BLUE SCORES!", col);
					}
				}else if (hitInfo.collider.gameObject.layer == 12) {
					// red scores 
					if (net.InServerMode && !net.gameOver) {
						ResetBall();
						net.team1Score++;
						net.netView.RPC("AnnounceTeamScores", RPCMode.Others, net.team1Score, net.team2Score);
						net.netView.RPC("AddEntry",RPCMode.All, "BASKETBALL: ", "TEAM RED SCORES!", col);
					}
				}else if (hitInfo.collider.gameObject.layer == 10) {
					// LAVA 
					if (net.InServerMode) {
						ResetBall();
						net.netView.RPC("AddEntry",RPCMode.All, "BASKETBALL: ", "OH NO, I FELL IN THE LAVA!", col);
					}
				}else{
					transform.position = hitInfo.point + (hitInfo.normal*0.5f);
					moveVector = Vector3.Reflect(moveVector, hitInfo.normal);
					moveVector *= 0.7f;

					if (moveVector.magnitude > 1.5f) {
						GetComponent<AudioSource>().clip = sfx_bounce;
						GetComponent<AudioSource>().pitch = Random.Range(1f, 1.2f);
						GetComponent<AudioSource>().Play();
					}
				}
			}

			lastPos = transform.position;
			
			if (net.InServerMode) {
				// let's check to see if any of the players can pick up the ball 
				bool captured = false;
				for (int i=0; i<net.Entities.Count; i++) {
					if (!captured && net.Entities[i].Health>0f && 
					    Vector3.Distance(transform.position, net.Entities[i].Actor.transform.position)<1.5f
				    ) {
						if (throwerID == null || 
						    net.Entities[i].viewID != throwerID || 
						    Time.time > throwTime+0.5f
					    ) {
							net.AnnounceBallCapture(net.Entities[i].viewID);
							captured = true;
						}
					}
				}
				
			}
		}
	}
}
