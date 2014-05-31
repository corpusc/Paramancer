using UnityEngine;
using System.Collections;


public class BeamEffect : MonoBehaviour {
	public Vector3 start;
	public Vector3 end;
	public Color col = new Color(1, 1, 1, 1f);

	// private 
	LineRenderer lr;

	

	void Start() {
		lr = GetComponent<LineRenderer>();
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		lr.SetColors(Color.clear, col);
	}
	
	void Update() {
		lr.SetColors(Color.clear, col);
		col.a -= Time.deltaTime / 2;

		if (col.a <= 0f){
			Destroy(gameObject);
		}
	}
}
