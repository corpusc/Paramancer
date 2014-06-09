using UnityEngine;
using System.Collections;



// an explosion sphere graphic 
public class SphereExplosion : MonoBehaviour {
	private Color color;
	public Color Color {
		set {
			Debug.Log("i am in the Color property of SphereExplosion");
			color = value;
			light.color = value;
			renderer.material.color = value;
		}
	}
	public float MaxSize = 5f;
	public bool IsRootSphere = true; // only the root/primary sphere should have a light, all other layers/stacks will be lit from it 

	// private 
	bool expanding = true;
	float startSize = 0.2f;
	Vector3 rotateSpeed;
	float expandSpeed = 35f;
	float shrinkSpeed = 20f;


	void Start() {
		Color = new Color(0.705f, 1f, 0f); // chartreuse for green grenades 

		transform.localScale = new Vector3(startSize, startSize, startSize);

		// random orientation/rotation speeds 
		int rnd = Random.Range(0, 3);
		switch (rnd) {
			case 0:
				rotateSpeed = new Vector3(0f, rand(), rand());
				break;
			case 1:
				rotateSpeed = new Vector3(rand(), 0f, rand());
				break;
			case 2:
				rotateSpeed = new Vector3(rand(), rand(), 0f);
				break;
		}

		if (!IsRootSphere)
			light.enabled = false;
	}

	void Update() {
		light.color = color;
		renderer.material.color = color;







		// scaling 
		var ls = transform.localScale;
		
		if (expanding) {
			var f = Time.deltaTime * expandSpeed;
			ls.x += f;
			ls.y += f;
			ls.z += f;

			if (transform.localScale.x > MaxSize)
				expanding = false;
		}else{ // ...shrinking 
			var f = Time.deltaTime * shrinkSpeed;
			ls.x -= f;
			ls.y -= f;
			ls.z -= f;
		}
		
		transform.localScale = ls;

		// rotation & light 
		if (IsRootSphere) {
			if (light != null) {
				light.range = transform.localScale.x * 4f;
			}
		}

		// spinning/rotation 
		transform.eulerAngles += rotateSpeed;

		// cleanup 
		if (transform.localScale.x < 0.1f) 
			Destroy(gameObject);
	}
	
	float rand() {
		var min = 12f;
		var max = 24f;
		var f = Random.Range(min, max);

		if (Random.Range(0, 2) == 1)
			f = -f;

		return f;		
	}
}
