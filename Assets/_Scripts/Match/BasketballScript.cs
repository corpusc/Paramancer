using UnityEngine;
using System.Collections;

public class BasketballScript : MonoBehaviour {
	
	//set this when the ball is picked up
	public NetworkViewID throwerID;
	
	public bool held = false;
	
	public Vector3 moveVector = Vector3.zero;
	
	private Vector3 lastPos;
	
	public AudioClip sfx_bounce;
	
	private CcNet theNetwork;
	
	private float throwTime = 0f;
	
	// Use this for initialization
	void Start () {
		theNetwork = GameObject.Find("Main Program").GetComponent<CcNet>();
		
		ResetBall();
	}
	
	public void ResetBall(){
		for (int i=0; i<theNetwork.players.Count; i++){
			theNetwork.players[i].hasBall = false;
		}
		transform.parent = null;
		transform.position = GameObject.Find("_BasketballStart").transform.position;
		lastPos = transform.position;
		moveVector = Vector3.zero;
	}
	
	public void Throw(Vector3 fromPos, Vector3 direction, float strength){
		throwTime = Time.time;
		for (int i=0; i<theNetwork.players.Count; i++){
			theNetwork.players[i].hasBall = false;
		}
		transform.parent = null;
		transform.position = fromPos;
		lastPos = transform.position;
		moveVector = direction * strength;
		held = false;
	}
	
	public void HoldBall(NetworkViewID viewID){
		throwerID = viewID;
		moveVector = Vector3.zero;
		held = true;
		
		for (int i=0; i<theNetwork.players.Count; i++){
			if (theNetwork.players[i].viewID == throwerID){
				theNetwork.players[i].hasBall = true;
				
				transform.parent = theNetwork.players[i].Entity.gunMesh1.transform.parent;
				transform.localPosition = (-Vector3.right * 0.7f) + (Vector3.forward * 0.2f);
				
				theNetwork.players[i].Entity.PlaySound("catchBall");
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!held){
			transform.position += moveVector * Time.deltaTime;
				
			moveVector.y -= Time.deltaTime * 10f;
				
			RaycastHit hitInfo = new RaycastHit();
			int layerMask = (1<<0)|(1<<10)|(1<<11)|(1<<12);
			Vector3 rayDirection = (transform.position - lastPos).normalized;
			if (Physics.SphereCast(lastPos, 0.5f, rayDirection, out hitInfo, Vector3.Distance(transform.position, lastPos), layerMask)){
				
				if (hitInfo.collider.gameObject.layer == 11){
					//blue scores
					if (theNetwork.isServer && !theNetwork.gameOver){
						ResetBall();
						theNetwork.team2Score++;
						theNetwork.networkView.RPC("AnnounceTeamScores", RPCMode.Others, theNetwork.team1Score, theNetwork.team2Score);
						theNetwork.networkView.RPC("AddToLog",RPCMode.All, "BASKETBALL: ", "TEAM BLUE SCORES!", theNetwork.ColToVec(new Color(1f,0.5f,0f,1f)));
					}
				}else if (hitInfo.collider.gameObject.layer == 12){
					//red scores
					if (theNetwork.isServer && !theNetwork.gameOver){
						ResetBall();
						theNetwork.team1Score++;
						theNetwork.networkView.RPC("AnnounceTeamScores", RPCMode.Others, theNetwork.team1Score, theNetwork.team2Score);
						theNetwork.networkView.RPC("AddToLog",RPCMode.All, "BASKETBALL: ", "TEAM RED SCORES!", theNetwork.ColToVec(new Color(1f,0.5f,0f,1f)));
					}
				}else if (hitInfo.collider.gameObject.layer == 10){
					//LAVA! :D
					if (theNetwork.isServer){
						ResetBall();
						theNetwork.networkView.RPC("AddToLog",RPCMode.All, "BASKETBALL: ", "OH NO, I FELL IN THE LAVA!", theNetwork.ColToVec(new Color(1f,0.5f,0f,1f)));
					}
				}else{
				
					transform.position = hitInfo.point + (hitInfo.normal*0.5f);
					moveVector = Vector3.Reflect(moveVector, hitInfo.normal);
					moveVector *= 0.7f;
						
					
					//Debug.Log(moveVector.magnitude);
					if (moveVector.magnitude>1.5f){
						audio.clip = sfx_bounce;
						audio.pitch = Random.Range(1f,1.2f);
						audio.Play();
					}
				}
			}
			lastPos = transform.position;
			
			if (theNetwork.isServer){
				//let's check to see if any of the players can pick up the ball
				bool captured = false;
				for (int i=0; i<theNetwork.players.Count; i++){
					if (!captured && theNetwork.players[i].health>0f && Vector3.Distance(transform.position, theNetwork.players[i].Entity.transform.position)<1.5f){
						if (throwerID == null || theNetwork.players[i].viewID != throwerID || Time.time > throwTime+0.5f){
							theNetwork.AnnounceBallCapture(theNetwork.players[i].viewID);
							captured = true;
						}
					}
				}
				
			}
		}else{
			//ball held
		}
		
	}
}
