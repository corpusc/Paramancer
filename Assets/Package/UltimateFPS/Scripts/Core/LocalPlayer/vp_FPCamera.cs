/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPCamera.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a first person camera class with weapon rendering and animation
//					features. animates the camera transform using springs, bob and
//					perlin noise, in response to user input
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]

public class vp_FPCamera : vp_Component
{

	// character controller of the parent gameobject
	public vp_FPController FPController = null;
	
	// camera input
	public Vector2 MouseSensitivity = new Vector2(5.0f, 5.0f);
	public int MouseSmoothSteps = 10;				// allowed range: 1-20
	public float MouseSmoothWeight = 0.5f;			// allowed range: 0.0f - 1.0f
	public bool MouseAcceleration = false;
	public float MouseAccelerationThreshold = 0.4f;
	protected Vector2 m_MouseMove = Vector2.zero;		// distance moved since last frame
	protected List<Vector2> m_MouseSmoothBuffer = new List<Vector2>();

	// camera rendering
	public float RenderingFieldOfView = 60.0f;
	public float RenderingZoomDamping = 0.2f;
	protected float m_FinalZoomTime = 0.0f;

	// camera position
	public Vector3 PositionOffset = new Vector3(0.0f, 1.75f, 0.1f);
	public float PositionGroundLimit = 0.1f;
	public float PositionSpringStiffness = 0.01f;
	public float PositionSpringDamping = 0.25f;
	public float PositionSpring2Stiffness = 0.95f;
	public float PositionSpring2Damping = 0.25f;
	public float PositionKneeling = 0.025f;
	public int PositionKneelingSoftness = 1;
	public float PositionEarthQuakeFactor = 1.0f;
	protected vp_Spring m_PositionSpring = null;		// spring for external forces (falling impact, bob, earthquakes)
	protected vp_Spring m_PositionSpring2 = null;		// 2nd spring for external forces (typically with stiffer spring settings)
	protected bool m_DrawCameraCollisionDebugLine = false;

	// camera rotation
	public Vector2 RotationPitchLimit = new Vector2(90.0f, -90.0f);
	public Vector2 RotationYawLimit = new Vector2(-360.0f, 360.0f);
	public float RotationSpringStiffness = 0.01f;
	public float RotationSpringDamping = 0.25f;
	public float RotationKneeling = 0.025f;
	public int RotationKneelingSoftness = 1;
	public float RotationStrafeRoll = 0.01f;
	public float RotationEarthQuakeFactor = 0.0f;
	protected float m_Pitch = 0.0f;
	protected float m_Yaw = 0.0f;
	protected vp_Spring m_RotationSpring = null;
	protected Vector2 m_InitialRotation = Vector2.zero;	// angle of camera at moment of startup

	// camera shake
	public float ShakeSpeed = 0.0f;
	public Vector3 ShakeAmplitude = new Vector3(10, 10, 0);
	protected Vector3 m_Shake = Vector3.zero;

	// camera bob
	public Vector4 BobRate = new Vector4(0.0f, 1.4f, 0.0f, 0.7f);			// TIP: use x for a mech / dino like walk cycle. y should be (x * 2) for a nice classic curve of motion. typical defaults for y are 0.9 (rate) and 0.1 (amp)
	public Vector4 BobAmplitude = new Vector4(0.0f, 0.25f, 0.0f, 0.5f);		// TIP: make x & y negative to invert the curve
	public float BobInputVelocityScale = 1.0f;								
	public float BobMaxInputVelocity = 100;									// TIP: calibrate using 'Debug.Log(Controller.velocity.sqrMagnitude);'
	public bool BobRequireGroundContact = true;
	protected float m_LastBobSpeed = 0.0f;
	protected Vector4 m_CurrentBobAmp = Vector4.zero;
	protected Vector4 m_CurrentBobVal = Vector4.zero;
	protected float m_BobSpeed = 0.0f;

	// camera bob step variables
	public delegate void BobStepDelegate();
	public BobStepDelegate BobStepCallback;
	public float BobStepThreshold = 10.0f;
	protected float m_LastUpBob = 0.0f;
	protected bool m_BobWasElevating = false;

