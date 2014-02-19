using UnityEngine;
using System.Collections;


public class BeamEffect : MonoBehaviour {
	public Vector3 start;
	public Vector3 end;

	// private 
	LineRenderer lr;
	Color col = new Color(1, 1, 1, 0.4f);

	

	void Start() {
		lr = GetComponent<LineRenderer>();
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		lr.SetColors(col, col);
	}
	
	void Update() {
		lr.SetColors(col ,col);
		col.a -= Time.deltaTime / 2;

		if (col.a <= 0f){
			Destroy(gameObject);
		}
	}
}
