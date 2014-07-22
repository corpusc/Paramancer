using UnityEngine;
using System.Collections.Generic;



public class Awesonator {
	// BUILD-like Editor.....named after Awesoken (Ken Silverman), the originator of this 
	// style of sectored/portaled maps 
	WallPoint prevWall;
	WallPoint cwt = null; // closest within threshold
	int wpI = 0; // wall point index 
	List<WallPoint> walls = new List<WallPoint>();
	GameObject gridSurface;



	public Awesonator() {
	}

	public void Update() {
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (gridSurface == null)
			gridSurface = GameObject.Find("TopDownPlane");

		if (wallPointNear(ray)) {
			cwt.GO.renderer.material.color = S.ShoutyBlue;
		}else{
			if (cwt != null) {
				cwt.GO.renderer.material.color = Color.white;
				cwt = null;
			}
		}
		
		
		
		if (CcInput.Started(UserAction.Activate)) {
			prevWall = maybeMakeBUILDStyleWall(ray);
		}
		
		if (CcInput.Ended(UserAction.Activate)) {
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
				nq.renderer.material = Mats.Get("sci_fi_003");
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
		if (gridSurface.collider.Raycast(ray, out rh, distance)) {
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
		if (gridSurface.collider.Raycast(ray, out rh, distance)) {
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
