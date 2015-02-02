using UnityEngine;
using System.Collections;



public class GunPickup : MonoBehaviour {
	public string Name;
	public GameObject Model; // this WAS is an IDE linked sub-object of the prefab 
	public SpawnData SpawnData;
	
	// private 
	CcNet net;
	GameObject luO; // local user game object 
	float sinny = 0f;
	float zOff; // rotation offset, so that they aren't all pointed in the same direction 
	bool isHealth;
	Vector3 start;


	
	
	void Start() {
		sinny = Random.Range(0f, 4f);
		net = GameObject.Find("Main Program").GetComponent<CcNet>();

		if (Name == "Health") {
			isHealth = true;
			zOff = 90f;
			transform.FindChild("Outer shell").renderer.material.color = S.Orange;
		}else{
			isHealth = false;
			zOff = Random.Range(0f, 360f);
		}


		// scale & aim offsets 
		if /***/ (Name == "Grenade Launcher") {
			Model.transform.Rotate(0, 0, zOff);
			Model.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
		}else if (Name == "Spatula") {
			Model.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			Model.transform.Rotate(0, 0, zOff);
		}else if (Name == "Health") {
			start = transform.position + Vector3.up * 0.33f;
			return;
		}else{
			Model.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
			Model.transform.Rotate(270, 0, zOff);
		}

		start = Model.transform.position + Vector3.up * 0.33f;
	}
	
	void Update() {
		if (luO == null) {
			if (null != net &&
				null != net.LocEnt &&
			    null != net.LocEnt.Actor
		    )
				luO = net.LocEnt.Actor.gameObject;
		}else{
			if (Vector3.Distance(
				luO.transform.position, transform.position) < 2f && 
				luO.transform.position.y > transform.position.y - 0.5f
			) {
				luO.GetComponent<Actor>().offeredPickup = Name;
				luO.GetComponent<Actor>().currentOfferedPickup = this;
			}
		}
		
		sinny += Time.deltaTime * 2f;

		if (isHealth) {
			transform.position = start + (Vector3.up * ((Mathf.Sin(sinny)*0.2f) + 0.3f));
			transform.Rotate(300f * Time.deltaTime, 0, 0);
		}else{
			//boxObj.transform.position = transform.position + (Vector3.up * ((Mathf.Sin(sinny)*0.1f) + 0.3f));      original setting for all BOX models
			Model.transform.position = start + (Vector3.up * ((Mathf.Sin(sinny)*0.2f) + 0.3f));
			Model.transform.Rotate(0, 0, 130f * Time.deltaTime);
		}
	}


	public void Pickup() {
		net.UnstockPickupPoint(SpawnData);
		Destroy(gameObject);
	}


	void makeQuad(ref GameObject go, float angle, float zOffs) {
		//		go = GameObject.CreatePrimitive(PrimitiveType.Quad);
		//		setMat(go, Pics.Health);//"Grenade Launcher"));
		//		go.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
		//		go.transform.position = transform.position + new Vector3(0, zOffs, 0);
		//		go.collider.enabled = false;
		//		go.transform.parent = vGen.PrimBag.transform; 
	}


	void setMat(GameObject go, Texture pic) {
		go.renderer.material.SetTexture("_MainTex", pic);
		go.renderer.material.shader = Shader.Find("Transparent/Cutout/Diffuse");
	}
}
