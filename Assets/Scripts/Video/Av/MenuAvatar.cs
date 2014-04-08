using UnityEngine;
using System.Collections;


public class MenuAvatar : MonoBehaviour {
	float autoSpinSpeed = 70f;
	float mouseSpeed = 10f;
	float mouseMoveSpeed = 0.1f;
	Vector3 center;



	void Start () {
		center = new Vector3(0f, 1f, 0f);
	}

	void Update () {
		if (Input.GetMouseButton(0) ||
		    Input.GetMouseButton(1)) 
		{
			transform.RotateAround(center, Vector3.up, -Input.GetAxis ("Mouse X") * mouseSpeed);
			transform.Translate(Vector3.up * Input.GetAxis ("Mouse Y") * mouseMoveSpeed);
		}else {
			transform.RotateAround(center, Vector3.up, Time.deltaTime * autoSpinSpeed);
			transform.position = Vector3.Lerp(transform.position, Vector3.zero, Time.deltaTime);
		}
	}
}
