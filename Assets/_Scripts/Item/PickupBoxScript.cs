using UnityEngine;
using System.Collections;

public class PickupBoxScript : MonoBehaviour {
	public string pickupName;
	public GameObject iconObj;
	public GameObject boxObj;
	public PickupPoint pickupPoint;
	
	// private
	CcNet net;
	GameObject localPlayer;
	float sinny = 0f;
	
	
	
	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f, 4f);
		boxObj.transform.Rotate(0, 0, Random.Range(0f, 360f));
	}
	
	void Update() {
		if (localPlayer == null){
			for (int i=0; i<net.players.Count; i++) {
				if (net.localPlayer.viewID == net.players[i].viewID) {
					localPlayer = net.players[i].Entity.gameObject;
				}
			}
		}else{
			if (Vector3.Distance(localPlayer.transform.position,transform.position) < 2f && 
				localPlayer.transform.position.y > transform.position.y-0.5f
			) {
				localPlayer.GetComponent<EntityClass>().offeredPickup = pickupName;
				localPlayer.GetComponent<EntityClass>().currentOfferedPickup = this;
			}
		}
		
		sinny += Time.deltaTime * 2f;
		boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));
		boxObj.transform.Rotate(0,0,10f * Time.deltaTime);
	}
	
	public void Pickup() {
		net.UnstockPickupPoint(pickupPoint);
		Destroy(gameObject);
	}
}
