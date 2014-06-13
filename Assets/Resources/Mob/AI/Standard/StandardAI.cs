using UnityEngine;
using System.Collections;



public class StandardAI : MonoBehaviour {
	[HideInInspector]
	public Vector3 TargetPos; // the position of the player targeted by the AI 
	[HideInInspector]
	public Vector3 DesiredPos; // next path node that the AI is heading towards 
	public float MovementSpeed = 5f;
	public float Health = 1f;

	// private 
	GameObject player;
	bool prevVisible = true;

	  
	 
	void Update() {
		player = GameObject.Find("FPSEntity(Clone)"); // put here for debug reasons 
		TargetPos = player.transform.position;

		if (Vector3.SqrMagnitude(TargetPos - transform.position) < 10000f) {
			if (
				Physics.Raycast(
					transform.position, 
            		(TargetPos - transform.position).normalized, 
	            	Vector3.Distance(
						transform.position, 
						TargetPos
					), 
            		1<<0
            	)
		    ) {
				if (prevVisible) {
					DesiredPos += (DesiredPos - TargetPos).normalized * 2f;
					prevVisible = false;
				}
			} else {
				DesiredPos = TargetPos;
				prevVisible = true;
			}
		} else {
			DesiredPos = new Vector3(0f, 0f, 0f);
			prevVisible = true;
		}

		if (Health > 0f) {
			rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, (DesiredPos - transform.position).normalized * MovementSpeed, Time.deltaTime);
			transform.forward = Camera.main.transform.forward;
		}
	}
}
