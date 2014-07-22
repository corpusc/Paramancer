/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleHUD.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a very primitive HUD displaying health, clips and ammo, along
//					with a soft red full screen flash for when taking damage
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class vp_SimpleHUD : MonoBehaviour
{

	public Texture DamageFlashTexture = null;
	public bool ShowHUD = true;
	Color m_MessageColor = new Color(2, 2, 0, 2);
	Color m_InvisibleColor = new Color(1, 1, 0, 0);
	Color m_DamageFlashColor = new Color(0.8f, 0, 0, 0);
	Color m_DamageFlashInvisibleColor = new Color(1, 0, 0, 0);
	string m_PickupMessage = "";
	protected static GUIStyle m_MessageStyle = null;
	public static GUIStyle MessageStyle
	{
		get
		{
			if (m_MessageStyle == null)
			{
				m_MessageStyle = new GUIStyle("Label");
				m_MessageStyle.alignment = TextAnchor.MiddleCenter;
			}
			return m_MessageStyle;
		}
	}

	private vp_FPPlayerEventHandler m_Player = null;


	/// <summary>
	///
	/// </summary>
	void Awake()
	{
		m_Player = transform.GetComponent<vp_FPPlayerEventHandler>();
	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		if (m_Player != null)
			m_Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{


		if (m_Player != null)
			m_Player.Unregister(this);

	}


	/// <summary>
	/// this draws a primitive HUD and also renders the current
	/// message, fading out in the middle of the screen
	/// </summary>
	protected virtual void OnGUI()
	{

		if (!ShowHUD)
			return;

		// display a simple 'Health' HUD
		GUI.Box(new Rect(10, Screen.height - 30, 100, 22), "Health: " + (int)(m_Player.Health.Get() * 100.0f) + "%");

		// display a simple 'Clips' HUD
		GUI.Box(new Rect(Screen.width - 220, Screen.height - 30, 100, 22), "Clips: " + m_Player.GetItemCount.Send("AmmoClip"));

		// display a simple 'Ammo' HUD
		GUI.Box(new Rect(Screen.width - 110, Screen.height - 30, 100, 22), "Ammo: " + m_Player.CurrentWeaponAmmoCount.Get());

		// show a message in the middle of the screen and fade it out
		if (!string.IsNullOrEmpty(m_PickupMessage) && m_MessageColor.a > 0.01f)
		{
			m_MessageColor = Color.Lerp(m_MessageColor, m_InvisibleColor, Time.deltaTime * 0.4f);
			GUI.color = m_MessageColor;
			GUI.Box(new Rect(200, 150, Screen.width - 400, Screen.height - 400), m_PickupMessage, MessageStyle);
			GUI.color = Color.white;
		}

		// show a red glow along the screen edges when damaged
		if (DamageFlashTexture != null && m_DamageFlashColor.a > 0.01f)
		{
			m_DamageFlashColor = Color.Lerp(m_DamageFlashColor, m_DamageFlashInvisibleColor, Time.deltaTime * 0.4f);
			GUI.color = m_DamageFlashColor;
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), DamageFlashTexture);
			GUI.color = Color.white;
		}

	}


	/// <summary>
	/// updates the HUD message text and makes it fully visible
	/// </summary>
	protected virtual void OnMessage_HUDText(string message)
	{

		m_MessageColor = Color.white;
		m_PickupMessage = (string)message;

	}


	/// <summary>
	/// shows or hides the HUD full screen flash 
	/// </summary>
	protected virtual void OnMessage_HUDDamageFlash(float intensity)
	{

		if (DamageFlashTexture == null)
			return;

		if (intensity == 0.0f)
			m_DamageFlashColor.a = 0.0f;
		else
			m_DamageFlashColor.a += intensity;

	}


}

