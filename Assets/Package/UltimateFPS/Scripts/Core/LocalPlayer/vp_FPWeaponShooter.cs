/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeaponShooter.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class adds firearm features to a vp_FPWeapon. it has all
//					the capabilities of its inherited class (vp_Shooter), adding
//					recoil, animations and an extended rule set for when it can fire
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


[RequireComponent(typeof(vp_FPWeapon))]

public class vp_FPWeaponShooter : vp_Shooter
{

	protected vp_FPWeapon m_FPSWeapon = null;			// the weapon affected by the shooter
	protected vp_FPCamera m_FPSCamera = null;			// the main first person camera view

	// projectile
	public float ProjectileTapFiringRate = 0.1f;		// minimum delay between shots fired when fire button is tapped quickly and repeatedly
	protected float m_LastFireTime = 0.0f;

	// motion
	public Vector3 MotionPositionRecoil = new Vector3(0, 0, -0.035f);	// positional force applied to weapon upon firing
	public Vector3 MotionRotationRecoil = new Vector3(-10.0f, 0, 0);	// angular force applied to weapon upon firing
	public float MotionRotationRecoilDeadZone = 0.5f;	// 'blind spot' center region for angular z recoil
	public float MotionPositionReset = 0.5f;			// how much to reset weapon to its normal position upon firing (0-1)
	public float MotionRotationReset = 0.5f;
	public float MotionPositionPause = 1.0f;			// time interval over which to freeze and fade swaying forces back in upon firing
	public float MotionRotationPause = 1.0f;
	public float MotionDryFireRecoil = -0.1f;			// multiplies recoil when the weapon is out of ammo
	public float MotionRecoilDelay = 0.0f;				// delay between fire button pressed and recoil

	// animation
	public AnimationClip AnimationFire = null;

	// sound
	public AudioClip SoundDryFire = null;						// out of ammo sound

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

		m_FPSCamera = transform.root.GetComponentInChildren<vp_FPCamera>();

		m_OperatorTransform = m_FPSCamera.transform;

		// reset the next allowed fire time
		m_NextAllowedFireTime = Time.time;

		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, (ProjectileFiringRate - 0.1f));
		
	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{

		base.Start();

		// defaults for using animation length as the fire and reload delay
		if (ProjectileFiringRate == 0.0f && AnimationFire != null)
			ProjectileFiringRate = AnimationFire.length;

		// store a reference to the FPSWeapon
		m_FPSWeapon = transform.GetComponent<vp_FPWeapon>();

		// defaults for using animation length as the fire delay
		if (ProjectileFiringRate == 0.0f && AnimationFire != null)
			ProjectileFiringRate = AnimationFire.length;
		
	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
	{

		base.Update();

		if (Player.Attack.Active)
			TryFire();

	}


	/// <summary>
	/// regulates whether a weapon may currently be discharged.
	/// NOTE: this method doesn't decide whether a player can
	/// start an attack activity (changing stance, raising weapon).
	/// it only determines whether the weapon's firing mechanism
	/// is currently ready to fire
	/// </summary>
	public override void TryFire()
	{

		// return if we can't fire yet
		if (Time.time < m_NextAllowedFireTime)
			return;

		if (Player.SetWeapon.Active)
			return;

		if (!m_FPSWeapon.Wielded)
			return;

		if (!Player.DepleteAmmo.Try())
		{
			DryFire();
			return;
		}

		Fire();

	}


	/// <summary>
	/// in addition to spawning the projectile in the base class,
	/// plays a fire animation on the weapon and applies recoil
	/// to the weapon spring. also regulates tap fire
	/// </summary>
	protected override void Fire()
	{

		m_LastFireTime = Time.time;

		// play fire animation
		if (AnimationFire != null)
			m_FPSWeapon.WeaponModel.animation.CrossFade(AnimationFire.name);

		// apply recoil
		if (MotionRecoilDelay == 0.0f)
			ApplyRecoil();
		else
			vp_Timer.In(MotionRecoilDelay, ApplyRecoil);

		base.Fire();
		
	}


	/// <summary>
	/// applies some advanced recoil motions on the weapon when fired
	/// </summary>
	protected virtual void ApplyRecoil()
	{

		// return the weapon to its forward looking state by certain
		// position, rotation and velocity factors
		m_FPSWeapon.ResetSprings(MotionPositionReset, MotionRotationReset,
							MotionPositionPause, MotionRotationPause);

		// add a positional and angular force to the weapon for one frame
		if (MotionRotationRecoil.z == 0.0f)
			m_FPSWeapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);
		else
		{
			// if we have rotation recoil around the z vector, also do dead zone logic
			m_FPSWeapon.AddForce2(MotionPositionRecoil,
				Vector3.Scale(MotionRotationRecoil, (Vector3.one + Vector3.back)) +	// recoil around x & y
				(((Random.value < 0.5f) ? Vector3.forward : Vector3.back) *	// spin direction (left / right around z)
				Random.Range(MotionRotationRecoil.z * MotionRotationRecoilDeadZone,
												MotionRotationRecoil.z))		// spin force
			);
		}

	}

	
	/// <summary>
	/// applies a scaled version of the recoil to the weapon to
	/// signify pulling the trigger with no discharge. then plays
	/// a dryfire sound. TIP: make 'MotionDryFireRecoil' about
	/// -0.1 for a subtle out-of-ammo effect
	/// </summary>
	public virtual void DryFire()
	{

		m_LastFireTime = Time.time;

		// apply dryfire recoil
		m_FPSWeapon.AddForce2(MotionPositionRecoil * MotionDryFireRecoil, MotionRotationRecoil * MotionDryFireRecoil);

		// play the dry fire sound and prevent further firing
		if (Audio != null)
		{
			Audio.pitch = Time.timeScale;
			Audio.PlayOneShot(SoundDryFire);
			DisableFiring();
		}

	}


	/// <summary>
	/// this callback is triggered when the activity in question
	/// deactivates
	/// </summary>
	protected virtual void OnStop_Attack()
	{

		if (ProjectileFiringRate == 0)
		{
			EnableFiring();
			return;
		}

		DisableFiring(ProjectileTapFiringRate - (Time.time - m_LastFireTime));

	}


}

