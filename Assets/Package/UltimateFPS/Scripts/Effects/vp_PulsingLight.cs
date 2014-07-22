/////////////////////////////////////////////////////////////////////////////////
//
//	vp_PulsingLight.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a simple script for making a light flash in a sinus motion
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_PulsingLight : MonoBehaviour
{

	Light m_Light = null;

	public float m_MinIntensity = 2.0f;
	public float m_MaxIntensity = 5.0f;
	public float m_Rate = 1.0f;


	/// <summary>
	/// Caches the light.
	/// </summary>
	void Start ()
	{
		m_Light = light;
	}


	/// <summary>
	/// Flashes the light up and down by applying a sine wave to its intensity.
	/// </summary>
	void Update ()
	{

		if (m_Light == null)
			return;

		m_Light.intensity = m_MinIntensity + Mathf.Abs(Mathf.Cos((Time.time * m_Rate)) * (m_MaxIntensity - m_MinIntensity));

	}

}
