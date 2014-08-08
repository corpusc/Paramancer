using UnityEngine;
using System.Collections;

public class Giblet : MonoBehaviour {
	public float Gravity = 10f;

	// private 
	Vector3 ov; // old 
	Vector3 nv; // new 
	float life = 2f;
	TrailRenderer tr;
	

	
	void Start() {
		nv = new Vector3(Random.Range(-5f, 5f),Random.Range(2f, 6f),Random.Range(-5f, 5f));
		transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);
		life += Random.Range(-0.5f, 0.4f);
		//tr.
	}
	
	void Update() {
		transform.position += nv * Time.deltaTime;
		nv.y -= Gravity * Time.deltaTime;
		nv.x -= (0-nv.x) * Time.deltaTime * -3f;
		nv.z -= (0-nv.z) * Time.deltaTime * -3f;
		
		life -= Time.deltaTime;
		if (life <= 0f) {
			Destroy(gameObject);
		}

		ov = nv;
	}
}
