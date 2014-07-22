/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Layer.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class defines the layers used on objects. you may want
//					to modify the integers assigned here to suit your needs.
//					for example, you may want to keep 'LocalPlayer' in another
//					layer or you may want to address rendering or physics issues
//					related to incompatibility with other systems
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public sealed class vp_Layer
{

	public static readonly vp_Layer instance = new vp_Layer();

	// built-in Unity layers
	public const int Default = 0;
	public const int TransparentFX = 1;
	public const int IgnoreRaycast = 2;
	public const int Water = 4;

	// standard layers
	public const int Enemy = 25;
	public const int Pickup = 26;
	public const int Trigger = 27;
	public const int MovableObject = 28;
	public const int Debris = 29;
	public const int LocalPlayer = 30;
	public const int Weapon = 31;

	public static class Mask
	{

		// layer mask for raycasting away from the local player, ignoring the player itself
		// and all non-solid objects, including rigidbody pickups (used for bullets)
		public const int BulletBlockers = ~((1 << LocalPlayer) | (1 << Debris) |
								(1 << IgnoreRaycast) | (1 << Trigger) | (1 << Water) | (1 << Pickup));

		// layer mask for raycasting away from the local player, ignoring the player itself
		// and all non-solid objects. (used for player physics)
		public const int ExternalBlockers = ~((1 << LocalPlayer) | (1 << Debris) | 
										(1 << IgnoreRaycast) |(1 << Trigger) | (1 << Water));

		// layer mask for detecting solid, moving objects. (used for spawn radius checking)
		public const int PhysicsBlockers = (1 << vp_Layer.LocalPlayer) | (1 << vp_Layer.MovableObject);

		// layer mask for filtering out small and walk-thru objects. (used for explosions)
		public const int IgnoreWalkThru = ~((1 << Debris) | (1 << IgnoreRaycast) | (1 << Trigger) |
											 (1 << Water) | (1 << Pickup));

	}


	/// <summary>
	///
	/// </summary>
	static vp_Layer()
	{
		Physics.IgnoreLayerCollision(LocalPlayer, Debris);		// player should never collide with small debris
		Physics.IgnoreLayerCollision(Debris, Debris);			// gun shells should not collide against each other
	}
	private vp_Layer(){}


	/// <summary>
	/// sets the layer of a gameobject and optionally its descendants
	/// </summary>
	public static void Set(GameObject obj, int layer, bool recursive = false)
	{

		if (layer < 0 || layer > 31)
		{
			Debug.LogError("vp_Layer: Attempted to set layer id out of range [0-31].");
			return;
		}

		obj.layer = layer;

		if (recursive)
		{
			foreach (Transform t in obj.transform)
			{
				Set(t.gameObject, layer, true);
			}
		}

	}
	

}

