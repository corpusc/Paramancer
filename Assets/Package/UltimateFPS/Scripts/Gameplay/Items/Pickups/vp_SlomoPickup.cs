/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SlomoPickup.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a pickup script which enables slow motion for 'RespawnDuration'
//					seconds
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_SlomoPickup : vp_Pickup
{

	vp_FPPlayerEventHandler m_Player = null;


	/// <summary>
	///
	/// </summary>
	protected override void Update()
	{

		// handle pickup rotation and bob, if enabled
		UpdateMotion();
		
		// handle time fading, and remove pickup if depleted
		if (m_Depleted)
		{

			if ((m_Player != null) && (m_Player.Dead.Active) && (!m_RespawnTimer.Active))
			{
				Respawn();
				return;
			}

			// this is where the magic happens.
			// NOTE: the supported timescale range is 0.1 - 1.0

			// don't remove pickup until time is done fading
			if (Time.timeScale > 0.2f && !vp_TimeUtility.Paused)		
				vp_TimeUtility.FadeTimeScale(0.2f, 0.1f);
			else if (!m_Audio.isPlaying)
				Remove();
		}
		// fade time back in when pickup respawns
		else if ((Time.timeScale < 1.0f) && !vp_TimeUtility.Paused)		
			vp_TimeUtility.FadeTimeScale(1.0f, 0.05f);

	}


	/// <summary>
	/// determines if this pickup is allowed to start slow motion.
	/// NOTE: actual slomo implementation occurs in Update
	/// </summary>
	protected override bool TryGive(vp_FPPlayerEventHandler player)
	{

		m_Player = player;

		// prevent the player from picking up the item again until any
		// currently running slomo timer has run its course
		if (m_Depleted || Time.timeScale != 1.0f)
			return false;

		// nothing more happens here the actual slomo logic is done in
		// Update, since it needs to call 'vp_TimeUtility.FadeTimeScale'
		// every frame

		return true;

	}


}
