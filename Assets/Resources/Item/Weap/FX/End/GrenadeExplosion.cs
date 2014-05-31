using UnityEngine;
using System.Collections;

public class GrenadeExplosion : MonoBehaviour {
	bool expanding = true;


	void Start() {
		var f = 0.2f;
		transform.localScale = new Vector3(f,f,f);
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
	}
}
