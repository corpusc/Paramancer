/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PlatformSwitch.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class allows the player to interact with vp_MovingPlatform.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_PlatformSwitch : vp_Interactable
{
	
	public float SwitchTimeout = 0;										// time in seconds before next switch can occur
	public vp_MovingPlatform Platform = null;							// platform to control
	public AudioSource AudioSource = null;
	public Vector2 SwitchPitchRange = new Vector2(1.0f, 1.5f);
	public List<AudioClip> SwitchSounds = new List<AudioClip>();		// list of sounds to randomly play when switched
	
	protected bool m_IsSwitched = false; // is this object in switched mode
	protected float m_Timeout = 0;
	
	
	protected override void Start()
	{
		
		base.Start();
		
		if(AudioSource == null)
			AudioSource = audio == null ? gameObject.AddComponent<AudioSource>() : audio;
		
	}
	
	
	/// <summary>
	/// try to interact with this object
	/// </summary>
	public override bool TryInteract(vp_FPPlayerEventHandler player)
	{
		
		if(Platform == null)
			return false;
		
		if(m_Player == null)
			m_Player = player;
		
		if(Time.time < m_Timeout)
			return false;
		
		PlaySound();
		
		Platform.SendMessage("GoTo", Platform.TargetedWaypoint == 0 ? 1 : 0, SendMessageOptions.DontRequireReceiver);
		
		m_Timeout = Time.time + SwitchTimeout;
		
		m_IsSwitched = !m_IsSwitched;
		
		return true;
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void PlaySound()
	{
		
		if(AudioSource == null)
		{
			Debug.LogWarning("Audio Source is not set");
			return;
		}
		
		if( SwitchSounds.Count == 0 )
			return;
		
		AudioClip soundToPlay = SwitchSounds[ Random.Range( 0, SwitchSounds.Count ) ];
		
		if(soundToPlay == null)
			return;
		
		AudioSource.pitch = Random.Range(SwitchPitchRange.x, SwitchPitchRange.y);
		AudioSource.PlayOneShot( soundToPlay );
		
	}
	
	
	/// <summary>
	/// this is triggered when an object enters the collider and
	/// InteractType is set to trigger
	/// </summary>
	protected override void OnTriggerEnter(Collider col)
	{
		
		// only do something if the trigger is of type Trigger
		if (InteractType != vp_InteractType.Trigger)
			return;

		// see if the colliding object was a valid recipient
		foreach(string s in RecipientTags)
		{
			if(col.gameObject.tag == s)
				goto isRecipient;
		}
		return;
		isRecipient:

		if (m_Player == null)
			m_Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		
		if(m_Player.Interactable.Get() != null && m_Player.Interactable.Get().collider == col)
			return;
		
		// calls the TryInteract method which is hopefully on the inherited class
		TryInteract(m_Player);
		
	}
	
}
