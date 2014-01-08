using UnityEngine;
using System.Collections;

public class UVScroll : MonoBehaviour {
	
	
	public Vector2 scrollVec;
	
	
	private Vector2[] uvs;
	private Mesh meshy;
	
	// Use this for initialization
	void Start () {
		meshy = GetComponent<MeshFilter>().mesh;
		uvs = meshy.uv;
	}
	
	// Update is called once per frame
	void Update () {
		for (int i=0; i<uvs.Length; i++){
			uvs[i] += scrollVec * Time.deltaTime;
		}
		meshy.uv = uvs;
	}
}
