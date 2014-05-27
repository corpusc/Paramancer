using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {
	void Update() {
		light.range -= Time.deltaTime * 20f;

		if (light.range < 1f) 
			Destroy(gameObject);
	}
}
