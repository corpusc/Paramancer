/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPInteractManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This allows interaction with vp_Interactable components in
//					the world via input. Check for the method InputInteract()
//					in the vp_FPInput class.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class vp_FPInteractManager : MonoBehaviour
{
	
	public float InteractDistance = 2; // sets the distance for interaction from the player
	public float MaxInteractDistance = 25; 	// sets the max distance any interaction can occur. If any interactables interactDistance
											// is higher than this, TryInteract() will not fire.
	public float CrosshairTimeoutTimer{ get; set; } // allows the current interactable to set a timeout before the crosshair can change again

	protected vp_FPPlayerEventHandler m_Player = null; // for caching our player event handler to send in the 'TryInteract' method
	protected vp_FPCamera m_Camera = null; // for caching vp_FPCamera ( the camera )
	protected vp_Interactable m_CurrentInteractable = null; // for caching what the player is currently interacting with
	protected Texture m_OriginalCrosshair = null; // for caching the original crosshair if any
	protected vp_Interactable m_LastInteractable = null; // for caching the last interactable
	protected Dictionary<Collider,vp_Interactable> m_Interactables = new Dictionary<Collider, vp_Interactable>(); // for testing interactable components
	protected vp_Interactable m_CurrentCrosshairInteractable = null; // the currently viewed interactable
	protected vp_Timer.Handle m_ShowTextTimer = new vp_Timer.Handle(); // to delay interactables text from showing
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
		
		m_Player = GetComponent<vp_FPPlayerEventHandler>(); // cache the player event handler
		m_Camera = GetComponentInChildren<vp_FPCamera>(); // cache vp_FPCamera
		
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
	/// if the player dies, stop interacting
	/// </summary>
	public virtual void OnStart_Dead()
	{
		
		ShouldFinishInteraction();
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public virtual void LateUpdate()
	{
		
		if(m_Player.Dead.Active)
			return;
		
		// set the default crosshair
		if(m_OriginalCrosshair == null && m_Player.Crosshair.Get() != null)
			m_OriginalCrosshair = m_Player.Crosshair.Get();
	
		// check for an interactable
		InteractCrosshair();
		
	}
	

	/// <summary>
	/// adds a condition (a rule set) that must be met for the
	/// event handler 'Interact' activity to successfully activate.
	/// NOTE: other scripts may have added conditions to this
	/// activity aswell
	/// </summary>
	protected virtual bool CanStart_Interact()
	{
		
		// if we are already interacting, we need to stop interacting so a new interaction can begin.
		if(ShouldFinishInteraction())
			return false;
		
		// if the weapon is being set, don't allow interaction
		if(m_Player.SetWeapon.Active)
			return false;
		
		vp_Interactable interactable = null;
		if(FindInteractable( out interactable ))
		{
			// if the interactable is of type vp_InteractType.Normal, carry on
			if(interactable.InteractType != vp_Interactable.vp_InteractType.Normal)
				return false;
			
			// check if we can interact with the interactable, if so, carry on
			if(!interactable.TryInteract(m_Player))
				return false;
			
			// reset some things
			ResetCrosshair(false);
			
			// cache this interactable
			m_LastInteractable = interactable;
			
			return true; // allow us interaction
		}

		return false; // if all else fails, don't allow interaction
		
	}
	
	
	/// <summary>
	/// finishes interaction with the current interactable
	/// </summary>
	protected virtual bool ShouldFinishInteraction()
	{
		
		if(m_Player.Interactable.Get() != null)
		{
			m_CurrentCrosshairInteractable = null; // set this to null to allow the crosshair to change again
			ResetCrosshair(); // reset the crosshair to default
			m_Player.Interactable.Get().FinishInteraction(); // end interaction with the active interactable
			m_Player.Interactable.Set(null); // set this to null to allow new interaction
			return true; // don't allow new interaction this time through
		}
			
		return false;
		
	}
	
	
	/// <summary>
	/// handles the displaying of the interactables
	/// crosshair if any
	/// </summary>
	protected virtual void InteractCrosshair()
	{

		// return if no player crosshair
		if(m_Player.Crosshair.Get() == null)
			return;
		
		// return if interacting
		if(m_Player.Interactable.Get() != null)
			return;
		
		// if interactable is found
		vp_Interactable interactable = null;
		if(FindInteractable( out interactable ))
		{	
			// if this is our first instance of looking at this interactable
			if(interactable != m_CurrentCrosshairInteractable)
			{
				// return if same interactable as last frame
				if(CrosshairTimeoutTimer > Time.time &&
					((m_LastInteractable != null) && (interactable.GetType() == m_LastInteractable.GetType())))
					return;
				
				// cache this interactable for crosshair checking
				m_CurrentCrosshairInteractable = interactable;
				
				// show the interactables hudtext if any
				if(interactable.InteractText != "" && !m_ShowTextTimer.Active)
				{
					vp_Timer.In(interactable.DelayShowingText, delegate() {
						m_Player.HUDText.Send(interactable.InteractText);
					}, m_ShowTextTimer);
				}
				
				// return if interactable doesn't have a crosshair
				if(interactable.m_InteractCrosshair == null)
					return;
				
				// set the crosshair to the interactables
				m_Player.Crosshair.Set( interactable.m_InteractCrosshair );
			}
		}
		else
			ResetCrosshair();
		
	}
	
	
	/// <summary>
	/// does a raycast to see if an interactable is in range
	/// and returns that interactable in the out as well as returns
	/// true if an interactable was found
	/// </summary>
	protected virtual bool FindInteractable( out vp_Interactable interactable )
	{
		
		interactable = null;
		
		RaycastHit hit;
		if(Physics.Raycast(m_Camera.Transform.position, m_Camera.Transform.forward, out hit, MaxInteractDistance, vp_Layer.Mask.BulletBlockers))
		{
			// test to see if we hit a collider and if that collider contains a vp_Interactable instance
			if(!m_Interactables.TryGetValue(hit.collider, out interactable))
				m_Interactables.Add(hit.collider, interactable = hit.collider.GetComponent<vp_Interactable>());
			
			// return if no interactable
			if(interactable == null)
				return false;
			
			// checks our distance, either from this instance's interactDistance, or if it's overridden on the interactable itself. If the hit is within range, carry on
			if(interactable.InteractDistance == 0 && hit.distance >= InteractDistance)
				return false;
			
			// make sure the interact distance isn't higher than the interactables
			if(interactable.InteractDistance > 0 && hit.distance >= interactable.InteractDistance)
				return false;
		}
		else
			return false;
		
		return true;
		
	}
	
	
	/// <summary>
	/// Resets the crosshair to it's default state.
	/// </summary>
	protected virtual void ResetCrosshair( bool reset = true )
	{
		
		if(m_OriginalCrosshair == null || m_Player.Crosshair.Get() == m_OriginalCrosshair)
			return;
		
		m_ShowTextTimer.Cancel();
		if(reset)
			m_Player.Crosshair.Set( m_OriginalCrosshair );
		m_CurrentCrosshairInteractable = null;
		
	}
	
	
	/// <summary>
	/// this allows the player to interact with collidable objects
	/// </summary>
	protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{

		Rigidbody body = hit.collider.attachedRigidbody;

		if (body == null || body.isKinematic)
			return;
		
		vp_Interactable interactable = null;
		if(!m_Interactables.TryGetValue(hit.collider, out interactable))
			m_Interactables.Add(hit.collider, interactable = hit.collider.GetComponent<vp_Interactable>());
		
		if(interactable == null)
			return;
		
		if(interactable.InteractType != vp_Interactable.vp_InteractType.CollisionTrigger)
			return;
			
		hit.gameObject.SendMessage("TryInteract", m_Player, SendMessageOptions.DontRequireReceiver);

	}
	
	
	/// <summary>
	/// sets and returns the current object being interacted with
	/// </summary>
	protected virtual vp_Interactable OnValue_Interactable
	{
		get { return m_CurrentInteractable; }
		set { m_CurrentInteractable = value; }
	}
	
	
}