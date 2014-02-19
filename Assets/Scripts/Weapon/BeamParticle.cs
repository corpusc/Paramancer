using UnityEngine;
using System.Collections;

public class BeamParticle : MonoBehaviour {
	// private 
	Vector3 moveVec = Vector3.zero;
	float life = 0.8f;



	void Start() {
		moveVec = new Vector3(Random.Range(-1f, 1f),Random.Range(-1f, 1f),Random.Range(-1f, 1f));
		transform.localScale = Vector3.one*10 * Random.Range(0.05f, 0.08f);
		life -= Random.Range(0f, 0.5f);
		life *= 2f;
	}
	
	void Update() {
		transform.position += moveVec * Time.deltaTime * 0.2f;
		transform.rotation = Camera.main.transform.rotation;
		//moveVec.y += 8f * Time.deltaTime;
		//moveVec.x -= (0-moveVec.x) * Time.deltaTime * -3f;
		//moveVec.z -= (0-moveVec.z) * Time.deltaTime * -3f;

		life -= Time.deltaTime;
		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
