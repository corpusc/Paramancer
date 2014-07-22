/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPInput.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a script to collect all mouse and keyboard input for first person
//					controls in one place
//
//					NOTES:
//					 1) this script uses Unity's default input axes where applicable.
//					for actions that have no default input axes, temporary hardcoded
//					key codes are used. to make those controls user-configurable, create
//					new input axes for your project in the editor (go to Edit -> Project
//					Settings -> Input). suggested axis names are provided below.
//					 2) for more advanced key binding features, check out the 'cInput'
//					asset which should integrate fine with this class:
//					http://cinput2.weebly.com/
//					 3) in v1.4, a lot of mouse logic is actually still in the camera
//					class. the goal is to move it here in an upcoming release
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class vp_FPInput : MonoBehaviour
{

	public vp_FPPlayerEventHandler Player = null;

	// components
	protected vp_FPCamera m_FPCamera = null;

	// mouse
	public Rect[] MouseCursorZones = null;			// screen regions where mouse cursor remains visible when clicking. may be set up in the Inspector
	public bool ForceCursor = false;				// when true, mouse cursor is enabled all over the screen and firing is disabled
	protected Vector2 m_MousePos = Vector2.zero;	// current mouse position in GUI coordinates (Y flipped)

	// misc
	protected bool m_AllowGameplayInput = true;
	public bool AllowGameplayInput
	{
		get	{	return m_AllowGameplayInput;	}
		set	{	m_AllowGameplayInput = value;	}
	}


	//////////////////////////////////////////////////////////
	// mouse input properties
	// NOTE: the source variables of these properties currently
	// reside in vp_FPCamera. they will likely be moved to this
	// class in an upcoming release
	//////////////////////////////////////////////////////////

	public Vector2 MousePos { get { return m_MousePos; } }
	public Vector2 MouseSensitivity { get { return m_FPCamera.MouseSensitivity; } set { m_FPCamera.MouseSensitivity = value; } }
	public int MouseSmoothSteps { get { return m_FPCamera.MouseSmoothSteps; } set { m_FPCamera.MouseSmoothSteps = (int)Mathf.Clamp(value, 1.0f, 10.0f); } }
	public float MouseSmoothWeight { get { return m_FPCamera.MouseSmoothWeight; } set { m_FPCamera.MouseSmoothWeight = Mathf.Clamp01(value); } }
	public bool MouseAcceleration { get { return m_FPCamera.MouseAcceleration; } set { m_FPCamera.MouseAcceleration = value; } }
	public float MouseAccelerationThreshold { get { return m_FPCamera.MouseAccelerationThreshold; } set { m_FPCamera.MouseAccelerationThreshold = value; } }
	

	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update()
	{

		// handle input for GUI
		UpdateCursorLock();

		UpdatePause();
		
		if(Player.Pause.Get() == true)
			return;

		// --- NOTE: everything below this line will be disabled on pause! ---

		if (!m_AllowGameplayInput)
			return;
		
		// interaction
		InputInteract();

		// handle input for moving
		InputMove();
		InputRun();
		InputJump();
		InputCrouch();

		// handle input for weapons
		InputAttack();
		InputZoom();
		InputReload();
		InputSetWeapon();

	}
	

	/// <summary>
	/// Handles interaction with the game world
	/// </summary>
	void InputInteract()
	{

		//if (Input.GetButtonUp("Interact"))	// suggested input axis
		if(Input.GetKeyDown(KeyCode.F))
			Player.Interact.TryStart();
		else
			Player.Interact.TryStop();

	}


	/// <summary>
	/// move the player forward, backward, left and right
	/// </summary>
	protected virtual void InputMove()
	{

		// NOTES: you may also use 'GetAxis', but that will add smoothing
		// to the input from both Ultimate FPS and from Unity, and might
		// require some tweaking in order not to feel laggy

		Player.InputMoveVector.Set(new Vector2(Input.GetAxisRaw("Horizontal"),
											Input.GetAxisRaw("Vertical")));

	}


	/// <summary>
	/// tell the player to enable or disable the 'Run' state.
	/// NOTE: since running is a state, it's not sent to the
	/// controller code (which doesn't know the state names).
	/// instead, the player class is responsible for feeding the
	/// 'Run' state to every affected component.
	/// </summary>
	protected virtual void InputRun()
	{

		//if (Input.GetButton("Run"))	// suggested input axis
		if (Input.GetKey(KeyCode.LeftShift))
			Player.Run.TryStart();
		else
			Player.Run.TryStop();

	}


	/// <summary>
	/// ask controller to jump when button is pressed (the current
	/// controller preset determines jump force).
	/// NOTE: if its 'MotorJumpForceHold' is non-zero, this
	/// also makes the controller accumulate jump force until
	/// button release.
	/// </summary>
	protected virtual void InputJump()
	{

		// TIP: to find out what determines if 'Jump.TryStart'
		// succeeds and where it is hooked up, search the project
		// for 'CanStart_Jump'

		if (Input.GetButton("Jump"))
			Player.Jump.TryStart();
		else
			Player.Jump.Stop();

	}


	/// <summary>
	/// asks the controller to halve the height of its Unity
	/// CharacterController collision capsule while player is
	/// holding the crouch modifier key. this activity will also
	/// typically trigger states on the camera and weapons. note
	/// that getting up again won't always succeed (for example
	/// the player might be crawling through a ventilation shaft
	/// or hiding under a table).
	/// </summary>
	protected virtual void InputCrouch()
	{

		// IMPORTANT: using the 'Crouch' activity for crouching
		// ensures that CharacterController (collision) height is only
		// updated when needed. this is important because changing its
		// height every frame will make trigger detection break!

		//if (Input.GetButton("Crouch"))	// suggested input axis
		if (Input.GetKey(KeyCode.C))
			Player.Crouch.TryStart();
		else
			Player.Crouch.TryStop();

		// TIP: to find out what determines if 'Crouch.TryStop'
		// succeeds and where it is hooked up, search the project
		// for 'CanStop_Crouch'

	}


	/// <summary>
	/// zoom in using the zoom modifier key(s)
	/// </summary>
	protected virtual void InputZoom()
	{

		//if (Input.GetButton("Zoom"))		// suggested  input axis
		if ((Input.GetButton("Fire2")) || (Input.GetButton("Fire3")))
			Player.Zoom.TryStart();
		else
			Player.Zoom.TryStop();

	}


	/// <summary>
	/// broadcasts a message to any listening components telling
	/// them to go into 'attack' mode. vp_FPShooter uses this to
	/// repeatedly fire the current weapon while the fire button
	/// is being pressed, but it could also be used by, for example,
	/// an animation script to make the player model loop an 'attack
	/// stance' animation.
	/// </summary>
	protected virtual void InputAttack()
	{

		// TIP: uncomment this to prevent player from attacking while running
		//if (Player.Run.Active)
		//	return;

		// if mouse cursor is visible, an extra click is needed
		// before we can attack
		if (!Screen.lockCursor)
			return;

		if (Input.GetButton("Fire1"))
			Player.Attack.TryStart();
		else
			Player.Attack.TryStop();

	}


	/// <summary>
	/// when the reload button is pressed, broadcasts a message
	/// to any listening components asking them to reload
	/// NOTE: reload may not succeed due to ammo status etc.
	/// </summary>
	protected virtual void InputReload()
	{

		//if (Input.GetButton("Reload"))	// suggested input axis
		if (Input.GetKeyDown(KeyCode.R))
			Player.Reload.TryStart();
		
	}

	
	/// <summary>
	/// handles cycling through carried weapons, wielding specific
	/// ones and clearing the current one
	/// </summary>
	protected virtual void InputSetWeapon()
	{

		// --- cycle to the next or previous weapon ---

		//if (Input.GetButton("SetPrevWeapon"))		// suggested input axis
		if (Input.GetKeyDown(KeyCode.Q))
			Player.SetPrevWeapon.Try();

		//if (Input.GetButton("SetNextWeapon"))		// suggested input axis
		if (Input.GetKeyDown(KeyCode.E))
			Player.SetNextWeapon.Try();
		
		// --- switch to weapon 1-10 by direct button press ---

		//if (Input.GetButton("SetWeapon1"))	// (etc.) suggested input axes
		if (Input.GetKeyDown(KeyCode.Alpha1)) Player.SetWeapon.TryStart(1);
		if (Input.GetKeyDown(KeyCode.Alpha2)) Player.SetWeapon.TryStart(2);
		if (Input.GetKeyDown(KeyCode.Alpha3)) Player.SetWeapon.TryStart(3);
		if (Input.GetKeyDown(KeyCode.Alpha4)) Player.SetWeapon.TryStart(4);
		if (Input.GetKeyDown(KeyCode.Alpha5)) Player.SetWeapon.TryStart(5);
		if (Input.GetKeyDown(KeyCode.Alpha6)) Player.SetWeapon.TryStart(6);
		if (Input.GetKeyDown(KeyCode.Alpha7)) Player.SetWeapon.TryStart(7);
		if (Input.GetKeyDown(KeyCode.Alpha8)) Player.SetWeapon.TryStart(8);
		if (Input.GetKeyDown(KeyCode.Alpha9)) Player.SetWeapon.TryStart(9);
		if (Input.GetKeyDown(KeyCode.Alpha0)) Player.SetWeapon.TryStart(10);

		// --- unwield current weapon by direct button press ---

		//if (Input.GetButton("ClearWeapon"))	// suggested input axis
		if (Input.GetKeyDown(KeyCode.Backspace)) Player.SetWeapon.TryStart(0);

	}


	/// <summary>
	/// toggles the game's pause state on / off
	/// </summary>
	protected virtual void UpdatePause()
	{

		//if (Input.GetButton("Pause"))		// suggested input axis
		if (Input.GetKeyUp(KeyCode.P))
			Player.Pause.Set(!Player.Pause.Get());

	}


	/// <summary>
	/// this method handles toggling between mouse pointer and
	/// firing modes. it can be used to deal with screen regions
	/// for button menus, inventory panels et cetera.
	/// NOTE: if your game supports multiple screen resolutions,
	/// make sure your 'MouseCursorZones' are always adapted to
	/// the current resolution. see 'vp_FPSDemo1.Start' for one
	/// example of how to this
	/// </summary>
	protected virtual void UpdateCursorLock()
	{

		// store the current mouse position as GUI coordinates
		m_MousePos.x = Input.mousePosition.x;
		m_MousePos.y = (Screen.height - Input.mousePosition.y);

		// uncomment this line to print the current mouse position
		//Debug.Log("X: " + (int)m_MousePos.x + ", Y:" + (int)m_MousePos.y);

		// if 'ForceCursor' is active, the cursor will always be visible
		// across the whole screen and firing will be disabled
		if (ForceCursor)
		{
			Screen.lockCursor = false;
			return;
		}

		// see if any of the mouse buttons are being held down
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{

			// if we have defined mouse cursor zones, check to see if the
			// mouse cursor is inside any of them
			if (MouseCursorZones.Length > 0)
			{
				foreach (Rect r in MouseCursorZones)
				{
					if (r.Contains(m_MousePos))
					{
						// mouse is being held down inside a mouse cursor zone, so make
						// sure the cursor is not locked and don't lock it this frame
						Screen.lockCursor = false;
						goto DontLock;
					}
				}
			}

			// no zones prevent firing the current weapon. hide mouse cursor
			// and lock it at the center of the screen
			Screen.lockCursor = true;

		}

	DontLock:

		// if user presses 'ENTER', toggle mouse cursor on / off
		if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
			Screen.lockCursor = !Screen.lockCursor;

	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{

		Player = (vp_FPPlayerEventHandler)transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));

		m_FPCamera = GetComponentInChildren<vp_FPCamera>();

	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		if (Player != null)
			Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{


		if (Player != null)
			Player.Unregister(this);

	}
	
	
	/// <summary>
	/// allows or prevents first person gameplay input. NOTE:
	/// gui (menu) input is still allowed
	/// </summary>
	protected virtual bool OnValue_AllowGameplayInput
	{
		get	{	return m_AllowGameplayInput;	}
		set	{	m_AllowGameplayInput = value;	}
	}


	/// <summary>
	/// pauses the game by setting timescale to zero, or unpauses
	/// it by resuming the timescale that was active upon pause
	/// </summary>
	protected virtual bool OnValue_Pause
	{
		get { return vp_TimeUtility.Paused; }
		set { vp_TimeUtility.Paused = value; }
	}


}

