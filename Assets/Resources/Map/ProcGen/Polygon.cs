using UnityEngine;
using System.Collections.Generic;


public class Polygon : MonoBehaviour {
	void Start () {
		// Create Vector2 vertices
		List<Vector2> vertices2D = new List<Vector2>();
		vertices2D.Add(new Vector2(0,0));
		vertices2D.Add(new Vector2(0,7));
		vertices2D.Add(new Vector2(5,5));
		vertices2D.Add(new Vector2(6,10));
		vertices2D.Add(new Vector2(0,10));
		vertices2D.Add(new Vector2(0,15));
		vertices2D.Add(new Vector2(15,15));
		vertices2D.Add(new Vector2(15,10));
		vertices2D.Add(new Vector2(10,10));
		vertices2D.Add(new Vector2(10,5));
		vertices2D.Add(new Vector2(15,5));
		vertices2D.Add(new Vector2(15,0));

		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate();
		
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Count];
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = new Vector3(vertices2D[i].x, 0.3f, vertices2D[i].y);
		}
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		// Set up game object with mesh;
		gameObject.AddComponent(typeof(MeshRenderer));
		MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		filter.mesh = msh;
	}
}