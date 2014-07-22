/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemo1.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	PLEASE NOTE: this class is very specialized	for the demo
//					walkthrough and is not meant to be used as the starting
//					point for a game, nor an example of best workflow practices.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public class vp_FPSDemo1 : MonoBehaviour
{

	public GameObject Player = null;
	private vp_FPSDemoManager m_Demo = null;

	// demo states
	private int m_ExamplesCurrentSel = 0;			// selection for the 'EXAMPLES' screen

	// timers
	private vp_Timer.Handle m_ChrashingAirplaneRestoreTimer = new vp_Timer.Handle();
	private vp_Timer.Handle m_WeaponSwitchTimer = new vp_Timer.Handle();

	// GUI
	private bool m_WeaponLayerToggle = false;		// state of the weapon layer toggle button

	// positions
	private Vector3 m_MouseLookPos = new Vector3(-8.093015f, 20.08f, 3.416737f);
	private Vector3 m_OverviewPos = new Vector3(1.246535f, 32.08f, 21.43753f);
	private Vector3 m_StartPos = new Vector3(-18.14881f, 20.08f, -24.16859f);
	private Vector3 m_WeaponLayerPos = new Vector3(-19.43989f, 16.08f, 2.10474f);
	private Vector3 m_ForcesPos = new Vector3(-8.093015f, 20.08f, 3.416737f);
	private Vector3 m_MechPos = new Vector3(0.02941191f, 1.08f, -93.50691f);
	private Vector3 m_DrunkPos = new Vector3(18.48685f, 21.08f, 24.05441f);
	private Vector3 m_SniperPos = new Vector3(0.8841875f, 33.08f, 21.3446f);
	private Vector3 m_OldSchoolPos = new Vector3(25.88745f, 0.08f, 23.08822f);
	private Vector3 m_AstronautPos = new Vector3(20.0f, 20.0f, 16.0f);
	protected Vector3 m_UnFreezePosition = Vector3.zero;

	// angles
	private Vector2 m_MouseLookAngle = new Vector2(0, 33.10683f);
	private Vector2 m_OverviewAngle = new Vector2(28.89369f, 224);
	private Vector2 m_PerspectiveAngle = new Vector2(27, 223);
	private Vector2 m_StartAngle = new Vector2(0, 0);
	private Vector2 m_WeaponLayerAngle = new Vector2(0, -90);
	private Vector2 m_ForcesAngle = new Vector2(-167, 33.10683f);
	private Vector2 m_MechAngle = new Vector3(0, 180);
	private Vector2 m_DrunkAngle = new Vector3(0, -90);
	private Vector2 m_SniperAngle = new Vector2(20, 180);
	private Vector2 m_OldSchoolAngle = new Vector2(0, 180);
	private Vector2 m_AstronautAngle = new Vector2(0, 269.5f);

	// textures
	public Texture m_ImageEditorPreview = null;
	public Texture m_ImageEditorPreviewShow = null;
	public Texture m_ImageCameraMouse = null;
	public Texture m_ImageWeaponPosition = null;
	public Texture m_ImageWeaponPerspective = null;
	public Texture m_ImageWeaponPivot = null;
	public Texture m_ImageEditorScreenshot = null;
	public Texture m_ImageLeftArrow = null;
	public Texture m_ImageRightArrow = null;
	public Texture m_ImageCheckmark = null;
	public Texture m_ImageLeftPointer = null;
	public Texture m_ImageRightPointer = null;
	public Texture m_ImageUpPointer = null;
	public Texture m_ImageCrosshair = null;
	public Texture m_ImageFullScreen = null;

	// sounds
	AudioSource m_AudioSource = null;
	public AudioClip m_StompSound = null;
	public AudioClip m_EarthquakeSound = null;
	public AudioClip m_ExplosionSound = null;

	// fx
	public GameObject m_ArtilleryFX = null;

	// presets
	public TextAsset ArtilleryCamera = null;
	public TextAsset ArtilleryController = null;
	public TextAsset AstronautCamera = null;
	public TextAsset AstronautController = null;
	public TextAsset CowboyCamera = null;
	public TextAsset CowboyController = null;
	public TextAsset CowboyWeapon = null;
	public TextAsset CowboyShooter = null;
	public TextAsset CrouchController = null;
	public TextAsset DefaultCamera = null;
	public TextAsset DefaultWeapon = null;
	public TextAsset DrunkCamera = null;
	public TextAsset DrunkController = null;
	public TextAsset ImmobileCamera = null;
	public TextAsset ImmobileController = null;
	public TextAsset MaceCamera = null;
	public TextAsset MaceWeapon = null;
	public TextAsset MafiaCamera = null;
	public TextAsset MafiaWeapon = null;
	public TextAsset MafiaShooter = null;
	public TextAsset MechCamera = null;
	public TextAsset MechController = null;
	public TextAsset MechWeapon = null;
	public TextAsset MechShooter = null;
	public TextAsset ModernCamera = null;
	public TextAsset ModernController = null;
	public TextAsset ModernWeapon = null;
	public TextAsset ModernShooter = null;
	public TextAsset MouseLowSensCamera = null;
	public TextAsset MouseRawUnityCamera = null;
	public TextAsset MouseSmoothingCamera = null;
	public TextAsset OldSchoolCamera = null;
	public TextAsset OldSchoolController = null;
	public TextAsset OldSchoolWeapon = null;
	public TextAsset OldSchoolShooter = null;
	public TextAsset Persp1999Camera = null;
	public TextAsset Persp1999Weapon = null;
	public TextAsset PerspModernCamera = null;
	public TextAsset PerspModernWeapon = null;
	public TextAsset PerspOldCamera = null;
	public TextAsset PerspOldWeapon = null;
	public TextAsset PivotChestWeapon = null;
	public TextAsset PivotElbowWeapon = null;
	public TextAsset PivotMuzzleWeapon = null;
	public TextAsset PivotWristWeapon = null;
	public TextAsset SmackController = null;
	public TextAsset SniperCamera = null;
	public TextAsset SniperWeapon = null;
	public TextAsset SniperShooter = null;
	public TextAsset StompingCamera = null;
	public TextAsset SystemOFFCamera = null;
	public TextAsset SystemOFFController = null;
	public TextAsset SystemOFFShooter = null;
	public TextAsset SystemOFFWeapon = null;
	public TextAsset SystemOFFWeaponGlideIn = null;
	public TextAsset TurretCamera = null;
	public TextAsset TurretWeapon = null;
	public TextAsset TurretShooter = null;
	public TextAsset WallFacingCamera = null;
	public TextAsset WallFacingWeapon = null;


	/// <summary>
	/// 
	/// </summary>
	void Start()
	{

		m_Demo = new vp_FPSDemoManager(Player);

		m_Demo.PlayerEventHandler.Register(this);

		m_Demo.CurrentFullScreenFadeTime = Time.time;
		m_Demo.DrawCrosshair = false;

		m_Demo.Input.MouseCursorZones = new Rect[3];
		m_Demo.Input.MouseCursorZones[0] = new Rect((Screen.width * 0.5f) - 370, 40, 80, 80);
		m_Demo.Input.MouseCursorZones[1] = new Rect((Screen.width * 0.5f) + 290, 40, 80, 80);
		m_Demo.Input.MouseCursorZones[2] = new Rect(0, 0, 150, Screen.height);
		Screen.lockCursor = false;

		m_Demo.Camera.RenderingFieldOfView = 20;
		m_Demo.Camera.SnapZoom();
		m_Demo.Camera.PositionOffset = new Vector3(0, 1.75f, 0.1f);

		// add an audio source to the camera, for playing various demo sounds
		m_AudioSource = (AudioSource)m_Demo.Camera.gameObject.AddComponent("AudioSource");

		// prevent regular event handler weapon switching events
		m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000);
		m_Demo.PlayerEventHandler.SetPrevWeapon.Try = delegate() { return false; };
		m_Demo.PlayerEventHandler.SetNextWeapon.Try = delegate() { return false; };

	}


	/// <summary>
	/// 
	/// </summary>
	void OnDisable()
	{

		if (m_Demo.PlayerEventHandler != null)
			m_Demo.PlayerEventHandler.Unregister(this);

	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{

		m_Demo.Update();

		// special case to make sure the weapon doesn't flicker into
		// view briefly in the first frame
		if (m_Demo.CurrentScreen == 1 && m_Demo.WeaponHandler.CurrentWeapon != null)
			m_Demo.WeaponHandler.SetWeapon(0);

		// input special cases for the 'EXAMPLES' screen
		if (m_Demo.CurrentScreen == 2)
		{

			// switch weapon examples using the 1-0 number keys
			if (Input.GetKeyDown(KeyCode.Backspace))
				m_Demo.ButtonSelection = 0;
			if (Input.GetKeyDown(KeyCode.Alpha1))
				m_Demo.ButtonSelection = 1;
			if (Input.GetKeyDown(KeyCode.Alpha2))
				m_Demo.ButtonSelection = 2;
			if (Input.GetKeyDown(KeyCode.Alpha3))
				m_Demo.ButtonSelection = 3;
			if (Input.GetKeyDown(KeyCode.Alpha4))
				m_Demo.ButtonSelection = 4;
			if (Input.GetKeyDown(KeyCode.Alpha5))
				m_Demo.ButtonSelection = 5;
			if (Input.GetKeyDown(KeyCode.Alpha6))
				m_Demo.ButtonSelection = 6;
			if (Input.GetKeyDown(KeyCode.Alpha7))
				m_Demo.ButtonSelection = 7;
			if (Input.GetKeyDown(KeyCode.Alpha8))
				m_Demo.ButtonSelection = 8;
			if (Input.GetKeyDown(KeyCode.Alpha9))
				m_Demo.ButtonSelection = 9;
			if (Input.GetKeyDown(KeyCode.Alpha0))
				m_Demo.ButtonSelection = 10;

			// cycle to previous example
			if (Input.GetKeyDown(KeyCode.Q))
			{
				m_Demo.ButtonSelection--;
				if (m_Demo.ButtonSelection < 1)
					m_Demo.ButtonSelection = 10;
			}

			// cycle to next example
			if (Input.GetKeyDown(KeyCode.E))
			{
				m_Demo.ButtonSelection++;
				if (m_Demo.ButtonSelection > 10)
					m_Demo.ButtonSelection = 1;
			}

		}

		// special case to cancel the crashing airplane example zoom reset
		// if user navigates away from the 'EXTERNAL FORCES' screen
		if (m_Demo.CurrentScreen != 3 && m_ChrashingAirplaneRestoreTimer.Active)
			m_ChrashingAirplaneRestoreTimer.Cancel();

	}


	/// <summary>
	/// demo screen to show a welcoming message
	/// </summary>
	private void DemoIntro()
	{

		// draw the three big boxes at the top of the screen;
		// the next & prev arrows, and the main text box
		m_Demo.DrawBoxes("part ii: under the hood", "Ultimate FPS features a NEXT-GEN first person camera system with ultra smooth PROCEDURAL ANIMATION of player movements. Camera and weapons are manipulated using over 100 parameters, allowing for a vast range of super-lifelike behaviors.", null, m_ImageRightArrow);

		// this bracket is only run the first frame of a new demo screen
		// being rendered. it is used to make some initial settings
		// specific to the current demo screen.
		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = false;
			m_Demo.FirstFrame = false;
			m_Demo.Camera.RenderingFieldOfView = 20;
			m_Demo.Camera.SnapZoom();
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, true);			// prevent player from moving
			m_Demo.LastInputTime -= 20;											// makes the big arrow start fading 20 seconds earlier on this screen
			m_Demo.RefreshDefaultState();
			m_Demo.Input.ForceCursor = true;
		}

		m_Demo.ForceCameraShake();

	}


	/// <summary>
	/// 
	/// </summary>
	private void SetWeapon(int i, string state = null, bool drawCrosshair = true,
							bool wieldMotion = true)
	{

		m_Demo.DrawCrosshair = drawCrosshair;

		if (m_Demo.WeaponHandler.CurrentWeaponID != i)
		{
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				if (m_ExamplesCurrentSel == 0)
					m_Demo.WeaponHandler.CurrentWeapon.SnapToExit();
				else
				{
					if (wieldMotion)
						m_Demo.WeaponHandler.CurrentWeapon.Wield(false);
				}
			}
			vp_Timer.In((wieldMotion ? 0.2f : 0.0f), delegate()
			{
				m_Demo.WeaponHandler.SetWeapon(i);
				if (m_Demo.WeaponHandler.CurrentWeapon != null)
				{
					if (wieldMotion)
						m_Demo.WeaponHandler.CurrentWeapon.Wield();
				}
				if (state != null)
					m_Demo.PlayerEventHandler.SetState(state);
			}, m_WeaponSwitchTimer);
		}
		else
		{
			if (state != null)
				m_Demo.PlayerEventHandler.SetState(state);
		}



	}


	/// <summary>
	/// demo screen to show some example presets that are possible
	/// with Ultimate FPS
	/// </summary>
	private void DemoExamples()
	{

		m_Demo.DrawBoxes("examples", "Try MOVING, JUMPING and STRAFING with the demo presets on the left.\nNote that NO ANIMATIONS are used in this demo. Instead, the camera and weapons are manipulated using realtime SPRING PHYSICS, SINUS BOB and NOISE SHAKING.\nCombining this with traditional animations (e.g. reload) can be very powerful!", m_ImageLeftArrow, m_ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_AudioSource.Stop();
			m_Demo.DrawCrosshair = true;
			m_Demo.Teleport(m_StartPos, m_StartAngle);						// place the player at start point
			m_Demo.FirstFrame = false;
			m_UnFreezePosition = m_Demo.Controller.transform.position;	// makes player return to start pos when unfreezed (from sniper, turret presets etc.)
			m_Demo.ButtonSelection = 0;
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.PlayerEventHandler.SetState("Freeze", false);
			m_Demo.PlayerEventHandler.SetState("SystemOFF");
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
				m_Demo.WeaponHandler.CurrentWeapon.SnapZoom();
			m_Demo.Camera.SnapZoom();
			m_Demo.Camera.SnapSprings();
			m_Demo.Input.ForceCursor = false;
		}

		// if selected button in toggle column has changed, change
		// the current preset
		if (m_Demo.ButtonSelection != m_ExamplesCurrentSel)
		{


			Screen.lockCursor = true;
			m_Demo.ResetState();

			m_Demo.PlayerEventHandler.Attack.Stop(0.5f);

			m_Demo.Camera.BobStepCallback = null;
			m_Demo.Camera.SnapSprings();

			if (m_ExamplesCurrentSel == 9)
			{
				if(m_Demo.WeaponHandler.CurrentWeapon != null)
				{
					m_Demo.WeaponHandler.CurrentWeapon.SnapZoom();
					m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
					m_Demo.WeaponHandler.CurrentWeapon.SnapPivot();
				}
			}

			switch (m_Demo.ButtonSelection)
			{
				case 0:	// --- System OFF ---
					m_Demo.PlayerEventHandler.Attack.Stop(10000000);
					m_Demo.DrawCrosshair = true;
					m_Demo.Controller.Stop();
					if (m_Demo.WeaponHandler.CurrentWeaponID == 5)	// mech cockpit is not allowed in 'system off' mode
					{
						m_Demo.WeaponHandler.SetWeapon(1);
						m_Demo.PlayerEventHandler.SetState("SystemOFF");
					}
					else
					{
						m_Demo.Camera.SnapZoom();
						m_Demo.PlayerEventHandler.SetState("SystemOFF");
						if (m_Demo.WeaponHandler.CurrentWeapon != null)
						{
							m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
							m_Demo.WeaponHandler.CurrentWeapon.SnapZoom();
						}
					}
					break;
				case 1:	// --- Mafia Boss ---
					SetWeapon(3, "MafiaBoss");
					break;
				case 2:	// --- Modern Shooter ---
					SetWeapon(1, "ModernShooter");
					break;
				case 3:	// --- Barbarian ---
					SetWeapon(4, "Barbarian", true);
					break;
				case 4:	// --- Sniper Breath ---
					SetWeapon(2, "SniperBreath");
					m_Demo.Controller.Stop();
					m_Demo.Teleport(m_SniperPos, m_SniperAngle);
					break;
				case 5:	// --- Astronaut ---
					SetWeapon(0, "Astronaut", false);
					m_Demo.Controller.Stop();
					m_Demo.Teleport(m_AstronautPos, m_AstronautAngle);
					break;
				case 6:	// --- Mech... or Dino? ---
					SetWeapon(5, "MechOrDino", true, false);
					m_UnFreezePosition = m_DrunkPos;
					m_Demo.Controller.Stop();
					m_Demo.Teleport(m_MechPos, m_MechAngle);
					m_Demo.Camera.BobStepCallback = delegate()
					{
						m_Demo.Camera.AddForce2(new Vector3(0.0f, -1.0f, 0.0f));
						if (m_Demo.WeaponHandler.CurrentWeapon != null)
							m_Demo.WeaponHandler.CurrentWeapon.AddForce(new Vector3(0, 0, 0), new Vector3(-0.3f, 0, 0));
						m_AudioSource.pitch = Time.timeScale;
						m_AudioSource.PlayOneShot(m_StompSound);
					};
					break;
				case 7:	// --- Tank Turret ---
					SetWeapon(3, "TankTurret", true, false);
					m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
					m_Demo.Controller.Stop();
					break;
				case 8:	// --- Drunk Person ---
					m_Demo.Controller.Stop();
					SetWeapon(0, "DrunkPerson", false);
					m_Demo.Controller.Stop();
					m_Demo.Teleport(m_DrunkPos, m_DrunkAngle);
					m_Demo.Camera.StopSprings();
					m_Demo.Camera.Refresh();
					break;
				case 9:	// --- Old School ---
					SetWeapon(1, "OldSchool");
					m_Demo.Controller.Stop();
					m_Demo.Teleport(m_OldSchoolPos, m_OldSchoolAngle);
					m_Demo.Camera.SnapSprings();
					m_Demo.Camera.SnapZoom();
					// temp fix for muzzleflash position
					vp_Timer.In(0.3f, delegate()
					{
						if (m_Demo.WeaponHandler.CurrentWeapon != null)
						{
							vp_Shooter fix = m_Demo.WeaponHandler.CurrentWeapon.GetComponentInChildren<vp_Shooter>();
							fix.MuzzleFlashPosition = new Vector3(0.0025736f, -0.0813138f, 1.662671f);
							fix.Refresh();
						}
					});
					break;
				case 10:// --- Crazy Cowboy ---
					SetWeapon(2, "CrazyCowboy");
					m_Demo.Teleport(m_StartPos, m_StartAngle);
					m_Demo.Controller.Stop();
					break;
			}

			m_ExamplesCurrentSel = m_Demo.ButtonSelection;

		}

		if (m_Demo.ShowGUI)
		{

			// show a toggle column, a compound control displaying a
			// column of buttons that can be toggled like radio buttons
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
			string[] strings = new string[] { "System OFF", "Mafia Boss", "Modern Shooter", "Barbarian", "Sniper Breath", "Astronaut", "Mech... or Dino?", "Tank Turret", "Drunk Person", "Old School", "Crazy Cowboy" };
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(140, 150, m_Demo.ButtonSelection, strings, false, true, m_ImageRightPointer, m_ImageLeftPointer);

		}

		// draw menu re-enable text
		if (m_Demo.ShowGUI && Screen.lockCursor)
		{
			GUI.color = new Color(1, 1, 1, ((m_Demo.ClosingDown) ? m_Demo.GlobalAlpha : 1.0f));
			GUI.Label(new Rect((Screen.width / 2) - 200, 140, 400, 20), "(Press ENTER to reenable menu)", m_Demo.CenterStyle);
			GUI.color = new Color(1, 1, 1, 1 * m_Demo.GlobalAlpha);
		}

	}


	/// <summary>
	/// demo screen to explain external forces
	/// </summary>
	private void DemoForces()
	{

		m_Demo.DrawBoxes("external forces", "The camera and weapon are mounted on 8 positional and angular SPRINGS.\nEXTERNAL FORCES can be applied to these in various ways, creating unique movement patterns every time. This is useful for shockwaves, explosion knockback and earthquakes.", m_ImageLeftArrow, m_ImageRightArrow);

		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = false;
			m_Demo.ResetState();
			m_Demo.Camera.Load(StompingCamera);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_Demo.Controller.Load(SmackController);
			m_Demo.Camera.SnapZoom();
			m_Demo.FirstFrame = false;
			m_Demo.Teleport(m_ForcesPos, m_ForcesAngle);
			m_Demo.ButtonColumnArrowY = -100.0f;
			m_Demo.Input.ForceCursor = true;
		}

		if (m_Demo.ShowGUI)
		{

			// draw toggle column showing force examples
			m_Demo.ButtonSelection = -1;
			string[] strings = new string[] { "Earthquake", "Boss Stomp", "Incoming Artillery", "Crashing Airplane" };
			m_Demo.ButtonSelection = m_Demo.ButtonColumn(150, m_Demo.ButtonSelection, strings, m_ImageRightPointer);
			if (m_Demo.ButtonSelection != -1)
			{
				switch (m_Demo.ButtonSelection)
				{
					case 0:	// --- Earthquake ---
						m_Demo.Camera.Load(StompingCamera);
						m_Demo.Controller.Load(SmackController);
						m_Demo.PlayerEventHandler.Earthquake.TryStart(new Vector3(0.2f, 0.2f, 10.0f));
						m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9;
						m_AudioSource.Stop();
						m_AudioSource.pitch = Time.timeScale;
						m_AudioSource.PlayOneShot(m_EarthquakeSound);
						break;
					case 1:	// --- Boss Stomp ---
						m_Demo.PlayerEventHandler.Earthquake.Stop();
						m_Demo.Camera.Load(ArtilleryCamera);
						m_Demo.Controller.Load(SmackController);
						m_Demo.PlayerEventHandler.GroundStomp.Send(1.0f);
						m_Demo.ButtonColumnArrowFadeoutTime = Time.time;
						m_AudioSource.Stop();
						m_AudioSource.pitch = Time.timeScale;
						m_AudioSource.PlayOneShot(m_StompSound);
						break;
					case 2:	// --- Incoming Artillery ---
						m_Demo.PlayerEventHandler.Earthquake.Stop();
						m_Demo.Camera.Load(ArtilleryCamera);
						m_Demo.Controller.Load(ArtilleryController);
						m_Demo.PlayerEventHandler.BombShake.Send(1.0f);
						m_Demo.Controller.AddForce(UnityEngine.Random.Range(-1.5f, 1.5f), 0.5f,
																		UnityEngine.Random.Range(-1.5f, -0.5f));
						m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 1;
						m_AudioSource.Stop();
						m_AudioSource.pitch = Time.timeScale;
						m_AudioSource.PlayOneShot(m_ExplosionSound);
						Vector3 explosionPos = m_Demo.Controller.transform.TransformPoint(Vector3.forward * UnityEngine.Random.Range(1, 2));
						explosionPos.y = m_Demo.Controller.transform.position.y + 1;
						GameObject.Instantiate(m_ArtilleryFX, explosionPos, Quaternion.identity);
						break;
					case 3:	// --- Crashing Airplane ---
						m_Demo.Camera.Load(StompingCamera);
						m_Demo.Controller.Load(SmackController);
						m_Demo.PlayerEventHandler.Earthquake.TryStart(new Vector3(0.25f, 0.2f, 10.0f));
						m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9;
						m_AudioSource.Stop();
						m_AudioSource.pitch = Time.timeScale;
						m_AudioSource.PlayOneShot(m_EarthquakeSound);
						m_Demo.Camera.RenderingFieldOfView = 80;
						m_Demo.Camera.RotationEarthQuakeFactor = 6.5f;
						m_Demo.Camera.Zoom();
						vp_Timer.In(9, delegate() { m_Demo.Camera.RenderingFieldOfView = 60; m_Demo.Camera.RotationEarthQuakeFactor = 0.0f; m_Demo.Camera.Zoom(); },
							m_ChrashingAirplaneRestoreTimer);
						break;
				}
				m_Demo.LastInputTime = Time.time;
			}


			// show a screenshot preview of the mouse input editor section
			// in the bottom left corner.
			m_Demo.DrawEditorPreview(m_ImageWeaponPosition, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}


	/// <summary>
	/// demo screen to explain mouse smoothing and acceleration
	/// </summary>
	private void DemoMouseInput()
	{

		m_Demo.DrawBoxes("mouse input", "Any good FPS should offer configurable MOUSE SMOOTHING and ACCELERATION.\n• Smoothing interpolates mouse input over several frames to reduce jittering.\n • Acceleration + low mouse sensitivity allows high precision without loss of turn speed.\n• Click the below buttons to compare some example setups.", m_ImageLeftArrow, m_ImageRightArrow);

		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_AudioSource.Stop();
			m_Demo.DrawCrosshair = true;
			m_Demo.FreezePlayer(m_MouseLookPos, m_MouseLookAngle, true);
			m_Demo.Camera.Load(MouseRawUnityCamera);
			m_Demo.FirstFrame = false;
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.Input.ForceCursor = true;
		}

		if (m_Demo.ShowGUI)
		{

			// show a toggle column for mouse examples
			int currentSel = m_Demo.ButtonSelection;
			bool showArrow = (m_Demo.ButtonSelection == 2) ? false : true;	// small arrow for the 'acceleration' button
			string[] strings = new string[] { "Raw Mouse Input", "Mouse Smoothing", "Low Sens. + Acceleration" };
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, true, showArrow, m_ImageRightPointer, m_ImageLeftPointer);

			if (m_Demo.ButtonSelection != currentSel)
			{
				switch (m_Demo.ButtonSelection)
				{
					case 0:	// --- Raw Unity Mouse Input ---
						m_Demo.Camera.Load(MouseRawUnityCamera);
						break;
					case 1:	// --- Mouse Smoothing ---
						m_Demo.Camera.Load(MouseSmoothingCamera);
						break;
					case 2:	// --- Low Sens. + Acceleration ---
						m_Demo.Camera.Load(MouseLowSensCamera);
						break;
				}
				m_Demo.LastInputTime = Time.time;
			}

			// separate small arrow for the 'ON / OFF' buttons. this one points
			// upward and is only shown if 'acceleration' is chosen
			showArrow = true;
			if (m_Demo.ButtonSelection != 2)
			{
				GUI.enabled = false;
				showArrow = false;
			}

			// show a 'button toggle', a compound control for a basic on / off toggle
			m_Demo.Camera.MouseAcceleration = m_Demo.ButtonToggle(new Rect((Screen.width / 2) + 110, 215, 90, 40),
														"Acceleration", m_Demo.Camera.MouseAcceleration, showArrow, m_ImageUpPointer);
			GUI.color = new Color(1, 1, 1, 1 * m_Demo.GlobalAlpha);
			GUI.enabled = true;

			m_Demo.DrawEditorPreview(m_ImageCameraMouse, m_ImageEditorPreview, m_ImageEditorScreenshot);

		}

	}


	/// <summary>
	/// demo screen to explain weapon FOV and offset
	/// </summary>
	private void DemoWeaponPerspective()
	{

		m_Demo.DrawBoxes("weapon perspective", "Proper WEAPON PERSPECTIVE is crucial to the final impression of your game!\nThe weapon has its own separate Field of View for full perspective control,\nalong with dynamic position and rotation offset.", m_ImageLeftArrow, m_ImageRightArrow);

		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.Camera.Load(PerspOldCamera);
			m_Demo.Camera.SnapZoom();	// prevents animated zooming and instead sets the zoom in one frame
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_PerspectiveAngle, true);
			m_Demo.Input.ForceCursor = true;
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.SetWeaponPreset(PerspOldWeapon, null, true);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
				m_Demo.WeaponHandler.CurrentWeapon.SetState("WeaponPersp");
			m_Demo.WeaponHandler.SetWeaponLayer(vp_Layer.Weapon);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				m_Demo.WeaponHandler.CurrentWeapon.SnapZoom();
				m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
				m_Demo.WeaponHandler.CurrentWeapon.SnapPivot();
			}
		}

		if (m_Demo.ShowGUI)
		{

			//// show toggle column for the weapon FOV example buttons
			int currentSel = m_Demo.ButtonSelection;
			string[] strings = new string[] { "Old School", "1999 Internet Café", "Modern Shooter" };
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, true, true, m_ImageRightPointer, m_ImageLeftPointer);
			if (m_Demo.ButtonSelection != currentSel)
			{
			    switch (m_Demo.ButtonSelection)
			    {
			        case 0:	// --- Old School ---
					m_Demo.SetWeaponPreset(PerspOldWeapon, null, true);
			            break;
					case 1:	// --- 1999 Internet Café ---
						m_Demo.SetWeaponPreset(Persp1999Weapon, null, true);
						break;
					case 2:	// --- Modern Shooter ---
						m_Demo.SetWeaponPreset(PerspModernWeapon, null, true);
						break;
				}
			    m_Demo.LastInputTime = Time.time;
			}

			m_Demo.DrawEditorPreview(m_ImageWeaponPerspective, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}


	/// <summary>
	/// demo screen for explaining weapon camera layer
	/// NOTE: weapon layer is hardcoded as layer 31. this is
	/// set in vp_Layer.cs
	/// </summary>
	private void DemoWeaponLayer()
	{

		m_Demo.DrawBoxes("weapon camera", "\nThe weapon can be rendered by a SEPARATE CAMERA so that it never sticks through walls or other geometry. Try toggling the weapon camera ON and OFF below.", m_ImageLeftArrow, m_ImageRightArrow);

		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.DrawCrosshair = true;
			m_Demo.Camera.Load(WallFacingCamera);
			m_Demo.WeaponHandler.SetWeapon(3);
			m_Demo.SetWeaponPreset(WallFacingWeapon);
			m_Demo.Camera.SnapZoom();
			m_WeaponLayerToggle = false;
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
			int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
			m_Demo.WeaponHandler.SetWeaponLayer(layer);
			m_Demo.Input.ForceCursor = true;
		}

		if (m_Demo.ShowGUI)
		{

			// show button toggle for enabling / disabling the weapon layer
			bool currentWeaponLayer = m_WeaponLayerToggle;
			m_WeaponLayerToggle = m_Demo.ButtonToggle(new Rect((Screen.width / 2) - 45, 180, 100, 40), "Weapon Camera",
													m_WeaponLayerToggle, true, m_ImageUpPointer);
			if (currentWeaponLayer != m_WeaponLayerToggle)
			{
				m_Demo.FreezePlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
				int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
				m_Demo.WeaponHandler.SetWeaponLayer(layer);
				m_Demo.LastInputTime = Time.time;
			}

		}

	}


	/// <summary>
	/// demo screen to explain pivot manipulation
	/// </summary>
	private void DemoPivot()
	{

		m_Demo.DrawBoxes("weapon pivot", "The PIVOT POINT of the weapon model greatly affects movement pattern.\nManipulating it at runtime can be quite useful, and easy with Ultimate FPS!\nClick the examples below and move the camera around.", m_ImageLeftArrow, m_ImageRightArrow, delegate() { m_Demo.LoadLevel(2); });

		if (m_Demo.FirstFrame)
		{
			m_Demo.ResetState();
			m_Demo.DrawCrosshair = false;
			m_Demo.Camera.Load(DefaultCamera);
			m_Demo.Controller.Load(ImmobileController);
			m_Demo.FirstFrame = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_Demo.SetWeaponPreset(DefaultWeapon);
			m_Demo.SetWeaponPreset(PivotMuzzleWeapon, null, true);
			if (m_Demo.WeaponHandler.CurrentWeapon != null)
				m_Demo.WeaponHandler.CurrentWeapon.SetPivotVisible(true);
			m_Demo.Input.ForceCursor = true;
			m_Demo.WeaponHandler.SetWeaponLayer(vp_Layer.Weapon);
		}

		if (m_Demo.ShowGUI)
		{

			// show toggle column for the various pivot examples
			int currentSel = m_Demo.ButtonSelection;
			string[] strings = new string[] { "Muzzle", "Grip", "Chest", "Elbow (Uzi Style)" };
			m_Demo.ButtonSelection = m_Demo.ToggleColumn(200, 150, m_Demo.ButtonSelection, strings, true, true, m_ImageRightPointer, m_ImageLeftPointer);
			if (m_Demo.ButtonSelection != currentSel)
			{
				switch (m_Demo.ButtonSelection)
				{
					case 0:	// --- Muzzle ---
						m_Demo.SetWeaponPreset(PivotMuzzleWeapon, null, true);
						break;
					case 1:	// --- Grip ---
						m_Demo.SetWeaponPreset(PivotWristWeapon, null, true);
						break;
					case 2:	// --- Chest ---
						m_Demo.SetWeaponPreset(PivotChestWeapon, null, true);
						break;
					case 3:	// --- Elbow (Uzi Style) ---
						m_Demo.SetWeaponPreset(PivotElbowWeapon, null, true);
						break;
				}
				m_Demo.LastInputTime = Time.time;
			}

			m_Demo.DrawEditorPreview(m_ImageWeaponPivot, m_ImageEditorPreview, m_ImageEditorScreenshot);
		}

	}

	
	/// <summary>
	/// 
	/// </summary>
	void OnGUI()
	{

		m_Demo.OnGUI();

		// perform drawing method specific to the current demo screen
		switch (m_Demo.CurrentScreen)
		{
			case 1: DemoIntro(); break;
			case 2: DemoExamples(); break;
			case 3: DemoForces(); break;
			case 4: DemoMouseInput(); break;
			case 5: DemoWeaponPerspective(); break;
			case 6: DemoWeaponLayer(); break;
			case 7: DemoPivot(); break;
		}


	}


}

