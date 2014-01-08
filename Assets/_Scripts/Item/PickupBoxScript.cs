using UnityEngine;
using System.Collections;

public class PickupBoxScript : MonoBehaviour {
	
	
	public string pickupName;
	public GameObject iconObj;
	public GameObject boxObj;
	
	
	private CcNet theNetwork;
	private GameObject localPlayer;
	
	private float sinny = 0f;
	
	public PickupPoint pickupPoint;
	
	
	// Use this for initialization
	void Start () {
		theNetwork = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f,4f);
		boxObj.transform.Rotate(0,0, Random.Range(0f,360f));
	}
	
	// Update is called once per frame
	void Update () {
	
		if (localPlayer == null){
			for (int i=0; i<theNetwork.players.Count; i++){
				if (theNetwork.localPlayer.viewID == theNetwork.players[i].viewID){
					localPlayer = theNetwork.players[i].Entity.gameObject;
				}
			}
		}else{
			if (Vector3.Distance(localPlayer.transform.position,transform.position) < 2f && localPlayer.transform.position.y > transform.position.y-0.5f){
				//Debug.Log("You can pick up the " + pickupName);
				localPlayer.GetComponent<EntityClass>().offeredPickup = pickupName;
				localPlayer.GetComponent<EntityClass>().currentOfferedPickup = this;
			}
		}
		
		
		sinny += Time.deltaTime * 2f;
		boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));
		boxObj.transform.Rotate(0,0,10f * Time.deltaTime);
	}
	
	public void Pickup(){
		//destroy this pickup
		
		
		theNetwork.UnstockPickupPoint(pickupPoint);
		
		Destroy(gameObject);
	}
}
