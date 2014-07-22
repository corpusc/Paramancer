/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemo2.cs
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
using System;
using System.Collections.Generic;

public class vp_FPSDemo2 : MonoBehaviour
{

	public GameObject Player = null;
	private vp_FPSDemoManager m_Demo = null;
	private vp_FPCamera m_FPSCamera = null;

	// GUI
	public Texture ImageLeftArrow = null;
	public Texture ImageRightArrow = null;
	public Texture ImageRightPointer = null;
	public Texture ImageLeftPointer = null;
	public Texture ImageCheckmark = null;
	public Texture ImagePresetDialogs = null;
	public Texture ImageShooter = null;
	public Texture ImageAllParams = null;
	private int m_ExamplesCurrentSel = 0;
	private float m_GoAgainButtonAlpha = 0.0f;
	private float m_WASDInfoClickTime = 0.0f;

	// positions
	private Vector3 m_SlimePos = new Vector3(115.3f, 113.3f, -94.5f);
	private Vector3 m_WetRoofPos = new Vector3(115.3f, 113.3f, -86.5f);
	private Vector3 m_FallDeflectPos = new Vector3(106.6f, 116.8f, -97.1f);
	private Vector3 m_BlownAwayPos = new Vector3(132.0f, 122.18f, -100.6f);
	private Vector3 m_ActionPos = new Vector3(127, 122.18f, -97.6f);
	private Vector3 m_HeadBumpPos = new Vector3(106.4f, 102.4f, -99.89f);
	private Vector3 m_WallBouncePos = new Vector3(114.2f, 104.6f, -91.9f);
	private Vector3 m_ExplorePos = new Vector3(134.0023f, 107.64261f, -109.5f);
	
	private Vector3 m_OverViewPos = new Vector3(135.0f, 105.8f, -70.7f);
	private Vector3 m_OutroPos = new Vector3(135.0f, 205.8f, -70.7f);
	private Vector2 m_OutroAngle = new Vector2(-19.3f, 241.7f);


	// angles
	private Vector2 m_SlimeAngle = new Vector2(0, 180);
	private Vector2 m_WetRoofAngle = new Vector2(30, 230);
	private Vector2 m_FallDeflectAngle = new Vector2(25, 180);
	private Vector2 m_BlownAwayAngle = new Vector2(0, -90);
	private Vector2 m_ActionAngle = new Vector2(0, 180);
	private Vector2 m_HeadBumpAngle = new Vector2(0, 180);
	private Vector2 m_WallBounceAngle = new Vector2(0, 130);
	private Vector2 m_ExploreAngle = new Vector2(30, 40);
	private Vector2 m_OverViewAngle = new Vector2(-16.5f, 215);

	// timers
	vp_Timer.Handle m_ForceTimer = new vp_Timer.Handle();
	vp_Timer.Handle m_GoAgainTimer = new vp_Timer.Handle();
	vp_Timer.Handle m_HeadBumpTimer1 = new vp_Timer.Handle();
	vp_Timer.Handle m_HeadBumpTimer2 = new vp_Timer.Handle();
	vp_Timer.Handle m_ActionTimer1 = new vp_Timer.Handle();
	vp_Timer.Handle m_ActionTimer2 = new vp_Timer.Handle();
	vp_Timer.Handle m_ActionTimer3 = new vp_Timer.Handle();
	vp_Timer.Handle m_ActionTimer4 = new vp_Timer.Handle();
	vp_Timer.Handle m_ActionTimer5 = new vp_Timer.Handle();
	private float m_OutroStartTime = 0.0f;

	// 'forced control' variables
	bool m_RunForward = false;
	bool m_Jump = false;
	int m_LookPoint = 0;
	private Vector3[] m_LookPoints = new Vector3[9];

	// sounds
	public AudioClip m_ExplosionSound = null;
	public List<AudioClip> FallImpactSounds = null;

	// fx
	public GameObject m_ExplosionFX = null;


