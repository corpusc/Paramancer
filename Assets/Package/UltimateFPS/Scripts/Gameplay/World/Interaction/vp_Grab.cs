/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Grab.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This script allows the player to grab objects in the game world
//					as long as they have a collider and a rigidbody. It forces the
//					'vp_InteractionType.Normal'. Once the object is picked up, the
//					current weapon is put away. When using the input key for
//					interaction again, the object will be thrown, inheriting the
//					player's force. Similar to weapons, it has sophisticated logic
//					to animate the object procedurally while carried. also, it has
//					functions to handle various physics and collision cases.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class vp_Grab : vp_Interactable
{

	// object properties
	public string OnGrabText = "";

	// icons
	public Texture GrabStateCrosshair = null; // crosshair to display while holding this object

	// physics
	public float FootstepForce = 0.015f;		// how much our footsteps affect object motion
	public float Kneeling = 5.0f;			// to what extent fall impact will affect object motion
	public float Stiffness = 0.5f;			// how sluggishly the object will react when moved
	public float ShakeSpeed = 0.1f;
	public Vector3 ShakeAmplitude = Vector3.one;
	public float ThrowStrength = 6;			// force applied to object when thrown
	public bool AllowThrowRotation = true;	// whether the object will rotate when thrown
	public float Burden = 0.0f;				// (0-1) will slow down the player accordingly
	public int MaxCollisionCount = 20;
	protected Vector3 m_CurrentShake = Vector3.zero;
	protected Vector3 m_CurrentRotationSway = Vector3.zero;
	protected Vector2 m_CurrentMouseMove;
	protected Vector3 m_CurrentSwayForce;
	protected Vector3 m_CurrentSwayTorque;
	protected float m_CurrentFootstepForce = 0.0f;
	protected Vector3 m_CurrentHoldAngle = Vector3.one;
	protected Collider m_LastExternalCollider = null;
	protected int m_CollisionCount = 0;

	// variables for grabbing logic
	public Vector3 CarryingOffset = new Vector3(0.0f, -0.5f, 1.5f);	// determines how the object is carried in relation to our body
	public float CameraPitchDownLimit = 0.0f;						// restricts the camera's downward pitch while carrying this object
	protected bool m_IsFetching = false;
	protected float duration = 0.0f;
	protected float m_FetchProgress = 0;
	protected vp_FPInteractManager m_InteractManager = null;	// caches the interaction manager
	protected AudioSource m_Audio = null;
	protected int m_LastWeaponEquipped = 0;						// used to store the id of our weapon so we can reequip it
	protected bool m_IsGrabbed = false;
	protected float m_OriginalPitchDownLimit = 0.0f;			// for restoring camera pitch

	// variables for backup of rigidbody properties
	protected bool m_DefaultGravity;
	protected float m_DefaultDrag;
	protected float m_DefaultAngularDrag;

	// sounds
	public Vector2 SoundsPitch = new Vector2(1.0f, 1.5f);	// random pitch range for grab and release sounds
	public List<AudioClip> GrabSounds = new List<AudioClip>(); // list of sounds to randomly play on grab
	public List<AudioClip> DropSounds = new List<AudioClip>(); // list of sounds to randomly play on drop
	public List<AudioClip> ThrowSounds = new List<AudioClip>(); // list of sounds to randomly play on throw

	// timers
	protected vp_Timer.Handle m_DisableAngleSwayTimer = new vp_Timer.Handle();


	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{

		base.Start();

		if (!rigidbody || !collider)
			this.enabled = false;

		// cache rigidbody vars
		if (rigidbody != null)
		{
			m_DefaultGravity = rigidbody.useGravity;
			m_DefaultDrag = m_Transform.rigidbody.drag;
			m_DefaultAngularDrag = m_Transform.rigidbody.angularDrag;
		}

		// for normal interaction type
		InteractType = vp_InteractType.Normal;

		m_InteractManager = GameObject.FindObjectOfType(typeof(vp_FPInteractManager)) as vp_FPInteractManager;

	}



	/// <summary>
	/// 
	/// </summary>
	protected virtual void FixedUpdate()
	{

		if (!m_IsGrabbed || (m_Transform.parent == null))
			return;

		UpdateInput();

		UpdateShake();

		UpdatePosition();

		UpdateRotation();

		UpdateBurden();

		DampenForces();

	}


	/// <summary>
	/// Handles mouse input and special cases for dropping the
	/// grabbable versus putting a weapon away.
	/// </summary>
	protected virtual void UpdateInput()
	{

		m_CurrentMouseMove.x = Input.GetAxisRaw("Mouse X") * Time.timeScale;
		m_CurrentMouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.timeScale;

		// toss object upon fire button pressed
		if (Input.GetButton("Fire1"))
		{
			m_Player.Interact.TryStart();
			return;
		}

		// force-prevent wielding weapons and grabbables at the same time
		if (m_Player.CurrentWeaponID.Get() != 0)
		{
			m_Player.SetWeapon.TryStart(0);
			return;
		}

	}


	/// <summary>
	/// Adds a subtle, realistic ambient shake to the grabbable.
	/// </summary>
	protected virtual void UpdateShake()
	{

		m_CurrentShake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
		m_Transform.localEulerAngles += m_CurrentShake;

	}


	/// <summary>
	/// Interpolates the object smoothly to the player's grip,
	/// and applies positional sway while moving around.
	/// </summary>
	protected virtual void UpdatePosition()
	{

		// calculate positional sway force
		m_CurrentSwayForce += m_Player.Velocity.Get() * 0.005f;
		m_CurrentSwayForce.y += m_CurrentFootstepForce;
		m_CurrentSwayForce += m_Camera.Transform.TransformDirection(new Vector3(
			m_CurrentMouseMove.x * 0.05f,
			// prevent vertical sway if we hit lower pitch limit
			(m_Player.Rotation.Get().x > m_Camera.RotationPitchLimit.y) ? m_CurrentMouseMove.y * 0.015f : m_CurrentMouseMove.y * 0.05f,
			0.0f));

		// update object position
		m_Transform.position = Vector3.Lerp(m_Transform.position, (m_Camera.Transform.position - m_CurrentSwayForce) +
			(m_Camera.Transform.right * CarryingOffset.x) +
			(m_Camera.Transform.up * m_Transform.localScale.y * (CarryingOffset.y + (m_CurrentShake.y * 0.5f))) +
			(m_Camera.Transform.forward * CarryingOffset.z),
			((m_FetchProgress < 1.0f) ? m_FetchProgress : (Time.deltaTime * (Stiffness * 60.0f))));

	}


	/// <summary>
	/// Applies angular sway while moving around, and limits the
	/// camera's lower pitch angle while carrying an object.
	/// </summary>
	protected virtual void UpdateRotation()
	{

		// update pitch limit
		m_Camera.RotationPitchLimit = Vector2.Lerp(m_Camera.RotationPitchLimit, new Vector2(m_Camera.RotationPitchLimit.x, CameraPitchDownLimit), m_FetchProgress);

		// update angular sway
		if (m_DisableAngleSwayTimer.Active)
			return;

		m_CurrentSwayTorque += m_Player.Velocity.Get() * 0.005f;

		m_CurrentRotationSway = m_Camera.Transform.InverseTransformDirection(m_CurrentSwayTorque * 1.5f);
		m_CurrentRotationSway.y = m_CurrentRotationSway.z;
		m_CurrentRotationSway.z = m_CurrentRotationSway.x;
		m_CurrentRotationSway.x = (-m_CurrentRotationSway.y * 0.5f);
		m_CurrentRotationSway.y = 0;

		Quaternion orig = m_Transform.localRotation;
		m_Transform.RotateAround(m_Camera.transform.forward, m_CurrentRotationSway.z * -0.5f * Time.timeScale);
		m_Transform.RotateAround(m_Camera.transform.right, m_CurrentRotationSway.x * -0.5f * Time.timeScale);
		Quaternion newer = m_Transform.localRotation;
		m_Transform.localRotation = orig;
		m_Transform.localRotation = Quaternion.Slerp(newer, Quaternion.Euler(m_CurrentHoldAngle + (m_CurrentShake * 50)), (Time.deltaTime * (Stiffness * 60.0f)));

	}


	/// <summary>
	/// Optionally slows the player down while carrying this grabbable.
	/// </summary>
	protected virtual void UpdateBurden()
	{

		if (Burden <= 0.0f)
			return;

		m_Player.MotorThrottle.Set(m_Player.MotorThrottle.Get() * (1.0f - Mathf.Clamp01(Burden)));

	}


	/// <summary>
	/// Makes all physics forces continually wear off.
	/// </summary>
	protected virtual void DampenForces()
	{

		m_CurrentSwayForce *= 0.9f;
		m_CurrentSwayTorque *= 0.9f;
		m_CurrentFootstepForce *= 0.9f;

	}


	/// <summary>
	/// Checks to see if we have a player capable of interacting
	/// with this object, and if so starts or stops grabbing.
	/// </summary>
	public override bool TryInteract(vp_FPPlayerEventHandler player)
	{

		if (m_Player == null)
			m_Player = player;

		if (player == null)
			return false;

		if (m_Controller == null)
			m_Controller = m_Player.GetComponent<vp_FPController>();

		if (m_Controller == null)
			return false;

		if (m_Camera == null)
			m_Camera = m_Player.GetComponentInChildren<vp_FPCamera>();

		if (m_Camera == null)
			return false;

		if (m_WeaponHandler == null)
			m_WeaponHandler = m_Player.GetComponentInChildren<vp_FPWeaponHandler>();

		if (m_Audio == null)
			m_Audio = m_Player.audio;

		m_Player.Register(this);

		if (!m_IsGrabbed)
			StartGrab(); // start the grab
		else
			StopGrab(); // if object grabbed, stop the grab

		// set this object as the one the player is currently
		// interacting with
		m_Player.Interactable.Set(this);

		// if we have a grab state crosshair, set it
		if (GrabStateCrosshair != null)
			m_Player.Crosshair.Set(GrabStateCrosshair);
		else
			m_Player.Crosshair.Set(new Texture2D(0, 0));

		return true;

	}


	/// <summary>
	/// Starts the grab state.
	/// </summary>
	protected virtual void StartGrab()
	{

		// play a grab sound
		vp_Utility.PlayRandomSound(m_Audio, GrabSounds, SoundsPitch);

		// show a HUD text, 
		if (!string.IsNullOrEmpty(OnGrabText))
			m_Player.HUDText.Send(OnGrabText);

		// hook into the camera's footstep system for motion
		m_Camera.BobStepCallback += Footstep;

		// backup ID of current weapon for later
		m_LastWeaponEquipped = m_Player.CurrentWeaponID.Get();

		// store the player's original pitch down limit, for later restoration
		m_OriginalPitchDownLimit = m_Camera.RotationPitchLimit.y;

		// if we have no weapon wielded, and don't have a running 'Fetch'
		// coroutine, start one now. otherwise, unwield weapon and let
		// our 'OnStop_SetWeapon' callback start the coroutine
		m_FetchProgress = 0.0f;
		if (m_LastWeaponEquipped != 0)
			m_Player.SetWeapon.TryStart(0);
		else if (!m_IsFetching)
			StartCoroutine("Fetch");

		// alter this object's physics to allow proper carrying motion
		if (m_Transform.rigidbody != null)
		{
			m_Transform.rigidbody.useGravity = false;
			m_Transform.rigidbody.drag = (Stiffness * 60.0f);
			m_Transform.rigidbody.angularDrag = (Stiffness * 60.0f);
		}

		// make player ignore collisions with this object
		if (m_Controller.Transform.collider.enabled && m_Transform.collider.enabled)
			Physics.IgnoreCollision(m_Controller.Transform.collider, m_Transform.collider, true);

		// parent this object to the camera, and make its angle the
		// initial angle it will have in our hands. NOTE: interaction
		// with other rigidbodies may twist this angle later
		m_Transform.parent = m_Camera.Transform;
		m_CurrentHoldAngle = m_Transform.localEulerAngles;
		
		// ready to start grabbing!
		m_IsGrabbed = true;

	}


	/// <summary>
	/// Stops the grab state.
	/// </summary>
	protected virtual void StopGrab()
	{

		// reset grab and fetch states
		m_IsGrabbed = false;
		m_FetchProgress = 1.0f;

		// stop listening to camera footsteps
		m_Camera.BobStepCallback -= Footstep;

		// ready the weapon we were using
		m_Player.SetWeapon.TryStart(m_LastWeaponEquipped);

		// reset the object back to it's defaults
		if (m_Transform.rigidbody != null)
		{
			m_Transform.rigidbody.useGravity = m_DefaultGravity;
			m_Transform.rigidbody.drag = m_DefaultDrag;
			m_Transform.rigidbody.angularDrag = m_DefaultAngularDrag;
		}

		// allow player to collide with this object again
		if (!m_Player.Dead.Active)	// NOTE: doing the below while player is dead will cause a crash
		{
			if (vp_Utility.IsActive(m_Transform.gameObject) && m_Controller.Transform.collider.enabled && m_Transform.collider.enabled)
				Physics.IgnoreCollision(m_Controller.Transform.collider, m_Transform.collider, false);
		}

		Vector3 finalRotation = m_Transform.eulerAngles;
		// remove this object from being a child of the camera
		m_Transform.parent = null;
		m_Transform.eulerAngles = finalRotation;

		// reset object's velocity
		if (m_Transform.rigidbody != null)
		{
			m_Transform.rigidbody.velocity = Vector3.zero;
			m_Transform.rigidbody.angularVelocity = Vector3.zero;
		}

		// throw the object if fire button used
		if (Input.GetButton("Fire1"))
		{

			vp_Utility.PlayRandomSound(m_Audio, ThrowSounds, SoundsPitch);
			if (m_Transform.rigidbody != null)
			{
				m_Transform.rigidbody.AddForce(m_Controller.Velocity + m_Player.Forward.Get() * ThrowStrength, ForceMode.Impulse);
				if (AllowThrowRotation)
					m_Transform.rigidbody.AddTorque(m_Camera.Transform.forward * (Random.value > 0.5f ? 0.5f : -0.5f) +
													m_Camera.Transform.right * (Random.value > 0.5f ? 0.5f : -0.5f),
													ForceMode.Impulse);
			}

		}
		else	// otherwise perform a normal drop
		{

			vp_Utility.PlayRandomSound(m_Audio, DropSounds, SoundsPitch);
			if (m_Transform.rigidbody != null)
				m_Transform.rigidbody.AddForce(m_Controller.Velocity + m_Player.Forward.Get(), ForceMode.Impulse);

		}

		// disallow the crosshair to change again for half a second
		m_InteractManager.CrosshairTimeoutTimer = Time.time + 0.5f;

		// restore original camera pitch limit (will a small delay,
		// or it won't always take effect)
		vp_Timer.In(0.1f, delegate()
		{
			m_Camera.RotationPitchLimit.y = m_OriginalPitchDownLimit;
		});

	}

	
	/// <summary>
	/// Stops interacting with this object.
	/// </summary>
	public override void FinishInteraction()
	{

		if (m_IsGrabbed)
			StopGrab();

	}


	/// <summary>
	/// This coroutine starts the fetch mode and regulates its duration.
	/// </summary>
	protected virtual IEnumerator Fetch()
	{

		// reset various variables
		m_IsFetching = true;
		m_CurrentSwayForce = Vector3.zero;
		m_CurrentSwayTorque = Vector3.zero;
		m_CurrentFootstepForce = 0.0f;
		m_FetchProgress = 0.0f;

		// the time it takes to grab something will depend on the
		// distance to it
		duration = Vector3.Distance(m_Camera.Transform.position, m_Transform.position) * 0.5f;

		// prohibit angular sway for a while post fetching
		vp_Timer.In(duration + 1.0f, delegate() { }, m_DisableAngleSwayTimer);

		while (m_FetchProgress < 1.0f)
		{
			m_FetchProgress += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
		m_IsFetching = false;

	}


	/// <summary>
	/// Makes rigidbody collision smoother.
	/// </summary>
	protected virtual void OnCollisionEnter(Collision col)
	{

		if (!m_IsGrabbed)
			return;

		// colliding with things while fetching can result in buggy
		// rigidbody motion, so speed up the fetch a little bit
		if (m_FetchProgress < 1.0f)
			m_FetchProgress *= 1.2f;

		// angular sway also interferes in a bad way with rigidbody
		// collisions, so after each impact we disable it for 2 secs.
		vp_Timer.In(2.0f, delegate() { }, m_DisableAngleSwayTimer);

		// another (aesthetic) reason for doing this is that rigidbody
		// physics objects will interact with eachother, twisting
		// the grabbable in your hands. for example, it will make
		// a stacked box tilt as it comes off the pile (not to mention
		// make it possible to stack boxes in the first place)

	}


	/// <summary>
	/// The purpose of this method is to prevent the player from pushing
	/// grabbables through walls (and generally to avoid funky physics
	/// with external, static objects). It detects rapid, repetitive
	/// collision with a single external non-kinematic rigidbody, and
	/// makes the player let go of this grabbable if necessary.
	/// </summary>
	protected virtual void OnCollisionStay(Collision col)
	{

		if (!m_IsGrabbed || MaxCollisionCount == 0)
			return;

		// if this is not the last object we collided with
		if (col.collider != m_LastExternalCollider)
		{
			// and it has a non-kinematic rigidbody
			if (!(col.collider.rigidbody && !col.collider.rigidbody.isKinematic))
			{
				// start counting collisions with this object
				m_LastExternalCollider = col.collider;
				m_CollisionCount = 1;
			}
		}
		// otherwise we're colliding with the same object again
		else
		{

			// if the obstructing object is straight in front of us, let go
			// early since this case is very prone to having the player
			// push objects through walls. letting go will instead make the
			// grabbable stick between the wall and the player, blocking the
			// player which is nicely realistic
			RaycastHit hit;
			if (Physics.Raycast(m_Transform.position, m_Camera.Transform.forward, out hit, 1.0f))
			{
				if (hit.collider == m_LastExternalCollider &&
									m_FetchProgress >= 1.0f)	// ... just don't interfere with fetching
					m_CollisionCount = MaxCollisionCount;
			}

			m_CollisionCount++;

			// if collision count exceeds maximum allowed, let go of this grabbable!
			// but before we do - raycast from the contact point and down to see if
			// it's actually the ground we're colliding with - in which case we don't
			// do anything
			if (m_CollisionCount > MaxCollisionCount &&
				!(Physics.Raycast(col.contacts[0].point + Vector3.up * 0.1f, -Vector3.up, out hit, 0.2f)
				&& (hit.collider == m_LastExternalCollider)))
			{
				m_CollisionCount = 0;
				m_FetchProgress = 1.0f;
				m_LastExternalCollider = null;
				if (m_Player != null)
					m_Player.Interact.TryStart();
			}
		}

	}


	/// <summary>
	/// Allows other rigidbodies to twist the grabbable in our hands.
	/// </summary>
	protected virtual void OnCollisionExit()
	{

		// update the hold angle (the angle of the grabbable in
		// our hands), this allows external objects to twist the
		// object in our hands by keeping the sway target updated.
		m_CurrentHoldAngle = m_Transform.localEulerAngles;

	}


	/// <summary>
	/// Delegate for adding a smooth footstep force.
	/// </summary>
	protected virtual void Footstep()
	{
		m_CurrentFootstepForce += FootstepForce;
	}


	/// <summary>
	/// Adds a small downward force when falling onto a surface.
	/// </summary>
	protected virtual void OnMessage_FallImpact(float impact)
	{
		m_CurrentSwayForce.y += impact * Kneeling;
	}


	/// <summary>
	/// Starts grabbing if the weapon was put away.
	/// </summary>
	protected virtual void OnStop_SetWeapon()
	{

		// if we are supposed to be grabbing something but don't
		// have a running 'Fetch' coroutine, start one now
		if (m_IsGrabbed && !m_IsFetching)
			StartCoroutine("Fetch");

	}


	/// <summary>
	/// Prevents player from wielding weapons while carrying an object.
	/// </summary>
	protected virtual bool CanStart_SetWeapon()
	{

		int id = (int)m_Player.SetWeapon.Argument;

		if (!m_IsGrabbed || id == 0)
			return true;

		return false;

	}


}


