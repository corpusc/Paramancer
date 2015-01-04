using UnityEngine;
using System.Collections;



public class AI : MonoBehaviour {
	[HideInInspector]	public Actor EC;
	[HideInInspector]	public Vector3 TargetPos;
	[HideInInspector]	public Vector3 DesiredPos;
	[HideInInspector]	public GameObject FraggedBy;
	public float MovementSpeed = 5f;
	public NetworkViewID viewID;
	
	// private 
	bool prevVisible = true; // if the target was visible in the previous frame 



	void Start() {
		// if an AI component is attached, it means that we are a mob 
		EC.isMob = true;
	}
	
//	void Update() {
//		if (EC.bod.Health <= 0f) {
//			EC.MakeBombInvisible();
//		}
//		
//		// item pick up 
//		if (EC.bod.Health > 0f) {
//			EC.HandlePickingUpItem();
//		}
//		
//		if (Vector3.SqrMagnitude(TargetPos - transform.position) < 10000f) {
//			if (Physics.Raycast(transform.position, 
//			                    (TargetPos - transform.position).normalized, 
//			                    Vector3.Distance(
//				transform.position, 
//				TargetPos), 
//			                    1<<0)) {
//				if (prevVisible) {
//					DesiredPos += (DesiredPos - TargetPos).normalized * 2f;
//					prevVisible = false;
//				}
//			} else {
//				DesiredPos = TargetPos;
//				prevVisible = true;
//			}
//		} else {
//			DesiredPos = new Vector3(0f, 0f, 0f);
//			prevVisible = true;
//		}
//		
//		// if alive 
//		if (EC.bod.Health > 0f) {
//			// movement & other stuff 
//			Vector3 lastPos = transform.position;
//
//			rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, (DesiredPos - transform.position).normalized * MovementSpeed, Time.deltaTime);
//			transform.forward = Camera.main.transform.forward;
//
//			Ray lavaRay = new Ray(lastPos, transform.position - lastPos);
//			RaycastHit lavaHit = new RaycastHit();
//			float lavaRayLength = Vector3.Distance(transform.position, lastPos);
//			int lavaLayer = (1<<10);
//			if (Physics.Raycast(lavaRay, out lavaHit, lavaRayLength, lavaLayer)) {
//				transform.position = lavaHit.point + Vector3.up * 0.35f;
//				EC.bod.Health = 0f;
//				EC.net.RegisterHit(Gun.Lava, viewID, viewID, lavaHit.point);
//			}
//		}
//	}
}