	/// <summary>
	/// 
	/// </summary>
	void Start()
	{

		m_FPSCamera = (vp_FPCamera)Component.FindObjectOfType(typeof(vp_FPCamera));

		m_Demo = new vp_FPSDemoManager(Player);
		m_Demo.CurrentFullScreenFadeTime = Time.time;
		m_Demo.DrawCrosshair = false;

		m_Demo.Input.MouseCursorZones = new Rect[3];
		m_Demo.Input.MouseCursorZones[0] = new Rect((Screen.width * 0.5f) - 370, 40, 80, 80);
		m_Demo.Input.MouseCursorZones[1] = new Rect((Screen.width * 0.5f) + 290, 40, 80, 80);
		m_Demo.Input.MouseCursorZones[2] = new Rect(0, 0, 150, Screen.height);
		Screen.lockCursor = false;

		m_LookPoints[1] = new Vector3(129.3f, 122, -186);
		m_LookPoints[2] = new Vector3(129.3f, 85, -186);
		m_LookPoints[3] = new Vector3(147, 85, -186);
		m_LookPoints[4] = new Vector3(12, 85, -214);
		m_LookPoints[5] = new Vector3(129, 122, -118);
		m_LookPoints[6] = new Vector3(125.175f, 106.1071f, -97.58212f);
		m_LookPoints[7] = new Vector3(119.6f, 104.2f, -89.1f);
		m_LookPoints[8] = new Vector3(129, 112, -150);


		// prevent regular event handler weapon switching events
		m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000);
		m_Demo.PlayerEventHandler.SetPrevWeapon.Try = delegate() { return false; };
		m_Demo.PlayerEventHandler.SetNextWeapon.Try = delegate() { return false; };

