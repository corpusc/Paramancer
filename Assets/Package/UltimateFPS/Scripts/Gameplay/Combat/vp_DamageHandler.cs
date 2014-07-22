/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DamageHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	class for having a gameobject take damage, die and respawn.
//					any other object can do damage on this monobehaviour like so:
//					    hitObject.SendMessage(Damage, 1.0f, SendMessageOptions.DontRequireReceiver);
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_DamageHandler : MonoBehaviour
{

	// health and death
	public float MaxHealth = 1.0f;			// initial health of the object instance, to be reset on respawn
	public GameObject [] DeathSpawnObjects = null;	// gameobjects to spawn when object dies.
													// TIP: could be fx, could also be rigidbody rubble
	public float MinDeathDelay = 0.0f;		// random timespan in seconds to delay death. good for cool serial explosions
	public float MaxDeathDelay = 0.0f;
	public float m_CurrentHealth = 0.0f;	// current health of the object instance

	// respawn
	public bool Respawns = true;			// whether to respawn object or just delete it
	public float MinRespawnTime = 3.0f;		// random timespan in seconds to delay respawn
	public float MaxRespawnTime = 3.0f;
	public float RespawnCheckRadius = 1.0f;	// area around object which must be clear of other objects before respawn
	protected AudioSource m_Audio = null;
	public AudioClip DeathSound = null;		// sound to play upon death
	public AudioClip RespawnSound = null;	// sound to play upon respawn
	protected Vector3 m_StartPosition;		// initial position detected and used for respawn
	protected Quaternion m_StartRotation;	// initial rotation detected and used for respawn

	// impact damage
	public float ImpactDamageThreshold = 10;
	public float ImpactDamageMultiplier = 0.0f;


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{

		m_Audio = audio;

		m_CurrentHealth = MaxHealth;
		m_StartPosition = transform.position;
		m_StartRotation = transform.rotation;

		if (DeathEffect != null)
			Debug.LogWarning("'vp_DamageHandler.DeathEffect' is obsolete and will be removed soon. Please use the new 'DeathSpawnObjects' array instead.");

	}


	/// <summary>
	/// reduces current health by 'damage' points and kills the
	/// object if health runs out
	/// </summary>
	public virtual void Damage(float damage)
	{

		if (!enabled)
			return;

		if (!vp_Utility.IsActive(gameObject))
			return;

		if (m_CurrentHealth <= 0.0f)
			return;

		m_CurrentHealth = Mathf.Min(m_CurrentHealth - damage, MaxHealth);

		if (m_CurrentHealth <= 0.0f)
		{

			if (m_Audio != null)
			{
				m_Audio.pitch = Time.timeScale;
				m_Audio.PlayOneShot(DeathSound);
			}

			vp_Timer.In(Random.Range(MinDeathDelay, MaxDeathDelay), Die);
			return;
		}

		// TIP: if you want to do things like play a special impact
		// sound upon every hit (but only if the object survives)
		// this is the place

	}


	/// <summary>
	/// removes the object, plays the death effect and schedules
	/// a respawn if enabled, otherwise destroys the object
	/// </summary>
	public virtual void Die()
	{

		if (!enabled || !vp_Utility.IsActive(gameObject))
			return;

			RemoveBulletHoles();

		vp_Utility.Activate(gameObject, false);

		if (DeathEffect != null)
			Object.Instantiate(DeathEffect, transform.position, transform.rotation);

		foreach (GameObject o in DeathSpawnObjects)
		{
			if(o != null)
				Object.Instantiate(o, transform.position, transform.rotation);
		}

		if (Respawns)
			vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
		else
			Object.Destroy(gameObject);
		
	}


	/// <summary>
	/// respawns the object if no other object is occupying the
	/// respawn area. otherwise reschedules respawning
	/// </summary>
	protected virtual void Respawn()
	{

		// return if the object has been destroyed (for example
		// as a result of loading a new level while it was gone)
		if (this == null)
			return;

		// don't respawn if checkradius contains the local player or props
		// TIP: this can be expanded upon to check for alternative object layers
		if (Physics.CheckSphere(m_StartPosition, RespawnCheckRadius, vp_Layer.Mask.PhysicsBlockers))
		{
			// attempt to respawn again until the checkradius is clear
			vp_Timer.In(Random.Range(MinRespawnTime, MaxRespawnTime), Respawn);
			return;
		}

		Reset();

		Reactivate();
		
	}


	/// <summary>
	/// resets health, position, angle and motion
	/// </summary>
	protected virtual void Reset()
	{
		
		m_CurrentHealth = MaxHealth;
		transform.position = m_StartPosition;
		transform.rotation = m_StartRotation;
		if (rigidbody != null && !rigidbody.isKinematic)
		{
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.velocity = Vector3.zero;
		}

	}

	
	/// <summary>
	/// reactivates object and plays spawn sound
	/// </summary>
	protected virtual void Reactivate()
	{
		
		vp_Utility.Activate(gameObject);

		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(RespawnSound);
		}
	
	}

	/// <summary>
	/// removes any bullet decals currently childed to this object
	/// </summary>
	protected virtual void RemoveBulletHoles()
	{

		foreach (Transform t in transform)
		{
			Component[] c;
			c = t.GetComponents<vp_HitscanBullet>();
			if (c.Length != 0)
				Object.Destroy(t.gameObject);
		}

	}


	/// <summary>
	/// 
	/// </summary>
	void OnCollisionEnter(Collision collision)
	{

		float force = collision.impactForceSum.sqrMagnitude * 0.1f;

		float damage = (force > ImpactDamageThreshold) ? (force * ImpactDamageMultiplier) : 0.0f;

		if (damage > 0.0f)
		{
			if (m_CurrentHealth - damage <= 0.0f)
				MaxDeathDelay = MinDeathDelay = 0.0f;
			Damage(damage);
		}

	}

	/// <summary>
	/// [DEPRECATED] Please use the 'DeathSpawnObjects' array instead.
	/// </summary>
	public GameObject DeathEffect = null;

}

