using UnityEngine;
using System.Collections;



public class VoxEntry : MonoBehaviour {
	VoxGen vg;


	void Start() {
		//Mats.Init();
		vg = new VoxGen();
		vg.GenerateMap(Random.Range(0, 100000), Theme.SciFi);	
	}
	
	void Update() {
		vg.Update();
	}
	
	void OnGUI() {
		vg.OnGUI();
	}
}