		// manually register our fallimpact method with the player event handler
		// NOTE: this is not the typical event handler workflow. see the manual
		// event handler chapter for more info
		m_Demo.PlayerEventHandler.FallImpact.Register(this, "FallImpact", 0);

	}


	/// <summary>
	/// 
	/// </summary>
	void Update()
	{

		m_Demo.Update();


	}


	/// <summary>
	/// 
	/// </summary>
	void DemoPhysics()
	{

		m_Demo.DrawBoxes("part iii: physics", "Ultimate FPS features a cool, tweakable MOTOR and PHYSICS simulation.\nAll motion is forwarded to the camera and weapon for some CRAZY MOVES that you won't see in an everyday FPS. Click these buttons for some quick examples ...", null, ImageRightArrow);
		if (m_Demo.FirstFrame)
		{
			m_Demo.DrawCrosshair = true;
			m_Demo.Teleport(m_SlimePos, m_SlimeAngle);						// place the player at start point
			m_Demo.FirstFrame = false;
			m_Demo.ButtonSelection = 0;
			m_Demo.Camera.SnapSprings();
			m_Demo.RefreshDefaultState();
			m_Demo.Input.ForceCursor = true;
			m_Demo.Teleport(m_SlimePos, m_SlimeAngle);
			m_Demo.WeaponHandler.SetWeapon(1);
			m_ExamplesCurrentSel = -1;
			m_RunForward = false;
			m_LookPoint = 0;
			m_Demo.LockControls();
		}

		// 'go again' button
		if (m_Demo.ShowGUI && !m_GoAgainTimer.Active && m_Demo.ButtonSelection != 3)
		{
			GUI.color = new Color(1, 1, 1, m_GoAgainButtonAlpha);
			m_GoAgainButtonAlpha = Mathf.Lerp(0, 1, m_GoAgainButtonAlpha + (Time.deltaTime));
			if (GUI.Button(new Rect((Screen.width / 2) - 60, 210, 120, 30), "Go again!"))
			{
				m_GoAgainButtonAlpha = 0.0f;
				m_ExamplesCurrentSel = -1;
			}
			GUI.color = new Color(1, 1, 1, 1 * m_Demo.GlobalAlpha);
		}

		// toggle examples using the buttons
		if (m_Demo.ButtonSelection != m_ExamplesCurrentSel)
		{

			m_WASDInfoClickTime = Time.time;
			m_Demo.Controller.Stop();
			m_Jump = false;
			m_LookPoint = 0;
			m_GoAgainButtonAlpha = 0.0f;
			m_ForceTimer.Cancel();
			m_Demo.Controller.Stop();
			m_Demo.PlayerEventHandler.RefreshActivityStates();
			m_Demo.Input.ForceCursor = true;

			// reset some values to default on the controller 
			m_Demo.Controller.PhysicsSlopeSlidiness = 0.15f;
			m_Demo.Controller.MotorAirSpeed = 0.7f;
			m_Demo.Controller.MotorAcceleration = 0.18f;
			m_Demo.Controller.PhysicsWallBounce = 0.0f;

			// cancel all example timers
			m_HeadBumpTimer1.Cancel();
			m_HeadBumpTimer2.Cancel();
			m_ActionTimer1.Cancel();
			m_ActionTimer2.Cancel();
			m_ActionTimer3.Cancel();
			m_ActionTimer4.Cancel();
			m_ActionTimer5.Cancel();
			m_GoAgainTimer.Cancel();
			m_ForceTimer.Cancel();

			Screen.lockCursor = true;

			m_Demo.Camera.SnapSprings();

			if (m_Demo.WeaponHandler.CurrentWeapon != null)
				m_Demo.Camera.SnapSprings();

			m_Demo.PlayerEventHandler.Platform.Set(null);

			switch (m_Demo.ButtonSelection)
			{
				case 0:
					vp_Timer.In(29, delegate() { }, m_GoAgainTimer);
					m_Demo.Teleport(m_SlimePos, m_SlimeAngle);
					break;
				case 1:
					vp_Timer.In(5, delegate()
					{
					}, m_GoAgainTimer);
					m_Demo.Teleport(m_WetRoofPos, m_WetRoofAngle);
					m_Demo.Controller.PhysicsSlopeSlidiness = 1.0f;
					break;
				case 2:
					m_Demo.Controller.MotorAirSpeed = 0.0f;
					m_Demo.Teleport(m_ActionPos, m_ActionAngle);
					m_Demo.SnapLookAt(m_LookPoints[1]);
					m_LookPoint = 1;
					m_RunForward = true;
					vp_Timer.In(1.75f, delegate() { m_LookPoint = 2; m_Jump = true; m_Demo.LookDamping = 1.0f; }, m_ActionTimer1);
					vp_Timer.In(2.25f, delegate() { m_LookPoint = 3; m_Jump = false; m_Demo.LookDamping = 1.0f; }, m_ActionTimer2);
					vp_Timer.In(3.5f, delegate() { m_LookPoint = 4; m_Demo.Controller.MotorAcceleration = 0.0f; m_Demo.LookDamping = 3.0f; }, m_ActionTimer3);
					vp_Timer.In(5.0f, delegate() { m_LookPoint = 5; m_RunForward = false; m_Demo.Controller.MotorAcceleration = 0.18f; m_Demo.LookDamping = 1.0f; }, m_ActionTimer4);
					vp_Timer.In(9.0f, delegate() { m_LookPoint = 8; }, m_ActionTimer5);
					vp_Timer.In(11, delegate() { }, m_GoAgainTimer);
					break;
				case 4:
					m_Demo.Teleport(m_HeadBumpPos, m_HeadBumpAngle);
					vp_Timer.In(1.0f, delegate() { m_Jump = true; }, m_HeadBumpTimer1);
					vp_Timer.In(1.25f, delegate() { m_Jump = false; }, m_HeadBumpTimer2);
					vp_Timer.In(2, delegate() { }, m_GoAgainTimer);
					break;
				case 5:
					m_Demo.Teleport(m_WallBouncePos, m_WallBounceAngle);
					m_LookPoint = 6;
					m_Demo.LookDamping = 0;
					vp_Timer.In(1, delegate()
					{
						m_LookPoint = 7;
						m_Demo.LookDamping = 3;
						m_Demo.Controller.PhysicsWallBounce = 0.0f;
						GameObject.Instantiate(m_ExplosionFX, m_Demo.Controller.transform.position + new Vector3(3, 0, 0), Quaternion.identity);
						m_Demo.PlayerEventHandler.BombShake.Send(0.3f);
						m_Demo.Controller.AddForce(Vector3.right * 3.0f);
						if (m_Demo.WeaponHandler.CurrentWeapon != null)
							m_Demo.WeaponHandler.CurrentWeapon.audio.PlayOneShot(m_ExplosionSound);
					}, m_ForceTimer);
					vp_Timer.In(5, delegate()
					{
						m_Demo.Controller.PhysicsWallBounce = 0.0f;
						m_LookPoint = 6;
						m_Demo.LookDamping = 0.5f;
						m_Demo.Teleport(m_WallBouncePos, m_WallBounceAngle);
					}, m_GoAgainTimer);
					break;
				case 6:
					vp_Timer.In(5, delegate() { }, m_GoAgainTimer);
					m_Demo.Teleport(m_FallDeflectPos, m_FallDeflectAngle);
					break;
				case 7:
					vp_Timer.In(7, delegate() { }, m_GoAgainTimer);
					vp_Timer.In(1, delegate()
					{
						GameObject.Instantiate(m_ExplosionFX, m_Demo.Controller.transform.position + new Vector3(-3, 0, 0), Quaternion.identity);
						m_Demo.PlayerEventHandler.BombShake.Send(0.5f);
						m_Demo.Controller.AddForce(Vector3.forward * 0.55f);
						if (m_Demo.WeaponHandler.CurrentWeapon != null)
							m_Demo.WeaponHandler.CurrentWeapon.audio.PlayOneShot(m_ExplosionSound);
					}, m_ForceTimer);
					m_Demo.Teleport(m_BlownAwayPos, m_BlownAwayAngle);
					break;
				case 3:
					m_Demo.Input.ForceCursor = false;
					m_Demo.WeaponHandler.SetWeapon(2);
					m_Demo.Teleport(m_ExplorePos, m_ExploreAngle);
					m_Demo.Input.AllowGameplayInput = true;
					break;

			}

			m_Demo.LastInputTime = Time.time;
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;

		}

		// some special cases to be run every frame

		if (m_Demo.ButtonSelection != 2 && m_Demo.ButtonSelection != 3)
		{
			m_Demo.LockControls();
			m_Demo.Input.AllowGameplayInput = false;
		}
		else if (m_Demo.ButtonSelection != 3)
		{
			m_Demo.LockControls();
			m_Demo.Input.AllowGameplayInput = false;
		}

		if (m_Demo.ButtonSelection != 3 && m_Demo.WeaponHandler.CurrentWeaponID != 1)
			m_Demo.WeaponHandler.SetWeapon(1);

		switch (m_Demo.ButtonSelection)
		{
			case 0:
				// force this camera angle since it has a tendency to get
				// altered by buffered mouse input during loading
				m_Demo.Camera.Angle = m_SlimeAngle;
				break;
			case 2:
				Vector2 input = m_Demo.PlayerEventHandler.InputMoveVector.Get();
				input.y = m_RunForward ? 1.0f : 0.0f;
				m_Demo.PlayerEventHandler.InputMoveVector.Set(input);
				m_Demo.PlayerEventHandler.Jump.Active = m_Jump;
				if (m_Demo.Controller.StateEnabled("Run") != m_RunForward)
					m_Demo.Controller.SetState("Run", m_RunForward, true);
				m_Demo.SmoothLookAt(m_LookPoints[m_LookPoint]);
				break;
			case 4:
				m_Demo.PlayerEventHandler.Jump.Active = m_Jump;
				break;
			case 5:
				m_Demo.SmoothLookAt(m_LookPoints[m_LookPoint]);
				break;
			case 3:
				// draw menu re-enable text
				if (m_Demo.ShowGUI && Screen.lockCursor)
				{
					GUI.color = new Color(1, 1, 1, ((m_Demo.ClosingDown) ? m_Demo.GlobalAlpha : 1.0f));
					GUI.Label(new Rect((Screen.width / 2) - 200, 140, 400, 20), "(Press ENTER to reenable menu)", m_Demo.CenterStyle);
					GUI.color = new Color(0, 0, 0, 1 * (1.0f- ((Time.time - m_WASDInfoClickTime) * 0.05f)));
					GUI.Label(new Rect((Screen.width / 2) - 200, 170, 400, 20), "(Use WASD to move around freely)", m_Demo.CenterStyle);
					GUI.color = new Color(1, 1, 1, 1 * m_Demo.GlobalAlpha);
				}
				break;
		}

		// show a toggle column, a compound control displaying a
		// column of buttons that can be toggled like radio buttons
		if (m_Demo.ShowGUI)
		{
			m_ExamplesCurrentSel = m_Demo.ButtonSelection;
			string[] strings = new string[] { "Mud... or Slime", "Wet Roof", "Action Hero", "Moving Platforms", "Head Bumps", "Wall Deflection", "Fall Deflection", "Blown Away" }; m_Demo.ButtonSelection = m_Demo.ToggleColumn(140, 150, m_Demo.ButtonSelection, strings, false, true, ImageRightPointer, ImageLeftPointer);
		}


	}


	/// <summary>
	/// demo screen to explain preset loading and saving features
	/// </summary>
	private void DemoPresets()
	{


		if (m_Demo.FirstFrame)
		{
			m_GoAgainTimer.Cancel();
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverViewPos, m_OverViewAngle, true);
			m_Demo.WeaponHandler.CancelTimers();
			m_Demo.WeaponHandler.SetWeapon(0);
			m_Demo.Input.MouseCursorZones[0] = new Rect((Screen.width * 0.5f) - 370, 40, 80, 80);
			m_Demo.Input.MouseCursorZones[1] = new Rect((Screen.width * 0.5f) + 290, 40, 80, 80);
			m_Demo.Input.ForceCursor = true;
		}

		m_Demo.DrawBoxes("states & presets", "You may easily design custom movement STATES (like running, crouching or proning).\nWhen happy with your tweaks, save them to PRESET FILES, and the STATE MANAGER\nwill blend smoothly between them at runtime.", ImageLeftArrow, ImageRightArrow, delegate() { m_Demo.LoadLevel(0); });
		m_Demo.DrawImage(ImagePresetDialogs);

		m_Demo.ForceCameraShake();

	}


	/// <summary>
	/// demo screen to show a final summary message and editor screenshot
	/// </summary>
	private void DemoOutro()
	{

		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OutroPos, m_OutroAngle, true);
			m_Demo.Input.ForceCursor = true;
			m_OutroStartTime = Time.time;
			m_Demo.PlayerEventHandler.Platform.Set(null);
		}

		// pan camera slowly for the clouds to move a little in the outro screen
		m_FPSCamera.Angle = new Vector2(
			m_OutroAngle.x,
			m_OutroAngle.y + (Mathf.Cos(((Time.time - m_OutroStartTime) + 50) * 0.03f) * 20)
			);

		m_Demo.DrawBoxes("WHAT YOU GET", "• An in-depth 100+ page MANUAL with many tutorials to get you started EASILY.\n• Tons of scripts, art & sound FX. • Full, well commented C# SOURCE CODE.\n• A FANTASTIC starting point (or upgrade) for any FPS project.\nBest part? It can be yours in a minute! GET IT NOW on visionpunk.com ...", ImageLeftArrow, ImageCheckmark, delegate() { m_Demo.LoadLevel(0); });
		m_Demo.DrawImage(ImageAllParams);

	}


	/// <summary>
	/// 
	/// </summary>
	void FallImpact(float f)
	{

		if (f > 0.2f)
			vp_Utility.PlayRandomSound(Player.audio, FallImpactSounds);

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
			case 1:
				DemoPhysics();
				break;
			case 2:
				DemoOutro();
				break;
		}

	}


}

