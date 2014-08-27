using UnityEngine;
using System.Collections;

public class AICount : MonoBehaviour {
	public Transform Player;
	


	void Start () {
		if(Player) {
			var p = (PlayerController)Player.GetComponent("PlayerController");

			if(p) {
				p.TotalAICount = p.TotalAICount+1;					
			}
		}
	}
	


	void Update () {
	}
}
