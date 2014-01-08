using UnityEngine;
using System.Collections;



public class Avatar : MonoBehaviour {
	public bool isGrounded = false;
	public float radius = 0.5f;
	public float height = 1.8f;
	
	
	
	public void Move(Vector3 moveVector) {
		// no move? no expensive spherecasts!
		if (moveVector.magnitude <0.001f) return; 
		
		// let's go
		isGrounded = false;
		Ray coreRay = new Ray(transform.position, moveVector);
		RaycastHit coreHit = new RaycastHit();
		int collisionLayer = 1<<0;
		
		if (Physics.SphereCast(coreRay, radius, out coreHit, moveVector.magnitude, collisionLayer)) {
			transform.position = coreHit.point + (coreHit.normal*radius*1.1f);
		}else{
			transform.position += moveVector;
		}
		
		Ray groundRay = new Ray(transform.position,-transform.up);
		RaycastHit groundHit = new RaycastHit();
		if (Physics.SphereCast(groundRay, radius, out groundHit, (height*.5f)-(radius), collisionLayer)) {
			isGrounded = true;
			Vector3 standPos = groundHit.point + (groundHit.normal*radius) + (transform.up * ((height*.5f)-(radius)));
			
			Ray standRay = new Ray(transform.position, standPos-transform.position);
			RaycastHit standHit = new RaycastHit();
			float standLength = Vector3.Distance(standPos, transform.position);
			if (Physics.SphereCast(standRay, radius, out standHit, standLength, collisionLayer)) {
				standPos = standHit.point + (standHit.normal * radius*1.1f);
			}
			
			transform.position = standPos;
		}
	}
}
