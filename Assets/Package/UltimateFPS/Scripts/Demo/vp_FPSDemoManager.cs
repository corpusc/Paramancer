/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSDemoManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	utility class for Ultimate FPS demo scripts
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_FPSDemoManager : vp_DemoManager
{

	public GameObject Player = null;
	public vp_FPController Controller = null;
	public vp_FPCamera Camera = null;
	public vp_FPWeaponHandler WeaponHandler = null;
	public vp_FPInput Input = null;
	public vp_Earthquake Earthquake = null;

	public vp_FPPlayerEventHandler PlayerEventHandler = null;

	private Vector3 m_UnFreezePosition = Vector3.zero;

	// smooth lookat
	private Vector3 m_CurrentLookPoint = Vector3.zero;
	private Vector3 m_LookVelocity = Vector3.zero;
	public float LookDamping = 0.3f;

	vp_Shooter m_CurrentShooter = null;
	public vp_Shooter CurrentShooter
	{
		get
		{
			if ((m_CurrentShooter == null) ||
				((m_CurrentShooter != null) && ((!m_CurrentShooter.enabled) || (!vp_Utility.IsActive(m_CurrentShooter.gameObject)))))
			{
				m_CurrentShooter = Player.GetComponentInChildren<vp_Shooter>();
			}
			return m_CurrentShooter;
		}
	}


	public bool DrawCrosshair
	{
		get
		{
			vp_SimpleCrosshair crosshair = (vp_SimpleCrosshair)Player.GetComponent(typeof(vp_SimpleCrosshair));
			if (crosshair == null)
				return false;
			return crosshair.enabled;
		}
		set
		{
			vp_SimpleCrosshair crosshair = (vp_SimpleCrosshair)Player.GetComponent(typeof(vp_SimpleCrosshair));
			if (crosshair != null)
				crosshair.enabled = value;
		}
	}


	/// </summary>
	///
	/// </summary>
	public vp_FPSDemoManager(GameObject player)
	{

		Player = player;

		Controller = Player.GetComponent<vp_FPController>();
		Camera = Player.GetComponentInChildren<vp_FPCamera>();
		WeaponHandler = Player.GetComponentInChildren<vp_FPWeaponHandler>();
		PlayerEventHandler = (vp_FPPlayerEventHandler)Player.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));
		Input = Player.GetComponent<vp_FPInput>();
		Earthquake = (vp_Earthquake)Component.FindObjectOfType(typeof(vp_Earthquake));

		// on small screen resolutions the editor preview screenshot
		// panel is minimized by default, otherwise expanded
		if (Screen.width < 1024)
			EditorPreviewSectionExpanded = false;

	}


	/// <summary>
	/// snaps the controller position and camera angle to a certain
	/// coordinate and yaw/pitch, respectively
	/// </summary>
	public void Teleport(Vector3 pos, Vector2 startAngle)
	{

		Controller.SetPosition(pos);
		Camera.SetRotation(startAngle);

	}


	/// <summary>
	/// if called every frame, smoothly force rotates the camera
	/// to look at the given lookpoint. set LookDamping to regulate
	/// transition speed
	/// </summary>
	public void SmoothLookAt(Vector3 lookPoint)
	{

		m_CurrentLookPoint = Vector3.SmoothDamp(m_CurrentLookPoint, lookPoint, ref m_LookVelocity, LookDamping);
		Camera.transform.LookAt(m_CurrentLookPoint);
		Camera.Angle = new Vector2(Camera.transform.eulerAngles.x, Camera.transform.eulerAngles.y);

	}


	/// <summary>
	/// if called every frame, fixes the camera angle at the
	/// given lookpoint
	/// </summary>
	public void SnapLookAt(Vector3 lookPoint)
	{

		m_CurrentLookPoint = lookPoint;
		Camera.transform.LookAt(m_CurrentLookPoint);
		Camera.Angle = new Vector2(Camera.transform.eulerAngles.x, Camera.transform.eulerAngles.y);

	}


	/// <summary>
	/// moves the controller to a coordinate in one frame and
	/// freezes movement and optionally camera input
	/// </summary>
	public void FreezePlayer(Vector3 pos, Vector2 startAngle, bool freezeCamera)
	{

		m_UnFreezePosition = Controller.transform.position;
		Teleport(pos, startAngle);

		Controller.SetState("Freeze", true);
		Controller.Stop();

		if (freezeCamera)
			Camera.SetState("Freeze", true);

	}

	public void FreezePlayer(Vector3 pos, Vector2 startAngle)
	{
		FreezePlayer(pos, startAngle, false);
	}


	/// <summary>
	/// unfreezes and restores a frozen player to the position
	/// it had before getting frozen
	/// </summary>
	public void UnFreezePlayer()
	{

		Controller.transform.position = m_UnFreezePosition;
		m_UnFreezePosition = Vector3.zero;

		Controller.SetState("Freeze", false);
		Camera.SetState("Freeze", false);

	}


	/// <summary>
	/// 
	/// </summary>
	public void LockControls()
	{

		Input.AllowGameplayInput = false;

		Camera.MouseSensitivity = Vector2.zero;

		if (WeaponHandler.CurrentWeapon != null)
			WeaponHandler.CurrentWeapon.RotationLookSway = Vector2.zero;

	}


	/// <summary>
	/// sets a new default weapon preset. an optional shooter
	/// preset may be provided. if the optional 'smoothFade' bool
	/// is set to false, the new preset will snap in place.
	/// NOTE: this is just demo walkthrough code. manipulating
	/// the default state like this is no longer recommended.
	/// use additional states instead.
	/// </summary>
	public void SetWeaponPreset(TextAsset weaponPreset, TextAsset shooterPreset = null, bool smoothFade = true)
	{

		if (WeaponHandler.CurrentWeapon == null)
			return;

		WeaponHandler.CurrentWeapon.Load(weaponPreset);

		if (!smoothFade)
		{
			WeaponHandler.CurrentWeapon.SnapSprings();
			WeaponHandler.CurrentWeapon.SnapPivot();
			WeaponHandler.CurrentWeapon.SnapZoom();
		}

		WeaponHandler.CurrentWeapon.Refresh();

		if (shooterPreset != null)
		{
			if (CurrentShooter != null)
				CurrentShooter.Load(shooterPreset);
		}
		CurrentShooter.Refresh();

	}


	/// <summary>
	/// helper method to refresh all default states. for use
	/// after applying a preset to a vp_Component via script
	/// </summary>
	public void RefreshDefaultState()
	{

		if (Controller != null)
			Controller.RefreshDefaultState();
		if (Camera != null)
		{
			Camera.RefreshDefaultState();
			if (WeaponHandler.CurrentWeapon != null)
				WeaponHandler.CurrentWeapon.RefreshDefaultState();
			if (CurrentShooter != null)
				CurrentShooter.RefreshDefaultState();
		}

	}


	/// <summary>
	/// helper method to reset all states. for use after applying
	/// a preset to a vp_Component via script
	/// </summary>
	public void ResetState()
	{

		if (Controller != null)
			Controller.ResetState();
		if (Camera != null)
		{
			Camera.ResetState();
			if (WeaponHandler.CurrentWeapon != null)
				WeaponHandler.CurrentWeapon.ResetState();
			if (CurrentShooter != null)
				CurrentShooter.ResetState();
		}

	}


	/// <summary>
	/// various camera and weapon resets
	/// </summary>
	protected override void Reset()
	{

		base.Reset();

		PlayerEventHandler.RefreshActivityStates();
		WeaponHandler.SetWeapon(0);

		PlayerEventHandler.Earthquake.Stop();
		Camera.BobStepCallback = null;
		Camera.SnapSprings();
		if (WeaponHandler.CurrentWeapon != null)
		{
			WeaponHandler.CurrentWeapon.SetPivotVisible(false);
			WeaponHandler.CurrentWeapon.SnapSprings();
			vp_Layer.Set(WeaponHandler.CurrentWeapon.gameObject, vp_Layer.Weapon, true);

		}

		if (Screen.width < 1024)
			EditorPreviewSectionExpanded = false;
		else
			EditorPreviewSectionExpanded = true;

		if (m_UnFreezePosition != Vector3.zero)
			UnFreezePlayer();

	}


	/// <summary>
	/// forces a subtle camera shake on the camera
	/// </summary>
	public void ForceCameraShake(float speed, Vector3 amplitude)
	{
		Camera.ShakeSpeed = speed;
		Camera.ShakeAmplitude = amplitude;
	}


	/// <summary>
	/// forces a subtle camera shake on the camera
	/// </summary>
	public void ForceCameraShake()
	{
		ForceCameraShake(0.0727273f, new Vector3(-10, 10, 0));
	}


}

