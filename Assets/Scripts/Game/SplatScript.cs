using UnityEngine;
using System.Collections;

public class SplatScript : MonoBehaviour {
	private Vector3 moveVec = Vector3.zero;
	private float life = 1f;
	

	
	void Start() {
		moveVec = new Vector3(Random.Range(-5f,5f),Random.Range(2f,6f),Random.Range(-5f,5f));
		transform.localScale = Vector3.one * Random.Range(0.1f,0.2f);
		life += Random.Range(-0.5f,0.4f);
	}
	
	void Update() {
		transform.position += moveVec * Time.deltaTime;
		moveVec.y -= 15f * Time.deltaTime;
		moveVec.x -= (0-moveVec.x) * Time.deltaTime * -3f;
		moveVec.z -= (0-moveVec.z) * Time.deltaTime * -3f;
		
		life -= Time.deltaTime;
		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
