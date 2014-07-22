/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Earthquake.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	generates procedural noise for purposes of shaking the camera
//					in response to earthquakes and bombs. this class is currently
//					under construction and will change in upcoming releases.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_Earthquake : MonoBehaviour
{

	protected Vector3 m_EarthQuakeForce = new Vector3();

	protected float m_Endtime = 0.0f;
	protected Vector2 m_Magnitude = Vector2.zero;

	protected vp_EventHandler EventHandler = null;

	// event handler property cast as a playereventhandler
	vp_FPPlayerEventHandler m_Player = null;
	vp_FPPlayerEventHandler Player
	{
		get
		{
			if (m_Player == null)
			{
				if (EventHandler != null)
					m_Player = (vp_FPPlayerEventHandler)EventHandler;
			}
			return m_Player;
		}
	}


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
		EventHandler = (vp_EventHandler)FindObjectOfType(typeof(vp_EventHandler));
	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		if (EventHandler != null)
			EventHandler.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{


		if (EventHandler != null)
			EventHandler.Unregister(this);

	}


	/// <summary>
	/// 
	/// </summary>
	protected void FixedUpdate()
	{

		if (Time.timeScale != 0.0f)
			UpdateEarthQuake();

	}

	
	/// <summary>
	/// 
	/// </summary>
	protected void UpdateEarthQuake()
	{

		if (!Player.Earthquake.Active)
		{
			m_EarthQuakeForce = Vector3.zero;
			return;
		}

		// the horizontal move is a perlin noise value between 0 and
		// 'm_EarthQuakeMagnitude' (depending on 'm_EarthQuakeTime').
		// horizMove will ease out during the last second.
		m_EarthQuakeForce = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(1),
							m_Magnitude.x * (Vector3.right + Vector3.forward) * Mathf.Min(m_Endtime - Time.time, 1.0f)
							* Time.timeScale);

		// vertMove will ease out during the last second.
		m_EarthQuakeForce.y = 0;
		if (UnityEngine.Random.value < (0.3f * Time.timeScale))
			m_EarthQuakeForce.y = UnityEngine.Random.Range(0, (m_Magnitude.y * 0.35f)) * Mathf.Min(m_Endtime - Time.time, 1.0f);

	}
	

	/// <summary>
	/// this callback is triggered right after the activity in question
	/// has been approved for activation
	/// </summary>
	protected virtual void OnStart_Earthquake()
	{

		Vector3 arg = (Vector3)Player.Earthquake.Argument;

		m_Magnitude.x = arg.x;
		m_Magnitude.y = arg.y;

		m_Endtime = Time.time + arg.z;

		Player.Earthquake.AutoDuration = arg.z;

	}


	/// <summary>
	/// makes the ground shake as if a bomb has gone off nearby
	/// </summary>
	protected virtual void OnMessage_BombShake(float impact)
	{

		Player.Earthquake.TryStart(new Vector3(impact * 0.5f, impact * 0.5f, 1.0f));

	}


	/// <summary>
	/// gets or sets the current earthquake force
	/// </summary>
	protected virtual Vector3 OnValue_EarthQuakeForce
	{
		get
		{
			return m_EarthQuakeForce;
		}
		set
		{
			m_EarthQuakeForce = value;
		}
	}


}

	