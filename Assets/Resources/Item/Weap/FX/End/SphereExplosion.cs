using UnityEngine;
using System.Collections;



// an explosion sphere graphic 
public class SphereExplosion : MonoBehaviour {
	private Color color = new Color(0.705f, 1f, 0f); // chartreuse for green grenades 
	public Color Color {
		set {
			Debug.Log("i am in the Color property of SphereExplosion");
			color = value;
			light.color = value;
		}
	}
	public float MaxSize;
	public bool IsRootSphere = true; // only the root/primary sphere should have a light, all other layers/stacks will be lit from it 

	// private 
	bool expanding = true;
	Vector3 startScale = new Vector3(0.2f, 0.2f, 0.2f);
	Vector3 rotateSpeed;
	float expandSpeed = 40f;
	float shrinkSpeed = 20f;


	void Start() {
		transform.localScale = startScale;

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

//		if (!IsRootSphere)
//			light.enabled = false;
	}

	void Update() {
		// scaling 
		var ls = transform.localScale;
		
		if (expanding) {
			Debug.Log("exPAND to MaxSize!!!!!!: " + MaxSize);
			float f = Time.deltaTime * expandSpeed;
			ls.x += f;
			ls.y += f;
			ls.z += f;

			if (ls.x > MaxSize)
				expanding = false;
		}else{ // ...shrinking 
			float f = Time.deltaTime * shrinkSpeed;
			ls.x -= f;
			ls.y -= f;
			ls.z -= f;
		}
		
		transform.localScale = ls;

		// rotation & light 
		if (IsRootSphere) {
			if (light != null) {
				light.color = color;
				light.range = ls.x * 4f;
			}
		}

		// spinning/rotation 
		transform.eulerAngles += rotateSpeed;

		// cleanup 
		if (ls.x < 0.1f) 
			Destroy(gameObject);
	}
	
	float rand() {
		float min = 12f;
		float max = 24f;
		float f = Random.Range(min, max);

		if (Random.Range(0, 2) == 1)
			f = -f;

		return f;		
	}
}
