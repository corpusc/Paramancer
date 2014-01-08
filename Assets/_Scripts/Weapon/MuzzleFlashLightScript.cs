using UnityEngine;
using System.Collections;

public class MuzzleFlashLightScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		light.range -= Time.deltaTime * 20f;
		if (light.range<1f) Destroy(gameObject);
	}
}
