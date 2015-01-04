using UnityEngine;
using System.Collections.Generic;



public class Awesonator {
	// BUILD-like Editor.....named after Awesoken (Ken Silverman), 
	// the originator of this style of sectored/portaled maps 
	public WallEdge NearbyWallEdge {
		get {
			return nearbyWallEdge;
		}

		set {
			if (value == null && nearbyWallEdge != null) {
				nearbyWallEdge.GO.renderer.material.color = Color.white;
				nearbyWallEdge.GO.transform.localScale = scale;
			}

			nearbyWallEdge = value;
		}
	}

	// private 
	Vector3 scale; // current scale 
	Vector3? currCursor; // current world cursor position (for now, its the spot where pointer/crosshair touches the gridplane) 
	Ray ray; // current ray 
	Vector3? begin = null; // ... start of a drawn wall 
	Vector3? end = null; // ...of a drawn wall 
	WallEdge prevWall;
	WallEdge closest = null; // closest wall edge (could be far away still) 
	WallEdge nearbyWallEdge = null; // only has value if it's within a certain threshold/distance 
	int wpI = 0; // wall point index 
	List<WallEdge> walls = new List<WallEdge>();
	GameObject gridSurface;
	float threshold = 0.2f; // this distance (or greater) is too far away to highlight (or SNAP to) this point-in-space marker) 
							// thats do i always want this the same as the space/span between quantized points? 



	public Awesonator() {
		float f = 0.06f; 
		scale = new Vector3(f, f, f);
	}

	public void Update() {
		if (gridSurface == null)
			gridSurface = GameObject.Find("TopDownPlane");
		


		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		currCursor = currRayIntersection();
		scanForNearestWallEdge();

		if (nearbyWallEdge != null) {
			// highlight it 
			scale.y *= 50f;
			nearbyWallEdge.GO.transform.localScale = scale;
			scale.y /= 50f;
			nearbyWallEdge.GO.renderer.material.color = S.ShoutyPurple;
			nearbyWallEdge.GO.transform.Rotate(new Vector3(0, Time.deltaTime, 0));
		}




		if (CcInput.Started(UserAction.Activate)) {
			begin = currRayIntersection();
		}
		
		if (CcInput.Ended(UserAction.Activate)) {
			end = currRayIntersection();

			makeWall();			
		}
	}
	
	Vector3? currRayIntersection() {
		RaycastHit rh;
		float distance = 100f;

		if (gridSurface.collider.Raycast(ray, out rh, distance))
			return rh.point;

		return null;
	}

	// what i'm trying to do here is have a per-frame cast, that turns nearby wallpoint into a highlighted color 
	void scanForNearestWallEdge() {
		if (currCursor == null)
			return;

		// if there was a hit, i would do the below 
		closest = null;
		foreach (var w in walls) {
			if (closest == null) {
				closest = w;
			}else{
				var distToHit /*'''*/ = Vector3.Distance(w.Pos, (Vector3)currCursor);
				var distToCurrClosest = Vector3.Distance(w.Pos, closest.Pos);
				
				if (distToHit < distToCurrClosest)
					closest = w;
			}				
		}
		
		if (closest != null && Vector3.Distance(closest.Pos, (Vector3)currCursor) < threshold)
			NearbyWallEdge = closest;
		else
			NearbyWallEdge = null;
	}
	
	WallEdge maybeMakeBUILDStyleWall() {
		Vector3 newPos;
		
		RaycastHit rh;
		float distance = 100f;
		if (gridSurface.collider.Raycast(ray, out rh, distance)) {
			// place precisely at an existing wall location if within a threshold 
			float threshold = 0.2f;
			WallEdge cc = null; // current closest wall point 
			foreach (var w in walls) {
				if (cc == null) {
					cc = w;
				}else{
					var distToHit /*'''*/ = Vector3.Distance(w.Pos, rh.point);
					var distToCurrClosest = Vector3.Distance(w.Pos, cc.Pos);
					
					if (distToHit < distToCurrClosest)
						cc = w;
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
			var nw = new WallEdge(newPos, prevWall, nc);
			walls.Add(nw);
			return nw;
		}
		
		return null;
	}

	void makeWall() {
		var nw = maybeMakeBUILDStyleWall();

		// if both points of line segment are valid, and far enough from each other 
		if (null != begin 
		    && 
		    null != end 
		    &&
		    Vector3.Distance(
				(Vector3)begin, 
				(Vector3)end) 
		    < 
		    threshold
	    ) {
			// make a new quad 
			var nq = GameObject.CreatePrimitive (PrimitiveType.Quad);
			var delta = nw.Pos - prevWall.Pos;
			var p = prevWall.Pos + delta / 2;
			p.y += 2f;
			nq.transform.position = p;
			nq.transform.right = delta;
			var f = 4f;
			var v = new Vector3 (delta.magnitude, f, f);
			nq.transform.localScale = v;
			v /= 4f;
			nq.renderer.material = Mats.Get ("sci_fi_003");
			nq.renderer.material.mainTextureScale = v;
			begin = null;
			end = null;
		}
	}
}
