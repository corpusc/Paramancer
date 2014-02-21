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
	float rotationOffset; // so that they aren't all pointed in the same direction
	Vector3 start;
	
	
	
	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f, 4f);
		rotationOffset = Random.Range(0f, 360f);
		//boxObj.transform.Rotate(0, 0, Random.Range(0f, 360f));        original setting for all BOX models
		boxObj.transform.Rotate(270, 0, rotationOffset);
		boxObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		start = boxObj.transform.position + Vector3.up/3;
	}
	
	void Update() {
		if (localPlayer == null) {
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
		boxObj.transform.position = start + (Vector3.up * ((Mathf.Sin(sinny)*0.2f) + 0.3f));
//		boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));      original setting for all BOX models
		boxObj.transform.Rotate(0, 0, 130f * Time.deltaTime);
	}
	
	public void Pickup() {
		net.UnstockPickupPoint(pickupPoint);
		Destroy(gameObject);
	}
}
