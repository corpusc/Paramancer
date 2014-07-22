/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeaponMeleeAttack.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:		a test class devised to see if melee combat would be
//						feasible using springs and without 'real' animations.
//						NOTE: this is still a bit of a prototype script
//						and not the easiest to use
//						
/////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections.Generic;

public class vp_FPWeaponMeleeAttack : vp_Component
{

#if (UNITY_EDITOR)
	public bool DrawDebugObjects = false;
#endif

	private vp_FPWeapon m_Weapon = null;
	private vp_FPController m_Controller = null;
	private vp_FPCamera m_Camera = null;
	
	public string WeaponStatePull = "Pull";			// weapon state for pulling back the weapon pre-slash
	public string WeaponStateSwing = "Swing";		// weapon state for the slash. NOTE: this is not a slash in itself. it just
													// orientates the weapon so that the soft forces can rotate it properly

	// soft forces are used for the slash itself. these are positional and angular
	// spring force impulses that are added over several frames

	public float SwingDelay = 0.5f;			// delay until slash begins after weapon has been raised
	public float SwingDuration = 0.5f;		// delay until the weapon swing is stopped
	public float SwingRate = 1.0f;
	protected float m_NextAllowedSwingTime = 0.0f;
	public int SwingSoftForceFrames = 50;				// number of frames over which to apply the forces of each attack
	public Vector3 SwingPositionSoftForce = new Vector3(-0.5f, -0.1f, 0.3f);
	public Vector3 SwingRotationSoftForce = new Vector3(50, -25, 0);

	public float ImpactTime = 0.11f;
	public Vector3 ImpactPositionSpringRecoil = new Vector3(0.01f, 0.03f, -0.05f);
	public Vector3 ImpactPositionSpring2Recoil = Vector3.zero;
	public Vector3 ImpactRotationSpringRecoil = Vector3.zero;
	public Vector3 ImpactRotationSpring2Recoil = new Vector3(0.0f, 0.0f, 10.0f);

	public string DamageMethodName = "Damage";	// user defined name of damage method on target
	public float Damage = 5.0f;				// the damage transmitted to target by the attack
	public float DamageRadius = 0.3f;
	public float DamageRange = 2.0f;
	public float DamageForce = 1000.0f;			// force applied to any rigidbody hit by the attack

	public bool AttackPickRandomState = true;
	protected int m_AttackCurrent = 0;	// current randomly selected attack

	public float SparkFactor = 0.1f;		// chance of attack impact generating a spark
	public GameObject m_DustPrefab = null;		// evaporating dust / moisture from the hit material
	public GameObject m_SparkPrefab = null;		// a quick spark, as if hitting stone or metal
	public GameObject m_DebrisPrefab = null;	// pieces of material thrust out of the hit point and / or falling to the ground

	public List<UnityEngine.Object> SoundSwing = new List<UnityEngine.Object>();	// list of impact sounds to be randomly played
	public List<UnityEngine.Object> SoundImpact = new List<UnityEngine.Object>();	// list of impact sounds to be randomly played
	public Vector2 SoundSwingPitch = new Vector2(0.5f, 1.5f);	// random pitch range for swing sounds
	public Vector2 SoundImpactPitch = new Vector2(1.0f, 1.5f);	// random pitch range for impact sounds

