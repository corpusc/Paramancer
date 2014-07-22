/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleInventory.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	keeps track of the carried items of a single, local player.
//					the purpose of the class is providing quick item status polling
//					via the event system in a decoupled manner. this class is
//					currently quite simple. it can be used as-is if your game has
//					modest inventory needs, or can be a placeholder during prototyping
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


public class vp_SimpleInventory : MonoBehaviour
{

	protected vp_FPPlayerEventHandler m_Player = null;

	// lists of available item and weapon type objects that may be
	// set up manually in the inspector

	[SerializeField]
	protected List<InventoryItemStatus> m_ItemTypes;
	[SerializeField]
	protected List<InventoryWeaponStatus> m_WeaponTypes;

	// this dictionary collects the contents of both the above lists
	// at runtime to speed things up a bit (dictionaries can't be
	// serialized by the editor so the lists are still needed)
	protected Dictionary<string, InventoryItemStatus> m_ItemStatusDictionary = null;

	protected InventoryWeaponStatus m_CurrentWeaponStatus = null;

	protected int m_RefreshWeaponStatusIterations = 0;

	// comparer to sort the weapons alphabetically. this is used to
	// sort weapon types alphabetically ingame
	protected class InventoryWeaponStatusComparer : IComparer
	{
		int IComparer.Compare(System.Object x, System.Object y)
		{ return ((new CaseInsensitiveComparer()).Compare(((InventoryWeaponStatus)x).Name, ((InventoryWeaponStatus)y).Name)); }
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
	/// 
	/// </summary>
	void Awake()
	{

		// store the first player event handler found in the top of our transform hierarchy
		m_Player = (vp_FPPlayerEventHandler)transform.root.GetComponentInChildren(typeof(vp_FPPlayerEventHandler));

		// sort the weapon types alphabetically
		IComparer comparer = new InventoryWeaponStatusComparer();
		m_WeaponTypes.Sort(comparer.Compare);


	}


	/////////////////////////////////////////////////////////////////////////////////
	//
	//	vp_SimpleInventory.InventoryItemStatus
	//
	//	description:	a simple counter for any type of item, with a max cap
	//
	/////////////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public class InventoryItemStatus
	{

		public string Name = "Unnamed";
		public int Have = 0;
		public int CanHave = 1;
		public bool ClearOnDeath = true;

	}


	/////////////////////////////////////////////////////////////////////////////////
	//
	//	vp_SimpleInventory.InventoryItemStatus
	//
	//	description:	this is basically a usable item type that can only be
	//					'used' a limited number of times before it needs to
	//					deplete another item in order to be used again
	/////////////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public class InventoryWeaponStatus : InventoryItemStatus
	{

		public string ClipType = "";
		public int LoadedAmmo = 0;
		public int MaxAmmo = 10;
		
	}


	/// <summary>
	/// copies the item lists into the item type dictionary
	/// whenever it is requested but hasn't been initialized yet.
	/// NOTE: at runtime, if duplicates exist of an item name in
	/// either list, all except the last duplicate item type will
	/// be removed
	/// </summary>
	protected Dictionary<string, InventoryItemStatus> ItemStatusDictionary
	{

		get
		{
			if (m_ItemStatusDictionary == null)
			{
				m_ItemStatusDictionary = new Dictionary<string, InventoryItemStatus>();

				for (int c = m_ItemTypes.Count - 1; c > -1; c--)
				{
					if (!m_ItemStatusDictionary.ContainsKey(m_ItemTypes[c].Name))
					{
						m_ItemStatusDictionary.Add(m_ItemTypes[c].Name, m_ItemTypes[c]);
					}
					else
						m_ItemTypes.Remove(m_ItemTypes[c]);
				}
				for (int c = m_WeaponTypes.Count - 1; c > -1; c--)
				{
					if (!m_ItemStatusDictionary.ContainsKey(m_WeaponTypes[c].Name))
					{
						m_ItemStatusDictionary.Add(m_WeaponTypes[c].Name, m_WeaponTypes[c]);
					}
					else
						m_WeaponTypes.Remove(m_WeaponTypes[c]);
				}
			}
			return m_ItemStatusDictionary;
		}

	}


	/// <summary>
	/// returns whether this inventory contains an object by 'name'
	/// </summary>
	public bool HaveItem(object name)
	{

		InventoryItemStatus type;
		if (!ItemStatusDictionary.TryGetValue((string)name, out type))
			return false;

		if (type.Have < 1)
			return false;

		return true;

	}


