using UnityEngine;
using System.Collections;



public class VoxEntry : MonoBehaviour {
	void Start() {
		VoxGen.GenerateMap(Random.Range(0, 100000), Theme.SciFi);	
	}
	
	void OnGUI() {
		VoxGen.OnGUI();
	}
}
