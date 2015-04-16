using UnityEngine;
using System.Collections;



public class TestCam : MonoBehaviour {
	void Start() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void OnGUI() {
		if (Cursor.lockState == CursorLockMode.Locked) {
			GUI.Label(new Rect(Screen.width/2, Screen.height/2, 99, 99), "P");
		}
	}
	
	void Update() {
		float speed = 10f;

		if (Cursor.lockState == CursorLockMode.Locked) {
			transform.localEulerAngles += new Vector3(
				-Input.GetAxis("Mouse Y") * 2f, 
				Input.GetAxis("Mouse X") * 2f, 
				0f);
		}else{
		}


		if /**/ (CcInput.Holding(UserAction.Sprint))       speed *= 4f;
		if /**/ (CcInput.Started(UserAction.Alt)) {
			if (Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}else{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = true;
			}
		}

		if /**/ (CcInput.Holding(UserAction.MoveForward))  transform.position += transform.forward * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveBackward)) transform.position -= transform.forward * Time.deltaTime * speed;
		if /**/ (CcInput.Holding(UserAction.MoveLeft))     transform.position -= transform.right * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveRight))    transform.position += transform.right * Time.deltaTime * speed;
		if /**/ (CcInput.Holding(UserAction.MoveUp))       transform.position += transform.up * Time.deltaTime * speed;
		else if (CcInput.Holding(UserAction.MoveDown))     transform.position -= transform.up * Time.deltaTime * speed;
	}
}
