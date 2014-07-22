/////////////////////////////////////////////////////////////////////////////////
//
//	vp_TimeUtility.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	vp_TimeUtility contains static utility methods for performing
//					common time related game programming tasks
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;


public static class vp_TimeUtility
{

	private static float m_MinTimeScale = 0.1f;
	private static float m_MaxTimeScale = 1.0f;

	private static bool m_Paused = false;
	private static float m_TimeScaleOnPause = 1.0f;


	/// <summary>
	/// backs up fixed timestep on startup (used by the 'TimeScale' property)
	/// </summary>
	public static float InitialFixedTimeStep = Time.fixedDeltaTime;


	/// <summary>
	/// helper property to set the global timescale and adjust the
	/// fixed timestep accordingly. for use with slow motion effects,
	/// to get proper physics
	/// </summary>
	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{

			value = ClampTimeScale(value);

			Time.timeScale = value;
			Time.fixedDeltaTime = (InitialFixedTimeStep * Time.timeScale);

		}
	}


	/// <summary>
	/// Interpolates the time scale to a new value over time. A typical
	/// value for 'fadeSpeed' is '0.2f' (a value of '1' will be instant).
	/// PLEASE NOTE: This method must be run every frame inside an Update
	/// method. It will have virtually no effect if run a single time.
	/// </summary>
	public static void FadeTimeScale(float targetTimeScale, float fadeSpeed)
	{

		if (TimeScale == targetTimeScale)
			return;

		targetTimeScale = ClampTimeScale(targetTimeScale);

		TimeScale = Mathf.Lerp(TimeScale, targetTimeScale, (Time.deltaTime * 60.0f) * fadeSpeed);
		if (Mathf.Abs(TimeScale - targetTimeScale) < 0.01f)
			TimeScale = targetTimeScale;

	}


	/// <summary>
	/// Clamps timescale between min and max timescale.
	/// </summary>
	private static float ClampTimeScale(float t)
	{

		if ((t < m_MinTimeScale) || (t > m_MaxTimeScale))
		{
			t = Mathf.Clamp(t, m_MinTimeScale, m_MaxTimeScale);
			Debug.LogWarning("Warning: (vp_TimeUtility) TimeScale was clamped to within the supported range " + "(" + m_MinTimeScale + " - " + m_MaxTimeScale + ").");
		}

		return t;

	}


	/// <summary>
	/// Sets or gets the application pause state by manipulating
	/// the timescale.
	/// </summary>
	public static bool Paused
	{

		get
		{
			return m_Paused;
		}

		set
		{
			if (value == true)
			{
				if (m_Paused)
					return;
				m_Paused = true;
				m_TimeScaleOnPause = Time.timeScale;
				Time.timeScale = 0.0f;
			}
			else
			{
				if (!m_Paused)
					return;
				m_Paused = false;
				Time.timeScale = m_TimeScaleOnPause;
				m_TimeScaleOnPause = 1.0f;
			}
		}

	}

}
