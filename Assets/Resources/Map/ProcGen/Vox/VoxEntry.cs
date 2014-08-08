using UnityEngine;
using System.Collections;



public class VoxEntry : MonoBehaviour {
	VoxGen vg = new VoxGen();


	void Start() {
		vg.GenerateMap(Random.Range(0, 100000), Theme.SciFi);	
	}
	
	void Update() {
		vg.Update();
	}
	
	void OnGUI() {
		vg.OnGUI();
	}
}
