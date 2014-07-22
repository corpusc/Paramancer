/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Interactable.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a generic interact base class which can be inherited to create
//					various ways of interacting with objects. An interactable can
//					be of two types, vp_InteractType.Normal or vp_InteractType.trigger.
//					Normal interactables require input for interaction whereas trigger 
//					interactables are trigger by the character controller on the player.
//					Typically Normal interactables require the vp_FPInteract manager
//					to fire while the trigger interactables do not.
//					
//					NOTES:
//					This script can not be added to a gameobject directly.
//					instead, you must create a class derived from this one, with
//					an overridden 'TryInteract' method in it, and add that script instead
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public abstract class vp_Interactable : MonoBehaviour
{
	
	/// <summary>
	/// Different types of interaction that can be set per Interactable
	/// </summary>
	public enum vp_InteractType
	{
		Normal, // normal interaction that requires input to trigger the interaction.
		Trigger, // trigger interaction allows for action when player comes in contact with this object. IsTrigger gets set to on.
		CollisionTrigger // Allows interaction when player comes into contact with collider.
	}
	
	public vp_InteractType InteractType; // Allows a type selection, vp_InteractType.Normal or vp_InteractType.Trigger. Feel free to add more types.
	public List<string> RecipientTags = new List<string>(); // An array of tags to test against for vp_InteractType.Trigger trigger events
	public float InteractDistance = 0; // This is used to override the vp_FPInteractManager interactDistance and only applies to vp_InteractType.Normal
	public Texture m_InteractCrosshair = null; // if supplied, will change the crosshair when in range
	//public bool AllowCollisionInteraction = false; // should interaction with colliders be possible
	public string InteractText = ""; // text to show when an interactable is detected
	public float DelayShowingText = 2; // the delay before the text will be shown
	
	protected Transform m_Transform = null; // Caches this GameObject's transform
	protected vp_FPController m_Controller = null; // Can be used to cache the players vp_FPController
	protected vp_FPCamera m_Camera = null; // Can be used to cache the players vp_FPCamera
	protected vp_FPWeaponHandler m_WeaponHandler = null; // Can be used to cache the players vp_FPWeaponHandler for acccess to weapon GameObjects
	protected vp_FPPlayerEventHandler m_Player = null; // Need this to access the above and the player. This should be sent in the TryInteract method
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{
		
		// cache the transform
		m_Transform = transform;
		
		// Add player tag if not tags are supplied
		if (RecipientTags.Count == 0)
			RecipientTags.Add("Player");
		
		// need to switch the collider trigger on if vp_InteractType is a Trigger
		if(InteractType == vp_InteractType.Trigger && collider != null)
			collider.isTrigger = true;
		
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
	/// This should be overriden and starts the interaction
	/// </param>
	public virtual bool TryInteract(vp_FPPlayerEventHandler player)
	{
		
		return false; // if not overridden, just prevent interaction
		
	}
	
	
	/// <summary>
	/// this is triggered when an object enters the collider and
	/// InteractType is set to trigger
	/// </summary>
	protected virtual void OnTriggerEnter(Collider col)
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

		m_Player = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();

		if (m_Player == null)
			return;
		
		// calls the TryInteract method which is hopefully on the inherited class
		TryInteract(m_Player);
		
	}
	
	
	/// <summary>
	/// By overriding this method, you can choose how the interaction should end.
	/// Mainly for vp_InteractType.Normal
	/// </summary>
	public virtual void FinishInteraction(){}
	

}