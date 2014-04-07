using UnityEngine;
using System.Collections;

public class MenuCamera : MonoBehaviour {
	public float DistanceFromChar = 5f;

	// private
	float currAng = 180f; // current angle 
	float moveSpeed = 30f;
	float height = 1.5f;
	Vector3 lookAtOffset;
	float lookAtSpeed = 0.13f;
	float maxOffset = 2f;



	void Start() {
		lookAtOffset = new Vector3(0f, 0f, 0f);

		transform.position = new Vector3(
			Mathf.Sin(Mathf.Deg2Rad * currAng) * DistanceFromChar, 
			height, 
			Mathf.Cos(Mathf.Deg2Rad * currAng) * DistanceFromChar);
	}
	
	void Update() {
		currAng += moveSpeed * Time.deltaTime;

		lookAtOffset = new Vector3(
			Mathf.Sin(Mathf.Deg2Rad * currAng * lookAtSpeed), 
			Mathf.Sin(currAng * 0.3f * Mathf.Deg2Rad) * 0.3f, 
			Mathf.Sin(Mathf.Deg2Rad * currAng * lookAtSpeed)
		) * maxOffset;

		transform.position = new Vector3(
			Mathf.Sin(Mathf.Deg2Rad * currAng) * DistanceFromChar, 
			height, 
			Mathf.Cos(Mathf.Deg2Rad * currAng) * DistanceFromChar);

		transform.LookAt(Vector3.up * height + lookAtOffset);
	}
}
