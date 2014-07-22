/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPWeaponReloader.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this component adds firearm reload logic, sound, animation and
//					reload duration to an FPWeapon. it doesn't handle ammo max caps
//					or levels. instead this should be governed by an inventory
//					system via the event handler
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


[RequireComponent(typeof(vp_FPWeapon))]

public class vp_FPWeaponReloader : MonoBehaviour
{

	protected vp_FPWeapon m_Weapon = null;
	protected vp_FPPlayerEventHandler m_Player = null;

	protected AudioSource m_Audio = null;
	public AudioClip SoundReload = null;
	public AnimationClip AnimationReload = null;

	public float ReloadDuration = 1.0f;


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{

		m_Audio = audio;

		// store the first player event handler found in the top of our transform hierarchy
		m_Player = (vp_FPPlayerEventHandler)transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));


	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{
		
		// store a reference to the FPSWeapon
		m_Weapon = transform.GetComponent<vp_FPWeapon>();
		
	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		// allow this monobehaviour to talk to the player event handler
		if (m_Player != null)
			m_Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{

		// unregister this monobehaviour from the player event handler
		if (m_Player != null)
			m_Player.Unregister(this);

	}


	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Reload' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Reload()
	{

		// can't reload if there's no weapon wielded
		if (m_Player.CurrentWeaponWielded.Get() == false)
			return false;

		// try removing a compatible ammo clip from inventory
		if (!m_Player.RemoveClip.Try())
			return false;

		return true;

	}


	/// <summary>
	/// this callback is triggered right after the activity in question
	/// has been approved for activation
	/// </summary>
	protected virtual void OnStart_Reload()
	{

		// end the Reload activity in 'ReloadDuration' seconds
		m_Player.Reload.AutoDuration = m_Player.CurrentWeaponReloadDuration.Get();

		// if reload duration is zero, fetch duration from the animation
		if (m_Player.Reload.AutoDuration == 0.0f && AnimationReload != null)
			m_Player.Reload.AutoDuration = AnimationReload.length;

		if (AnimationReload != null)
			m_Weapon.WeaponModel.animation.CrossFade(AnimationReload.name);

		if (m_Audio != null)
		{
			m_Audio.pitch = Time.timeScale;
			m_Audio.PlayOneShot(SoundReload);
		}

	}


	/// <summary>
	/// this callback is triggered when the activity in question
	/// deactivates
	/// </summary>
	protected virtual void OnStop_Reload()
	{

		string weaponName = m_Player.CurrentWeaponName.Get();

		if (!string.IsNullOrEmpty(weaponName))
			m_Player.AddAmmo.Try(new object[] { weaponName });

	}


	/// <summary>
	/// returns the reload duration of the current weapon
	/// </summary>
	protected virtual float OnValue_CurrentWeaponReloadDuration
	{
		get
		{
			return ReloadDuration;
		}
	}





}

