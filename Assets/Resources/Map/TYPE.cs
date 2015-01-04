using UnityEngine;
using System.Collections.Generic;



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

// BUILDinator style maps (arbitrary line-segment-rimmed sectors) 
public class WallEdge {
	public Vector3 Pos;
	public WallEdge Next; // shouldn't need this, cuz the closed loop of wall indexes should be in Sector 
	// however, we DO need the index of the adjacent sector on the other side of this wall 
	public GameObject GO;
	
	public WallEdge(Vector3 pos, WallEdge next, GameObject go) {
		Pos = pos;
		Next = next;
		GO = go;
	}
}

public class Tetragon { // we'll replace quads with..... say.... can quads vertices be reconfigured to a non perfect cube? 
	// anyways..... we'll need 5 vertices, so we don't get that UV compression only in one triangle artifact 
	public int Begin = int.MaxValue;
	public int End = int.MaxValue;
}

public class Sector {
	public List<int> WallIndices = new List<int>();
}
