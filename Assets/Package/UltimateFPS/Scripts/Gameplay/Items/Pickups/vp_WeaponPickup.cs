/////////////////////////////////////////////////////////////////////////////////
//
//	vp_WeaponPickup.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a simple script for making a new weapon available to the player
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_WeaponPickup : vp_Pickup
{

	public int AmmoIncluded = 0;
	
	/// <summary>
	/// tries to add weapon 'InventoryName' to the inventory
	/// </summary>
	protected override bool TryGive(vp_FPPlayerEventHandler player)
	{

		if (player.Dead.Active)
			return false;

		if (!base.TryGive(player))
			return false;

		// try wielding the new weapon (if it fails that's ok)
		player.SetWeaponByName.Try(InventoryName);

		// try setting ammo on the new weapon to 'AmmoIncluded'
		if (AmmoIncluded > 0)
			player.AddAmmo.Try(new object[] { InventoryName, AmmoIncluded });

		return true;

	}

}
