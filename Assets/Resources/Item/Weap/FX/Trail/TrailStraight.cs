using UnityEngine;
using System.Collections;


public class TrailStraight : MonoBehaviour {
	public Vector3 Begin;
	public Vector3 End;
	public Color Color = new Color(1, 1, 1, 1f);

	// private 
	LineRenderer lr;

	

	void Start() {
		lr = GetComponent<LineRenderer>();
		lr.SetPosition(0, Begin);
		lr.SetPosition(1, End);
		lr.SetColors(Color, Color);
	}
	
	void Update() {
		//lr.SetColors(Color.clear, col);
		Color.a -= Time.deltaTime / 2;

		if (Color.a <= 0f){
			Destroy(gameObject);
		}
	}
}
