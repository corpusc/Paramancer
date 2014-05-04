using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class RlikePlane : MonoBehaviour {
	RoguelikeLevel lev;



	void Start () {
		lev = ScriptableObject.CreateInstance<RoguelikeLevel>();
		lev.MapSize.x = 64;
		lev.MapSize.y = 64;
		lev.Forms = 40;
		lev.MaxArea = 50;
		lev.MaxFormWidth = 16;
		lev.MinFormWidth = 1;
		lev.MaxOverride = 0.1f;
		lev.Init();
		Texture2D texture = new Texture2D(64, 64);
		texture.filterMode = FilterMode.Point;
		renderer.material.mainTexture = texture;
		for (int x = 0; x < texture.width; x++)
		for(int y = 0; y < texture.height; y++) {
				Color color = (lev.Cells[x, y].IsAir ? Color.white : Color.black);
				texture.SetPixel(x, y, color);
		}
		texture.Apply();
		lev.Scale = new Vector3(3f, 3f, 3f);
		lev.Pos = new Vector3(
			lev.Scale.x, 
			-lev.Scale.y*2, 
			lev.Scale.z);
		lev.Build3D();
	}






	// UPDATE is SOLELY an experimental/prototype of a BUILD-like Editor 
	public class Wall {
		public Vector3 Pos;
		public Wall Next;

		public Wall(Vector3 pos, Wall next) {
			Pos = pos;
			Next = next;
		}
	}
	Wall prevWall;
	List<Wall> walls = new List<Wall>();
	void Update() {
		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			prevWall = maybeMakeBUILDStyleWall();
		}
		if (Input.GetKeyUp(KeyCode.Mouse0)) {
			var nw = maybeMakeBUILDStyleWall();

			if (prevWall != null) {
				var nq = GameObject.CreatePrimitive(PrimitiveType.Quad);
				var delta = nw.Pos - prevWall.Pos;
				nq.transform.position = prevWall.Pos + delta/2;
				nq.transform.right = delta;
				var f = 3f;
				nq.transform.localScale = new Vector3 (delta.magnitude, f, f);
				prevWall = null;	
			}
		}
	}

	Wall maybeMakeBUILDStyleWall() {
		var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit rh;
		float distance = 100f;
		if (collider.Raycast (ray, out rh, distance)) {
			Debug.Log ("hit plane");
			var nc = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var f = .1f;
			nc.transform.localScale = new Vector3 (f, f, f);
			nc.transform.position = rh.point;
			var nw = new Wall(rh.point, prevWall);
			walls.Add(nw);
			return nw;
		}

		return null;
	}
}
