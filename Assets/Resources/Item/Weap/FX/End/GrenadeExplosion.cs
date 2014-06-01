using UnityEngine;
using System.Collections;

public class GrenadeExplosion : MonoBehaviour {
	public float MaxExpand;
	public float ExpandSpeed;


	Vector3 oriSpeed;
	float cExpand = 0f; // current expand val
	float f = 0.2f;


	void Start() {
		transform.localScale = new Vector3(f,f,f);
		// orientation/rotation speeds 
		oriSpeed = new Vector3(rand(), rand(), rand());

		MaxExpand = Random.Range(8f, 12f);
		ExpandSpeed = Random.Range(20f, 40f);
	}

	void Update() {
		// expanding
		cExpand += Time.deltaTime * ExpandSpeed;
		transform.localScale = new Vector3(f,f,f) * cExpand * (MaxExpand - cExpand); // gaussian-like distribution

		// rotation & light 
		if (light != null)
			light.range = transform.localScale.x * 3f;
		transform.eulerAngles += oriSpeed;

		// cleanup 
		if (transform.localScale.x < 0.1f) 
			Destroy(gameObject);
	}
	
	float rand() {
		var min = 6f;
		var max = 11f;
		var f = Random.Range(min, max);

		if (Random.Range(0, 2) == 1)
			f = -f;

		return f;		
	}
}
