using UnityEngine;
using System.Collections;



public class CcBody : MonoBehaviour {
	public bool isGrounded = false;
	public bool JumpBoosted = false;
	public float radius = 0.5f;
	public float MiddleOfHead = 2f;
	public float SprintMultiplier = 2.0f;
	public Vector3 UpVector = new Vector3(0.0f, 1.0f, 0.0f);
	public bool sprinting = false;
	
	// powerups
	public float SpeedBoost = 2f;
	public float SpeedBoostEnd = 0f; // the time at which the speed boost ends
	public float SprintRegenSpeed = 0.3f; // set when you pick up a regen buff
	public float SprintRegenEnd = 0f;
	
	// private 
	float sprintActivatedTime = 0f;
	const float sprintDuration = 5f;
	
<<<<<<< HEAD
	public void TickEnergy () {
=======
	public void Move(Vector3 moveVector, bool startSprint = false) {
		// no move? no expensive spherecasts!
		if (moveVector.magnitude < 0.001f){
			//sprinting = false; // if the player stopped moving, they also stopped sprinting 
			return;
		}
		
		// sprinting
		if (startSprint) 
			sprinting = !sprinting;
>>>>>>> 13663e674c5907ccbcb7340e7469b108989d5f30
		
		if (sprintActivatedTime > sprintDuration) 
			sprinting = false;
		
		if (Time.time > SprintRegenEnd)
			SprintRegenSpeed = 0.3f;
		else
			SprintRegenSpeed = 1f;
		
		// regenerate energy
		if (!sprinting)
			sprintActivatedTime = Mathf.Max(sprintActivatedTime - Time.deltaTime * SprintRegenSpeed, 0f);
	}
	
	public void Move(Vector3 moveVector) {
		
		if (sprinting) {
			moveVector *= SprintMultiplier;
			sprintActivatedTime += Time.deltaTime;
		}
		
		if (Time.time <= SpeedBoostEnd) {
			moveVector *= SpeedBoost;
		}
		
		isGrounded = false;
		Ray coreRay = new Ray(transform.position, moveVector);
		RaycastHit coreHit = new RaycastHit();
		Ray headRay = new Ray(transform.position + MiddleOfHead * UpVector, moveVector);
		RaycastHit headHit = new RaycastHit();
		int collisionLayer = 1<<0;
		
		if (Physics.SphereCast(coreRay, radius, out coreHit, moveVector.magnitude, 1<<13)) JumpBoosted = true;
		else JumpBoosted = false;
		
		if (Physics.SphereCast(coreRay, radius, out coreHit, moveVector.magnitude, collisionLayer)) {
			transform.position = coreHit.point + (coreHit.normal*radius*1.1f);
		}else if (Physics.SphereCast(headRay, radius, out headHit, moveVector.magnitude, collisionLayer) &&
		          !Physics.Raycast(transform.position + MiddleOfHead * UpVector, -UpVector, MiddleOfHead, collisionLayer)) {
			transform.position = headHit.point + (headHit.normal*radius*1.1f) - MiddleOfHead * UpVector;
		}else{
			transform.position += moveVector;
		}
		
		Ray groundRay = new Ray(transform.position,-transform.up);
		RaycastHit groundHit = new RaycastHit();
		if (Physics.SphereCast(groundRay, radius, out groundHit, (MiddleOfHead*.5f)-(radius), collisionLayer)) {
			isGrounded = true;
			Vector3 standPos = groundHit.point + (groundHit.normal*radius) + (transform.up * ((MiddleOfHead*.5f)-(radius)));
			
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