	vp_Timer.Handle SwingDelayTimer = new vp_Timer.Handle();
	vp_Timer.Handle ImpactTimer = new vp_Timer.Handle();
	vp_Timer.Handle SwingDurationTimer = new vp_Timer.Handle();
	vp_Timer.Handle ResetTimer = new vp_Timer.Handle();

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
	protected override void Start()
	{

		base.Start();

		m_Controller = (vp_FPController)Root.GetComponent(typeof(vp_FPController));
		m_Camera = (vp_FPCamera)Root.GetComponentInChildren(typeof(vp_FPCamera));
		m_Weapon = (vp_FPWeapon)Transform.GetComponent(typeof(vp_FPWeapon));

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
	{

		base.Update();

		UpdateAttack();

	}


	/// <summary>
	/// simulates a melee swing by carrying out a sequence of
	/// timers, state changes and soft forces on the weapon springs
	/// </summary>
	private void UpdateAttack()
	{

		if (!Player.Attack.Active)
			return;

		if (Player.SetWeapon.Active)
			return;

		if (m_Weapon == null)
			return;

		if (!m_Weapon.Wielded)
			return;

		if (Time.time < m_NextAllowedSwingTime)
			return;

		m_NextAllowedSwingTime = Time.time + SwingRate;

		// enable random attack states on the melee and weapon components
		if (AttackPickRandomState)
			PickAttack();

		// set 'raise' state (of the chosen attack state) on the weapon component
		m_Weapon.SetState(WeaponStatePull);
		m_Weapon.Refresh();

		// after a short delay, swing the weapon
		vp_Timer.In(SwingDelay, delegate()
		{

			// play a random swing sound
			if (SoundSwing.Count > 0)
			{
				Audio.pitch = Random.Range(SoundSwingPitch.x, SoundSwingPitch.y) * Time.timeScale;
				Audio.clip = (AudioClip)SoundSwing[(int)Random.Range(0, (SoundSwing.Count))];
				Audio.Play();
			}

			// switch to the swing state
			m_Weapon.SetState(WeaponStatePull, false);
			m_Weapon.SetState(WeaponStateSwing);
			m_Weapon.Refresh();

			// apply soft forces of the current attack
			m_Weapon.AddSoftForce(SwingPositionSoftForce, SwingRotationSoftForce, SwingSoftForceFrames);

			// check for target impact after a predetermined duration
			vp_Timer.In(ImpactTime, delegate()
			{

				// perform a sphere cast ray from center of controller, at height
				// of camera and along camera angle
				RaycastHit hit;
				Ray ray = new Ray(new Vector3(	m_Controller.Transform.position.x,	m_Camera.Transform.position.y,
												m_Controller.Transform.position.z),	m_Camera.Transform.forward);
				Physics.SphereCast(ray, DamageRadius, out hit, DamageRange, vp_Layer.Mask.BulletBlockers);

				// hit something: perform impact functionality
				if (hit.collider != null)
				{

					SpawnImpactFX(hit);

					ApplyDamage(hit);

					ApplyRecoil();

#if (UNITY_EDITOR)
					// draw debug info if applicable
					if(DrawDebugObjects)
						DebugDrawImpact(hit);
#endif

				}
				else
				{

					// didn't hit anything: carry on swinging until time is up
					vp_Timer.In(SwingDuration - ImpactTime, delegate()
					{
						m_Weapon.StopSprings();
						Reset();
					}, SwingDurationTimer);

				}

			}, ImpactTimer);

		}, SwingDelayTimer);
		
	}


	/// <summary>
	/// picks a random component state for the commencing attack
	/// </summary>
	void PickAttack()
	{

		int attack = States.Count - 1;

		reroll:

			attack = UnityEngine.Random.Range(0, States.Count - 1);
			if ((States.Count > 1) && (attack == m_AttackCurrent) && (Random.value < 0.5f))
				goto reroll;

		//else
		//    attack = Mathf.Clamp(((States.Count-1) - (int)Player.Attack.Argument), 0, States.Count-1);

		m_AttackCurrent = attack;

		// set chosen attack state on the melee component
		SetState(States[m_AttackCurrent].Name);

	}


	/// <summary>
	/// 
	/// </summary>
	void Attack()
	{

	
	}


	/// <summary>
	/// plays a random impact sound and instantiates the three
	/// pre-declared FX prefabs of this melee attack. they are
	/// named dust, spark and debris but could be anything
	/// </summary>
	void SpawnImpactFX(RaycastHit hit)
	{

		Quaternion hitNormal = Quaternion.LookRotation(hit.normal);

		// spawn dust effect
		if (m_DustPrefab != null)
			Object.Instantiate(m_DustPrefab, hit.point, hitNormal);

		// spawn spark effect
		if (m_SparkPrefab != null)
		{
			if (Random.value < SparkFactor)
				Object.Instantiate(m_SparkPrefab, hit.point, hitNormal);
		}

		// spawn debris particle fx
		if (m_DebrisPrefab != null)
			Object.Instantiate(m_DebrisPrefab, hit.point, hitNormal);

		// play impact sound
		if (SoundImpact.Count > 0)
		{
			Audio.pitch = Random.Range(SoundImpactPitch.x, SoundImpactPitch.y) * Time.timeScale;
			Audio.PlayOneShot((AudioClip)SoundImpact[(int)Random.Range(0, (SoundImpact.Count))]);
		}

	}


	/// <summary>
	/// sends damage and applies a physics force to the target
	/// </summary>
	void ApplyDamage(RaycastHit hit)
	{

		// do damage on the target
		hit.collider.SendMessage(DamageMethodName, Damage, SendMessageOptions.DontRequireReceiver);

		// if hit object has physics, add the damage force to it
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body != null && !body.isKinematic)
			body.AddForceAtPosition((m_Camera.Transform.forward * DamageForce) / Time.timeScale, hit.point);

	}


