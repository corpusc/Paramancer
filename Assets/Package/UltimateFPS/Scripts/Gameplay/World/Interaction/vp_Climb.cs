/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Climb.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	A class to allow the player to climb objects.
//					This script attempts to keep the player a certain distance from
//					the object which is useful for climbing objects that are at an angle.
//					This script also takes over the regular player controls.
//					
//					Note:
//					This script works best if the object it's attached to has 2
//					child GameObjects, one named 'MountTop' which is to align the player
//					when interacting from the top of the object and 'MountBottom'
//					to align the player with the bottom of the object on interaciton.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_Climb : vp_Interactable
{
	
	[System.Serializable]
	public class vp_ClimbingSounds{
		public AudioSource AudioSource = null;
		public List<AudioClip> MountSounds = new List<AudioClip>();
		public List<AudioClip> DismountSounds = new List<AudioClip>();
		public float ClimbingSoundSpeed = 4;
		public Vector2 ClimbingPitch = new Vector2(1.0f, 1.5f);	// random pitch range for climbing
		public List<AudioClip> ClimbingSounds = new List<AudioClip>();
	}
	
	public float MinimumClimbSpeed = 3;
	public float ClimbSpeed = 16; // speed at which the player climbs this object
	public float MountSpeed = 5;
	public float DistanceToClimbable = 1; // the distance to keep from the climbable while climbing
	public float MinVelocityToClimb = 7; // minimum velocity in order to trigger this interactable
	public float ClimbAgainTimeout = 1; // Time in seconds before climbing on this climbable is allowed again
	public bool MountAutoRotatePitch = false; // if enabled, camera pitch will be rotated straight forward on mount
	public bool SimpleClimb = true; // if enabled, camera pitch will not alter climb direction
	public float DismountForce = 0.2f;
	public vp_ClimbingSounds Sounds;
	
	protected int m_LastWeaponEquipped = 0; // used to store our weapon so we can reequip it
	protected bool m_IsClimbing = false; // lets us know if we are climbing or not
	protected float m_CanClimbAgain = 0; // timer for ClimbAgainThreshold
	protected Vector3 m_CachedDirection = Vector3.zero; // cache the direction to keep proper distance from climbable
	protected Vector2 m_CachedRotation = Vector2.zero; // cache the rotation so we can limit yaw
	protected vp_Timer.Handle m_ClimbingSoundTimer = new vp_Timer.Handle();
	protected AudioClip m_SoundToPlay = null;				// the current sound to be played
	protected AudioClip m_LastPlayedSound = null;			// used to make sure we don't place the same sound twice in a row
	
	
	protected override void Start()
	{
		
		base.Start();
		
		m_CanClimbAgain = Time.time;
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override bool TryInteract(vp_FPPlayerEventHandler player){
		
		if(!enabled)
			return false;
		
		if(Time.time < m_CanClimbAgain)
			return false;
		
		if(m_IsClimbing)
		{
			m_Player.Climb.TryStop();
			return false;
		}
		
		if(m_Player == null)
			m_Player = player;
		
		if(m_Player.Interactable.Get() != null)
			return false;
		
		if(m_Controller == null)
			m_Controller = m_Player.GetComponent<vp_FPController>();
		
		if(m_Controller.Velocity.magnitude > MinVelocityToClimb)
			return false;
		
		if(m_Camera == null)
			m_Camera = m_Player.GetComponentInChildren<vp_FPCamera>();
		
		float angle = Vector3.Angle(-m_Transform.forward, m_Transform.position - m_Camera.Transform.position);
		if (Mathf.Abs(angle) > 75 && m_Controller.Transform.position.y < GetTopOfCollider(m_Transform))
    		return false;
		
		if(Sounds.AudioSource == null)
			Sounds.AudioSource = m_Player.audio;
		
		m_Player.Register(this);
		
		m_Player.Interactable.Set(this); // sets what the player is currently interacting with
		
		return m_Player.Climb.TryStart();
		
	}
	
	
	/// <summary>
	/// Sets up some things to allow climbing to work correctly. For one
	/// we need to override normal input so we can use our custom climbing input.
	/// </summary>
	protected virtual void OnStart_Climb()
	{	
		
		// turn physics off
		m_Controller.PhysicsGravityModifier = 0.0f;

		// reset camera initial rotation (otherwise the camera
		// will be rotated with the offset it had at spawn-time)
		m_Camera.SetRotation(m_Camera.Transform.eulerAngles, false, true);

		// stop jumping
		m_Player.Jump.Stop();
		
		// disallow normal input while climbing
		m_Player.AllowGameplayInput.Set(false);
		
		// stop any movement on our controller. this is helpful in case we jumped onto the climbable.
		m_Player.Stop.Send();
		
		// let's unequip our weapon
		m_LastWeaponEquipped = m_Player.CurrentWeaponID.Get();
		
		// put the weapon away
		m_Player.SetWeapon.TryStart(0);
		
		m_Player.Interactable.Set(null);
		
		PlaySound(Sounds.MountSounds);

		if (m_Controller.Transform.collider.enabled && m_Transform.collider.enabled)
			Physics.IgnoreCollision(m_Controller.Transform.collider, m_Transform.collider, true); // ignore collisions with this object
		
		// start the initial mounting of the climbable object
		StartCoroutine("LineUp");
		
	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void PlaySound( List<AudioClip> sounds )
	{
		
		if(Sounds.AudioSource == null)
			return;
		
		if(sounds == null || sounds.Count == 0)
			return;
		
		reroll:
		m_SoundToPlay = sounds[Random.Range(0,sounds.Count)];
		
		if(m_SoundToPlay == null)
			return;
		
		if (m_SoundToPlay == m_LastPlayedSound && sounds.Count > 1)
			goto reroll;
		
		if(sounds == Sounds.ClimbingSounds)
			Sounds.AudioSource.pitch = Random.Range(Sounds.ClimbingPitch.x, Sounds.ClimbingPitch.y) * Time.timeScale;
		else
			Sounds.AudioSource.pitch = 1;
	
		Sounds.AudioSource.PlayOneShot( m_SoundToPlay );
		m_LastPlayedSound = m_SoundToPlay;
		
	}
	
	
	/// <summary>
	/// This lines the player up with the climbable and rotates the player
	/// towards the climbable. The duration is calculated based on distance.
	/// </summary>
	protected virtual IEnumerator LineUp()
	{
		
		Vector3 startPosition = m_Player.Position.Get(); // cache the start position
		Vector3 endPosition = GetNewPosition(); // cache the end position
		Quaternion startingRotation = m_Camera.transform.rotation; // cache start rotation
		Quaternion endRotation = Quaternion.LookRotation(-m_Transform.forward); // cache end rotation



		// set a bool to test if we are starting our climb from the top or not
		bool fromTop = m_Controller.Transform.position.y > m_Transform.collider.bounds.center.y;

		// modify the ending position based on where we start our climb
		if(fromTop)
			endPosition += Vector3.down * m_Controller.CharacterController.height; // modifies the ending position to be down from the top a little
		else
			endPosition += m_Controller.Transform.up * (m_Controller.CharacterController.height / 2); // modifies the ending position to be up from the bottom a little

		// modify the ending rotation based on where we start our climb
		if (fromTop && m_Transform.InverseTransformDirection(m_Player.Forward.Get()).z > 0.0f)
			endRotation = Quaternion.Euler(new Vector3(45.0f, endRotation.eulerAngles.y, endRotation.eulerAngles.z));
		else
			endRotation = Quaternion.Euler(new Vector3(-45.0f, endRotation.eulerAngles.y, endRotation.eulerAngles.z));

		// align our end position with the center of this climbable
		endPosition = new Vector3(m_Transform.collider.bounds.center.x, endPosition.y, m_Transform.collider.bounds.center.z);
		endPosition += m_Transform.forward;
		
		float t = 0;
		
		float duration = Vector3.Distance(m_Controller.Transform.position, endPosition) / (!fromTop ? MountSpeed / 1.25f : MountSpeed);
		
		// moves the player to initial climbing position over a short period
		while(t < 1)
		{	
			t += Time.deltaTime/duration;
			
			Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, t);
			m_Player.Position.Set(newPosition);

			Quaternion newRotation = Quaternion.Slerp(startingRotation, endRotation, t);
			m_Player.Rotation.Set(new Vector2(MountAutoRotatePitch ? newRotation.eulerAngles.x : m_Player.Rotation.Get().x, newRotation.eulerAngles.y));
			
			yield return new WaitForEndOfFrame();
		}
		
		m_CachedDirection = m_Camera.Transform.forward;
		m_CachedRotation = m_Player.Rotation.Get();
		
		m_IsClimbing = true; // allow the player to start climbing by giving them back the controls
		
	}
	
	
	/// <summary>
	/// Stops climbing by reverting our player back to a normal state
	/// </summary>
	protected virtual void OnStop_Climb()
	{	
		
		m_Player.Interactable.Set(null);
		
		// re-allow normal input
		m_Player.AllowGameplayInput.Set(true);
		
		// lets reequip the weapon we had
		m_Player.SetWeapon.TryStart(m_LastWeaponEquipped);
		
		m_Player.Unregister(this);
		
		m_CanClimbAgain = Time.time + ClimbAgainTimeout;

		if (m_Controller.Transform.collider.enabled && m_Transform.collider.enabled)
			Physics.IgnoreCollision(m_Controller.Transform.collider, m_Transform.collider, false); // allow player to collide with this object again
		
		PlaySound(Sounds.DismountSounds);

		// add a force repelling player from climbable upon dismount.
		Vector3 force = m_Controller.Transform.forward * DismountForce;
		if (m_Transform.collider.bounds.center.y < m_Player.Position.Get().y)
		{
			force *= 2.0f;
			force.y = DismountForce * 0.5f;	// dismounting at top: add slight up-force (push forward & up)
		}
		else
		{
			force = -force * 0.5f;		// dismounting at bottom: invert force (push back & down)
		}
		m_Player.Stop.Send();
		m_Controller.AddForce(force);
		
		m_IsClimbing = false; // give control back to player
		m_Player.SetState("Default"); // restore default state on all components

		// interpolate camera pitch to look straight ahead
		StartCoroutine("RestorePitch");

	}


	/// <summary>
	/// This restores player pitch after dismounting the climbable.
	/// </summary>
	protected virtual IEnumerator RestorePitch()
	{

		float t = 0;

		// rotates the camera pitch to straight ahead over a short period
		while (t < 1 && Input.GetAxisRaw("Mouse Y") == 0.0f)	// just don't interfere with mouselook
		{

			t += Time.deltaTime;
			m_Player.Rotation.Set(Vector2.Lerp(m_Player.Rotation.Get(), new Vector2(0.0f, m_Player.Rotation.Get().y), t));
			yield return new WaitForEndOfFrame();

		}

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Interact' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Interact()
	{
		
		if(m_IsClimbing)
			m_Player.Climb.TryStop();
		
		return true;
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void FixedUpdate()
	{
		
		Climbing();
		
		InputJump();
		
	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnStart_Dead()
	{
		
		FinishInteraction();
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override void FinishInteraction()
	{
		
		if( m_IsClimbing )
			m_Player.Climb.TryStop();
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Climbing()
	{
		
		 // no need to run through here if we aren't climbing
		if(m_Player == null || !m_IsClimbing)
			return;
		
		// turn physics off
		m_Controller.PhysicsGravityModifier = 0.0f;
		
		// limit yaw and pitch
		m_Camera.RotationYawLimit = new Vector2(m_CachedRotation.y - 90, m_CachedRotation.y + 90);
		m_Camera.RotationPitchLimit = new Vector2(90, -90);
		
		// Store the position so we can alter it towards the climbable
		Vector3 newPosition = GetNewPosition();
		
		// get our move vector from controls and camera direction
		Vector3 moveVector = Vector3.zero;
		float pitch = m_Player.Rotation.Get().x / 90;
		
		float minimumClimbSpeed = MinimumClimbSpeed / ClimbSpeed;
		
		if(Mathf.Abs( pitch ) < minimumClimbSpeed)
			pitch = pitch > 0 ? minimumClimbSpeed : minimumClimbSpeed * -1;
		
		if(pitch < 0)
			moveVector = Vector3.up * -pitch;
		else if(pitch > 0)
			moveVector = Vector3.down * pitch;
		
		float speed = ClimbSpeed;
		float testDirection = (moveVector * Input.GetAxisRaw("Vertical")).y;
		if(SimpleClimb)
		{
			moveVector = Vector3.up;
			speed *= .75f;
			testDirection = Input.GetAxisRaw("Vertical");
		}

		// If we reach the top or bottom of the current climbable, stop climbing
		if((testDirection > 0 && newPosition.y > GetTopOfCollider(m_Transform) - m_Controller.CharacterController.height * 0.25f) ||
			(testDirection < 0 && m_Controller.Grounded && m_Controller.GroundTransform.GetInstanceID() != m_Transform.GetInstanceID()))
		{
			m_Player.Climb.TryStop();
			return;
		}
		
		// cancel the climbing sound timer if not moving
		if(Input.GetAxisRaw("Vertical") == 0)
			m_ClimbingSoundTimer.Cancel();
		
		// play climbing sounds
		if(Input.GetAxisRaw("Vertical") != 0 && !m_ClimbingSoundTimer.Active && Sounds.ClimbingSounds.Count > 0)
		{
			float t = Mathf.Abs((5 / moveVector.y) * (Time.deltaTime * 5) / Sounds.ClimbingSoundSpeed);
			vp_Timer.In( SimpleClimb ? t*3 : t, delegate() {
				PlaySound(Sounds.ClimbingSounds);
			}, m_ClimbingSoundTimer);
		}
			
		// Move our player by the climbSpeed
		newPosition += moveVector * speed * Time.deltaTime * Input.GetAxisRaw("Vertical");
		
		m_Player.Position.Set( Vector3.Slerp( m_Controller.Transform.position, newPosition, Time.deltaTime * speed) );
		
	}
	
	
	/// <summary>
	/// a helper to get the position of our player for the next frame
	/// </summary>
	protected virtual Vector3 GetNewPosition()
	{
			
		Vector3 newPosition = m_Controller.Transform.position;
		
		RaycastHit hit;
		Ray ray = new Ray(m_Controller.Transform.position, m_CachedDirection);
		Physics.Raycast(ray, out hit, DistanceToClimbable * 4);
		
		if(hit.collider != null)
			// check if we are within the threshold of the distance to the climbable and we hit the m_Transform
			if(hit.transform.GetInstanceID() == m_Transform.GetInstanceID() && (hit.distance > DistanceToClimbable || hit.distance < DistanceToClimbable))
				// reset the position to be within our distanceToClimbable threshold
	  			newPosition = (newPosition - hit.point).normalized * DistanceToClimbable + hit.point;
		
		return newPosition;
		
	}
	
	
	/// <summary>
	/// If trying to jump while climbing, jump off the climbable
	/// </summary>
	protected virtual void InputJump()
	{
		
		if((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.F)) && m_Player != null && m_IsClimbing)
		{
			m_Player.Climb.TryStop();
			
			if(Input.GetButtonDown("Jump"))
				m_Controller.AddForce(-m_Controller.Transform.forward * m_Controller.MotorJumpForce);
		}
		
	}
	
	
	/// <summary>
	/// Utility method to find the top of a collider
	/// </summary>
	public static float GetTopOfCollider(Transform t)
	{
		
		return t.position.y + t.collider.bounds.size.y/2;
		
	}
	

}