	// camera collision
	protected Vector3 m_CameraCollisionStartPos = Vector3.zero;
	protected Vector3 m_CameraCollisionEndPos = Vector3.zero;
	protected RaycastHit m_CameraHit;

	// debug
	public bool DrawCameraCollisionDebugLine { get { return m_DrawCameraCollisionDebugLine; } set { m_DrawCameraCollisionDebugLine = value; } }	// for editor use

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

	//////////////////////////////////////////////////////////
	// angle properties
	//////////////////////////////////////////////////////////

	public Vector2 Angle
	{
		get { return new Vector2(m_Pitch, m_Yaw); }
		set
		{
			Pitch = value.x;
			Yaw = value.y;
		}
	}
	
	public Vector3 Forward
	{
		get { return m_Transform.forward; }
	}
		
	public float Pitch
	{
		// pitch is rotation around the x-vector
		get { return m_Pitch; }
		set
		{
			if (value > 90)
				value -= 360;
			m_Pitch = value;
		}
	}

	public float Yaw
	{
		// yaw is rotation around the y-vector
		get { return m_Yaw; }
		set
		{
			// discard initial editor rotation
			m_Yaw = value;
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

		FPController = Root.GetComponent<vp_FPController>();

		// detect angle of camera at moment of startup. this will be added to all mouse
		// input and is needed to retain initial rotation set by user in the editor.
		m_InitialRotation = new Vector2(Transform.eulerAngles.y, Transform.eulerAngles.x);

		// set parent gameobject layer to 'LocalPlayer', so camera can exclude it
		Parent.gameObject.layer = vp_Layer.LocalPlayer;
		foreach (Transform b in Parent)
		{
			b.gameObject.layer = vp_Layer.LocalPlayer;
		}

		// main camera initialization
		// render everything except body and weapon
		camera.cullingMask &= ~((1 << vp_Layer.LocalPlayer) | (1 << vp_Layer.Weapon));
		camera.depth = 0;

		// weapon camera initialization
		// find a regular Unity Camera component existing in a child
		// gameobject to the FPSCamera's gameobject. if we don't find
		// a weapon cam, that's OK (some games don't have weapons)
		Camera weaponCam = null;
		foreach (Transform t in Transform)
		{
			weaponCam = (Camera)t.GetComponent(typeof(Camera));
			if (weaponCam != null)
			{
				weaponCam.transform.localPosition = Vector3.zero;
				weaponCam.transform.localEulerAngles = Vector3.zero;
				weaponCam.clearFlags = CameraClearFlags.Depth;
				weaponCam.cullingMask = (1 << vp_Layer.Weapon);	// only render the weapon
				weaponCam.depth = 1;
				weaponCam.farClipPlane = 100;
				weaponCam.nearClipPlane = 0.01f;
				weaponCam.fov = 60;
				break;
			}
		}

		// create springs for camera motion

		// --- primary position spring ---
		// this is used for all sorts of positional force acting on the camera
		m_PositionSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.Position, false);
		m_PositionSpring.MinVelocity = 0.00001f;
		m_PositionSpring.RestState = PositionOffset;

		// --- secondary position spring ---
		// this is mainly intended for positional force from recoil, stomping and explosions
		m_PositionSpring2 = new vp_Spring(Transform, vp_Spring.UpdateMode.PositionAdditive, false);
		m_PositionSpring2.MinVelocity = 0.00001f;

		// --- rotation spring ---
		// this is used for all sorts of angular force acting on the camera
		m_RotationSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.RotationAdditive, false);
		m_RotationSpring.MinVelocity = 0.00001f;


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

		Refresh();

