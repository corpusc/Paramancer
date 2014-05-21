using UnityEngine;
using System.Collections;



public class PickupPoint : MonoBehaviour {
	public int pickupType = -1; // this is the pickupSlot (1-5)
	public int pickupPointID = 0; // this is the (Item) that spawns
	public float RestockTime = 0f;
	public bool stocked = false;
	public GameObject currentAvailablePickup;
}
