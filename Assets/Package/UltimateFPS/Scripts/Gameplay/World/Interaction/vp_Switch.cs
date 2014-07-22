/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Switch.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class will allow the player to interact with an object
//					in the world by input or by a trigger. The script takes a target
//					object and a message can be sent to that target object.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_Switch : vp_Interactable
{
	
	public GameObject Target = null;
	public string TargetMessage = "";
	public AudioSource AudioSource = null;
	public Vector2 SwitchPitchRange = new Vector2(1.0f, 1.5f);
	public List<AudioClip> SwitchSounds = new List<AudioClip>();		// list of sounds to randomly play when switched
	
	
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
		
		if(Target == null)
			return false;
		
		if(m_Player == null)
			m_Player = player;
		
		PlaySound();
		
		Target.SendMessage(TargetMessage, SendMessageOptions.DontRequireReceiver);
		
		return true;
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void PlaySound()
	{
		
		if(AudioSource == null)
			return;
		
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
		
		// calls the TryInteract method which is hopefully on the inherited class
		TryInteract(m_Player);
		
	}
	
}