	/// <summary>
	/// returns an item status object for item 'name'
	/// </summary>
	InventoryItemStatus GetItemStatus(string name)
	{

		InventoryItemStatus type;

		if (!ItemStatusDictionary.TryGetValue(name, out type))
			Debug.LogError("Error: (" + this + "). Unknown item type: '" + name + "'.");

		return type;
	
	}


	/// <summary>
	/// returns an item status object for weapon 'name'
	/// </summary>
	InventoryWeaponStatus GetWeaponStatus(string name)
	{

		if (name == null)
			return null;

		InventoryItemStatus type;

		if (!ItemStatusDictionary.TryGetValue((string)name, out type))
		{
			Debug.LogError("Error: (" + this + "). Unknown item type: '" + name + "'.");
			return null;
		}

		if (type.GetType() != typeof(InventoryWeaponStatus))
		{
			Debug.LogError("Error: (" + this + "). Item is not a weapon: '" + name + "'.");
			return null;
		}

		return (InventoryWeaponStatus)type;

	}


	/// <summary>
	/// temporary solution for cases where we can't get the
	/// current weapon status yet because the current weapon
	/// isn't done getting wielded (this should really be done
	/// using some sort of callback).
	/// </summary>
	protected void RefreshWeaponStatus()
	{

		// if new weapon isn't done getting wielded yet, reschedule
		// this method in 0.1 secs
		if (m_Player.CurrentWeaponWielded.Get() == false &&
			m_RefreshWeaponStatusIterations < 50)	// stop trying after 5 seconds
		{
			m_RefreshWeaponStatusIterations++;
			vp_Timer.In(0.1f, RefreshWeaponStatus);
			return;
		}
		m_RefreshWeaponStatusIterations = 0;

		// new weapon is in place; now fetch its type
		string weaponName = m_Player.CurrentWeaponName.Get();
		if (string.IsNullOrEmpty(weaponName))
			return;

		m_CurrentWeaponStatus = GetWeaponStatus(weaponName);

	}


	/// <summary>
	/// gets or sets the current weapon's ammo count
	/// </summary>
	protected virtual int OnValue_CurrentWeaponAmmoCount
	{
		get
		{
			return (m_CurrentWeaponStatus != null) ? m_CurrentWeaponStatus.LoadedAmmo : 0;
		}
		set
		{
			if (m_CurrentWeaponStatus == null)
				return;
			m_CurrentWeaponStatus.LoadedAmmo = value;
		}
	}


	/// <summary>
	/// gets the amount of clips this inventory has stored that
	/// is compatible with the current weapon type
	/// </summary>
	protected virtual int OnValue_CurrentWeaponClipCount
	{
		get
		{

			if (m_CurrentWeaponStatus == null)
				return 0;

			InventoryItemStatus type;
			if (!ItemStatusDictionary.TryGetValue(m_CurrentWeaponStatus.ClipType, out type))
				return 0;

			return type.Have;

		}

	}


	/// <summary>
	/// returns the clip type of the current weapon
	/// </summary>
	protected virtual string OnValue_CurrentWeaponClipType
	{
		get
		{
			return (m_CurrentWeaponStatus != null) ? m_CurrentWeaponStatus.ClipType : "";
		}
	}


	/// <summary>
	/// returns the amount of items in this inventory with 'name'
	/// </summary>
	protected virtual int OnMessage_GetItemCount(string name)
	{

		InventoryItemStatus type;
		if (!ItemStatusDictionary.TryGetValue((string)name, out type))
			return 0;

		return type.Have;

	}


	/// <summary>
	/// tries to remove one unit from ammo level of current weapon
	/// </summary>
	protected virtual bool OnAttempt_DepleteAmmo()
	{

		// fail if we don't have the weapon or it is unknown
		if (m_CurrentWeaponStatus == null)
			return false;

		// check ammo
		if (m_CurrentWeaponStatus.LoadedAmmo < 1)
		{

			// if 'MaxAmmo' is zero, this weapon has infinite ammo
			if (m_CurrentWeaponStatus.MaxAmmo == 0)
				return true;

			// can't use ammo if empty
			return false;

		}

		// all ok: use one ammo
		m_CurrentWeaponStatus.LoadedAmmo--;

		return true;

	}


