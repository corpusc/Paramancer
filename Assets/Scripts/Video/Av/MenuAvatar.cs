using UnityEngine;
using System.Collections;


public class MenuAvatar : MonoBehaviour {
	float autoSpinSpeed = 70f;
	float mouseSpeed = 100f;
	Vector3 startPos;
	Vector3 center;



	void Start () {
		startPos = transform.position;
		center = new Vector3(0f, 1f, 0f);
	}

	void Update () {
		if (Input.GetMouseButton(0) ||
		    Input.GetMouseButton(1)) 
		{
			transform.RotateAround(center, Vector3.up, -Input.GetAxis ("Mouse X") * Time.deltaTime * mouseSpeed);
			transform.Translate(Vector3.up * Input.GetAxis ("Mouse Y") * Time.deltaTime * (mouseSpeed/50));
		}else 
			transform.RotateAround(center, Vector3.up, Time.deltaTime * autoSpinSpeed);
	}
}