	/// <summary>
	/// adds recoil to the weapon springs in response to hitting
	/// something
	/// </summary>
	void ApplyRecoil()
	{

		m_Weapon.StopSprings();
		m_Weapon.AddForce(ImpactPositionSpringRecoil, ImpactRotationSpringRecoil);
		m_Weapon.AddForce2(ImpactPositionSpring2Recoil, ImpactRotationSpring2Recoil);
		Reset();

	}


	/// <summary>
	/// resets the weapon state to normal
	/// </summary>
	void Reset()
	{

		vp_Timer.In(0.05f, delegate()
		{
			if (m_Weapon != null)
			{
				m_Weapon.SetState(WeaponStatePull, false);
				m_Weapon.SetState(WeaponStateSwing, false);
				m_Weapon.Refresh();
				if(AttackPickRandomState)
					ResetState();
			}
		}, ResetTimer);

	}


	/// <summary>
	/// if in editor mode, draws debug graphics in the scene view
	/// should either of the Swing, Impact or Damage foldouts be
	/// open. the debug objects indicate the path of the sphere
	/// cast used by the attack's collision test, and its point
	/// of impact. NOTE: in reality, the collision test is not
	/// conducted by 5 spheres, but rather a single sphere swept
	/// along a path from the camera to the impact point
	/// </summary>
#if (UNITY_EDITOR)
	void DebugDrawImpact(RaycastHit hit)
	{

		Material material1 = new Material(Shader.Find("Transparent/Diffuse"));
		material1.color = new Color(1, 1, 0, 0.2f);
		Material material2 = new Material(Shader.Find("Transparent/Diffuse"));
		material2.color = new Color(1, 0, 0, 0.5f);

		GameObject[] pathBall = new GameObject[5];
		for (int v = 0; v < 5; v++)
		{
			pathBall[v] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			pathBall[v].collider.enabled = false;
			pathBall[v].transform.position = new Vector3(m_Controller.transform.position.x,
						m_Camera.transform.position.y,
						m_Controller.transform.position.z) + m_Camera.transform.TransformDirection(Vector3.forward * ((hit.distance / 5.0f) * (v+1)));
			pathBall[v].transform.localScale = Vector3.one * DamageRadius * 2;
			pathBall[v].renderer.material = material1;

		}

		GameObject impactBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		impactBall.collider.enabled = false;
		impactBall.transform.position = hit.point;
		impactBall.transform.localScale = Vector3.one * 0.125f;
		impactBall.renderer.material = material2;

	}
#endif

}
