/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Shooter.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this component can be added to any gameobject, giving it the capability
//					of firing projectiles. it handles firing rate, projectile spawning,
//					muzzle flashes, shell casings and shooting sound. call the 'TryFire'
//					method to fire the shooter (whether this succeeds is determined by
//					firing rate)
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


[RequireComponent(typeof(AudioSource))]

public class vp_Shooter : vp_Component
{

	protected CharacterController m_CharacterController = null;

	protected Transform m_OperatorTransform = null;		// the object that is operating this shooter. typically a vp_FPCamera or an enemy

	// projectile
	public GameObject ProjectilePrefab = null;			// prefab with a mesh and projectile script
	public float ProjectileScale = 1.0f;				// scale of the projectile decal
	public float ProjectileFiringRate = 0.3f;			// delay between shots fired when fire button is held down
	public float ProjectileSpawnDelay = 0.0f;			// delay between fire button pressed and projectile launched
	public int ProjectileCount = 1;						// amount of projectiles to fire at once
	public float ProjectileSpread = 0.0f;				// accuracy deviation in degrees (0 = spot on)
	protected float m_NextAllowedFireTime = 0.0f;		// the next time firing will be allowed after having recently fired a shot

	// muzzle flash
	public Vector3 MuzzleFlashPosition = Vector3.zero;	// position of the muzzle in relation to the parent
	public Vector3 MuzzleFlashScale = Vector3.one;		// scale of the muzzleflash
	public float MuzzleFlashFadeSpeed = 0.075f;			// the amount of muzzle flash alpha to deduct each frame
	public GameObject MuzzleFlashPrefab = null;			// muzzleflash prefab, typically with a mesh and vp_MuzzleFlash script
	public float MuzzleFlashDelay = 0.0f;				// delay between fire button pressed and muzzleflash appearing
	protected GameObject m_MuzzleFlash = null;			// the instantiated muzzle flash. one per weapon that's always there
	
	// shell casing
	public GameObject ShellPrefab = null;				// shell prefab with a mesh and (typically) a vp_Shell script
	public float ShellScale = 1.0f;						// scale of ejected shell casings
	public Vector3 ShellEjectDirection = new Vector3(1, 1, 1);	// direction of ejected shell casing
	public Vector3 ShellEjectPosition = new Vector3(1, 0, 1);	// position of ejected shell casing in relation to parent
	public float ShellEjectVelocity = 0.2f;				// velocity of ejected shell casing
	public float ShellEjectDelay = 0.0f;				// time to wait before ejecting shell after firing (for shotguns, grenade launchers etc.)
	public float ShellEjectSpin = 0.0f;					// amount of angular rotation of the shell upon spawn

	// sound
	public AudioClip SoundFire = null;							// sound to play upon firing
	public float SoundFireDelay = 0.0f;							// delay between fire button pressed and fire sound played
	public Vector2 SoundFirePitch = new Vector2(1.0f, 1.0f);	// random pitch range for firing sound

	public GameObject MuzzleFlash { get { return m_MuzzleFlash; } }


	/// <summary>
	/// in 'Awake' we do things that need to be run once at the
	/// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	/// can not be altered in 'Awake'
	/// </summary>
	protected override void Awake()
	{
		
		base.Awake();

		m_OperatorTransform = Transform;	// may be changed by derived classes

		m_CharacterController = m_OperatorTransform.root.GetComponentInChildren<CharacterController>();

		// reset the next allowed fire time
		m_NextAllowedFireTime = Time.time;
		ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, (ProjectileFiringRate - 0.1f));

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

		// instantiate muzzleflash
		if (MuzzleFlashPrefab != null)
		{
			m_MuzzleFlash = (GameObject)Object.Instantiate(MuzzleFlashPrefab,
															m_OperatorTransform.position,
															m_OperatorTransform.rotation);
			m_MuzzleFlash.name = transform.name + "MuzzleFlash";
			m_MuzzleFlash.transform.parent = m_OperatorTransform;
		}

		// audio defaults
		Audio.playOnAwake = false;
		Audio.dopplerLevel = 0.0f;

		RefreshDefaultState();

