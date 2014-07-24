using UnityEngine;



public class OfflineAvatar : MonoBehaviour {
	float ySpeed = 0.1f;
	float rHSpeed = 5f; // return home speed 
	// text 
	static float textH = 2.7f; // height 
	Vector3 textHome = new Vector3(0f, textH, 0f);
	// avatar 
	Vector3 avHome = Vector3.zero;
	float autoSpinSpeed = 70f;
	float yawSpeed = 10f;
	GameObject chara;



	void Start () { // for avatar 
		chara = GameObject.Find("dummyChara02");
	}
	
	void Update () {
		// text 
		if (Input.GetMouseButton(1))
			transform.Translate(Vector3.up * Input.GetAxis("Mouse Y") * ySpeed);
		else
			transform.position = Vector3.Lerp(
				transform.position, textHome, Time.deltaTime*rHSpeed);

		// avatar 
		if (Input.GetMouseButton(1)) {
			chara.transform.RotateAround(avHome, Vector3.up, -Input.GetAxis("Mouse X") * yawSpeed);
			chara.transform.Translate(Vector3.up * Input.GetAxis("Mouse Y") * ySpeed);
		}else {
			chara.transform.RotateAround(avHome, Vector3.up, Time.deltaTime * autoSpinSpeed);
			chara.transform.position = Vector3.Lerp(
				chara.transform.position, avHome, Time.deltaTime*rHSpeed);
		}
	}
}
