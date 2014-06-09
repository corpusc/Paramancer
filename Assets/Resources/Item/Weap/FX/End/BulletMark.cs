using UnityEngine;
using System.Collections;



public class BulletMark : MonoBehaviour {
	public float MaxLife = 30f;
	public Color StartCol = Color.gray;

	// private 
	float life;



	void Start() {
		life = MaxLife;
	}
	
	void Update() {
		renderer.material.color = Color.Lerp(Color.clear, StartCol, life / MaxLife);
		life -= Time.deltaTime;

		if (life < 0f) 
			Destroy(gameObject);
	}
}