	/// <summary>
	/// tries to add load ammo into a weapon in this inventory
	/// by weapon name and ammo count. note that the argument
	/// is of type 'object' and should be an object array
	/// containing a string and an int, respectively.
	/// </summary>
	protected virtual bool OnAttempt_AddAmmo(object arg)
	{

		// unbox object argument into an object array
		object[] args = (object[])arg;

		// get weapon name from the first argument
		string weaponName = (string)args[0];

		// if we have a 2nd argument, get ammo from it, otherwise set ammo to max (-1)
		int ammo = (args.Length == 2) ? (int)args[1] : -1;

		// fail if we don't have the weapon or it is unknown
		InventoryWeaponStatus weapon = GetWeaponStatus((string)weaponName);
		if (weapon == null)
			return false;

		if (ammo == -1)
			weapon.LoadedAmmo = weapon.MaxAmmo;					// ammo '-1' means 'fill her up'
		else
			weapon.LoadedAmmo = Mathf.Min(weapon.LoadedAmmo + ammo, weapon.MaxAmmo);	// add 'ammo' units, capping at max

		return true;

	}


	/// <summary>
	/// tries to add an amount of units to the item count. fails
	/// if item count is maxed out
	/// </summary>
	protected virtual bool OnAttempt_AddItem(object args)
	{

		object[] arr = (object[])args;
		string name = (string)arr[0];
		int amount = (arr.Length == 2) ? (int)arr[1] : 1;

		// fail if item type is unknown
		InventoryItemStatus type = GetItemStatus(name);
		if (type == null)
			return false;

		// enforce ability to have atleast 1 item of each type
		type.CanHave = Mathf.Max(1, type.CanHave);

		// fail if we already have max units of this item
		if (type.Have >= type.CanHave)
			return false;

		// add amount (cap at max)
		type.Have = Mathf.Min(type.Have + amount, type.CanHave);

		return true;

	}


	/// <summary>
	/// tries to remove one unit from the item count.
	/// fails if item count is zero
	/// </summary>
	protected virtual bool OnAttempt_RemoveItem(object args)
	{

		object []arr = (object[])args;
		string name = (string)arr[0];
		int amount = (arr.Length == 2) ? (int)arr[1] : 1;

		// fail if item type is unknown
		InventoryItemStatus type = GetItemStatus(name);
		if (type == null)
			return false;

		// fail if we have zero units of this item
		if (type.Have <= 0)
			return false;

		// reduce amount (cap at zero)
		type.Have = Mathf.Max(type.Have - amount, 0);
		
		return true;

	}


	/// <summary>
	/// tries to remove one unit from the external
	/// clip item type and returns result
	/// </summary>
	protected virtual bool OnAttempt_RemoveClip()
	{

		// fail if we don't have the weapon or it is unknown
		if (m_CurrentWeaponStatus == null)
			return false;


		// fail if ammo type is unknown
		InventoryItemStatus ammo = GetItemStatus(m_CurrentWeaponStatus.ClipType);
		if (ammo == null)
			return false;

		// can't reload if ammo is full
		if (m_CurrentWeaponStatus.LoadedAmmo >= m_CurrentWeaponStatus.MaxAmmo)
			return false;

		// try to remove an ammo clip
		if (!m_Player.RemoveItem.Try(new object[] { m_CurrentWeaponStatus.ClipType }))
			return false;

		return true;

	}


	/// <summary>
	/// returns true if the inventory contains a weapon by the
	/// number last fed as an argument to the 'SetWeapon' activity.
	/// false if not. this is used to regulate which weapons the
	/// player currently has access to.
	/// </summary>
	protected virtual bool CanStart_SetWeapon()
	{

		int id = (int)m_Player.SetWeapon.Argument;

		if (id == 0)
			return true;

		if ((id < 0) || (id > m_WeaponTypes.Count))
			return false;

		return HaveItem(m_WeaponTypes[id - 1].Name);

	}


	/// <summary>
	/// this callback is triggered when the activity in question
	/// deactivates
	/// </summary>
	protected virtual void OnStop_SetWeapon()
	{
		RefreshWeaponStatus();
	}


	/// <summary>
	/// this callback is triggered right after the activity in
	/// question has been approved for activation. it strips the
	/// player of all items that have the 'ClearOnDeath' flag
	/// </summary>
	protected virtual void OnStart_Dead()
	{

		foreach (InventoryItemStatus i in m_ItemStatusDictionary.Values)
		{
			if (i.ClearOnDeath)
			{
				i.Have = 0;
				if (i.GetType() == typeof(InventoryWeaponStatus))
				{
					((InventoryWeaponStatus)i).LoadedAmmo = 0;
				}
			}
		}

	}
	

}

