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
	public Color MidColor = Color.green;
	public bool UseMidColor = false;
	public float MidColorPos = 0.5f; // reaches midcolor at maxlife * MidColorPos 
	public Color EndColor = Color.red; // always use something transparent 
	public float life = 4f;
	public float f = 0.05f;
	public float MinSize = 1f;
	public float MaxSize = 3f;
	public ParticleType ParticType = ParticleType.Puff;
	public float acceleration = 1f; // multiply the speed by this 

	// private 
	Mesh mesh;
	Vector3[] vertices;
	Color[] colors;
	Vector3 shrinkFactor;
	float maxLife;



	void Start() {
		shrinkFactor = new Vector3(f, f, f);

		switch (ParticType) {
			case ParticleType.Circle:
				renderer.material = (Material)Resources.Load("Mat/Weap/RifleParticle", typeof(Material));
				break;
			case ParticleType.Multiple:
				if (Random.value < 0.5f)
					renderer.material = (Material)Resources.Load("Mat/Weap/MultipleParticle", typeof(Material));
				else
					renderer.material = (Material)Resources.Load("Mat/Weap/MultipleParticle2", typeof(Material));
				break;
			default:
				break; // puff is loaded by default, from the inspector 
		}

		mesh = GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		colors = new Color[vertices.Length];
		for (var i = 0; i < vertices.Length; i++)
			colors[i] = StartColor;
		mesh.colors = colors;
		transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
		life -= Random.Range(0f, 0.5f);
		maxLife = life;
		moveVec *= MaxSpeed;
		transform.rotation = Camera.main.transform.rotation;
	}
	
	void Update() {
		if(UseMidColor) {
			if(life > maxLife * MidColorPos)
				for (var i = 0; i < vertices.Length; i++) {
				colors[i] = Color.Lerp(MidColor, StartColor, (life - maxLife * MidColorPos) / (maxLife * MidColorPos));
			}
			else for (var i = 0; i < vertices.Length; i++) {
				colors[i] = Color.Lerp(EndColor, MidColor, life / (maxLife * MidColorPos));
			}
		} else {
		for (var i = 0; i < vertices.Length; i++)
			colors[i] = Color.Lerp(EndColor, StartColor, life / maxLife);
		}
		mesh.colors = colors;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, moveVec.normalized, out hit, moveVec.magnitude * Time.deltaTime, 1<<0))
			moveVec = hit.normal * moveVec.magnitude;
		
		transform.position += moveVec * Time.deltaTime;
		transform.rotation = Camera.main.transform.rotation;
		//transform.forward = Camera.main.transform.forward + new Vector3(0f, Time.time, 0f); //for some reason, doesn't work
		transform.localScale += Time.deltaTime * shrinkFactor;

		moveVec *= Mathf.Pow(acceleration, Time.deltaTime);
		life -= Time.deltaTime;

		if (life <= 0f) {
			Destroy(gameObject);
		}
	}
}
