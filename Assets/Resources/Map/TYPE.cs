using UnityEngine;



public struct Vec2i {
	public int x;
	public int z;
};

public struct Vec3i {
	public int X;
	public int Y;
	public int Z;
};

public class Surfaces {
	public Material Lip; // the mouldings that line corners 
	public Material Walls;
	public Material Floor;
	public Material Ceiling;
};

public class VoxelRoom {
	public Vec3i Pos;
	public Vec3i Size;
	public Surfaces Surfaces;
};

public class Cell {
	// GenGrid ONLY! 
	public char CeilingHeight = (char)0;
	public bool NeedsNWLip = true;
	public bool NeedsNELip = true;
	public bool NeedsSELip = true;
	public bool NeedsSWLip = true;
	// *** end of GenGrid *** 

	public Surfaces Surfaces;
	public bool IsAir; 
	public bool NorthWall;
	public bool SouthWall;
	public bool EastWall;
	public bool WestWall;
	public bool Ceiling;
	public bool Floor;
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
