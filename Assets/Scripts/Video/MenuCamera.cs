using UnityEngine;
using System.Collections;

public class MenuCamera : MonoBehaviour {

	float currentAngle = 180f;
	float moveSpeed = 30f;
	float height = 2f;
	Vector3 lookAtOffset;
	float lookAtSpeed = 0.13f;
	float maxOffset = 2f;
	public float DistanceFromChar = 5f;

	// Use this for initialization
	void Start () {
		lookAtOffset = new Vector3(0f, 0f, 0f);
		transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * currentAngle) * DistanceFromChar, height, Mathf.Cos(Mathf.Deg2Rad * currentAngle) * DistanceFromChar);
	}
	
	// Update is called once per frame
	void Update () {
		currentAngle += moveSpeed * Time.deltaTime;
		lookAtOffset = new Vector3(Mathf.Sin(Mathf.Deg2Rad * currentAngle * lookAtSpeed), Mathf.PingPong(currentAngle * 0.003f, 0.5f), Mathf.Sin(Mathf.Deg2Rad * currentAngle * lookAtSpeed)) * maxOffset;
		transform.position = new Vector3(Mathf.Sin(Mathf.Deg2Rad * currentAngle) * DistanceFromChar, height, Mathf.Cos(Mathf.Deg2Rad * currentAngle) * DistanceFromChar);
		transform.LookAt(Vector3.up * height + lookAtOffset);
	}
}