		Refresh();


	}


	/// <summary>
	/// calls the fire method if the firing rate of this shooter
	/// allows it. override this method to add further rules
	/// </summary>
	public virtual void TryFire()
	{

		// return if we can't fire yet
		if (Time.time < m_NextAllowedFireTime)
			return;

		Fire();

	}


	/// <summary>
	/// spawns projectiles, shell cases, and the muzzleflash.
	/// plays a firing sound and updates the firing rate. NOTE:
	/// don't call this every frame, instead call it via 'TryFire'
	/// to subject it to firing rate.
	/// </summary>
	protected virtual void Fire()
	{
				
		// regulate firing and reload rate
		m_NextAllowedFireTime = Time.time + ProjectileFiringRate;

		 //play fire sound
		if (SoundFireDelay == 0.0f)
		    PlayFireSound();
		else
		    vp_Timer.In(SoundFireDelay, PlayFireSound);

		// spawn projectiles
		if (ProjectileSpawnDelay == 0.0f)
			SpawnProjectiles();
		else
			vp_Timer.In(ProjectileSpawnDelay, SpawnProjectiles);

		// spawn shell casing
		if (ShellEjectDelay == 0.0f)
			EjectShell();
		else
			vp_Timer.In(ShellEjectDelay, EjectShell);

		// show muzzle flash
		if (MuzzleFlashDelay == 0.0f)
			ShowMuzzleFlash();
		else
			vp_Timer.In(MuzzleFlashDelay, ShowMuzzleFlash);

	}


	/// <summary>
	/// plays the fire sound
	/// </summary>
	protected virtual void PlayFireSound()
	{

		if (Audio == null)
			return;

		Audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y) * Time.timeScale;
		Audio.clip = SoundFire;
		Audio.Play();
		// LORE: we must use 'Play' rather than 'PlayOneShot' for the
		// AudioSource to be regarded as 'isPlaying' which is needed
		// for 'vp_Component:DeactivateWhenSilent'
	
	}



	/// <summary>
	/// spawns one or more projectiles in a customizable conical
	/// pattern. NOTE: this does not send the projectiles flying.
	/// the spawned gameobjects need to have their own movement
	/// logic
	/// </summary>
	protected virtual void SpawnProjectiles()
	{

		for (int v = 0; v < ProjectileCount; v++)
		{
			if (ProjectilePrefab != null)
			{
				GameObject p = null;
				p = (GameObject)Object.Instantiate(ProjectilePrefab, m_OperatorTransform.position, m_OperatorTransform.rotation);
				p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale

				// apply conical spread as defined in preset
				p.transform.Rotate(0, 0, Random.Range(0, 360));									// first, rotate up to 360 degrees around z for circular spread
				p.transform.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);		// then rotate around y with user defined deviation
			}
		}
	
	}


	/// <summary>
	/// sends a message to the muzzle flash object telling it
	/// to show itself
	/// </summary>
	protected virtual void ShowMuzzleFlash()
	{

		if (m_MuzzleFlash == null)
			return;

		m_MuzzleFlash.SendMessage("Shoot", SendMessageOptions.DontRequireReceiver);

	}





	/// <summary>
	/// spawns the 'ShellPrefab' gameobject and gives it a velocity
	/// </summary>
	protected virtual void EjectShell()
	{

		if (ShellPrefab == null)
			return;

		// spawn the shell
		GameObject s = null;
		s = (GameObject)Object.Instantiate(ShellPrefab,
										m_OperatorTransform.position + m_OperatorTransform.TransformDirection(ShellEjectPosition),
										m_OperatorTransform.rotation);
		s.transform.localScale = new Vector3(ShellScale, ShellScale, ShellScale);
		vp_Layer.Set(s.gameObject, vp_Layer.Debris);

		// send it flying
		if (s.rigidbody)
		{

			Vector3 force = (transform.TransformDirection(ShellEjectDirection) * ShellEjectVelocity);
			s.rigidbody.AddForce(force, ForceMode.Impulse);
			
		}

		// make the shell inherit the current speed of the controller (if any)
		if (m_CharacterController)
		{

			Vector3 velocityForce = (m_CharacterController.velocity);
			s.rigidbody.AddForce(velocityForce, ForceMode.VelocityChange);

		}

		// add random spin if user-defined
		if (ShellEjectSpin > 0.0f)
		{
			if (Random.value > 0.5f)
				s.rigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * ShellEjectSpin);
			else
				s.rigidbody.AddRelativeTorque(Random.rotation.eulerAngles * ShellEjectSpin);
		}

	}


	/// <summary>
	/// this method prevents the shooter from firing for 'seconds',
	/// useful e.g. while switching weapons. if no value is given,
	/// shooting will be disabled practically forever
	/// </summary>
	public virtual void DisableFiring(float seconds = 10000000)
	{

		m_NextAllowedFireTime = Time.time + seconds;

	}


	/// <summary>
	/// makes the shooter immediately ready for firing
	/// </summary>
	public virtual void EnableFiring()
	{

		m_NextAllowedFireTime = Time.time;

	}
	

	/// <summary>
	/// this method is called to reset various shooter settings,
	/// typically after creating or loading a shooter
	/// </summary>
	public override void Refresh()
	{

		// update muzzle flash position, scale and fadespeed from preset
		if (m_MuzzleFlash != null)
		{
			m_MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
			m_MuzzleFlash.transform.localScale = MuzzleFlashScale;
			m_MuzzleFlash.SendMessage("SetFadeSpeed", MuzzleFlashFadeSpeed, SendMessageOptions.DontRequireReceiver);
		}

	}

	
	/// <summary>
	/// performs special logic for activating a shooter properly
	/// </summary>
	public override void Activate()
	{

		base.Activate();

		if (m_MuzzleFlash != null)
			vp_Utility.Activate(m_MuzzleFlash);

	}


	/// <summary>
	/// performs special logic for deactivating a shooter properly
	/// </summary>
	public override void Deactivate()
	{

		base.Deactivate();

		if (m_MuzzleFlash != null)
			vp_Utility.Activate(m_MuzzleFlash, false);

	}
	

}

