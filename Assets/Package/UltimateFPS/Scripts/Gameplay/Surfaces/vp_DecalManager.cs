/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DecalManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class handles fading out and removal of decals.
//					by default there can be 100 decals in the scene. as new
//					decals appear, older ones are faded out and removed.
//					see comment on the 'Add' method for more details.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public sealed class vp_DecalManager
{

	public static readonly vp_DecalManager instance = new vp_DecalManager();

	// list of all decals in the scene
	private static List<GameObject> m_Decals = new List<GameObject>();

	// exposed through properties
	private static float m_MaxDecals = 100;			// max amount of decals
	private static float m_FadedDecals = 20;		// amount of decals to fade out at the same time

	// internal
	private static float m_NonFadedDecals = 0.0f;
	private static float m_FadeAmount = 0.0f;
	
	public static float MaxDecals { get { return m_MaxDecals; } set { m_MaxDecals = value; Refresh(); } }
	public static float FadedDecals
	{
		get { return m_FadedDecals; }
		set
		{
			if (value > m_MaxDecals)
			{
				Debug.LogError("FadedDecals can't be larger than MaxDecals");
				return;
			}
			m_FadedDecals = value;
			Refresh();
		}
	}


	/// <summary>
	/// explicit static constructor to tell c# compiler
	/// not to mark type as beforefieldinit
	/// </summary>
	static vp_DecalManager()
	{
		Refresh();
	}
	private vp_DecalManager(){}


	/// <summary>
	/// adds a gameobject to the decal manager, making it subject
	/// to later removal and deletion. NOTE: all items added to
	/// the decal manager should have a material with initial
	/// alpha set to 1.0
	/// </summary>
	public static void Add(GameObject decal)
	{

		m_Decals.Add(decal);
		FadeAndRemove();
		//DebugOutput();

	}


	/// <summary>
	/// this method fades out a set number of the oldest decals,
	/// and destroys decals that are no longer needed
	/// </summary>
	private static void FadeAndRemove()
	{

		if (m_Decals.Count > m_NonFadedDecals)
		{

			// loop a predetermined amount of the oldest decals
			for (int v = 0; v < (m_Decals.Count - m_NonFadedDecals); v++)
			{
				// and fade them out a tiny bit. older decals will
				// accumulate more and more transparency over time
				if (m_Decals[v] != null)
				{
					Color col = m_Decals[v].renderer.material.color;
					col.a -= m_FadeAmount;
					m_Decals[v].renderer.material.color = col;
				}
			}

		}
		
		// kill the oldest decal as it becomes fully transparent
		if (m_Decals[0] != null)
		{
			if (m_Decals[0].renderer.material.color.a <= 0.0f)
			{
				Object.Destroy(m_Decals[0]);
				m_Decals.Remove(m_Decals[0]);
			}
		}
		else
			m_Decals.RemoveAt(0);

	}


	/// <summary>
	/// calculates the variables used internally from the ones
	/// exposed through properties
	/// </summary>
	private static void Refresh()
	{

		if (m_MaxDecals < m_FadedDecals)
			m_MaxDecals = m_FadedDecals;
		m_FadeAmount = (m_MaxDecals / m_FadedDecals) / m_MaxDecals;
		m_NonFadedDecals = m_MaxDecals - m_FadedDecals;

	}


	/// <summary>
	/// use this method to print some info on the current states
	/// and amounts of decals to the console
	/// </summary>
	private static void DebugOutput()
	{

		int fullyVisibleDecals = 0;
		int partlyFadedDecals = 0;
		foreach (GameObject o in m_Decals)
		{
			if (o.renderer.material.color.a == 1)
				fullyVisibleDecals++;
			else
				partlyFadedDecals++;
		}
		Debug.Log("Decal count: " + m_Decals.Count + ", Full: " + fullyVisibleDecals + ", Faded: " + partlyFadedDecals);

	}


}