		// snap the camera to its start values when first activated
		SnapSprings();
		SnapZoom();

	}

	
	/// <summary>
	/// in 'Init' we do things that must be run once at the
	/// beginning, but only after all other components have
	/// run their 'Start' calls. this method is called once
	/// by the vp_Component base class in its first 'Update'
	/// </summary>
	protected override void Init()
	{

		base.Init();
		
	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
	{

		base.Update();

		if (Time.timeScale == 0.0f)
		    return;

		UpdateMouseLook();

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void FixedUpdate()
	{

		base.FixedUpdate();

		if (Time.timeScale == 0.0f)
			return;

		UpdateZoom();

		UpdateSwaying();

		UpdateBob();

		UpdateEarthQuake();

		UpdateShakes();

		UpdateSprings();

	}


	/// <summary>
	/// actual rotation of the player model and camera is performed in
	/// LateUpdate, since by then all game logic should be finished
	/// </summary>
	protected override void LateUpdate()
	{

		base.LateUpdate();

		if (Time.timeScale == 0.0f)
			return;

		// fetch the FPSController's SmoothPosition. this reduces jitter
		// by moving the camera at arbitrary update intervals while
		// controller and springs move at the fixed update interval
		m_Transform.position = FPController.SmoothPosition;

		// apply current spring offsets
		m_Transform.localPosition += (m_PositionSpring.State + m_PositionSpring2.State);

		// prevent camera from intersecting objects
		DoCameraCollision();

		// rotate the parent gameobject (i.e. player model)
		// NOTE: this rotation does not pitch the player model, it only applies yaw
		Quaternion xQuaternion = Quaternion.AngleAxis(m_Yaw + m_InitialRotation.x, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis(0, Vector3.left);
		Parent.rotation =
			vp_Utility.NaNSafeQuaternion((xQuaternion * yQuaternion), Parent.rotation);

		// pitch and yaw the camera
		yQuaternion = Quaternion.AngleAxis((-m_Pitch) - m_InitialRotation.y, Vector3.left);
		Transform.rotation =
			vp_Utility.NaNSafeQuaternion((xQuaternion * yQuaternion), Transform.rotation);

		// roll the camera
		Transform.localEulerAngles +=
			vp_Utility.NaNSafeVector3(Vector3.forward * m_RotationSpring.State.z);
		
	}


	/// <summary>
	/// prevents the camera from intersecting other objects by
	/// raycasting from the controller to the camera and blocking
	/// the camera on the first object hit
	/// </summary>
	protected virtual void DoCameraCollision()
	{

		// start position is the center of the character controller
		// and height of the camera PositionOffset. this will detect
		// objects between the camera and controller even if the
		// camera PositionOffset is far from the controller
		m_CameraCollisionStartPos = FPController.Transform.TransformPoint(0, PositionOffset.y, 0);

		// end position is the current camera position plus we'll move it
		// back the distance of our Controller.radius in order to reduce
		// camera clipping issues very close to walls
		// TIP: for solving such issues, you can also try reducing the
		// main camera's near clipping plane 
		m_CameraCollisionEndPos = Transform.position + (Transform.position - m_CameraCollisionStartPos).normalized * FPController.CharacterController.radius;

		if (Physics.Linecast(m_CameraCollisionStartPos, m_CameraCollisionEndPos, out m_CameraHit, vp_Layer.Mask.ExternalBlockers))
		{
			if (!m_CameraHit.collider.isTrigger)
				Transform.position = m_CameraHit.point - (m_CameraHit.point - m_CameraCollisionStartPos).normalized * FPController.CharacterController.radius;
		}

#if UNITY_EDITOR
		// draw a camera intersection debug line in the scene view. this is
		// enabled by the vp_FPCameraEditor class when the camera position
		// spring foldout is open
		if (m_DrawCameraCollisionDebugLine)
			Debug.DrawLine(m_CameraCollisionStartPos, m_CameraCollisionEndPos, (m_CameraHit.collider == null) ? Color.yellow : Color.red);
#endif

		// also, prevent the camera from ever going below the player's
		// feet (not even when up in the air)
		if (Transform.localPosition.y < PositionGroundLimit)
			Transform.localPosition = new Vector3(Transform.localPosition.x,
											PositionGroundLimit, Transform.localPosition.z);

	}


	/// <summary>
	/// pushes the camera position spring along the 'force' vector
	/// for one frame. For external use.
	/// </summary>
	public virtual void AddForce(Vector3 force)
	{
		m_PositionSpring.AddForce(force);
	}


	/// <summary>
	/// pushes the camera position spring along the 'force' vector
	/// for one frame. For external use.
	/// </summary>
	public virtual void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}


	/// <summary>
	/// pushes the 2nd camera position spring along the 'force'
	/// vector for one frame. for external use.
	/// </summary>
	public virtual void AddForce2(Vector3 force)
	{
		m_PositionSpring2.AddForce(force);
	}

	/// <summary>
	/// pushes the 2nd camera position spring along the 'force'
	/// vector for one frame. for external use.
	/// </summary>
	public void AddForce2(float x, float y, float z)
	{
		AddForce2(new Vector3(x, y, z));
	}


	/// <summary>
	/// twists the camera around its z vector for one frame
	/// </summary>
	public virtual void AddRollForce(float force)
	{
		m_RotationSpring.AddForce(Vector3.forward * force);
	}


	/// <summary>
	/// rotates the camera for one frame
	/// </summary>
	public virtual void AddRotationForce(Vector3 force)
	{
		m_RotationSpring.AddForce(force);
	}


	/// <summary>
	/// rotates the camera for one frame
	/// </summary>
	public void AddRotationForce(float x, float y, float z)
	{
		AddRotationForce(new Vector3(x, y, z));
	}


	/// <summary>
	/// mouse look implementation with smooth filtering
	/// </summary>
	protected virtual void UpdateMouseLook()
	{

		// --- fetch mouse input ---

		m_MouseMove.x = Input.GetAxisRaw("Mouse X") * Time.timeScale;
		m_MouseMove.y = Input.GetAxisRaw("Mouse Y") * Time.timeScale;

		// --- mouse smoothing ---
		
		// make sure the defined smoothing vars are within range
		MouseSmoothSteps = Mathf.Clamp(MouseSmoothSteps, 1, 20);
		MouseSmoothWeight = Mathf.Clamp01(MouseSmoothWeight);

		// keep mousebuffer at a maximum of (MouseSmoothSteps + 1) values
		while (m_MouseSmoothBuffer.Count > MouseSmoothSteps)
			m_MouseSmoothBuffer.RemoveAt(0);

		// add current input to mouse input buffer
		m_MouseSmoothBuffer.Add(m_MouseMove);

		// calculate mouse smoothing
		float weight = 1;
		Vector2 average = Vector2.zero;
		float averageTotal = 0.0f;
		for (int i = m_MouseSmoothBuffer.Count - 1; i > 0; i--)
		{
			average += m_MouseSmoothBuffer[i] * weight;
			averageTotal += (1.0f * weight);
			weight *= (MouseSmoothWeight / Delta);
		}

		// store the averaged input value
		averageTotal = Mathf.Max(1, averageTotal);
		Vector2 input = vp_Utility.NaNSafeVector2(average / averageTotal);

		// --- mouse acceleration ---

		float mouseAcceleration = 0.0f;

		float accX = Mathf.Abs(input.x);
		float accY = Mathf.Abs(input.y);

		if (MouseAcceleration)
		{
			mouseAcceleration = Mathf.Sqrt((accX * accX) + (accY * accY)) / Delta;
			mouseAcceleration = (mouseAcceleration <= MouseAccelerationThreshold) ? 0.0f : mouseAcceleration;
		}
		
		// --- update camera ---

		// modify pitch and yaw with input, sensitivity and acceleration
		m_Yaw += input.x * (MouseSensitivity.x + mouseAcceleration);
		m_Pitch -= input.y * (MouseSensitivity.y + mouseAcceleration);

		// clamp angles
		m_Yaw = m_Yaw < -360.0f ? m_Yaw += 360.0f : m_Yaw;
		m_Yaw = m_Yaw > 360.0f ? m_Yaw -= 360.0f : m_Yaw;
		m_Yaw = Mathf.Clamp(m_Yaw, RotationYawLimit.x, RotationYawLimit.y);
		m_Pitch = m_Pitch < -360.0f ? m_Pitch += 360.0f : m_Pitch;
		m_Pitch = m_Pitch > 360.0f ? m_Pitch -= 360.0f : m_Pitch;
		m_Pitch = Mathf.Clamp(m_Pitch, -RotationPitchLimit.x, -RotationPitchLimit.y);

	}

	
	/// <summary>
	/// interpolates to the target FOV value
	/// </summary>
	protected virtual void UpdateZoom()
	{

		if (m_FinalZoomTime <= Time.time)
			return;

		RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
		float zoom = 1.0f - ((m_FinalZoomTime - Time.time) / RenderingZoomDamping);
		gameObject.camera.fov = Mathf.SmoothStep(gameObject.camera.fov, RenderingFieldOfView, zoom);

	}


	/// <summary>
	/// interpolates to the target FOV using 'RenderingZoomDamping'
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

		gameObject.camera.fov = RenderingFieldOfView;

	}

	
	/// <summary>
	/// updates the procedural shaking of the camera.
	/// NOTE: x and y shakes are applied to the actual controls.
	/// if you increase the shakes, the result will be a drunken
	/// / sick / drugged movement experience. this can also be used
	/// for things like sniper breathing since it affects aiming
	/// </summary>
	protected virtual void UpdateShakes()
	{

		// apply camera shakes
		if (ShakeSpeed != 0.0f)
		{
			m_Yaw -= m_Shake.y;			// subtract shake from last frame or camera will drift
			m_Pitch -= m_Shake.x;
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
			m_Yaw += m_Shake.y;			// apply new shake
			m_Pitch += m_Shake.x;
			m_RotationSpring.AddForce(Vector3.forward * m_Shake.z * Time.timeScale);
		}
	
	}


	/// <summary>
	/// speed should be the magnitude speed of the character
	/// controller. if controller has no ground contact, '0.0f'
	/// should be passed and the bob will fade to a halt
	/// </summary>
	protected virtual void UpdateBob()
	{

		if (BobAmplitude == Vector4.zero || BobRate == Vector4.zero)
			return;

		m_BobSpeed = ((BobRequireGroundContact && !FPController.Grounded) ? 0.0f : FPController.CharacterController.velocity.sqrMagnitude);

		// scale and limit input velocity
		m_BobSpeed = Mathf.Min(m_BobSpeed * BobInputVelocityScale, BobMaxInputVelocity);

		// reduce number of decimals to avoid floating point imprecision bugs
		m_BobSpeed = Mathf.Round(m_BobSpeed * 1000.0f) / 1000.0f;

		// if speed is zero, this means we should just fade out the last stored
		// speed value. NOTE: it's important to clamp it to the current max input
		// velocity since the preset may have changed since last bob!
		if (m_BobSpeed == 0)
			m_BobSpeed = Mathf.Min((m_LastBobSpeed * 0.93f), BobMaxInputVelocity);

		m_CurrentBobAmp.y = (m_BobSpeed * (BobAmplitude.y * -0.0001f));
		m_CurrentBobVal.y = (Mathf.Cos(Time.time * (BobRate.y * 10.0f))) * m_CurrentBobAmp.y;

		m_CurrentBobAmp.x = (m_BobSpeed * (BobAmplitude.x * 0.0001f));
		m_CurrentBobVal.x = (Mathf.Cos(Time.time * (BobRate.x * 10.0f))) * m_CurrentBobAmp.x;

		m_CurrentBobAmp.z = (m_BobSpeed * (BobAmplitude.z * 0.0001f));
		m_CurrentBobVal.z = (Mathf.Cos(Time.time * (BobRate.z * 10.0f))) * m_CurrentBobAmp.z;

		m_CurrentBobAmp.w = (m_BobSpeed * (BobAmplitude.w * 0.0001f));
		m_CurrentBobVal.w = (Mathf.Cos(Time.time * (BobRate.w * 10.0f))) * m_CurrentBobAmp.w;

		m_PositionSpring.AddForce((Vector3)m_CurrentBobVal * Time.timeScale);

		AddRollForce(m_CurrentBobVal.w * Time.timeScale);

		m_LastBobSpeed = m_BobSpeed;

		DetectBobStep(m_BobSpeed, m_CurrentBobVal.y);

	}


	/// <summary>
	/// the bob step callback is triggered when the vertical
	/// camera bob reaches its bottom value (provided that the
	/// speed is higher than the bob step threshold). this can
	/// be used for various footstep sounds and behaviors.
	/// </summary>
	protected virtual void DetectBobStep(float speed, float upBob)
	{

		if (BobStepCallback == null)
			return;

		if (speed < BobStepThreshold)
			return;

		bool elevating = (m_LastUpBob < upBob) ? true : false;
		m_LastUpBob = upBob;

		if (elevating && !m_BobWasElevating)
			BobStepCallback();

		m_BobWasElevating = elevating;

	}


	/// <summary>
	/// applies swaying forces to the camera in response to user
	/// input and character controller motion.
	/// </summary>
	protected virtual void UpdateSwaying()
	{

		Vector3 localVelocity = Transform.InverseTransformDirection(FPController.CharacterController.velocity * 0.016f) * Time.timeScale;
		AddRollForce(localVelocity.x * RotationStrafeRoll);

	}


	/// <summary>
	/// shakes the camera according to any global earthquake
	/// detectable via the event handler
	/// </summary>
	protected virtual void UpdateEarthQuake()
	{

		if (Player == null)
			return;

		if (!Player.Earthquake.Active)
			return;
				
		// apply horizontal move to the camera spring.
		// NOTE: this will only shake the camera, though it will give
		// the appearance of pushing the player around.

		// the vertical move has a 30% chance of occuring each frame.
		// when it does, it alternates between positive and negative.
		// this produces sharp shakes with nice spring smoothness inbetween.
		if (m_PositionSpring.State.y >= m_PositionSpring.RestState.y)
		{
			Vector3 earthQuakeForce = Player.EarthQuakeForce.Get();
			earthQuakeForce.y = -earthQuakeForce.y;
			Player.EarthQuakeForce.Set(earthQuakeForce);
		}
		m_PositionSpring.AddForce(Player.EarthQuakeForce.Get() * PositionEarthQuakeFactor);

		// apply earthquake roll force on the camera rotation spring
		m_RotationSpring.AddForce(Vector3.forward * (-Player.EarthQuakeForce.Get().x * 2) * RotationEarthQuakeFactor);

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void UpdateSprings()
	{
	
		m_PositionSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring.FixedUpdate();
	
	}

	
	/// <summary>
	/// shakes the camera according to the defined bomb forces
	/// </summary>
	public virtual void DoBomb(Vector3 positionForce, float minRollForce, float maxRollForce)
	{

		AddForce2(positionForce);

		float roll = Random.Range(minRollForce, maxRollForce);
		if (Random.value > 0.5f)
			roll = -roll;
		AddRollForce(roll);


	}


	/// <summary>
	/// this method is called to reset various camera settings,
	/// typically after creating or loading a camera
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

			m_PositionSpring.MinState.y = PositionGroundLimit;
			m_PositionSpring.RestState = PositionOffset;

		}

		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.Stiffness =
			new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
			m_PositionSpring2.Damping = Vector3.one -
				new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);

			m_PositionSpring2.MinState.y = (-PositionOffset.y) + PositionGroundLimit;
			// we don't force a position offset for position spring 2
		}

		if (m_RotationSpring != null)
		{
			m_RotationSpring.Stiffness =
			new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
			m_RotationSpring.Damping = Vector3.one -
				new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);
		}

		Zoom();

	}


	/// <summary>
	/// resets all the springs to their default positions, i.e.
	/// for when loading a new camera or switching a weapon
	/// </summary>
	public virtual void SnapSprings()
	{

		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset;
			m_PositionSpring.State = PositionOffset;
			m_PositionSpring.Stop(true);
		}

		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop(true);
		}

		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = Vector3.zero;
			m_RotationSpring.State = Vector3.zero;
			m_RotationSpring.Stop(true);
		}

	}



	/// <summary>
	/// stops all the springs
	/// </summary>
	public virtual void StopSprings()
	{

		if (m_PositionSpring != null)
			m_PositionSpring.Stop(true);

		if (m_PositionSpring2 != null)
			m_PositionSpring2.Stop(true);

		if (m_RotationSpring != null)
			m_RotationSpring.Stop(true);

		m_BobSpeed = 0.0f;
		m_LastBobSpeed = 0.0f;

	}


	/// <summary>
	/// stops the springs and zoom
	/// </summary>
	public virtual void Stop()
	{
		SnapSprings();
		SnapZoom();
		Refresh();
	}


	/// <summary>
	/// sets camera rotation and snaps springs and zoom to a halt
	/// </summary>
	public virtual void SetRotation(Vector2 eulerAngles, bool stop = true, bool resetInitialRotation = true)
	{

		Angle = eulerAngles;

		if(stop)
			Stop();

		// initial rotation is used to retain the rotation given to a player
		// when placed in the editor. upon teleport it can be disregarded
		if (resetInitialRotation)
			m_InitialRotation = Vector2.zero;

	}


	/// <summary>
	/// applies various forces to the camera and weapon springs
	/// in response to falling impact.
	/// </summary>
	protected virtual void OnMessage_FallImpact(float impact)
	{

		impact = (float)Mathf.Abs((float)impact * 55.0f);
		// ('55' is for preset backwards compatibility)

		float posImpact = (float)impact * PositionKneeling;
		float rotImpact = (float)impact * RotationKneeling;

		// smooth step the impacts to make the springs react more subtly
		// from short falls, and more aggressively from longer falls
		posImpact = Mathf.SmoothStep(0, 1, posImpact);
		rotImpact = Mathf.SmoothStep(0, 1, rotImpact);
		rotImpact = Mathf.SmoothStep(0, 1, rotImpact);

		// apply impact to camera position spring
		if (m_PositionSpring != null)
			m_PositionSpring.AddSoftForce(Vector3.down * posImpact, PositionKneelingSoftness);

		// apply impact to camera rotation spring
		if (m_RotationSpring != null)
		{
			float roll = Random.value > 0.5f ? (rotImpact * 2) : -(rotImpact * 2);
			m_RotationSpring.AddSoftForce(Vector3.forward * roll, RotationKneelingSoftness);
		}

	}


	/// <summary>
	/// applies a force to the camera rotation spring (intended
	/// for when the controller bumps into objects above it)
	/// </summary>
	protected virtual void OnMessage_HeadImpact(float impact)
	{

		if ((m_RotationSpring != null) && (Mathf.Abs(m_RotationSpring.State.z) < 30.0f))
		{

			// apply impact to camera rotation spring
			m_RotationSpring.AddForce(Vector3.forward * (impact * 20.0f) * Time.timeScale);

		}

	}


	/// <summary>
	/// makes the ground shake as if a large dinosaur or mech is
	/// approaching. great for bosses!
	/// </summary>
	protected virtual void OnMessage_GroundStomp(float impact)
	{

		AddForce2(new Vector3(0.0f, -1.0f, 0.0f) * impact);

	}


	/// <summary>
	/// makes the ground shake as if a bomb has gone off nearby
	/// </summary>
	protected virtual void OnMessage_BombShake(float impact)
	{

		DoBomb((new Vector3(1.0f, -10.0f, 1.0f) * impact),
			1,
			2);

	}

	
	/// <summary>
	/// this callback is triggered right after the activity in question
	/// has been approved for activation. it prevents the player from
	/// running while zooming
	/// </summary>
	protected virtual void OnStart_Zoom()
	{

		if (Player == null)
			return;

		Player.Run.Stop();

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Run' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Run()
	{

		if (Player == null)
			return true;

		// can't start running while zooming
		if (Player.Zoom.Active)
			return false;

		return true;

	}


	/// <summary>
	/// gets or sets the rotation of the camera
	/// </summary>
	protected virtual Vector2 OnValue_Rotation
	{
		get
		{
			return Angle;
		}
		set
		{
			Angle = value;
		}
	}


	/// <summary>
	/// snaps the camera springs and zoom to a halt
	/// </summary>
	protected virtual void OnMessage_Stop()
	{
		Stop();
	}


	/// <summary>
	/// gets the forward vector of the camera
	/// </summary>
	protected virtual Vector3 OnValue_Forward
	{
		get
		{
			return Forward;
		}
	}




}

	