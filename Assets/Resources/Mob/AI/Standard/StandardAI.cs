using UnityEngine;
using System.Collections;

public class StandardAI : MonoBehaviour {

	[HideInInspector]
	public Vector3 DesiredPos;
	public float MaxJumpHeight = 5f;
	public float MovementSpeed = 2f;
	public float BodyHeight = 0.5f;
	public float BodyWidth = 0.5f;
	public float Health = 1f;
	public float Gravity = 10f;

	Vector3 jumpVec;
	
	void Start () {
		jumpVec = new Vector3(0f, MaxJumpHeight, 0f);
	}

	void Update () {
		Vector3 ap = transform.position + Vector3.Normalize(DesiredPos - transform.position) * MovementSpeed * Time.deltaTime; // attempted pos
		RaycastHit hit;
		if (Physics.Raycast(transform.position, ap - transform.position, out hit, Vector3.Magnitude(ap - transform.position) + BodyWidth, 1<<0)) {
			transform.position = hit.point - Vector3.Normalize(ap - transform.position) * BodyWidth;
		} else {
			transform.position = ap;
		}
	}
}
