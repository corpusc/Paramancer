using UnityEngine;
using System.Collections;

public class HealthPotion : MonoBehaviour {
	public Transform Player;
	public float HpBoost = 100;
	


	void Start () {
	}
	
	void Update () {
	}

	void OnTriggerEnter(Collider other) {
		if (other.transform == Player) {
			var hp = (Health)other.transform.GetComponent("Health");

			if (hp) {
				if (hp.CurrentHealth < hp.MaxHealth) {		
					hp.CurrentHealth = hp.CurrentHealth+HpBoost;
					Destroy(gameObject);
				}
			}
		}
	}
}
