using UnityEngine;
using System.Collections;

public class GrenadeExplosion : MonoBehaviour {
	bool expanding = true;
	Vector3 oriSpeed;


	void Start() {
		var f = 0.2f;
		transform.localScale = new Vector3(f,f,f);
		// orientation/rotation speeds 
		oriSpeed = new Vector3(rand(), rand(), rand());
	}

	void Update() {
		var expandSpeed = 35f;
		var shrinkSpeed = 20f;
		var ls = transform.localScale;

		if (expanding) {
			ls.x += Time.deltaTime * expandSpeed;
			ls.y += Time.deltaTime * expandSpeed;
			ls.z += Time.deltaTime * expandSpeed;

			if (transform.localScale.x > 5f)
				expanding = false;
		}else{ // shrinking 
			ls.x -= Time.deltaTime * shrinkSpeed;
			ls.y -= Time.deltaTime * shrinkSpeed;
			ls.z -= Time.deltaTime * shrinkSpeed;
		}

		transform.localScale = ls;
		
		if (transform.localScale.x < 0.1f) 
			Destroy(gameObject);

		light.range = transform.localScale.x * 3f;
		transform.eulerAngles += oriSpeed;
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
