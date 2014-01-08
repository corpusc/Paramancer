using UnityEngine;
using System.Collections;

public class PickupPoint : MonoBehaviour {

	public int pickupType = -1;
	
	public int pickupPointID = 0;
	public float restockTime = 0f;
	
	public bool stocked = false;
	
	public GameObject currentAvailablePickup;
	
}
