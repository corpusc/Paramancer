using UnityEngine;
using System.Collections;

public class MenuCamera : MonoBehaviour {

	float speed = 0.1f;
	float currentY = 1f;

	void Start() {
	}
	
	void Update() {
		if (Input.GetMouseButton(0) ||
		    Input.GetMouseButton(1))
			currentY += Input.GetAxis("Mouse Y") * speed;
		else
			currentY = Mathf.Lerp(currentY, 1f, Time.deltaTime);

		transform.LookAt(new Vector3(-2.5f, currentY, 0f));
	}
}
