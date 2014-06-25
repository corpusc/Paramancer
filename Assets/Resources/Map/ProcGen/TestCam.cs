using UnityEngine;
using System.Collections;



public class TestCam : MonoBehaviour {
	GameObject o;
	TrailRenderer tr;



	void Start() {
		Screen.lockCursor = true;
		o = new GameObject("Camera Trail");
		o.AddComponent<TrailRenderer>();
		tr = o.GetComponent<TrailRenderer>();
		tr.renderer.material = Mats.Get("TrailJagged");
		tr.renderer.material.color = Color.red;
		tr.time = 4f;
	}
	
	void Update() {
		transform.localEulerAngles += new Vector3(
			-Input.GetAxis("Mouse Y") * 2f, 
			Input.GetAxis("Mouse X") * 2f, 
			0f);
		float speed = 10f;

		if /**/ (CcInput.Holding(UserAction.Sprint))       speed *= 4f;

		if /**/ (CcInput.Holding(UserAction.MoveForward))  transform.position += transform.forward * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveBackward)) transform.position -= transform.forward * Time.deltaTime * speed;
		if /**/ (CcInput.Holding(UserAction.MoveLeft))     transform.position -= transform.right * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveRight))    transform.position += transform.right * Time.deltaTime * speed;
		if /**/ (CcInput.Holding(UserAction.MoveUp))       transform.position += transform.up * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveDown))     transform.position -= transform.up * Time.deltaTime * speed;

		if (Input.GetKeyDown("space")) 
			tr.enabled = !tr.enabled;

		o.transform.position = transform.position;// + transform.up;
	}
}
