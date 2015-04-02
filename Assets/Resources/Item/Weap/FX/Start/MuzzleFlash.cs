using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {
	void Update() {
		GetComponent<Light>().range -= Time.deltaTime * 20f;

		if (GetComponent<Light>().range < 1f) 
			Destroy(gameObject);
	}
}
