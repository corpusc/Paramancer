using UnityEngine;
using System.Collections;



public class PickupPoint : MonoBehaviour {
	public int pickupPointID = 0; // this is the (Gun) that spawns 
	public float RestockTime = 0f;
	public bool stocked = false;
	public GameObject currentAvailablePickup;
}
