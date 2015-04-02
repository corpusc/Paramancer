using UnityEngine;
using System.Collections;



public class TestCam : MonoBehaviour {
	void Start() {
		Screen.lockCursor = true;
	}

	void OnGUI() {
		if (Screen.lockCursor) {
			GUI.Label(new Rect(Screen.width/2, Screen.height/2, 99, 99), "P");
		}
	}
	
	void Update() {
		float speed = 10f;

		if (Screen.lockCursor) {
			transform.localEulerAngles += new Vector3(
				-Input.GetAxis("Mouse Y") * 2f, 
				Input.GetAxis("Mouse X") * 2f, 
				0f);
		}else{
		}


		if /**/ (CcInput.Holding(UserAction.Sprint))       speed *= 4f;
		if /**/ (CcInput.Started(UserAction.Alt)) {
			if (Screen.lockCursor) {
				Screen.lockCursor = false;
				Cursor.visible = true;
			}else{
				Screen.lockCursor = true;
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
