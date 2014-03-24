using UnityEngine;
using System.Collections;

public class BeamParticle : MonoBehaviour {
	private Vector3 moveVec = Random.insideUnitSphere;
	public Vector3 MoveVec {
		get { return moveVec; }
		set {
			moveVec = value;
			moveVec *= Random.Range(1f, 2f);
		}
	}
	public float MaxSpeed = 0.2f; // random max
	public Color StartColor = Color.blue;
	public Color EndColor = Color.red; // always use something transparent
	public float life = 0.8f;
	public float f = 0.05f;
	public float MinSize = 1f;
	public float MaxSize = 3f;
	public bool ShotFromRifle = false; // use a different particle if yes

	// private 
	Mesh mesh;
	Vector3[] vertices;
	Color[] colors;
	Vector3 shrinkFactor;
	float maxLife;



	void Start() {
		shrinkFactor = new Vector3(f, f, f);
		if (ShotFromRifle) {
			renderer.material = (Material)Resources.Load("Mat/Weap/RifleParticle", typeof(Material));
		}
		mesh = GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		colors = new Color[vertices.Length];
		for (var i = 0; i < vertices.Length; i++)
			colors[i] = StartColor;
		mesh.colors = colors;
		transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
		life -= Random.Range(0f, 0.5f);
		life *= 5f;
		maxLife = life;
		moveVec *= MaxSpeed;
		transform.rotation = Camera.main.transform.rotation;
	}
	
	void Update() {
		for (var i = 0; i < vertices.Length; i++)
			colors[i] = Color.Lerp(EndColor, StartColor, life / maxLife);
		mesh.colors = colors;

		transform.position += moveVec * Time.deltaTime;
		transform.rotation = Camera.main.transform.rotation;
		transform.localScale += Time.deltaTime * shrinkFactor;

		life -= Time.deltaTime;

		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
