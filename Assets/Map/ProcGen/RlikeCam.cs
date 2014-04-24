using UnityEngine;
using System.Collections;

public class RlikeCam : MonoBehaviour {

	GameObject nl;
	TrailRenderer tr;

	// Use this for initialization
	void Start () {
		nl = new GameObject("CoolThing");
		nl.AddComponent<TrailRenderer>();
		tr = nl.GetComponent<TrailRenderer>();
		tr.renderer.material = (Material)Resources.Load("Mat/Weap/Beam", typeof(Material));
		tr.time = 50f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localEulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * 2f, Input.GetAxis("Mouse X") * 2f, 0f);
		float speed = 10f;

		if (Input.GetKey ("left shift")) 
			speed *= 2f;
		if /**/ (Input.GetKey("w")) 
			transform.position += transform.forward * Time.deltaTime * speed;
		else if (Input.GetKey("s")) 
			transform.position -= transform.forward * Time.deltaTime * speed;
		if /**/ (Input.GetKey("a")) 
			transform.position -= transform.right * Time.deltaTime * speed;
		else if (Input.GetKey("d")) 
			transform.position += transform.right * Time.deltaTime * speed;
		if /**/ (Input.GetKey("c")) 
			transform.position -= transform.up * Time.deltaTime * speed;
		else if (Input.GetKey("e")) 
			transform.position += transform.up * Time.deltaTime * speed;

		if (Input.GetKeyDown("space")) 
			tr.enabled = !tr.enabled;

		nl.transform.position = transform.position + transform.up;
	}
}
