using UnityEngine;
using System.Collections;

public class BeamParticle : MonoBehaviour {
	private Vector3 moveVec;
	public Vector3 MoveVec {
		get { return moveVec; }
		set {
			moveVec = value;
			moveVec *= Random.Range(1f, 2f);
		}
	}

	// private 
	float life = 0.8f;



	void Start() {
		transform.localScale = Vector3.one * Random.Range(0.5f, 0.8f);
		life -= Random.Range(0f, 0.5f);
		life *= 5f;
	}
	
	void Update() {
		transform.position += moveVec * Time.deltaTime;
		transform.rotation = Camera.main.transform.rotation;
		transform.localScale = transform.localScale * Time.deltaTime*58.5f;

		life -= Time.deltaTime;

		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
