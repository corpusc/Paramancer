/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SpeedPickup.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a simple script to set a time-limited state. sets
//					the state 'MegaSpeed' on the player FPSController for  a few
//					seconds. ofcourse, any state can be set or disabled in the
//					same way! you could do anything from increasing jump speed to
//					enabling a 'drunk' camera state, or making the player appear
//					five feet taller
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_SpeedPickup : vp_Pickup
{

	// timer handle to manage multiple timers
	protected vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	
	/// <summary>
	///
	/// </summary>
	protected override void Update()
	{

		// handle rotation and bob, if enabled
		UpdateMotion();

		// handle time fading, and remove pickup if depleted
		if (m_Depleted)
		{

			if (!m_Audio.isPlaying)
				Remove();

		}
		
	}


	/// <summary>
	/// tries to enable the 'MegaSpeed' state on the player
	/// </summary>
	protected override bool TryGive(vp_FPPlayerEventHandler player)
	{

		// prevent the player from picking up the item again until any
		// currently running speed timer has run its course
		if (m_Timer.Active)
			return false;

		// --- proper way of doing it ---

		// for something like this we use the State Manager and vp_Timer!
		// in the pickup demo folder you will find a controller preset
		// named 'ControllerMegaSpeed.txt' which boosts player acceleration
		// and increases its push force on rigidbodies.
		// in the pickup demo scene this has been added as a state named
		// 'MegaSpeed' to the controller component

		player.SetState("MegaSpeed");

		// restore speed after 'Value' seconds
		vp_Timer.In(RespawnDuration, delegate()
		{
			player.SetState("MegaSpeed", false);
		}, m_Timer);

		// NOTE: binding the 'm_Timer' handle above makes sure this timer
		// is canceled and restarted if it's already running. if you allow
		// players to pick up multiple powerups, this will prevent a depleted
		// pickup from disabling the state if the player has enabled a new one
		// while the previous one is active (i.e. the timer will be restarted)

		// --- buggy way of doing it ---

		// the below would also be a way of adding speed, but it would get messed up
		// if player pressed or released the 'Run' modifier key. speed would multiply
		// in case of several pickups and we would have to store the original controller
		// acceleration value in a 'Start' method. messy and error-prone. use states.

		//Player.Controller.MotorAcceleration *= 4.0f;
		//vp_Timer.In(Value, delegate()
		//{
		//    Player.Controller.MotorAcceleration *= 0.25f;	// ... or a stored original speed
		//});

		return true;

	}


}
