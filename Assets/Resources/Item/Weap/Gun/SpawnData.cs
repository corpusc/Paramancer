using UnityEngine;
using System.Collections;



public class SpawnData : MonoBehaviour {
	public int Gun = 0; //formerly pickupPointID...... this is the [enum Gun] that spawns 
	public float RestockTime = 0f;
	public bool stocked = false;
	public GameObject Takeable; //formerly currentAvailablePickup......... takeable object, which currently floats/spins.....null for a period/duration when taken 
}
