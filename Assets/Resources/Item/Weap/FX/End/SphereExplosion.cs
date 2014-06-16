using UnityEngine;
using System.Collections;



// an explosion sphere graphic 
public class SphereExplosion : MonoBehaviour {

	public float MaxRadius;
	public bool IsRootSphere = true; // only the root/primary sphere should have a light, all other layers/stacks will be lit from it 
	public Color Color;

	// private 
	bool expanding = true;
	Vector3 startScale = new Vector3(0.2f, 0.2f, 0.2f);
	Vector3 rotateSpeed;
	float expandSpeed = 40f;
	float shrinkSpeed = 20f;

	MeshFilter mf;
	Mesh startMesh;



	void Start() {
		mf = GetComponent<MeshFilter>();
		startMesh = mf.mesh;

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

		if (!IsRootSphere)
			light.enabled = false;
	}

	void Update() {
		// scaling 
		var ls = transform.localScale;
		
		if (expanding) {
			float f = Time.deltaTime * expandSpeed;
			ls.x += f;
			ls.y += f;
			ls.z += f;

			if (ls.x > MaxRadius*2f)
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
				light.range = ls.x * 2f;
				light.color = Color;
			}
		}

		// spinning/rotation 
		transform.eulerAngles += rotateSpeed;

		// noise
		Mesh aMesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = aMesh.vertices;
		int i = 0;
		while( i < vertices.Length ) {
			Vector3 v = new Vector3(vertices[i].x * 0.1f, vertices[i].z * 0.1f, Time.time * 1f);
			vertices[i] += new Vector3(vertices[i].x, Random.Range(0f, 1f), vertices[i].z);
			i++;
		}
		
		aMesh.vertices = vertices;
		aMesh.RecalculateBounds();
		
		
		// cleanup 
		if (ls.x < 0.1f) 
			Destroy(gameObject);
	}
	
	float rand() {
		float min = 9f;
		float max = 18f;
		float f = Random.Range(min, max);

		if (Random.Range(0, 2) == 1)
			f = -f;

		return f;		
	}
}
