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
	float zOff; // rotation offset, so that they aren't all pointed in the same direction 
	Vector3 start;
	bool isHealth;
	
	
	
	void Start() {
		if (pickupName == "Health") {
			isHealth = true;
			zOff = 90f;
		}else{
			isHealth = false;
			zOff = Random.Range(0f, 360f);
		}

		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f, 4f);
		//boxObj.transform.Rotate(0, 0, Random.Range(0f, 360f));        original setting for all BOX models 
		if (pickupName == "Grenade Launcher") {
			boxObj.transform.Rotate(0, 0, zOff);
			boxObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			start = boxObj.transform.position + Vector3.up * 0.33f;
		}else if (pickupName == "Spatula"){
			boxObj.transform.Rotate(0, 0, zOff);
			start = boxObj.transform.position + Vector3.up * 0.33f;
			boxObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		} else {
			boxObj.transform.Rotate(270, 0, zOff);
			start = boxObj.transform.position + Vector3.up * 0.33f;
			boxObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		}
	}
	
	void Update() {
		if (localPlayer == null) {
			for (int i=0; i<net.players.Count; i++) {
				if (net.localPlayer.viewID == net.players[i].viewID) {
					localPlayer = net.players[i].Entity.gameObject;
				}
			}
		}else{
			if (Vector3.SqrMagnitude(localPlayer.transform.position - transform.position) < 4f && 
				localPlayer.transform.position.y > transform.position.y - 0.5f
			) {
				localPlayer.GetComponent<EntityClass>().offeredPickup = pickupName;
				localPlayer.GetComponent<EntityClass>().currentOfferedPickup = this;
			}
		}
		
		sinny += Time.deltaTime * 2f;
		boxObj.transform.position = start + (Vector3.up * ((Mathf.Sin(sinny)*0.2f) + 0.3f));
//		boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));      original setting for all BOX models

		if (isHealth)
			boxObj.transform.Rotate(300f * Time.deltaTime, 0, 0);
		else
			boxObj.transform.Rotate(0, 0, 130f * Time.deltaTime);
	}
	
	public void Pickup() {
		net.UnstockPickupPoint(pickupPoint);
		Destroy(gameObject);
	}
}
