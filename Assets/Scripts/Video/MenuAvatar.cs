using UnityEngine;
using System.Collections;

public class MenuAvatar : MonoBehaviour {
	float speed = 10f;
	float mouseSpeed = 2f;
	Quaternion startRot;
	Vector3 startPos;
	Vector3 center;

	void Start () {
		startRot = transform.rotation;
		startPos = transform.position;
		center = new Vector3(0f, 1f, 0f);
	}

	void Update () {
		if (Input.GetMouseButtonUp(1)) {
			transform.rotation = startRot;
			transform.position = startPos;
		}
		if (Input.GetMouseButton(1)) {
			transform.RotateAround(center, Vector3.up, -Input.GetAxis ("Mouse X") * mouseSpeed);
			transform.RotateAround(center, Vector3.left, -Input.GetAxis ("Mouse Y") * mouseSpeed);
		} else transform.RotateAround(center, Vector3.up, Time.deltaTime * speed);
	}
}
