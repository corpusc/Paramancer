using UnityEngine;
using System.Collections;



public struct Vec2i {
	public int x;
	public int z;
};

public class Cell {
	public Material MatWalls;
	public Material MatFloor;
	public Material MatCeiling;
	public Material MatLip;
	public bool IsAir = false; 
	public bool Floor = false; // for holes connecting levels 
	public char CeilingHeight = (char)0;
	public bool NeedsNWLip = true;
	public bool NeedsNELip = true;
	public bool NeedsSELip = true;
	public bool NeedsSWLip = true;
}

// BUILDinator style (arbitrary line segment rimmed) maps 
public class WallPoint {
	public Vector3 Pos;
	public WallPoint Next;
	public GameObject GO;
	
	public WallPoint(Vector3 pos, WallPoint next, GameObject go) {
		Pos = pos;
		Next = next;
		GO = go;
	}
}
