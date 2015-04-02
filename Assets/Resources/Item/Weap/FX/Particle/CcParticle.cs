using UnityEngine;
using System.Collections;

public class CcParticle : MonoBehaviour {
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
	public bool UseMidColor = true;
	public float MidColorPos = 0.5f; // reaches midcolor at maxlife * MidColorPos 
	public Color EndColor = Color.red; // always use something transparent 
	public float Dura = 4f; // lifetime duration 
	public float OneDScale = 0.05f;
	public float MinSize = 0.4f;
	public float MaxSize = 0.411f;
	public ParticleType ParticType = ParticleType.Puff;
	public float acceleration = 1f; // multiply the speed by this 
	public float MaxRotationSpeed = 360f;

	// private 
	Mesh mesh;
	Vector3[] vertices;
	Color[] colors;
	Vector3 shrinkFactor;
	float maxLife;
	float rotSpeed; // the actual rotation speed 



	void Start() {
		shrinkFactor = new Vector3(OneDScale, OneDScale, OneDScale);
		rotSpeed = Random.Range(-MaxRotationSpeed, MaxRotationSpeed);

		switch (ParticType) {
			case ParticleType.Circle:
				GetComponent<Renderer>().material = Mats.Get("Linearish");
				break;
			case ParticleType.Multiple:
				if (Random.value < 0.5f)
				GetComponent<Renderer>().material = Mats.Get("MultipleParticle");
				else
					GetComponent<Renderer>().material = Mats.Get("MultipleParticle2");
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
		Dura -= Random.Range(0f, 0.5f);
		maxLife = Dura;
		moveVec *= MaxSpeed;
		// if needing to set this rotation here, it actually should start with a random rotation, rather than 
		// mirroring the Update() logic, RIGHT?!
		//transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0f, 0f, Time.time * rotSpeed);
	}
	
	void Update() {
		if (UseMidColor) {
			if (Dura > maxLife * MidColorPos) {
				for (var i = 0; i < vertices.Length; i++) {
					colors[i] = Color.Lerp(MidColor, StartColor, (Dura - maxLife * MidColorPos) / (maxLife * MidColorPos));
				}
			}else for (var i = 0; i < vertices.Length; i++) {
				colors[i] = Color.Lerp(EndColor, MidColor, Dura / (maxLife * MidColorPos));
			}
		}else{
			for (var i = 0; i < vertices.Length; i++)
				colors[i] = Color.Lerp(EndColor, StartColor, Dura / maxLife);
		}
		mesh.colors = colors;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, moveVec.normalized, out hit, moveVec.magnitude * Time.deltaTime, 1<<0))
			//moveVec = hit.normal * moveVec.magnitude;      IceFlame changed to below line 
			moveVec = Vector3.Reflect(moveVec, hit.normal);
		
		transform.position += moveVec * Time.deltaTime;
		//transform.rotation = Camera.main.transform.rotation;       // IceFlame added the quaternion below 
		transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0f, 0f, Time.time * rotSpeed);
		transform.localScale += Time.deltaTime * shrinkFactor;

		moveVec *= Mathf.Pow(acceleration, Time.deltaTime);
		Dura -= Time.deltaTime;

		if (Dura <= 0f) {
			Destroy(gameObject);
		}
	}
}
