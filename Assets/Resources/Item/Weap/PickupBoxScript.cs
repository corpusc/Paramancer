using UnityEngine;
using System.Collections;



public class PickupBoxScript : MonoBehaviour {
	public string Name;
	public GameObject Icon;
	public GameObject boxObj;
	public PickupPoint pickupPoint;
	
	// private 
	CcNet net;
	GameObject lu; // local user game object 
	float sinny = 0f;
	float zOff; // rotation offset, so that they aren't all pointed in the same direction 
	Vector3 start;
	bool isHealth;
	
	
	
	void Start() {
		if (Name == "Health") {
			Icon.renderer.material.SetTexture("_MainTex", Pics.Health);
			Icon.renderer.material.shader = Shader.Find("Transparent/Cutout/Diffuse");

			isHealth = true;
			zOff = 90f;
		}else{
			isHealth = false;
			zOff = Random.Range(0f, 360f);
		}

		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f, 4f);

		if (Name == "Grenade Launcher") {
			boxObj.transform.Rotate(0, 0, zOff);
			boxObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			start = boxObj.transform.position + Vector3.up * 0.33f;
		}else if (Name == "Spatula"){
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
		if (lu == null) {
			lu = net.LocUs.Entity.gameObject;
		}else{
			if (Vector3.Distance(
				lu.transform.position, transform.position) < 2f && 
				lu.transform.position.y > transform.position.y - 0.5f
			) {
				lu.GetComponent<EntityClass>().offeredPickup = Name;
				lu.GetComponent<EntityClass>().currentOfferedPickup = this;
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
