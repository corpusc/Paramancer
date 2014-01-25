using UnityEngine;
using System.Collections;

public class PickupPoint : MonoBehaviour {
	public int SlotNum = -1;
	public int Id = 0;
	public float RestockTime = 0f;
	public bool stocked = false;
	public GameObject currentAvailablePickup;
}
