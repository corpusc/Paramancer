using UnityEngine;
using System.Collections;

public class MenuText : MonoBehaviour { //playername text
	float speed = 10f;
	float mouseSpeed = 2f;
	Vector3 startPos;
	Vector3 center;

	void Start () {
		startPos = transform.position;
		center = new Vector3(0f, 1f, 0f);
	}

	void Update () {
		if (Input.GetMouseButtonUp(1)) {
			transform.position = startPos;
		}
		if (Input.GetMouseButton(1)) {
			transform.RotateAround(center, Vector3.up, -Input.GetAxis ("Mouse X") * mouseSpeed);
			transform.RotateAround(center, Vector3.left, -Input.GetAxis ("Mouse Y") * mouseSpeed);
		} else transform.RotateAround(center, Vector3.up, Time.deltaTime * speed);

		transform.rotation = Camera.main.transform.rotation;
	}
}
