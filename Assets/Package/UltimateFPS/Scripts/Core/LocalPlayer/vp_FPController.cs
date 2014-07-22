/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPController.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a first person controller class with tweakable physics parameters
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(vp_FPPlayerEventHandler))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(vp_FPInput))]

public class vp_FPController : vp_Component
{

	// general
	protected CharacterController m_CharacterController = null;
	protected Vector3 m_FixedPosition = Vector3.zero;		// exact position. updates at a fixed interval and is used for gameplay
	protected Vector3 m_SmoothPosition = Vector3.zero;		// smooth position. updates as often as possible and is only used for the camera

	// collision
	public bool Grounded { get { return m_Grounded; } }
	public bool HeadContact { get { return m_HeadContact; } }
	public Vector3 GroundNormal { get { return m_GroundHit.normal; } }
	public float GroundAngle { get { return Vector3.Angle(m_GroundHit.normal, Vector3.up); } }
	public Transform GroundTransform { get { return m_GroundHit.transform; } }
	protected bool m_Grounded = false;
	protected bool m_HeadContact = false;
	protected RaycastHit m_GroundHit;					// contains info about the ground we're standing on, if any
	protected RaycastHit m_LastGroundHit;				// ground hit from last frame: used to detect ground collision changes
	protected RaycastHit m_CeilingHit;					// contains info about any ceilings we may have bumped into
	protected RaycastHit m_WallHit;						// contains info about any horizontal blockers we may have collided with
	protected float m_FallImpact = 0.0f;
	protected Terrain m_CurrentTerrain = null;			// caches the current terrain the controller is on
	protected vp_SurfaceIdentifier m_CurrentSurface = null;// caches the current vp_SurfaceIdentifier under the player

	// motor
	public float MotorAcceleration = 0.18f;
	public float MotorDamping = 0.17f;
	public float MotorAirSpeed = 0.35f;
	public float MotorSlopeSpeedUp = 1.0f;
	public float MotorSlopeSpeedDown = 1.0f;
	public bool MotorFreeFly = false;
	protected Vector3 m_MoveDirection = Vector3.zero;
	protected float m_SlopeFactor = 1.0f;
	protected Vector3 m_MotorThrottle = Vector3.zero;
	protected float m_MotorAirSpeedModifier = 1.0f;
	protected float m_CurrentAntiBumpOffset = 0.0f;
	protected Vector2 m_MoveVector = Vector2.zero;

	// jump
	public float MotorJumpForce = 0.18f;
	public float MotorJumpForceDamping = 0.08f;
	public float MotorJumpForceHold = 0.003f;
	public float MotorJumpForceHoldDamping = 0.5f;
	protected int m_MotorJumpForceHoldSkipFrames = 0;
	protected float m_MotorJumpForceAcc = 0.0f;
	bool m_MotorJumpDone = true;

	// gravity
	protected float m_FallSpeed = 0.0f;
	protected float m_LastFallSpeed = 0.0f;
	protected float m_HighestFallSpeed = 0.0f;

	// physics
	public float PhysicsForceDamping = 0.05f;			// damping of external forces
	public float PhysicsPushForce = 5.0f;				// mass for pushing around rigidbodies
	public float PhysicsGravityModifier = 0.2f;			// affects fall speed
	public float PhysicsSlopeSlideLimit = 30.0f;		// steepness in angles above which controller will start to slide
	public float PhysicsSlopeSlidiness = 0.15f;			// slidiness of the surface that we're standing on. will be additive if steeper than CharacterController.slopeLimit
	public float PhysicsWallBounce = 0.0f;				// how much to bounce off walls
	public float PhysicsWallFriction = 0.0f;
	public bool PhysicsHasCollisionTrigger = true;		// whether to automatically generate a child object with a trigger on startup
	protected GameObject m_Trigger = null;				// trigger for detection of incoming objects
	protected Vector3 m_ExternalForce = Vector3.zero;	// current velocity from external forces (explosion knockback, jump pads, rocket packs)
	protected Vector3[] m_SmoothForceFrame = new Vector3[120];
	protected bool m_Slide = false;						// are sliding on a steep surface without moving?
	protected bool m_SlideFast = false;					// have we accumulated a quick speed from standing on a slope above 'slopeLimit'
	protected float m_SlideFallSpeed = 0.0f;			// fall speed resulting from sliding fast into free fall
	protected float m_OnSteepGroundSince = 0.0f;		// the point in time at which we started standing on a slope above 'slopeLimit'. used to calculate slide speed accumulation
	protected float m_SlopeSlideSpeed = 0.0f;			// current velocity from sliding
	protected Vector3 m_PredictedPos = Vector3.zero;
	protected Vector3 m_PrevPos = Vector3.zero;			// position at start of each fixed timestep. used for calculating velocity
	protected Vector3 m_PrevDir = Vector3.zero;
	protected Vector3 m_NewDir = Vector3.zero;
	protected float m_ForceImpact = 0.0f;
	protected float m_ForceMultiplier = 0.0f;
	protected Vector3 CapsuleBottom = Vector3.zero;
	protected Vector3 CapsuleTop = Vector3.zero;
	protected float m_SkinWidth = 0.08f;				// NOTE: this should be kept the same as the Unity CharacterController's
														// 'Skin Width' parameter, which is unfortunately not exposed to script
	public Vector3 SmoothPosition { get { return m_SmoothPosition; } }	// a version of the controller position calculated in 'Update' to get smooth camera motion
	public Vector3 Velocity { get { return m_CharacterController.velocity; } }

