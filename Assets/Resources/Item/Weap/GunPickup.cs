using UnityEngine;
using System.Collections;



public class GunPickup : MonoBehaviour {
	public string Name;
	public GameObject Model; // this WAS is an IDE linked sub-object of the prefab 
	public PickupPoint PickupPoint;
	
	// private 
	CcNet net;
	GameObject luO; // local user game object 
	GameObject iconFront; // ...or top, since kit model defaults to "laying flat on the ground" 
	GameObject iconBack; // ...or bottom ^^ 
	float sinny = 0f;
	float zOff; // rotation offset, so that they aren't all pointed in the same direction 
	Vector3 start;
	bool isHealth;
	
	
	
	void makeQuad(ref GameObject go, float angle, float zOffs) {
		go = GameObject.CreatePrimitive(PrimitiveType.Quad);
		setMat(go, Pics.Get("Grenade Launcher"));
		go.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
		go.transform.position = transform.position + new Vector3(0, zOffs, 0);
		//go.transform.parent = Model.transform;     can't make children in prefab!!!!!!!!! 
	}
	void setMat(GameObject go, Texture pic) {
		go.renderer.material.SetTexture("_MainTex", pic);
		go.renderer.material.shader = Shader.Find("Transparent/Cutout/Diffuse");
	}

	void Start() {
		net = GameObject.Find("Main Program").GetComponent<CcNet>();
		sinny = Random.Range(0f, 4f);

		makeQuad(ref iconFront, 90f, 1f);
		makeQuad(ref iconBack, -90f, -1f);

		if (Name == "Health") {
			//setMat(iconFront, Pics.Health);
			//setMat(iconBack, Pics.Health);

			isHealth = true;
			zOff = 90f;
		}else{
			isHealth = false;
			zOff = Random.Range(0f, 360f);
		}



		if (Name == "Grenade Launcher") {
			Model.transform.Rotate(0, 0, zOff);
			Model.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
			start = Model.transform.position + Vector3.up * 0.33f;
		}else if (Name == "Spatula") {
			Model.transform.Rotate(0, 0, zOff);
			Model.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			start = Model.transform.position + Vector3.up * 0.33f;
		} else {
			Model.transform.Rotate(270, 0, zOff);
			Model.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
			start = Model.transform.position + Vector3.up * 0.33f;
		}
	}
	
	void Update() {
		if (luO == null) {
			if (null != net &&
				null != net.LocUs &&
			    null != net.LocUs.Visuals
		    )
				luO = net.LocUs.Visuals.gameObject;
		}else{
			if (Vector3.Distance(
				luO.transform.position, transform.position) < 2f && 
				luO.transform.position.y > transform.position.y - 0.5f
			) {
				luO.GetComponent<EntityClass>().offeredPickup = Name;
				luO.GetComponent<EntityClass>().currentOfferedPickup = this;
			}
		}
		
		sinny += Time.deltaTime * 2f;
		Model.transform.position = start + (Vector3.up * ((Mathf.Sin(sinny)*0.2f) + 0.3f));
//		boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));      original setting for all BOX models

		if (isHealth) {
			transform.Rotate(300f * Time.deltaTime, 0, 0);
			iconFront.transform.Rotate(300f * Time.deltaTime, 0, 0);
			iconBack.transform.Rotate(300f * Time.deltaTime, 0, 0);
		}else
			Model.transform.Rotate(0, 0, 130f * Time.deltaTime);
	}
	
	public void Pickup() {
		net.UnstockPickupPoint(PickupPoint);
		Destroy(gameObject);
	}
}
