using UnityEngine;
using System.Collections;


public class MenuText : MonoBehaviour {
	float mouseMoveSpeed = 0.1f;
	Vector3 destination = new Vector3(0f, 2.5f, 0f);
	
	void Start () {
	}
	
	void Update () {
		if (Input.GetMouseButton(0) ||
		    Input.GetMouseButton(1))
				transform.Translate(Vector3.up * Input.GetAxis ("Mouse Y") * mouseMoveSpeed);
		else
			transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime);
	}
}