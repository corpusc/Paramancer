/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPPistolReloader.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:		this is provided as an example of how it is possible to play
//						around with states and timers to create animations. study to
//						learn more about spring forces and timers!
//
//						PLEASE NOTE: this is provided just as an example. it is not
//						really the recommended way of doing reload animation on
//						firearms. any complex animation such as a pistol reload
//						sequence should have a model with moving weapon & hand parts
//						and thus use a regular animation
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;

public class vp_FPPistolReloader : vp_FPWeaponReloader
{

	vp_Timer.Handle m_Timer = new vp_Timer.Handle();
		

	/// <summary>
	/// this callback is triggered right after the activity in question
	/// has been approved for activation. it reloads the current
	/// weapon and performs a hardcoded spring-and-timer based
	/// pistol reload animation on it
	/// </summary>
	protected override void OnStart_Reload()
	{

		// NOTE: this method assumes that a 'Reload' state has
		// already been triggered and tilted the weapon to the side
		// it also requires a 'Reload2' state, where the pistol
		// should be low and pointed upwards

		if (m_Weapon.gameObject != gameObject)
			return;

		// always abort if the timer is running, to avoid potential
		// hiccups caused by button spamming
		if (m_Timer.Active)
			return;

		// call the base event listener manually when overriding it, since
		// the event system will hide base members in derived classes
		base.OnStart_Reload();
		
		// after 0.4 seconds, simulate replacing the clip
		vp_Timer.In(0.4f, delegate()
		{

			// but first make sure we're still reloading since the player
			// may have switched weapons
			if (!vp_Utility.IsActive(m_Weapon.gameObject))
				return;

			if (!m_Weapon.StateEnabled("Reload"))
				return;

			// apply a force as if slapping a clip into the gun from below
			m_Weapon.AddForce2(new Vector3(0, 0.05f, 0), new Vector3(0, 0, 0));

			// 0.15 seconds later, twist the gun backwards
			vp_Timer.In(0.15f, delegate()
			{

				if (!vp_Utility.IsActive(m_Weapon.gameObject))
					return;

				if (!m_Weapon.StateEnabled("Reload"))
					return;

				// to do this, switch from the pistol 'Reload' state to
				// its 'Reload2' state
				m_Weapon.SetState("Reload", false);
				m_Weapon.SetState("Reload2", true);
				m_Weapon.RotationOffset.z = 0;
				m_Weapon.Refresh();

				// after 0.35 seconds, pull the slide
				vp_Timer.In(0.35f, delegate()
				{

					if (!vp_Utility.IsActive(m_Weapon.gameObject))
						return;

					if (!m_Weapon.StateEnabled("Reload2"))
						return;

					// apply a force pulling the whole gun backwards briefly
					m_Weapon.AddForce2(new Vector3(0, 0, -0.05f), new Vector3(5, 0, 0));

					// 0.1 seconds later, disable the reload state to point
					// the gun forward again
					vp_Timer.In(0.1f, delegate()
					{

						m_Weapon.SetState("Reload2", false);

					});

				});
			});

			// NOTE: the below hook to a 'vp_Timer.Handle' object is what allows us
			// to check whether the timer is active at the beginning of the method
		}, m_Timer);

	}

}

