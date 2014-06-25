using UnityEngine;



[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class MakeMesh : MonoBehaviour {
    void Start() {
        GetComponent<MeshFilter>().mesh = makeQuadMesh();
    }

    Mesh makeQuadMesh() {
        var mesh = new Mesh();
        
		Vector3[] vertices = new Vector3[] {
		    new Vector3( 1, 0, 1),
		    new Vector3( 1, 0, -1),
		    new Vector3(-1, 0, 1),
		    new Vector3(-1, 0, -1),
		};

		Vector2[] uv = new Vector2[] {
		    new Vector2(1, 1),
		    new Vector2(1, 0),
		    new Vector2(0, 1),
		    new Vector2(0, 0),
		};

		int[] triangles = new int[] {
		    0, 1, 2,
		    2, 1, 3,
		};

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}