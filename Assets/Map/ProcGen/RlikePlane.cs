using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class RlikePlane : MonoBehaviour {
	ProcGenMaze grid;
	ProcGenVoxel vox;



	void Start() {
		Debug.Log("Start() of plane");
		grid = new ProcGenMaze();
		Texture2D texture = new Texture2D(grid.Max.x, grid.Max.z);
		texture.filterMode = FilterMode.Point;
		renderer.material.mainTexture = texture;

		for (int x = 0; x < texture.width; x++)
		for(int y = 0; y < texture.height; y++) {
				Color color = (grid.Cells[x, y].IsAir ? Color.white : Color.black);
				texture.SetPixel(x, y, color);
		}

		texture.Apply();

		grid.Scale = new Vector3(3f, 3f, 3f);
		grid.Pos = new Vector3(
			grid.Scale.x, 
			-grid.Scale.y*2, 
			grid.Scale.z);
		grid.Build3D();

		vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		vox.MapSize.x = 64;
		vox.MapSize.y = 32;
		vox.MapSize.z = 64;
		vox.Pos = new Vector3(-100f, 0f, 0f);
		vox.Init();
		vox.Build();
		vox.Build3d();
	}






	// UPDATE is SOLELY an experimental/prototype of a BUILD-like Editor 
	WallPoint prevWall;
	WallPoint cwt = null; // closest within threshold
	int wpI = 0; // wall point index 
	List<WallPoint> walls = new List<WallPoint>();
	void Update() {
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (wallPointNear(ray)) {
			cwt.GO.renderer.material.color = S.ShoutyColor;
		}else{
			if (cwt != null) {
				cwt.GO.renderer.material.color = Color.white;
				cwt = null;
			}
		}



		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			prevWall = maybeMakeBUILDStyleWall(ray);
		}

		if (Input.GetKeyUp(KeyCode.Mouse0)) {
			var nw = maybeMakeBUILDStyleWall(ray);

			if (prevWall != null) {
				var nq = GameObject.CreatePrimitive(PrimitiveType.Quad);
				var delta = nw.Pos - prevWall.Pos;
				var p = prevWall.Pos + delta/2;
				p.y += 2f;
				nq.transform.position = p;
				nq.transform.right = delta;
				var f = 4f;
				var v = new Vector3 (delta.magnitude, f, f);
				nq.transform.localScale = v;
				v /= 4f;
				nq.renderer.material = grid.SciFiMat;
				nq.renderer.material.mainTextureScale = v;
				prevWall = null;	
			}
		}
	}
	
	bool wallPointNear(Ray ray) {
		// what i'm trying to do here is have a per-frame cast, that turns nearby wallpoint into a highlighted color 
		float threshold = 0.2f;

		RaycastHit rh;
		float distance = 100f;
		if (collider.Raycast(ray, out rh, distance)) {
			foreach (var v in walls) {
				if (cwt == null) {
					cwt = v;
				}else{
					var distToHit /*'''*/ = Vector3.Distance(v.Pos, rh.point);
					var distToCurrClosest = Vector3.Distance(v.Pos, cwt.Pos);
					
					if (distToHit < distToCurrClosest)
						cwt = v;
				}				
			}
			
			if (cwt != null && Vector3.Distance(cwt.Pos, rh.point) < threshold) {
				return true;
			}
		}
		
		return false;
	}
	
	WallPoint maybeMakeBUILDStyleWall(Ray ray) {
		Vector3 newPos;
		
		RaycastHit rh;
		float distance = 100f;
		if (collider.Raycast(ray, out rh, distance)) {
			// place precisely at an existing wall location if within a threshold 
			float threshold = 0.2f;
			WallPoint cc = null; // current closest wall point 
			foreach (var v in walls) {
				if (cc == null) {
					cc = v;
				}else{
					var distToHit /*'''*/ = Vector3.Distance(v.Pos, rh.point);
					var distToCurrClosest = Vector3.Distance(v.Pos, cc.Pos);
					
					if (distToHit < distToCurrClosest)
						cc = v;
				}				
			}
			
			if (cc != null && Vector3.Distance(cc.Pos, rh.point) < threshold) {
				newPos = cc.Pos;
			}else{
				newPos = rh.point;
			}
			
			// now create the point 
			var nc = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var f = .1f;
			nc.transform.localScale = new Vector3 (f, f, f);
			nc.transform.position = newPos;
			var nw = new WallPoint(newPos, prevWall, nc);
			walls.Add(nw);
			return nw;
		}
		
		return null;
	}
}
