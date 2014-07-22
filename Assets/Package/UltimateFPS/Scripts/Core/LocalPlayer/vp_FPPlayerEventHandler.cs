/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPPlayerEventHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class handles communication between all the behaviours that
//					make up an FPS player. it declares all events that will be
//					available to objects in the player hierarchy, and binds several
//					of the object component states to player activity events
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

public class vp_FPPlayerEventHandler : vp_StateEventHandler
{

	// these declarations determine which events are supported by the
	// player event handler. it is then up to external classes to fill
	// them up with delegates for communication.

	// TIPS:
	//  1) mouse-over on the event types (e.g. vp_Message) for usage info.
	//  2) to find the places where an event is SENT, you can do 'Find All
	// References' on the event in your IDE. if this is not available, you
	// can search the project for the event name preceded by '.' (.Reload)
	//  3) to find the methods that LISTEN to an event, search the project
	// for its name preceded by '_' (_Reload)

	// gui
	public vp_Message<float> HUDDamageFlash;
	public vp_Message<string> HUDText;

	// input
	public vp_Value<Vector2> InputMoveVector;
	public vp_Value<bool> AllowGameplayInput;

	// player properties
	public vp_Value<float> Health;
	public vp_Value<Vector3> Position;
	public vp_Value<Vector2> Rotation;
	public vp_Value<Vector3> Forward;
	public vp_Value<Vector3> Velocity;
	public vp_Value<Vector3> MotorThrottle;

	// player activities
	public vp_Activity Dead;
	public vp_Activity Run;
	public vp_Activity Jump;
	public vp_Activity Crouch;
	public vp_Activity Zoom;
	public vp_Activity Attack;
	public vp_Activity Reload;
	public vp_Activity Climb;
	public vp_Activity Interact;
	public vp_Activity<int> SetWeapon;
	public vp_Activity<Vector3> Earthquake;

	// weapon handling
	public vp_Attempt SetPrevWeapon;
	public vp_Attempt SetNextWeapon;
	public vp_Attempt<string> SetWeaponByName;
	public vp_Value<int> CurrentWeaponID;
	public vp_Value<string> CurrentWeaponName;
	public vp_Value<bool> CurrentWeaponWielded;
	public vp_Value<float> CurrentWeaponReloadDuration;
	public vp_Value<string> CurrentWeaponClipType;

	// inventory
	public vp_Message<string, int> GetItemCount;
	public vp_Attempt<object> AddItem;
	public vp_Attempt<object> RemoveItem;
	public vp_Attempt<object> AddAmmo;
	public vp_Attempt DepleteAmmo;
	public vp_Attempt RemoveClip;
	public vp_Value<int> CurrentWeaponAmmoCount;
	public vp_Value<int> CurrentWeaponClipCount;

	// physics
	public vp_Message<float> FallImpact;
	public vp_Message<float> HeadImpact;
	public vp_Message<Vector3> ForceImpact;
	public vp_Message<float> GroundStomp;
	public vp_Message<float> BombShake;
	public vp_Value<Vector3> EarthQuakeForce;
	public vp_Message Stop;
	public vp_Value<Transform> Platform;

	// misc
	public vp_Value<bool> Pause;
	public vp_Value<vp_Interactable> Interactable;
	public vp_Value<Texture> Crosshair;
	public vp_Value<Texture> GroundTexture;
	public vp_Value<vp_SurfaceIdentifier> SurfaceType;


	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		// TIP: please see the manual for the difference
		// between (player) activities and (component) states

		// --- activity state bindings ---
		// whenever these activities are toggled they will enable and
		// disable any component states with the same names. disable
		// these lines to make states independent of activities
		BindStateToActivity(Run);
		BindStateToActivity(Jump);
		BindStateToActivity(Crouch);
		BindStateToActivity(Zoom);
		BindStateToActivity(Reload);
		BindStateToActivity(Dead);
		BindStateToActivity(Climb);
		BindStateToActivityOnStart(Attack);	// <--
		// in this default setup the 'Attack' activity will enable
		// - but not disable - the component attack states when toggled.
		
		// --- activity AutoDurations ---
		// automatically stops an activity after a set timespan
		SetWeapon.AutoDuration = 1.0f;		// NOTE: altered at runtime by each weapon
		Reload.AutoDuration = 1.0f;			// NOTE: altered at runtime by each weapon

		// --- activity MinDurations ---
		// prevents player from aborting an activity too soon after starting
		Zoom.MinDuration = 0.2f;
		Crouch.MinDuration = 0.5f;

		// --- activity MinPauses ---
		// prevents player from restarting an activity too soon after stopping
		Jump.MinPause = 0.0f;			// increase this to enforce a certain delay between jumps
		SetWeapon.MinPause = 0.2f;

	}
	

}

