using UnityEngine;
using System.Collections;

public class BeamParticle : MonoBehaviour {
	private Vector3 moveVec = new Vector3(
		Random.Range(-max, max), 
		Random.Range(-max, max), 
		Random.Range(-max, max));
	public Vector3 MoveVec {
		get { return moveVec; }
		set {
			moveVec = value;
			moveVec *= Random.Range(1f, 2f);
		}
	}

	// private 
	const float max = -0.2f; // random max
	const float f = -0.2f;
	float life = 0.8f;
	Vector3 shrinkFactor = new Vector3(f, f, f);



	void Start() {
		transform.localScale = Vector3.one * Random.Range(0.5f, 0.8f);
		life -= Random.Range(0f, 0.5f);
		life *= 5f;
	}
	
	void Update() {
		transform.position += moveVec * Time.deltaTime;
		transform.rotation = Camera.main.transform.rotation;
		transform.localScale += Time.deltaTime * shrinkFactor;

		life -= Time.deltaTime;

		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