	// moving platforms
	protected Transform m_Platform = null;						// current rigidbody or object in the 'MovableObject' layer that we are standing on
	protected Vector3 m_PositionOnPlatform = Vector3.zero;		// local position in relation to the movable object we're currently standing on
	protected float m_LastPlatformAngle;						// used for rotating controller along with movable object
	protected Vector3 m_LastPlatformPos = Vector3.zero;			// used for calculating inherited speed upon platform dismount

	// crouching
	protected float m_NormalHeight = 0.0f;				// height of the player controller when not crouching (stored from the character controller in Start)
	protected Vector3 m_NormalCenter = Vector3.zero;	// forced to half of the controller height (for crouching logic)
	protected float m_CrouchHeight = 0.0f;				// height of the player controller when crouching (calculated in Start)
	protected Vector3 m_CrouchCenter = Vector3.zero;	// will be half of the crouch height, but no smaller than the crouch radius

	// misc
	public CharacterController CharacterController { get { return m_CharacterController; } }

	// event handler property cast as a playereventhandler
	vp_FPPlayerEventHandler m_Player = null;
	vp_FPPlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				if (EventHandler != null)
					m_Player = (vp_FPPlayerEventHandler)EventHandler;
			}
			return m_Player;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		m_CharacterController = gameObject.GetComponent<CharacterController>();

		// store the initial CharacterController height for crouching logic. NOTE: we force
		// the 'Center' parameter of the CharacterController to its 'Height' / 2 (placing
		// its pivot point at the bottom), and its 'Radius' to the 'Height' / 4, otherwise
		// the player may fall through the ground when crouching
		m_NormalHeight = CharacterController.height;
		CharacterController.center = m_NormalCenter = new Vector3(0, m_NormalHeight * 0.5f, 0);
		CharacterController.radius = m_NormalHeight * 0.25f;	// NOTE: don't change radius in-game (it may cause missed wall collisions)
		m_CrouchHeight = m_NormalHeight * 0.5f;	// NOTE: due to the workings of a capsule collider, height can never be smaller than radius
		m_CrouchCenter = m_NormalCenter * 0.5f;

	}

	
	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{

		base.Start();

		SetPosition(Transform.position);

		// if set, automagically sets up a trigger for interacting with
		// incoming rigidbodies
		if (PhysicsHasCollisionTrigger)
		{
			m_Trigger = new GameObject("Trigger");
			m_Trigger.transform.parent = m_Transform;
			CapsuleCollider collider = m_Trigger.AddComponent<CapsuleCollider>();
			collider.isTrigger = true;
			collider.radius = CharacterController.radius + m_SkinWidth;
			collider.height = CharacterController.height + (m_SkinWidth * 2.0f);
			collider.center = CharacterController.center;
			m_Trigger.layer = vp_Layer.LocalPlayer;
			m_Trigger.transform.localPosition = Vector3.zero;
		}

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
	{

		base.Update();

		// simulate high-precision movement for smoothest possible camera motion
		SmoothMove();

		// TIP: uncomment either of these lines to debug print the
		// speed of the character controller
		//Debug.Log(Velocity.magnitude);		// speed in meters per second
		//Debug.Log(Controller.Velocity.sqrMagnitude);	// speed as used by the camera bob

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void FixedUpdate()
	{

		if (Time.timeScale == 0.0f)
			return;

		// convert user input to motor throttle
		UpdateMotor();

		// apply motion generated by tapping or holding the jump button
		UpdateJump();

		// handle external forces like gravity, explosion shockwaves or wind
		UpdateForces();

		// apply sliding in slopes
		UpdateSliding();

		// update controller position based on current motor- & external forces
		FixedMove();

		// respond to environment collisions that may have happened during the move
		UpdateCollisions();

		// move and rotate player along with rigidbodies & moving platforms
		UpdatePlatformMove();

		// store final position for next frame's physics calculations
		m_PrevPos = Transform.position;

	}


	/// <summary>
	/// simulates velocity acceleration and damping in the cardinal
	/// directions
	/// </summary>
	protected virtual void UpdateMotor()
	{

		if (!MotorFreeFly)
			UpdateThrottleWalk();
		else
			UpdateThrottleFree();

		// snap super-small values to zero to avoid floating point issues
		m_MotorThrottle = vp_Utility.SnapToZero(m_MotorThrottle);

	}


	/// <summary>
	/// throttle logic for moving a grounded controller, taking ground
	/// slope and air speed into consideration
	/// </summary>
	protected virtual void UpdateThrottleWalk()
	{

		// if on the ground, make movement speed dependent on ground slope
		UpdateSlopeFactor();

		// update air speed modifier
		// (at 1.0, this will completely prevent the controller from altering
		// its trajectory while in the air, and will disable motor damping)
		m_MotorAirSpeedModifier = (m_Grounded ? 1.0f : MotorAirSpeed);

		// convert horizontal input to forces in the motor
		m_MotorThrottle += m_MoveVector.y * (Transform.TransformDirection(
			Vector3.forward *
			(MotorAcceleration * 0.1f) *
			m_MotorAirSpeedModifier) *
			m_SlopeFactor);
		m_MotorThrottle += m_MoveVector.x * (Transform.TransformDirection(
			Vector3.right *
			(MotorAcceleration * 0.1f) *
			m_MotorAirSpeedModifier) *
			m_SlopeFactor);

		// dampen motor force
		m_MotorThrottle.x /= (1.0f + (MotorDamping * m_MotorAirSpeedModifier * Time.timeScale));
		m_MotorThrottle.z /= (1.0f + (MotorDamping * m_MotorAirSpeedModifier * Time.timeScale));

	}


	/// <summary>
	/// throttle logic for moving a flying controller in an arbitrary
	/// direction based on player (camera) forward vector. this can be
	/// used for spectator cams, zero gravity, swimming underwater,
	/// jetpacks and superhero style flying
	/// </summary>
	protected virtual void UpdateThrottleFree()
	{

		// convert input to forces in the motor
		m_MotorThrottle += m_MoveVector.y * (Transform.TransformDirection(
			Transform.InverseTransformDirection(Player.Forward.Get()) *
			(MotorAcceleration * 0.1f)));
		m_MotorThrottle += m_MoveVector.x * (Transform.TransformDirection(
			Vector3.right *
			(MotorAcceleration * 0.1f)));

		// dampen motor force
		m_MotorThrottle.x /= (1.0f + (MotorDamping * Time.timeScale));
		m_MotorThrottle.z /= (1.0f + (MotorDamping * Time.timeScale));

	}


	/// <summary>
	/// handles all jump logic, including impulse jumping, continuous
	/// jumping, vertical movement during free fly mode and stopping
	/// the controller on ceiling contact
	/// </summary>
	protected virtual void UpdateJump()
	{

		// abort all jumping activity for 1 second if head touches a ceiling
		if (m_HeadContact)
			Player.Jump.Stop(1.0f);

		if (!MotorFreeFly)
			UpdateJumpForceWalk();
		else
			UpdateJumpForceFree();

		// apply accumulated 'hold jump' force
		m_MotorThrottle.y += m_MotorJumpForceAcc * Time.timeScale;

		// dampen forces
		m_MotorJumpForceAcc /= (1.0f + (MotorJumpForceHoldDamping * Time.timeScale));
		m_MotorThrottle.y /= (1.0f + (MotorJumpForceDamping * Time.timeScale));

	}


	/// <summary>
	/// performs jump logic for a grounded controller
	/// </summary>
	protected virtual void UpdateJumpForceWalk()
	{

		if (Player.Jump.Active)
		{
			if (!m_Grounded)
			{
				// accumulate 'hold jump' force if the jump button is still being held
				// down 2 fixed frames after the impulse jump
				if (m_MotorJumpForceHoldSkipFrames > 2)
				{
					// but only if jump button hasn't been released on the way down
					if (!(m_CharacterController.velocity.y < 0.0f))
						m_MotorJumpForceAcc += MotorJumpForceHold;
				}
				else
					m_MotorJumpForceHoldSkipFrames++;
			}
		}

	}


	/// <summary>
	/// performs vertical movement logic for a free flying controller,
	/// going straight up or down while the jump or crouch activities
	/// are active, respectively
	/// </summary>
	protected virtual void UpdateJumpForceFree()
	{

		if (Player.Jump.Active && Player.Crouch.Active)
			return;

		if (Player.Jump.Active)
			m_MotorJumpForceAcc += MotorJumpForceHold;
		else if (Player.Crouch.Active)
		{

			m_MotorJumpForceAcc -= MotorJumpForceHold;

			// trigger crouch collision update on ground contact
			if (Grounded && CharacterController.height == m_NormalHeight)
			{
				CharacterController.height = m_CrouchHeight;
				CharacterController.center = m_CrouchCenter;
			}

		}

	}


	/// <summary>
	/// updates the controller according to a simple physics
	/// simulation including gravity and smooth external forces
	/// </summary>
	protected virtual void UpdateForces()
	{

		// accumulate gravity
		if (m_Grounded && (m_FallSpeed <= 0.0f))
			// when not falling, stick controller to the ground by a small, fixed gravity
			m_FallSpeed = (Physics.gravity.y * (PhysicsGravityModifier * 0.002f));
		else
			m_FallSpeed += (Physics.gravity.y * (PhysicsGravityModifier * 0.002f));

		// store highest reached fall speed (for calculating fall
		// impact in 'UpdateCollisions')
		if (m_FallSpeed < m_LastFallSpeed)
			m_HighestFallSpeed = m_FallSpeed;
		m_LastFallSpeed = m_FallSpeed;

		// apply smooth force (forces applied over several frames)
		if (m_SmoothForceFrame[0] != Vector3.zero)
		{
			AddForceInternal(m_SmoothForceFrame[0]);
			for (int v = 0; v < 120; v++)
			{
				m_SmoothForceFrame[v] = (v < 119) ? m_SmoothForceFrame[v + 1] : Vector3.zero;
				if (m_SmoothForceFrame[v] == Vector3.zero)
					break;
			}
		}

		// dampen external forces
		m_ExternalForce /= (1.0f + (PhysicsForceDamping * Time.timeScale));

	}


	/// <summary>
	/// simulates sliding in slopes. the controller may slide at
	/// a constant or accumulated rate. see the docs for available
	/// parameters
	/// </summary>
	protected virtual void UpdateSliding()
	{

		bool wasSlidingFast = m_SlideFast;
		bool wasSliding = m_Slide;

		// --- handle slope sliding ---
		// TIP: alter 'PhysicsSlopeSlidiness' and 'SlopeSlideLimit' in realtime
		// using the state manager, depending on the current ground surface
		m_Slide = false;
		if (!m_Grounded)
		{
			m_OnSteepGroundSince = 0.0f;
			m_SlideFast = false;
		}
		// start sliding if ground is steep enough in angles
		else if (GroundAngle > PhysicsSlopeSlideLimit)
		{

			m_Slide = true;

			// if ground angle is within slopelimit, slide at a constant speed
			if (GroundAngle <= m_CharacterController.slopeLimit)
			{
				m_SlopeSlideSpeed = Mathf.Max(m_SlopeSlideSpeed, (PhysicsSlopeSlidiness * 0.01f));
				m_OnSteepGroundSince = 0.0f;
				m_SlideFast = false;
				// apply slope speed damping (and snap to zero if miniscule, to avoid
				// floating point errors)
				m_SlopeSlideSpeed = (Mathf.Abs(m_SlopeSlideSpeed) < 0.0001f) ? 0.0f :
					(m_SlopeSlideSpeed / (1.0f + (0.05f * Time.timeScale)));
			}
			else	// if steeper than slopelimit, slide with accumulating slide speed
			{
				if ((m_SlopeSlideSpeed) > 0.01f)
					m_SlideFast = true;
				if (m_OnSteepGroundSince == 0.0f)
					m_OnSteepGroundSince = Time.time;
				m_SlopeSlideSpeed += (((PhysicsSlopeSlidiness * 0.01f) * ((Time.time - m_OnSteepGroundSince) * 0.125f)) * Time.timeScale);
				m_SlopeSlideSpeed = Mathf.Max((PhysicsSlopeSlidiness * 0.01f), m_SlopeSlideSpeed);	// keep minimum slidiness
			}

			// add horizontal force in the slope direction, multiplied by slidiness
			AddForce(Vector3.Cross(Vector3.Cross(GroundNormal, Vector3.down), GroundNormal) *
				m_SlopeSlideSpeed * Time.timeScale);

		}
		else
		{
			m_OnSteepGroundSince = 0.0f;
			m_SlideFast = false;
			m_SlopeSlideSpeed = 0.0f;
		}

		// if player is moving by its own, external components should not
		// consider it slow-sliding. this is intended for retaining movement
		// fx (like weapon bob) on less slidy surfaces
		if (m_MotorThrottle != Vector3.zero)
			m_Slide = false;

		// handle fast sliding into free fall
		if (m_SlideFast)
			m_SlideFallSpeed = Transform.position.y;	// store y to calculate difference next frame
		else if (wasSlidingFast && !Grounded)
			m_FallSpeed = Transform.position.y - m_SlideFallSpeed;	// lost grounding while sliding fast: kickstart gravity at slide fall speed

		// detect whether the slide variables have changed, and broadcast
		// messages so external components can update accordingly

		if (wasSliding != m_Slide)
			Player.SetState("Slide", m_Slide);

		if (wasSlidingFast != m_SlideFast)
			Player.SetState("SlideFast", m_SlideFast);

	}


	/// <summary>
	/// combines motor & external forces into a move direction
	/// and sets the resulting controller position
	/// </summary>
	protected virtual void FixedMove()
	{

		// --- apply forces ---
		m_MoveDirection = Vector3.zero;
		m_MoveDirection += m_ExternalForce;
		m_MoveDirection += m_MotorThrottle;
		m_MoveDirection.y += m_FallSpeed;

		// --- apply anti-bump offset ---
		// this pushes the controller towards the ground to prevent the character
		// from "bumpety-bumping" when walking down slopes or stairs. the strength
		// of this effect is determined by the character controller's 'Step Offset'
		m_CurrentAntiBumpOffset = 0.0f;
		if (m_Grounded && m_MotorThrottle.y <= 0.001f)
		{
			m_CurrentAntiBumpOffset = Mathf.Max(m_CharacterController.stepOffset, Vector3.Scale(m_MoveDirection, (Vector3.one - Vector3.up)).magnitude);
			m_MoveDirection += m_CurrentAntiBumpOffset * Vector3.down;
		}

		// --- predict move result ---
		// do some prediction in order to detect blocking and deflect forces on collision
		m_PredictedPos = Transform.position + vp_Utility.NaNSafeVector3(m_MoveDirection * Delta * Time.timeScale);

		// --- move the charactercontroller ---

		// ride along with movable objects
		if (m_Platform != null && m_PositionOnPlatform != Vector3.zero)
			m_CharacterController.Move(vp_Utility.NaNSafeVector3(m_Platform.TransformPoint(m_PositionOnPlatform) -
																	m_Transform.position));

		// move on our own
		m_CharacterController.Move(vp_Utility.NaNSafeVector3(m_MoveDirection * Delta * Time.timeScale));

		// while there is an active death event, block movement input
		if (Player.Dead.Active)
		{
			m_MoveVector = Vector2.zero;
			return;
		}

		// --- store ground info ---
		// perform a sphere cast (as wide as the character) from ~knees to ground, and
		// save hit info in the 'm_GroundHit' variable. this gives access to lots of
		// data on the object directly below us, object transform, ground angle etc.
		Physics.SphereCast(new Ray(	Transform.position + Vector3.up * (m_CharacterController.radius), Vector3.down),
									(m_CharacterController.radius), out m_GroundHit, (m_SkinWidth + 0.001f),
									vp_Layer.Mask.ExternalBlockers);
		m_Grounded = (m_GroundHit.collider != null);

		// --- store head contact info ---
		// spherecast upwards for some info on the surface touching the top of the collider, if any
		if (!m_Grounded && (m_CharacterController.velocity.y > 0.0f))
		{
			Physics.SphereCast(new Ray(	Transform.position, Vector3.up),
										m_CharacterController.radius, out m_CeilingHit,
										m_CharacterController.height - (m_CharacterController.radius - m_SkinWidth) + 0.01f,
										vp_Layer.Mask.ExternalBlockers);
			m_HeadContact = (m_CeilingHit.collider != null);
		}
		else
			m_HeadContact = false;

		// --- handle loss of grounding ---
		if ((m_GroundHit.transform == null) && (m_LastGroundHit.transform != null))
		{

			// if we lost contact with a moving object, inherit its speed
			// then forget about it
			if (m_Platform != null && m_PositionOnPlatform != Vector3.zero)
			{
				AddForce(m_Platform.position - m_LastPlatformPos);
				m_Platform = null;
			}

			// undo anti-bump offset to make the fall smoother
			if (m_CurrentAntiBumpOffset != 0.0f)
			{
				m_CharacterController.Move(vp_Utility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * Delta * Time.timeScale);
				m_PredictedPos += vp_Utility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * Delta * Time.timeScale;
				m_MoveDirection += m_CurrentAntiBumpOffset * Vector3.up;
			}

		}


	}


	/// <summary>
	/// since the controller is moved in FixedUpdate and the
	/// camera in Update there will be noticeable camera jitter.
	/// this method simulates the controller move in Update and
	/// stores the smooth position for the camera to read
	/// </summary>
	protected virtual void SmoothMove()
	{

		if (Time.timeScale == 0.0f)
			return;

		// restore last smoothpos
		m_FixedPosition = Transform.position;	// backup fixedpos
		Transform.position = m_SmoothPosition;

		// move controller to get the smooth position
		m_CharacterController.Move(vp_Utility.NaNSafeVector3((m_MoveDirection * Delta * Time.timeScale)));
		m_SmoothPosition = Transform.position;
		Transform.position = m_FixedPosition;	// restore fixedpos

		// if smoothpos deviates too much, reset it
		if (Vector3.Distance(Transform.position, m_SmoothPosition) > m_CharacterController.radius)
			m_SmoothPosition = Transform.position;

		// if we're on an platform travelling vertically, disregard
		// smooth y to prevent jitter
		if ((m_Platform != null) && ((m_LastPlatformPos.y < m_Platform.position.y) ||
									(m_LastPlatformPos.y > m_Platform.position.y)))
			m_SmoothPosition.y = Transform.position.y;

		// lerp smoothpos back to fixedpos slowly over time
		m_SmoothPosition = Vector3.Lerp(m_SmoothPosition, Transform.position, Time.deltaTime);

	}


	/// <summary>
	/// updates controller motion according to detected collisions
	/// against objects below, above and around the controller
	/// </summary>
	protected virtual void UpdateCollisions()
	{

		// --- respond to ground collision ---
		if ((m_GroundHit.transform != null) &&
			(m_GroundHit.transform != m_LastGroundHit.transform))
		{

			m_SmoothPosition.y = Transform.position.y;	// sync camera y pos

			// store fall impact
			if (!MotorFreeFly)
				m_FallImpact = -m_HighestFallSpeed * Time.timeScale;
			else
				m_FallImpact = -(CharacterController.velocity.y * 0.01f) * Time.timeScale;

			// deflect the controller sideways under some circumstances
			DeflectDownForce();

			m_HighestFallSpeed = 0.0f;

			// transmit fall impact to the player
			Player.FallImpact.Send(m_FallImpact);

			// reset all the jump variables
			m_MotorThrottle.y = 0.0f;
			m_MotorJumpForceAcc = 0.0f;
			m_MotorJumpForceHoldSkipFrames = 0;

			// detect and store moving platforms
			if (m_GroundHit.collider.gameObject.layer == vp_Layer.MovableObject)
			{
				m_Platform = m_GroundHit.transform;
				m_LastPlatformAngle = m_Platform.eulerAngles.y;
			}
			else
				m_Platform = null;
			
			// detect if there is terrain and store it if so
			Terrain ter = m_GroundHit.transform.GetComponent<Terrain>();
			if(ter != null)
				m_CurrentTerrain = ter;
			else
				m_CurrentTerrain = null;
			
			vp_SurfaceIdentifier sid = m_GroundHit.transform.GetComponent<vp_SurfaceIdentifier>();
			if(sid != null)
				m_CurrentSurface = sid;
			else
				m_CurrentSurface = null;
		}
		else
			m_FallImpact = 0.0f;



		// store ground hit for detecting fall impact and loss of grounding
		// in next frame
		m_LastGroundHit = m_GroundHit;
		
		// --- respond to ceiling collision ---
		// deflect forces that push the controller upward, in order to prevent
		// getting stuck in ceilings
		if ((m_PredictedPos.y > Transform.position.y) && (m_ExternalForce.y > 0 || m_MotorThrottle.y > 0))
			DeflectUpForce();

		// --- respond to wall collision ---
		// if the controller didn't end up at the predicted position, some
		// external object has blocked its way, so deflect the movement forces
		// to avoid getting stuck at walls
		if ((m_PredictedPos.x != Transform.position.x) ||
			(m_PredictedPos.z != Transform.position.z) &&
			(m_ExternalForce != Vector3.zero))
			DeflectHorizontalForce();



	}


	/// <summary>
	/// moves and rotates the controller while standing on top a movable
	/// object such as a rigidbody or a moving platform. NOTE: any moving
	/// platforms must be in the'MovableObject' layer!
	/// </summary>
	void UpdatePlatformMove()
	{

		if (m_Platform == null)
			return;

		// calculate the controller's local position in relation to movable object
		// NOTE: if this method is disabled, 'm_PositionOnPlatform' will remain zero,
		// disabling platform logic in other methods also
		m_PositionOnPlatform = m_Platform.InverseTransformPoint(m_Transform.position);

		// rotate along with the movable
		m_Player.Rotation.Set(new Vector2(m_Player.Rotation.Get().x, m_Player.Rotation.Get().y -
			Mathf.DeltaAngle(m_Platform.eulerAngles.y, m_LastPlatformAngle)));
		m_LastPlatformAngle = m_Platform.eulerAngles.y;

		// store movement delta for calculating inherited velocity on dismount
		m_LastPlatformPos = m_Platform.position;

		// sync smooth position to fixed position while on a movable
		m_SmoothPosition = Transform.position;

	}


	/// <summary>
	/// this method calculates a controller velocity multiplier
	/// depending on ground slope. at 'MotorSlopeSpeed' 1.0,
	/// velocity in slopes will be kept roughly the same as on
	/// flat ground. values lower or higher than 1 will make the
	/// controller slow down / speed up, depending on whether
	/// we're moving uphill or downhill
	/// </summary>
	protected virtual void UpdateSlopeFactor()
	{

		if (!m_Grounded)
		{
			m_SlopeFactor = 1.0f;
			return;
		}

		// determine if we're moving uphill or downhill
		m_SlopeFactor = 1 + (1.0f - (Vector3.Angle(m_GroundHit.normal, m_MotorThrottle) / 90.0f));

		if (Mathf.Abs(1 - m_SlopeFactor) < 0.01f)
			m_SlopeFactor = 1.0f;		// standing still or moving on flat ground, or moving perpendicular to a slope
		else if (m_SlopeFactor > 1.0f)
		{
			// moving downhill
			if (MotorSlopeSpeedDown == 1.0f)
			{
				// 1.0 means 'no change' so we'll alter the value to get
				// roughly the same velocity as if ground was flat
				m_SlopeFactor = 1.0f / m_SlopeFactor;
				m_SlopeFactor *= 1.2f;
			}
			else
				m_SlopeFactor *= MotorSlopeSpeedDown;	// apply user defined multiplier
		}
		else
		{
			// moving uphill
			if (MotorSlopeSpeedUp == 1.0f)
			{
				// 1.0 means 'no change' so we'll alter the value to get
				// roughly the same velocity as if ground was flat
				m_SlopeFactor *= 1.2f;
			}
			else
				m_SlopeFactor *= MotorSlopeSpeedUp;	// apply user defined multiplier

			// kill motor if moving into a slope steeper than 'slopeLimit'. this serves
			// to prevent exploits with being able to walk up steep surfaces and walls
			m_SlopeFactor = (GroundAngle > m_CharacterController.slopeLimit) ? 0.0f : m_SlopeFactor;

		}

	}


	/// <summary>
	/// sets the position of the FPController
	/// </summary>
	public virtual void SetPosition(Vector3 position)
	{

		Transform.position = position;
		m_SmoothPosition = position;
		m_PrevPos = position;

	}


	/// <summary>
	/// adds external force to the controller, such as explosion
	/// knockback, wind or jump pads
	/// </summary>
	protected virtual void AddForceInternal(Vector3 force)
	{
		m_ExternalForce += force;
	}

	/// <summary>
	/// adds external force to the controller, such as explosion
	/// knockback, wind or jump pads
	/// </summary>
	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}


	/// <summary>
	/// adds external velocity to the spring in one frame
	/// </summary>
	public virtual void AddForce(Vector3 force)
	{

		if (Time.timeScale >= 1.0f)
			AddForceInternal(force);
		else
			AddSoftForce(force, 1);

	}


	/// <summary>
	/// adds a force distributed over up to 120 fixed frames
	/// </summary>
	public virtual void AddSoftForce(Vector3 force, float frames)
	{

		force /= Time.timeScale;

		frames = Mathf.Clamp(frames, 1, 120);

		AddForceInternal(force / frames);

		for (int v = 0; v < (Mathf.RoundToInt(frames) - 1); v++)
		{
			m_SmoothForceFrame[v] += (force / frames);
		}

	}


	/// <summary>
	/// clears any soft forces currently buffered on this controller
	/// </summary>
	public virtual void StopSoftForce()
	{

		for (int v = 0; v < 120; v++)
		{
			if (m_SmoothForceFrame[v] == Vector3.zero)
				break;
			m_SmoothForceFrame[v] = Vector3.zero;
		}

	}


	/// <summary>
	/// completely stops the character controller in one frame
	/// </summary>
	public virtual void Stop()
	{

		m_CharacterController.Move(Vector3.zero);
		m_MotorThrottle = Vector3.zero;
		m_ExternalForce = Vector3.zero;
		StopSoftForce();
		m_MoveVector = Vector2.zero;
		m_FallSpeed = 0.0f;
		m_SmoothPosition = Transform.position;

	}


	/// <summary>
	/// typically we don't deflect downward forces since there is
	/// always a ground collision imposed by gravity (and it would
	/// be annoying from a controls perspective). this deals with
	/// the couple of cases when deflection does happen
	/// </summary>
	public virtual void DeflectDownForce()
	{

		// if we land on a surface tilted above the slide limit, convert
		// fall speed into slide speed on impact
		if (GroundAngle > PhysicsSlopeSlideLimit)
			m_SlopeSlideSpeed = m_FallImpact * (0.25f * Time.timeScale);

		// deflect away from nearly vertical surfaces. this serves to make
		// falling along walls smoother, and to prevent the controller
		// from getting stuck on vertical walls when falling into them
		if (GroundAngle > 85)
		{
			m_MotorThrottle += (vp_Utility.HorizontalVector((GroundNormal * m_FallImpact)));
			m_Grounded = false;
		}

	}


	/// <summary>
	/// this method deflects the controller away from ceilings
	/// in response to collisions resulting from upward forces
	/// such as jumping, explosions or jump pads. wall friction
	/// is applied in the collision
	/// </summary>
	protected virtual void DeflectUpForce()
	{

		if (!m_HeadContact)
			return;

		// convert the vertical force into horizontal force, deflecting the
		// controller away from any tilted ceilings (a perfectly horizontal
		// ceiling simply kills the vertical force). also, store impact force
		m_NewDir = Vector3.Cross(Vector3.Cross(m_CeilingHit.normal, Vector3.up), m_CeilingHit.normal);
		m_ForceImpact = (m_MotorThrottle.y + m_ExternalForce.y);
		Vector3 newForce = m_NewDir * (m_MotorThrottle.y + m_ExternalForce.y) * (1.0f - PhysicsWallFriction);
		m_ForceImpact = m_ForceImpact - newForce.magnitude;
		AddForce(newForce * Time.timeScale);
		m_MotorThrottle.y = 0.0f;
		m_ExternalForce.y = 0.0f;
		m_FallSpeed = 0.0f;

		// transmit headbump for other components to perform effects. make the
		// impact positive or negative depending on whether the ceiling we
		// bumped into repelled us to the left or right. if any other direction
		// (forward / backward / none) then pick randomly
		m_NewDir.x = (Transform.InverseTransformDirection(m_NewDir).x);

		Player.HeadImpact.Send(((m_NewDir.x < 0.0f) || (m_NewDir.x == 0.0f && (Random.value < 0.5f))) ? -m_ForceImpact : m_ForceImpact);

	}


	/// <summary>
	/// this method is called when the controller collides with
	/// something while moving horizontally. it calculates a new
	/// movement direction based on the impact normal and the new
	/// position decided by the physics engine, and deflects the
	/// controller's current horizontal force along the new vector.
	/// wall friction and bouncing are also applied
	/// </summary>
	protected virtual void DeflectHorizontalForce()
	{

		// flatten positions (this is 2d) and get our direction at point of impact
		m_PredictedPos.y = Transform.position.y;
		m_PrevPos.y = Transform.position.y;
		m_PrevDir = (m_PredictedPos - m_PrevPos).normalized;

		// get the origins of the controller capsule's spheres at prev position
		CapsuleBottom = m_PrevPos + Vector3.up * (m_CharacterController.radius);
		CapsuleTop = CapsuleBottom + Vector3.up * (m_CharacterController.height - (m_CharacterController.radius * 2));

		// capsule cast from the previous position to the predicted position to find
		// the exact impact point. this capsule cast does not include the skin width
		// (it's not really needed plus we don't want ground collisions)
		if (!(Physics.CapsuleCast(CapsuleBottom, CapsuleTop, m_CharacterController.radius, m_PrevDir,
			out m_WallHit, Vector3.Distance(m_PrevPos, m_PredictedPos), vp_Layer.Mask.ExternalBlockers)))
			return;

		// the force will be deflected perpendicular to the impact normal, and to the
		// left or right depending on whether the previous position is to our left or
		// right when looking back at the impact point from the current position
		m_NewDir = Vector3.Cross(m_WallHit.normal, Vector3.up).normalized;
		if ((Vector3.Dot(Vector3.Cross((m_WallHit.point - Transform.position),
			(m_PrevPos - Transform.position)), Vector3.up)) > 0.0f)
			m_NewDir = -m_NewDir;

		// calculate how the current force gets absorbed depending on angle of impact.
		// if we hit a wall head-on, almost all force will be absorbed, but if we
		// barely glance it, force will be almost unaltered (depending on friction)
		m_ForceMultiplier = Mathf.Abs(Vector3.Dot(m_PrevDir, m_NewDir)) * (1.0f - (PhysicsWallFriction));

		// if the controller has wall bounciness, apply it
		if (PhysicsWallBounce > 0.0f)
		{
			m_NewDir = Vector3.Lerp(m_NewDir, Vector3.Reflect(m_PrevDir, m_WallHit.normal), PhysicsWallBounce);
			m_ForceMultiplier = Mathf.Lerp(m_ForceMultiplier, 1.0f, (PhysicsWallBounce * (1.0f - (PhysicsWallFriction))));
		}

		// deflect current force and report the impact
		m_ForceImpact = 0.0f;
		float yBak = m_ExternalForce.y;
		m_ExternalForce.y = 0.0f;
		m_ForceImpact = m_ExternalForce.magnitude;
		m_ExternalForce = m_NewDir * m_ExternalForce.magnitude * m_ForceMultiplier;
		m_ForceImpact = m_ForceImpact - m_ExternalForce.magnitude;
		for (int v = 0; v < 120; v++)
		{
			if (m_SmoothForceFrame[v] == Vector3.zero)
				break;
			m_SmoothForceFrame[v] = m_SmoothForceFrame[v].magnitude * m_NewDir * m_ForceMultiplier;
		}
		m_ExternalForce.y = yBak;

		// TIP: the force that was absorbed by the bodies during the impact can be used for
		// things like damage, so an event could be sent here with the amount of absorbed force

	}


	/// <summary>
	/// simple solution for pushing rigid bodies. the push force
	/// of the FPSController is used to determine how much we
	/// can affect the other object, and we don't affect fast
	/// falling objects.
	/// </summary>
	protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{

		Rigidbody body = hit.collider.attachedRigidbody;

		if (body == null || body.isKinematic)
			return;

		if (hit.moveDirection.y < -0.3f)
			return;

		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		body.velocity = (pushDir * (PhysicsPushForce / body.mass));

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Jump' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Jump()
	{

		// always allowed to move vertically in free fly mode
		if (MotorFreeFly)
			return true;

		// can't jump without ground contact
		if (!m_Grounded)
			return false;

		// can't jump until the previous jump has stopped
		if (!m_MotorJumpDone)
			return false;

		// can't bunny-hop up steep surfaces
		if (GroundAngle > m_CharacterController.slopeLimit)
			return false;

		// passed the test!
		return true;

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Run' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Run()
	{

		// can't start running while crouching
		if (Player.Crouch.Active)
			return false;

		return true;

	}


	/// <summary>
	/// this callback is triggered right after the activity in question
	/// has been approved for activation
	/// </summary>
	protected virtual void OnStart_Jump()
	{

		m_MotorJumpDone = false;

		// sync camera y pos
		m_SmoothPosition.y = Transform.position.y;

		// disable impulse jump if we have no grounding in free fly mode 
		if (MotorFreeFly && !Grounded)
			return;

		// perform impulse jump
		m_MotorThrottle.y = (MotorJumpForce / Time.timeScale);

	}


	/// <summary>
	/// this callback is triggered when the activity in question
	/// deactivates
	/// </summary>
	protected virtual void OnStop_Jump()
	{

		m_MotorJumpDone = true;

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Crouch' activity to successfully deactivate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStop_Crouch()
	{

		// can't stop crouching if there is a blocking object above us
		if (Physics.SphereCast(new Ray(Transform.position, Vector3.up),
				m_CharacterController.radius,
				(m_NormalHeight - m_CharacterController.radius + 0.01f),
				vp_Layer.Mask.ExternalBlockers))
		{

			// regulate stop test interval to reduce amount of sphere casts
			Player.Crouch.NextAllowedStopTime = Time.time + 1.0f;

			// found a low ceiling above us - abort getting up
			return false;

		}

		// nothing above us - okay to get up
		return true;

	}


	/// <summary>
	/// this callback is triggered right after the activity in
	/// question has been approved for activation
	/// </summary>
	protected virtual void OnStart_Crouch()
	{

		// skip modifying collider size if free flying without ground contact
		if (MotorFreeFly && !Grounded)
			return;

		// modify collider size for crouching
		CharacterController.height = m_CrouchHeight;
		CharacterController.center = m_CrouchCenter;

	}


	/// <summary>
	/// this callback is triggered when the activity in question
	/// deactivates
	/// </summary>
	protected virtual void OnStop_Crouch()
	{

		CharacterController.height = m_NormalHeight;
		CharacterController.center = m_NormalCenter;

	}


	/// <summary>
	/// adds external force to the controller
	/// </summary>
	protected virtual void OnMessage_ForceImpact(Vector3 force)
	{
		AddForce(force);
	}


	/// <summary>
	/// stops the controller in one frame, killing all forces
	/// acting upon it
	/// </summary>
	protected virtual void OnMessage_Stop()
	{
		Stop();
	}


	/// <summary>
	/// gets or sets the world position of the controller
	/// </summary>
	protected virtual Vector3 OnValue_Position
	{
		get { return Transform.position; }
		set { SetPosition(value); }
	}


	/// <summary>
	/// returns the current horizontal and vertical input vector
	/// </summary>
	protected virtual Vector2 OnValue_InputMoveVector
	{
		get { return m_MoveVector; }
		set { m_MoveVector = value.y < 0 ? value.normalized * 0.65f : value.normalized; }
	}


	/// <summary>
	/// returns the current velocity
	/// </summary>
	protected virtual Vector3 OnValue_Velocity
	{
		get { return Velocity; }
	}


	/// <summary>
	/// gets or sets the current motor throttle
	/// </summary>
	protected virtual Vector3 OnValue_MotorThrottle
	{
		get { return m_MotorThrottle; }
		set { m_MotorThrottle = value; }
	}


	/// <summary>
	/// returns transform of current platform we're standing on
	/// </summary>
	protected virtual Transform OnValue_Platform
	{
		get { return m_Platform; }
		set { m_Platform = value; }
	}
	

	/// <summary>
	/// returns the current mainTexture under the controller.
	/// gets the texture from terrain if over terrain, else it
	/// looks in the transform of the current object the controller
	/// is over
	/// </summary>
	protected virtual Texture OnValue_GroundTexture
	{
		get
		{

			if (GroundTransform == null)
				return null;

			// return if no renderer and no terrain under the controller
			if(GroundTransform.renderer == null && m_CurrentTerrain == null)
				return null;
			
			int terrainTextureID = -1;
			
			// check to see if a main texture can be retrieved from the terrain
			if(m_CurrentTerrain != null)
			{
				terrainTextureID = vp_FootstepManager.GetMainTerrainTexture( m_Player.Position.Get(), m_CurrentTerrain );
				if(terrainTextureID > m_CurrentTerrain.terrainData.splatPrototypes.Length - 1)
					return null;
			}
			
			// return the texture
			return m_CurrentTerrain == null ? GroundTransform.renderer.material.mainTexture : m_CurrentTerrain.terrainData.splatPrototypes[ terrainTextureID ].texture;

		}
	}
	

	/// <summary>
	/// returns the current surface type under the controller
	/// </summary>
	protected virtual vp_SurfaceIdentifier OnValue_SurfaceType
	{
		get { return m_CurrentSurface; }
	}


}


