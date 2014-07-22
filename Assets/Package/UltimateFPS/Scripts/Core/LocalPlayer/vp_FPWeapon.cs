/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeapon.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	animates a weapon transform using springs, bob and perlin noise,
//					in response to user input
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class vp_FPWeapon : vp_Component
{

	// weapon model
	public GameObject WeaponPrefab = null;
	protected GameObject m_WeaponModel = null;

	// character controller of the parent gameobject
	protected CharacterController Controller = null;

	public float RenderingZoomDamping = 0.5f;
	protected float m_FinalZoomTime = 0.0f;

	// weapon rendering
	public float RenderingFieldOfView = 35.0f;
	public Vector2 RenderingClippingPlanes = new Vector2(0.01f, 10.0f);
	public float RenderingZScale = 1.0f;

	// weapon position spring
	public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);
	public float PositionSpringStiffness = 0.01f;
	public float PositionSpringDamping = 0.25f;
	public float PositionFallRetract = 1.0f;
	public float PositionPivotSpringStiffness = 0.01f;
	public float PositionPivotSpringDamping = 0.25f;
	public float PositionSpring2Stiffness = 0.95f;
	public float PositionSpring2Damping = 0.25f;
	public float PositionKneeling = 0.06f;
	public int PositionKneelingSoftness = 1;
	public Vector3 PositionWalkSlide = new Vector3(0.5f, 0.75f, 0.5f);
	public Vector3 PositionPivot = Vector3.zero;
	public Vector3 RotationPivot = Vector3.zero;
	public float PositionInputVelocityScale = 1.0f;
	public float PositionMaxInputVelocity = 25;
	protected vp_Spring m_PositionSpring = null;		// spring for player motion (shake, falling impact, sway, bob etc.)
	protected vp_Spring m_PositionSpring2 = null;		// spring for secondary forces like recoil (typically with stiffer spring settings)
	protected vp_Spring m_PositionPivotSpring = null;
	protected vp_Spring m_RotationPivotSpring = null;
	protected GameObject m_WeaponCamera = null;
	protected GameObject m_WeaponGroup = null;
	protected GameObject m_Pivot = null;
	protected Transform m_WeaponGroupTransform = null;

	// weapon rotation spring
	public Vector3 RotationOffset = Vector3.zero;
	public float RotationSpringStiffness = 0.01f;
	public float RotationSpringDamping = 0.25f;
	public float RotationPivotSpringStiffness = 0.01f;
	public float RotationPivotSpringDamping = 0.25f;
	public float RotationSpring2Stiffness = 0.95f;
	public float RotationSpring2Damping = 0.25f;
	public float RotationKneeling = 0;
	public int RotationKneelingSoftness = 1;
	public Vector3 RotationLookSway = new Vector3(1.0f, 0.7f, 0.0f);
	public Vector3 RotationStrafeSway = new Vector3(0.3f, 1.0f, 1.5f);
	public Vector3 RotationFallSway = new Vector3(1.0f, -0.5f, -3.0f);
	public float RotationSlopeSway = 0.5f;
	public float RotationInputVelocityScale = 1.0f;
	public float RotationMaxInputVelocity = 15;
	protected vp_Spring m_RotationSpring = null;		// spring for player motion (falling impact, sway, bob etc.)
	protected vp_Spring m_RotationSpring2 = null;		// spring for secondary forces like recoil (typically with stiffer spring settings)
	protected Vector3 m_SwayVel = Vector3.zero;
	protected Vector3 m_FallSway = Vector3.zero;

	// weapon retraction
	public float RetractionDistance = 0.0f;				// disabled by default. a typical value for distance is 1.0f
	public Vector2 RetractionOffset = new Vector2(0.0f, 0.0f);
	public float RetractionRelaxSpeed = 0.25f;
	protected bool m_DrawRetractionDebugLine = false;

	// weapon shake
	public float ShakeSpeed = 0.05f;
	public Vector3 ShakeAmplitude = new Vector3(0.25f, 0.0f, 2.0f);
	protected Vector3 m_Shake = Vector3.zero;

	// weapon bob
	public Vector4 BobRate = new Vector4(0.9f, 0.45f, 0.0f, 0.0f);			// TIP: y should be (x * 2) for a nice classic curve of motion
	public Vector4 BobAmplitude = new Vector4(0.35f, 0.5f, 0.0f, 0.0f);		// TIP: make x & y negative to invert the curve
	public float BobInputVelocityScale = 1.0f;
	public float BobMaxInputVelocity = 100;
	public bool BobRequireGroundContact = true;
	protected float m_LastBobSpeed = 0.0f;
	protected Vector4 m_CurrentBobAmp = Vector4.zero;
	protected Vector4 m_CurrentBobVal = Vector4.zero;
	protected float m_BobSpeed = 0.0f;

	// weapon footstep impact variables
	public Vector3 StepPositionForce = new Vector3(0, -0.0012f, -0.0012f);
	public Vector3 StepRotationForce = new Vector3(0, 0, 0);
	public int StepSoftness = 4;
	public float StepMinVelocity = 0.0f;
	public float StepPositionBalance = 0.0f;
	public float StepRotationBalance = 0.0f;
	public float StepForceScale = 1.0f;
	protected float m_LastUpBob = 0.0f;
	protected bool m_BobWasElevating = false;
	protected Vector3 m_PosStep = Vector3.zero;
	protected Vector3 m_RotStep = Vector3.zero;

	// sound
	public AudioClip SoundWield = null;							// sound for bringing out the weapon
	public AudioClip SoundUnWield = null;						// sound for putting the weapon away

	// animation
	public AnimationClip AnimationWield = null;
	public AnimationClip AnimationUnWield = null;
	public List<UnityEngine.Object> AnimationAmbient = new List<UnityEngine.Object>();
	protected List<bool> m_AmbAnimPlayed = new List<bool>();
	public Vector2 AmbientInterval = new Vector2(2.5f, 7.5f);
	protected int m_CurrentAmbientAnimation = 0;
	protected vp_Timer.Handle m_AnimationAmbientTimer = new vp_Timer.Handle();

	// weapon switching
	public Vector3 PositionExitOffset = new Vector3(0.0f, -1.0f, 0.0f);		// used by the camera when switching the weapon out of view
	public Vector3 RotationExitOffset = new Vector3(40.0f, 0.0f, 0.0f);
	protected bool m_Wielded = true;
	public bool Wielded { get { return (m_Wielded && Rendering); } }

	// misc
	public GameObject WeaponCamera { get { return m_WeaponCamera; } }
	public GameObject WeaponModel { get { return m_WeaponModel; } }
	public Vector3 DefaultPosition { get { return (Vector3)DefaultState.Preset.GetFieldValue("PositionOffset"); } }
	public Vector3 DefaultRotation { get { return (Vector3)DefaultState.Preset.GetFieldValue("RotationOffset"); } }
	public bool DrawRetractionDebugLine { get { return m_DrawRetractionDebugLine; } set { m_DrawRetractionDebugLine = value; } }	// for editor use
	protected Vector2 m_MouseMove = Vector2.zero;		// mouse distance moved since last frame

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
	/// in 'Awake' we do things that need to be run once at the
	/// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	/// can not be altered in 'Awake'
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		if (transform.parent == null)
		{
			Debug.LogError("Error (" + this + ") Must not be placed in scene root. Disabling self.");
			vp_Utility.Activate(gameObject, false);
			return;
		}

		// store a reference to the Unity CharacterController
		Controller = Transform.root.GetComponent<CharacterController>();

		if (Controller == null)
		{
			Debug.LogError("Error (" + this + ") Could not find CharacterController. Disabling self.");
			vp_Utility.Activate(gameObject, false);
			return;
		}

		// always start with zero angle
		Transform.eulerAngles = Vector3.zero;

		// hook up the weapon camera - find a regular Unity Camera
		// component existing in a child gameobject to our parent
		// gameobject. if we don't find a camera, this weapon won't
		// render.
		Camera weaponCam = null;
		foreach (Transform t in Transform.parent)
		{
			weaponCam = (Camera)t.GetComponent(typeof(Camera));
			if (weaponCam != null)
			{
				m_WeaponCamera = weaponCam.gameObject;
				break;
			}
		}

		// disallow colliders on the weapon or we may get issues with
		// player collision
		if (collider != null)
			collider.enabled = false;


	}

	
	/// <summary>
	/// in 'Start' we do things that need to be run once at the
	/// beginning, but potentially depend on all other scripts
	/// first having run their 'Awake' calls.
	/// NOTE: don't do anything here that depends on activity
	/// in other 'Start' calls
	/// </summary>
	protected override void Start()
	{

		base.Start();

		// attempt to spawn a weapon model, if available
		InstantiateWeaponModel();

		// set up the weapon group, to which the weapon and pivot
		// object will be childed at runtime (the main purpose of
		// this is allowing runtime pivot manipulation)
		m_WeaponGroup = new GameObject(name + "Transform");
		m_WeaponGroupTransform = m_WeaponGroup.transform;
		m_WeaponGroupTransform.parent = Transform.parent;
		m_WeaponGroupTransform.localPosition = PositionOffset;
		vp_Layer.Set(m_WeaponGroup, vp_Layer.Weapon);

		// reposition weapon under weapon group gameobject
		Transform.parent = m_WeaponGroupTransform;
		Transform.localPosition = Vector3.zero;
		m_WeaponGroupTransform.localEulerAngles = RotationOffset;

		// put this gameobject and all its descendants in the 'WeaponLayer'
		// so the weapon camera can render them separately from the scene
		if (m_WeaponCamera != null && vp_Utility.IsActive(m_WeaponCamera.gameObject))
			vp_Layer.Set(gameObject, vp_Layer.Weapon, true);

		// setup weapon pivot object
		m_Pivot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		m_Pivot.name = "Pivot";
		m_Pivot.collider.enabled = false;
		m_Pivot.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		m_Pivot.transform.parent = m_WeaponGroupTransform;
		m_Pivot.transform.localPosition = Vector3.zero;
		m_Pivot.layer = vp_Layer.Weapon;
		vp_Utility.Activate(m_Pivot.gameObject, false);
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.color = new Color(0, 0, 1, 0.5f);
		m_Pivot.renderer.material = material;

		// setup the weapon springs
		m_PositionSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Position);
		m_PositionSpring.RestState = PositionOffset;

		m_PositionPivotSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.Position);
		m_PositionPivotSpring.RestState = PositionPivot;

		m_PositionSpring2 = new vp_Spring(Transform, vp_Spring.UpdateMode.PositionAdditive);
		m_PositionSpring2.MinVelocity = 0.00001f;

		m_RotationSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Rotation);
		m_RotationSpring.RestState = RotationOffset;

		m_RotationPivotSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.Rotation);
		m_RotationPivotSpring.RestState = RotationPivot;

		m_RotationSpring2 = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.RotationAdditive);
		m_RotationSpring2.MinVelocity = 0.00001f;

		// snap the springs so they always start out rested & in the right place
		SnapSprings();
		Refresh();

	}


	/// <summary>
	/// if a weapon model has been assigned in the editor slot,
	/// instantiates it and resets its position and rotation
	/// </summary>
	public virtual void InstantiateWeaponModel()
	{

		if (WeaponPrefab != null)
		{
			if (m_WeaponModel != null && m_WeaponModel != this.gameObject)
				Destroy(m_WeaponModel);
			m_WeaponModel = (GameObject)Object.Instantiate(WeaponPrefab);
			m_WeaponModel.transform.parent = transform;
			m_WeaponModel.transform.localPosition = Vector3.zero;
			m_WeaponModel.transform.localScale = new Vector3(1, 1, RenderingZScale);
			m_WeaponModel.transform.localEulerAngles = Vector3.zero;

			// set layer here too in case the method is called at runtime from the editor
			if (m_WeaponCamera != null && vp_Utility.IsActive(m_WeaponCamera.gameObject))
				vp_Layer.Set(m_WeaponModel, vp_Layer.Weapon, true);
		}
		else
			m_WeaponModel = this.gameObject;

		CacheRenderers();

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Init()
	{

		base.Init();

		// schedule the first ambient animation
		ScheduleAmbientAnimation();

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
	{

		base.Update();

		if (Time.timeScale != 0.0f)
			UpdateMouseLook();

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void FixedUpdate()
	{

		base.FixedUpdate();

		if (Time.timeScale != 0.0f)
		{

			UpdateZoom();

			UpdateSwaying();

			UpdateBob();

			UpdateEarthQuake();

			UpdateStep();

			UpdateShakes();

			UpdateRetraction();

			UpdateSprings();

		}

	}


	/// <summary>
	/// applies positional and angular force to the weapon. the
	/// typical use for this method is applying recoil force.
	/// </summary>
	public virtual void AddForce2(Vector3 positional, Vector3 angular)
	{

		m_PositionSpring2.AddForce(positional);
		m_RotationSpring2.AddForce(angular);

	}


	public virtual void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}


	/// <summary>
	/// pushes the weapon position spring along the 'force' vector
	/// for one frame. For external use.
	/// </summary>
	public virtual void AddForce(Vector3 force)
	{
		m_PositionSpring.AddForce(force);
	}


	/// <summary>
	/// pushes the weapon position spring along the 'force' vector
	/// for one frame. For external use.
	/// </summary>
	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}


	/// <summary>
	/// applies positional and angular force to the weapon
	/// </summary>
	public virtual void AddForce(Vector3 positional, Vector3 angular)
	{

		m_PositionSpring.AddForce(positional);
		m_RotationSpring.AddForce(angular);

	}


	/// <summary>
	/// applies positional and angular force to the weapon
	/// </summary>
	public virtual void AddForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}


	/// <summary>
	/// pushes the weapon position spring along the 'force' vector
	/// distributed over 'frames' fixed frames
	/// </summary>
	public virtual void AddSoftForce(Vector3 force, int frames)
	{
		m_PositionSpring.AddSoftForce(force, frames);
	}


	/// <summary>
	/// pushes the weapon position spring along the 'force' vector
	/// distributed over 'frames' fixed frames
	/// </summary>
	public virtual void AddSoftForce(float x, float y, float z, int frames)
	{
		AddSoftForce(new Vector3(x, y, z), frames);
	}


	/// <summary>
	/// applies soft positional and angular force to the weapon
	/// </summary>
	public virtual void AddSoftForce(Vector3 positional, Vector3 angular, int frames)
	{

		m_PositionSpring.AddSoftForce(positional, frames);
		m_RotationSpring.AddSoftForce(angular, frames);

	}


	/// <summary>
	/// applies soft positional and angular force to the weapon
	/// </summary>
	public virtual void AddSoftForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot, int frames)
	{
		AddSoftForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot), frames);
	}


	/// <summary>
	/// performs a mouselook implementation specific to weapon motions
	/// </summary>
	protected virtual void UpdateMouseLook()
	{

		// get mouse input for this frame. unlike camera mouse input, this must
		// be divided by delta for rotation spring framerate independence, and
		// multiplied by timescale _twice_  for proper slow motion behavior
		m_MouseMove.x = Input.GetAxisRaw("Mouse X") / Delta * Time.timeScale * Time.timeScale;
		m_MouseMove.y = Input.GetAxisRaw("Mouse Y") / Delta * Time.timeScale * Time.timeScale;

		// limit rotation velocity to protect against extreme mouse sensitivity
		m_MouseMove *= RotationInputVelocityScale;
		m_MouseMove = Vector3.Min(m_MouseMove, Vector3.one * RotationMaxInputVelocity);
		m_MouseMove = Vector3.Max(m_MouseMove, Vector3.one * -RotationMaxInputVelocity);

	}


	/// <summary>
	/// interpolates to the target FOV value
	/// </summary>
	protected virtual void UpdateZoom()
	{

		if (m_FinalZoomTime <= Time.time)
			return;

		if (!m_Wielded)
			return;

		RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
		float zoom = 1.0f - ((m_FinalZoomTime - Time.time) / RenderingZoomDamping);

		if (m_WeaponCamera != null && vp_Utility.IsActive(m_WeaponCamera.gameObject))
			m_WeaponCamera.camera.fov = Mathf.SmoothStep(m_WeaponCamera.gameObject.camera.fov,
																	RenderingFieldOfView, zoom);

	}


	/// <summary>
	/// interpolates to the target FOV using 'WeaponRenderingZoomDamping'
	/// as interval
	/// </summary>
	public virtual void Zoom()
	{
		m_FinalZoomTime = Time.time + RenderingZoomDamping;
	}


	/// <summary>
	/// instantly sets camera to the target FOV
	/// </summary>
	public virtual void SnapZoom()
	{

		if (m_WeaponCamera != null && vp_Utility.IsActive(m_WeaponCamera.gameObject))
			m_WeaponCamera.camera.fov = RenderingFieldOfView;

	}


	/// <summary>
	/// updates the procedural shaking of the weapon.
	/// this is a purely aesthetic motion to breathe life into the first
	/// person arm / weapon. if one wanted to expand on this, one could
	/// alternate between higher & lower speeds and amplitudes.
	/// </summary>
	protected virtual void UpdateShakes()
	{

		// apply weapon shake
		if (ShakeSpeed != 0.0f)
		{
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
			m_RotationSpring.AddForce(m_Shake * Time.timeScale);
		}

	}


	/// <summary>
	/// prevents the weapon from intersecting other objects by
	/// raycasting ahead and blocking the weapon on the first
	/// surface hit. NOTE: this works by moving the actual
	/// weapon transform. springs are not involved
	/// </summary>
	protected virtual void UpdateRetraction(bool firstIteration = true)
	{

		// retraction is disabled by default
		if (RetractionDistance == 0.0f)
			return;

		// start position is the weapon model world position plus local
		// XY retraction offset
		Vector3 startPos = WeaponModel.transform.TransformPoint((Vector3)RetractionOffset);

		// end position is 'RetractionPoint' meters ahead of the weapon model
		Vector3 endPos = startPos + (WeaponModel.transform.forward * RetractionDistance);

		// raycast to see if we hit an external blocker
		RaycastHit hit;
		if (Physics.Linecast(startPos, endPos, out hit, vp_Layer.Mask.ExternalBlockers) &&
			!hit.collider.isTrigger)
		{

			// block weapon model at the hit point, but allow it to intersect
			// just a tiny bit (0.99) to avoid jittering
			WeaponModel.transform.position = hit.point - (hit.point - startPos).normalized *
				(RetractionDistance * 0.99f);
			// the above jittering fix can make the weapon stick to surfaces under some
			// circumstances, unless we prevent it by capping weapon z pos to zero
			WeaponModel.transform.localPosition = Vector3.forward *
				Mathf.Min(WeaponModel.transform.localPosition.z, 0.0f);

		}
		else if (firstIteration && (WeaponModel.transform.localPosition != Vector3.zero) &&
			(WeaponModel != this.gameObject))
		{

			// if we end up here, the weapon model doesn't intersect an object
			// but is still retracted from a previous intersection. we'll smooth
			// step it back to its normal position. this assumes that when
			// retraction is not in effect, the local position of the weapon
			// transform is always Vector3.zero (NOTE: it can still have offset)
			WeaponModel.transform.localPosition = Vector3.forward *
				Mathf.SmoothStep(WeaponModel.transform.localPosition.z,
				0,
				RetractionRelaxSpeed * Time.timeScale);

			// update retraction once again in case the above resulted in
			// a new intersection = jittering. setting 'firstIteration' to
			// false means this only runs once (preventing an infinite loop)
			UpdateRetraction(false);

		}

		// draw a retraction debug line in the scene view. this is
		// automatically set to true if the retraction foldout is
		// open and allowed in the editor
#if UNITY_EDITOR
		if (m_DrawRetractionDebugLine)
			Debug.DrawLine(startPos, endPos, (hit.collider == null) ? Color.yellow : Color.red);
#endif

	}


	/// <summary>
	/// speed should be the magnitude speed of the character
	/// controller. if controller has no ground contact, '0.0f'
	/// should be passed and the bob will fade to a halt.
	/// </summary>
	protected virtual void UpdateBob()
	{

		if (BobAmplitude == Vector4.zero || BobRate == Vector4.zero)
			return;

		m_BobSpeed = ((BobRequireGroundContact && !Controller.isGrounded) ? 0.0f : Controller.velocity.sqrMagnitude);

		// scale and limit input velocity
		m_BobSpeed = Mathf.Min(m_BobSpeed * BobInputVelocityScale, BobMaxInputVelocity);

		// reduce number of decimals to avoid floating point imprecision bugs
		m_BobSpeed = Mathf.Round(m_BobSpeed * 1000.0f) / 1000.0f;

		// if speed is zero, this means we should just fade out the last stored
		// speed value. NOTE: it's important to clamp it to the current max input
		// velocity since the preset may have changed since last bob!
		if (m_BobSpeed == 0)
			m_BobSpeed = Mathf.Min((m_LastBobSpeed * 0.93f), BobMaxInputVelocity);

		m_CurrentBobAmp.x = (m_BobSpeed * (BobAmplitude.x * -0.0001f));
		m_CurrentBobVal.x = (Mathf.Cos(Time.time * (BobRate.x * 10.0f))) * m_CurrentBobAmp.x;

		m_CurrentBobAmp.y = (m_BobSpeed * (BobAmplitude.y * 0.0001f));
		m_CurrentBobVal.y = (Mathf.Cos(Time.time * (BobRate.y * 10.0f))) * m_CurrentBobAmp.y;

		m_CurrentBobAmp.z = (m_BobSpeed * (BobAmplitude.z * 0.0001f));
		m_CurrentBobVal.z = (Mathf.Cos(Time.time * (BobRate.z * 10.0f))) * m_CurrentBobAmp.z;

		m_CurrentBobAmp.w = (m_BobSpeed * (BobAmplitude.w * 0.0001f));
		m_CurrentBobVal.w = (Mathf.Cos(Time.time * (BobRate.w * 10.0f))) * m_CurrentBobAmp.w;

		m_RotationSpring.AddForce(m_CurrentBobVal * Time.timeScale);
		m_PositionSpring.AddForce(Vector3.forward * m_CurrentBobVal.w * Time.timeScale);

		m_LastBobSpeed = m_BobSpeed;

	}


	/// <summary>
	/// shakes the weapon according to any global earthquake
	/// detectable via the event handler
	/// </summary>
	protected virtual void UpdateEarthQuake()
	{

		if (Player == null)
			return;

		if (!Player.Earthquake.Active)
			return;

		if (!Controller.isGrounded)
			return;

		// apply earthquake force on the weapon
		Vector3 earthQuakeForce = Player.EarthQuakeForce.Get();
		AddForce(new Vector3(0, 0, -earthQuakeForce.z * 0.015f) /** m_EarthQuakeWeaponShakeFactor*/,
					new Vector3(earthQuakeForce.y * 2, -earthQuakeForce.x, earthQuakeForce.x * 2) /** m_EarthQuakeWeaponShakeFactor*/);

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void UpdateSprings()
	{

		m_PositionSpring.FixedUpdate();
		m_PositionPivotSpring.FixedUpdate();
		m_RotationPivotSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring.FixedUpdate();
		m_RotationSpring2.FixedUpdate();

	}


	/// <summary>
	/// applies force to the weapon springs to simulate a fine
	/// footstep impact in sync with the weapon bob. a footstep
	/// is triggered when the vertical weapon bob reaches its
	/// bottom value, provided that the controller's squared
	/// velocity is higher than 'FootStepMinVelocity' (which
	/// must be above zero)
	/// </summary>
	protected virtual void UpdateStep()
	{

		if (StepMinVelocity <= 0.0f ||
			(BobRequireGroundContact && !Controller.isGrounded) ||
			Controller.velocity.sqrMagnitude < StepMinVelocity)
			return;

		bool elevating = (m_LastUpBob < m_CurrentBobVal.x) ? true : false;
		m_LastUpBob = m_CurrentBobVal.x;

		// if bob is dipping, add soft down force to the weapon
		if (elevating && !m_BobWasElevating)
		{

			// apply soft footstep force on the weapon's position and
			// rotation springs, and multiply footstep force depending
			// on current "foot". TIP: this can be used to reduce or
			// enhance the effect of limping

			// every other time the bob dips, apply the force for left
			// versus right foot depending on balance
			if ((Mathf.Cos(Time.time * (BobRate.x * 5.0f))) > 0)
			{
				m_PosStep = StepPositionForce - (StepPositionForce * StepPositionBalance);
				m_RotStep = StepRotationForce - (StepPositionForce * StepRotationBalance);
			}
			else
			{
				m_PosStep = StepPositionForce + (StepPositionForce * StepPositionBalance);
				m_RotStep = Vector3.Scale(StepRotationForce - (StepPositionForce * StepRotationBalance),
												-Vector3.one + (Vector3.right * 2));	// invert y & z rotation
			}

			AddSoftForce(m_PosStep * StepForceScale, m_RotStep * StepForceScale, StepSoftness);

		}

		m_BobWasElevating = elevating;

	}


	/// <summary>
	/// applies swaying forces to the weapon in response to user
	/// input and character controller motion. this includes
	/// mouselook, falling, strafing and walking.
	/// </summary>
	protected virtual void UpdateSwaying()
	{

		// limit position velocity to protect against extreme speeds
		m_SwayVel = Controller.velocity * PositionInputVelocityScale;
		m_SwayVel = Vector3.Min(m_SwayVel, Vector3.one * PositionMaxInputVelocity);
		m_SwayVel = Vector3.Max(m_SwayVel, Vector3.one * -PositionMaxInputVelocity);

		m_SwayVel *= Time.timeScale;

		// calculate local velocity
		Vector3 localVelocity = Transform.InverseTransformDirection(m_SwayVel / 60);

		// --- pitch & yaw rotational sway ---

		// sway the weapon transform using mouse input and weapon 'weight'
		m_RotationSpring.AddForce(new Vector3(
			(m_MouseMove.y * (RotationLookSway.x * 0.025f)),
			(m_MouseMove.x * (RotationLookSway.y * -0.025f)),
			m_MouseMove.x * (RotationLookSway.z * -0.025f)));

		// --- falling ---

		// rotate weapon while falling. this will take effect in reverse when being elevated,
		// for example walking up a ramp. however, the weapon will only rotate around the z
		// vector while going down
		m_FallSway = (RotationFallSway * (m_SwayVel.y * 0.005f));
		// if grounded, optionally reduce fallsway
		if (Controller.isGrounded)
			m_FallSway *= RotationSlopeSway;
		m_FallSway.z = Mathf.Max(0.0f, m_FallSway.z);
		m_RotationSpring.AddForce(m_FallSway);

		// drag weapon towards ourselves
		m_PositionSpring.AddForce(Vector3.forward * -Mathf.Abs((m_SwayVel.y) * (PositionFallRetract * 0.000025f)));

		// --- weapon strafe & walk slide ---
		// PositionWalkSlide x will slide sideways when strafing
		// PositionWalkSlide y will slide down when strafing (it can't push up)
		// PositionWalkSlide z will slide forward or backward when walking
		m_PositionSpring.AddForce(new Vector3(
			(localVelocity.x * (PositionWalkSlide.x * 0.0016f)),
			-(Mathf.Abs(localVelocity.x * (PositionWalkSlide.y * 0.0016f))),
			(-localVelocity.z * (PositionWalkSlide.z * 0.0016f))));

		// --- weapon strafe rotate ---
		// RotationStrafeSway x will rotate up when strafing (it can't push down)
		// RotationStrafeSway y will rotate sideways when strafing
		// RotationStrafeSway z will twist weapon around the forward vector when strafing
		m_RotationSpring.AddForce(new Vector3(
			-Mathf.Abs(localVelocity.x * (RotationStrafeSway.x * 0.16f)),
			-(localVelocity.x * (RotationStrafeSway.y * 0.16f)),
			localVelocity.x * (RotationStrafeSway.z * 0.16f)));

	}


	/// <summary>
	/// use this method to force the weapon back to its default
	/// pose by the 'reset' values (0-1). this can be used to make
	/// a weapon always fire in the forward direction regardless
	/// of current weapon angles. optional 'pauseTime' parameters
	/// may be provided, instantly freezing any forces acting on
	/// the primary position and rotation springs and easing
	/// them back in over 'pauseTime' interval in seconds.
	/// </summary>
	public virtual void ResetSprings(float positionReset, float rotationReset, float positionPauseTime = 0.0f, float rotationPauseTime = 0.0f)
	{

		m_PositionSpring.State = Vector3.Lerp(m_PositionSpring.State, m_PositionSpring.RestState, positionReset);
		m_RotationSpring.State = Vector3.Lerp(m_RotationSpring.State, m_RotationSpring.RestState, rotationReset);
		m_PositionPivotSpring.State = Vector3.Lerp(m_PositionPivotSpring.State, m_PositionPivotSpring.RestState, positionReset);
		m_RotationPivotSpring.State = Vector3.Lerp(m_RotationPivotSpring.State, m_RotationPivotSpring.RestState, rotationReset);

		if (positionPauseTime != 0.0f)
			m_PositionSpring.ForceVelocityFadeIn(positionPauseTime);

		if (rotationPauseTime != 0.0f)
			m_RotationSpring.ForceVelocityFadeIn(rotationPauseTime);

		if (positionPauseTime != 0.0f)
			m_PositionPivotSpring.ForceVelocityFadeIn(positionPauseTime);

		if (rotationPauseTime != 0.0f)
			m_RotationPivotSpring.ForceVelocityFadeIn(rotationPauseTime);

	}


	/// <summary>
	/// this method is called to reset various weapon settings,
	/// typically after creating or loading a weapon
	/// </summary>
	public override void Refresh()
	{

		if (!Application.isPlaying)
			return;

		if (m_PositionSpring != null)
		{
			m_PositionSpring.Stiffness =
				new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
			m_PositionSpring.Damping = Vector3.one -
				new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
		}

		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.Stiffness =
				new Vector3(PositionPivotSpringStiffness, PositionPivotSpringStiffness, PositionPivotSpringStiffness);
			m_PositionPivotSpring.Damping = Vector3.one -
				new Vector3(PositionPivotSpringDamping, PositionPivotSpringDamping, PositionPivotSpringDamping);
			m_PositionPivotSpring.RestState = PositionPivot;
		}

		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.Stiffness =
				new Vector3(RotationPivotSpringStiffness, RotationPivotSpringStiffness, RotationPivotSpringStiffness);
			m_RotationPivotSpring.Damping = Vector3.one -
				new Vector3(RotationPivotSpringDamping, RotationPivotSpringDamping, RotationPivotSpringDamping);
			m_RotationPivotSpring.RestState = RotationPivot;

		}

		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.Stiffness =
				new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
			m_PositionSpring2.Damping = Vector3.one -
				new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
			m_PositionSpring2.RestState = Vector3.zero;
		}

		if (m_RotationSpring != null)
		{
			m_RotationSpring.Stiffness =
				new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
			m_RotationSpring.Damping = Vector3.one -
				new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);
			m_RotationSpring.RestState = RotationOffset;

		}

		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.Stiffness =
				new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
			m_RotationSpring2.Damping = Vector3.one -
				new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);
			m_RotationSpring2.RestState = Vector3.zero;
		}


		if (Rendering)
		{

			if ((m_WeaponCamera != null) && vp_Utility.IsActive(m_WeaponCamera.gameObject))
			{
				m_WeaponCamera.camera.nearClipPlane = RenderingClippingPlanes.x;
				m_WeaponCamera.camera.farClipPlane = RenderingClippingPlanes.y;
			}

			Zoom();

		}


	}


	/// <summary>
	/// performs special activation logic for wielding a weapon
	/// properly
	/// </summary>
	public override void Activate()
	{

		m_Wielded = true;
		Rendering = true;
		m_DeactivationTimer.Cancel();
		SnapZoom();
		if (m_WeaponGroup != null)
		{
			if (!vp_Utility.IsActive(m_WeaponGroup))
				vp_Utility.Activate(m_WeaponGroup);
		}
		SetPivotVisible(false);

	}


	/// <summary>
	/// performs special deactivation logic for unwielding a
	/// weapon properly
	/// </summary>
	public override void Deactivate()
	{

		m_Wielded = false;
		if (m_WeaponGroup != null)
		{
			if (vp_Utility.IsActive(m_WeaponGroup))
				vp_Utility.Activate(m_WeaponGroup, false);
		}

	}


	/// <summary>
	/// this method is called to reset the pivot of the weapon
	/// model, typically after creating or loading a camera or
	/// a weapon
	/// </summary>
	public virtual void SnapPivot()
	{

		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
		}
		if (m_WeaponGroup != null)
			m_WeaponGroupTransform.localPosition = PositionOffset - PositionPivot;

		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
		}

		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.RestState = RotationPivot;
			m_RotationPivotSpring.State = RotationPivot;
		}

		Transform.localPosition = PositionPivot;
		Transform.localEulerAngles = RotationPivot;

	}


	/// <summary>
	/// toggles visibility of the weapon model pivot for editor
	/// purposes
	/// </summary>
	public virtual void SetPivotVisible(bool visible)
	{

		if (m_Pivot == null)
			return;

		vp_Utility.Activate(m_Pivot.gameObject, visible);
		
	}


	/// <summary>
	/// forces the 'exit offset' position & angle immediately
	/// </summary>
	public virtual void SnapToExit()
	{

		RotationOffset = RotationExitOffset;
		PositionOffset = PositionExitOffset;
		SnapSprings();
		SnapPivot();

	}


	/// <summary>
	/// resets all the springs to their default positions, i.e.
	/// for when loading a new camera or switching a weapon
	/// </summary>
	public virtual void SnapSprings()
	{

		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
			m_PositionSpring.Stop(true);
		}
		if (m_WeaponGroup != null)
			m_WeaponGroupTransform.localPosition = PositionOffset - PositionPivot;

		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
			m_PositionPivotSpring.Stop(true);
		}
		Transform.localPosition = PositionPivot;

		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop(true);
		}

		if (m_RotationPivotSpring != null)
		{
			m_RotationPivotSpring.RestState = RotationPivot;
			m_RotationPivotSpring.State = RotationPivot;
			m_RotationPivotSpring.Stop(true);
		}
		Transform.localEulerAngles = RotationPivot;

		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = RotationOffset;
			m_RotationSpring.State = RotationOffset;
			m_RotationSpring.Stop(true);
		}

		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.RestState = Vector3.zero;
			m_RotationSpring2.State = Vector3.zero;
			m_RotationSpring2.Stop(true);
		}

	}


	/// <summary>
	/// stops all the springs
	/// </summary>
	public virtual void StopSprings()
	{

		if (m_PositionSpring != null)
			m_PositionSpring.Stop(true);

		if (m_PositionPivotSpring != null)
			m_PositionPivotSpring.Stop(true);

		if (m_PositionSpring2 != null)
			m_PositionSpring2.Stop(true);

		if (m_RotationSpring != null)
			m_RotationSpring.Stop(true);

		if (m_RotationPivotSpring != null)
			m_RotationPivotSpring.Stop(true);

		if (m_RotationSpring2 != null)
			m_RotationSpring2.Stop(true);

	}


	/// <summary>
	/// transitions the weapon into / out of view depending on
	/// the 'active' parameter, playing wield or unwield sounds
	/// and animations accordingly
	/// </summary>
	public virtual void Wield(bool showWeapon = true)
	{

		if (showWeapon)
			SnapToExit();

		// smoothly rotate and move the weapon into / out of view
		PositionOffset = (showWeapon ? DefaultPosition : PositionExitOffset);
		RotationOffset = (showWeapon ? DefaultRotation : RotationExitOffset);

		m_Wielded = showWeapon;

		Refresh();
		StateManager.CombineStates();

		// play sound
		if (Audio != null)
		{

			if ((showWeapon ? SoundWield : SoundUnWield) != null)
			{
				if (vp_Utility.IsActive(gameObject))
				{
					Audio.pitch = Time.timeScale;
					Audio.PlayOneShot((showWeapon ? SoundWield : SoundUnWield));
				}
			}
		}

		// play animation
		if ((showWeapon ? AnimationWield : AnimationUnWield) != null)
		{
			if (vp_Utility.IsActive(gameObject))
				m_WeaponModel.animation.CrossFade((showWeapon ? AnimationWield : AnimationUnWield).name);
		}


	}


	/// <summary>
	/// schedules playing an ambient animation on the weapon
	/// with random intervals
	/// </summary>
	public virtual void ScheduleAmbientAnimation()
	{

		if ((AnimationAmbient.Count == 0) || (!vp_Utility.IsActive(gameObject)))
			return;

		vp_Timer.In(Random.Range(AmbientInterval.x, AmbientInterval.y), delegate()
		{
			// need to check for active here too since weapon may have
			// been deactivated since ambient animation was scheduled
			if (vp_Utility.IsActive(gameObject))
			{
				m_CurrentAmbientAnimation = Random.Range(0, AnimationAmbient.Count);
				if (AnimationAmbient[m_CurrentAmbientAnimation] != null)
				{
					m_WeaponModel.animation.CrossFadeQueued(AnimationAmbient[m_CurrentAmbientAnimation].name);
					ScheduleAmbientAnimation();
				}
			}
		}, m_AnimationAmbientTimer);

	}


	/// <summary>
	/// applies a falling impact to the weapon position spring
	/// </summary>
	protected virtual void OnMessage_FallImpact(float impact)
	{

		if (m_PositionSpring != null)
			m_PositionSpring.AddSoftForce(Vector3.down * impact * PositionKneeling, PositionKneelingSoftness);
	
		if (m_RotationSpring != null)
			m_RotationSpring.AddSoftForce(Vector3.right * impact * RotationKneeling, RotationKneelingSoftness);

	}


	/// <summary>
	/// applies a force to the camera rotation spring (intended
	/// for when the controller bumps into objects above it)
	/// </summary>
	protected virtual void OnMessage_HeadImpact(float impact)
	{

		AddForce(Vector3.zero, Vector3.forward * (Mathf.Abs((float)impact) * 20.0f) * Time.timeScale);

	}


	/// <summary>
	/// makes the weapon shake as if a large dinosaur or mech
	/// is approaching. great for bosses!
	/// </summary>
	protected virtual void OnMessage_GroundStomp(float impact)
	{

		AddForce(Vector3.zero, new Vector3(-0.25f, 0.0f, 0.0f) * impact);

	}


	/// <summary>
	/// makes the weapon shake as if a bomb has gone off nearby
	/// </summary>
	protected virtual void OnMessage_BombShake(float impact)
	{
		
		AddForce(Vector3.zero, new Vector3(-0.3f, 0.1f, 0.5f) * impact);

	}

	
}


