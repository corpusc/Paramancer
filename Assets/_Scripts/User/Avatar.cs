using UnityEngine;
using System.Collections;



public class Avatar : MonoBehaviour {
	public bool isGrounded = false;
	public float radius = 0.5f;
	public float height = 1.8f;
	public float SprintMultiplier = 2.0f;
	
	private bool sprinting = true;
	private float sprintActivatedTime = 0f;
	private const float sprintDuration = 5f;
	
	public void Move(Vector3 moveVector, bool StartSprint = false) {
		// no move? no expensive spherecasts!
		if (moveVector.magnitude <0.001f){
			//sprinting = false; //if the player stopped moving, they also stopped sprinting
			return;
		}
		
		//if(StartSprint) sprinting = true;
		if (StartSprint) 
			sprinting = !sprinting;
		
		// let's go
		if (sprintActivatedTime > sprintDuration) 
			sprinting = false;
		
		if (sprinting) {
			moveVector *= SprintMultiplier;
			sprintActivatedTime += Time.deltaTime;
		}else 
			sprintActivatedTime = (sprintActivatedTime - Time.deltaTime < 0f) ? 0f : (sprintActivatedTime - Time.deltaTime);
		
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
	
	public float GetEnergy() {
		return (sprintDuration - sprintActivatedTime) / sprintDuration;
	}
	
}
