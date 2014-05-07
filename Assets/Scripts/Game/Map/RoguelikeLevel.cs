using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Vec2i {
	public int x;
	public int y;
};

public class RoguelikeLevel : ScriptableObject {
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

	public Cell[,] Cells;
	public Vec2i MapSize;
	public int Forms = 50; // the amount of rooms/hallways to create 
	public float MaxOverride = 0.2f; // only create a form if there aren't too many things already in there 
	public int MinFormWidth = 16;
	public int MaxFormWidth = 500;
	public int MaxArea = 10000; // limits the creation of extremely large rooms, favorizes corridors
	public Vector3 Pos = Vector3.zero;
	public Vector3 Scale = Vector3.one;
	public int MinHeight = 2; // the minimal height a room can have
	public int MaxHeight = 5; // the maximal height a room can have
	public bool CreateOverhangs = true;

	public Material MetalFloor;
	public Material MetalGroovedEdges;
	public Material MetalWithRivets;
	public Material SciFiMat;

	// private 
	int numTries = 50000; // ...before Build() gives up 
	
	List<Material> floors = new List<Material>();
	List<Material> walls = new List<Material>();
	List<Material> ceilings = new List<Material>();
	List<Material> lips = new List<Material>();

	char currH; // current cell height 
	Cell neiN; // current neighbor 
	Cell neiS;
	Cell neiE;
	Cell neiW;
	Cell neiNW;
	
	// lips of passageway/mouth 
	float thickMAX = 1 / 8f;
	float thinMAX = 1 / 16f;



	public void Init () {
		Cells = new Cell[MapSize.x, MapSize.y];

		MetalFloor = (Material)Resources.Load("Mat/Allegorithmic/metal_floor_003", typeof(Material));
		MetalGroovedEdges = (Material)Resources.Load("Mat/Allegorithmic/metal_plate_005", typeof(Material));
		MetalWithRivets = (Material)Resources.Load("Mat/Allegorithmic/metal_plate_008", typeof(Material));
		SciFiMat = (Material)Resources.Load("Mat/Allegorithmic/sci_fi_003", typeof(Material));

		ceilings.Add(MetalFloor);
		ceilings.Add(MetalGroovedEdges);
		ceilings.Add(MetalWithRivets);
		ceilings.Add(SciFiMat);

		walls.Add(SciFiMat);
		walls.Add(MetalGroovedEdges);
		walls.Add(MetalWithRivets);

		floors.Add(MetalFloor);
		floors.Add(MetalGroovedEdges);
		floors.Add(SciFiMat);

		lips.Add(MetalFloor);
		lips.Add(MetalWithRivets);

		EmptyMap();
		Build();
	}

	public void Build () {
		preBuild();

		int formsMade = 0;
		for (int i = 0; i < numTries && formsMade < Forms; i++) {
			Vec2i t;
			t.x = Random.Range(0, MapSize.x);
			t.y = Random.Range(0, MapSize.y);

			Vec2i u;
			u.x = Random.Range(0, MapSize.x);
			u.y = Random.Range(0, MapSize.y); // this is last-exclusive, hence all the <= and ...+ 1 later on 
			
			Vec2i start;
			Vec2i end;
			start.x = Mathf.Min(t.x, u.x);
			start.y = Mathf.Min(t.y, u.y);
			end.x = Mathf.Max(t.x, u.x);
			end.y = Mathf.Max(t.y, u.y);
			if (containsBlocks(start, end)) {
				if ((end.x >= start.x + MinFormWidth) && (end.y >= start.y + MinFormWidth))
				if ((end.x <= start.x + MaxFormWidth) && (end.y <= start.y + MaxFormWidth))
				if ((end.x - start.x + 1) * (end.y - start.y + 1) <= MaxArea)

					if (blocksInRect(start, end) < (end.x - start.x + 1) * (end.y - start.y + 1) * MaxOverride) {
						fillRect(start, end);
					formsMade++;
					}
			}
		}
	}

	void preBuild() {
		Vec2i a;
		a.x = Random.Range(0, MapSize.x - MaxFormWidth);
		a.y = Random.Range(0, MapSize.y - MaxFormWidth);
		Vec2i b;
		b.x = Random.Range(a.x + MinFormWidth, a.x + MaxFormWidth);
		b.y = Random.Range(a.y + MinFormWidth, a.y + MaxFormWidth);
		fillRect(a, b);
	}

	bool containsBlocks (Vec2i start, Vec2i end) {
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Cells[i, j].IsAir) return true;

		return false;
	}

	int blocksInRect (Vec2i start, Vec2i end) {
		int c = 0;
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Cells[i, j].IsAir) c++;

		return c;
	}

	void fillRect(Vec2i start, Vec2i end) {
		start.x = Mathf.Clamp(start.x, 0, MapSize.x - 1);
		start.y = Mathf.Clamp(start.y, 0, MapSize.y - 1);
		end.x = Mathf.Clamp(end.x, 0, MapSize.x - 1);
		end.y = Mathf.Clamp(end.y, 0, MapSize.y - 1); // because sometimes MaxFormWidth > MapSize

		// random heights and materials 
		char height = (char)Random.Range(MinHeight, MaxHeight);
		var floo = floors[Random.Range(0, floors.Count)];
		var ceil = ceilings[Random.Range(0, ceilings.Count)];
		var lip = lips[Random.Range(0, lips.Count)];
		var wall = walls[Random.Range(0, walls.Count)];
		while (wall == lip)
			wall = walls[Random.Range(0, walls.Count)];

		// set all cells in rect 
		for (int i = start.x; i <= end.x; i++)
		for (int j = start.y; j <= end.y; j++) {
			Cells[i, j].IsAir = true;
			if (CreateOverhangs)
				Cells[i, j].CeilingHeight = height;
			else if (height >= Cells[i, j].CeilingHeight)
				Cells[i, j].CeilingHeight = height;
			Cells[i, j].Floor = true;
			Cells[i, j].MatWalls = wall;
			Cells[i, j].MatFloor = floo;
			Cells[i, j].MatCeiling = ceil;
			Cells[i, j].MatLip = lip;
		}
	}

	public void EmptyMap() {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) 
			Cells[i, j] = new Cell();
	}
	
	public bool GetBlock(int x, int y) { 
		// only difference between adressing blocks as an array is that this will return false if out of the map 
		if (x < 0 || x >= MapSize.x) return true;
		if (y < 0 || y >= MapSize.y) return true;
		//MonoBehaviour.print("Test passed, x = " + x.ToString() + " y = " + y.ToString()); 
		return Cells[x, y].IsAir;
	}
	
	
	public Cell GetCell(int x, int y) { 
		// only difference between adressing blocks as an array is that this will return false if out of the map 
		if (x < 0 || x >= MapSize.x)   return new Cell();
		if (y < 0 || y >= MapSize.y)   return new Cell();

		return Cells[x, y];
	}
	
	void buildWall(Vector3 offset,
	               Vector3 ori, // orientation/direction 
	               Vector3 scale, 
	               char orH, // other room ceiling height 
	               int i,
	               int j, 
	               Vector2 uvScale, 
	               bool xIsThinner = false) {

		char pwH; // partial wall height 
		char sH; // space height 

		if (orH < currH) {
				pwH = (char)(currH - orH);
				sH = (char)(currH - pwH);

			var v = new Vector3(
				(float)i * Scale.x + offset.x / 2f, 
				(float)pwH * Scale.y / 2f + Scale.y * sH,
				(float)j * Scale.z + offset.z / 2f);

			var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
			np.transform.localScale = new Vector3(scale.x, scale.y * pwH, scale.z);
			np.transform.position = Pos + v;
			np.transform.forward = ori;
			np.renderer.material = Cells[i, j].MatWalls;

			if (uvScale == Vector2.zero) { // no offset 
				uvScale = new Vector2(1, pwH); // (normal full cell thickness) 
			}else{
				uvScale.y = pwH;
				var uo = new Vector2(thickMAX*3, 0f); // UV offset   (FIXME? hardwired to a particular quadrant where rivets spanned the entirety) 
				if (xIsThinner)
					uo.x += thickMAX/4; // FIXME?  hardwired
				np.renderer.material = Cells[i, j].MatLip;
				np.renderer.material.mainTextureOffset = uo;
				np.renderer.material.SetTextureOffset("_BumpMap", uo);
			}

			np.renderer.material.mainTextureScale = uvScale;
			np.renderer.material.SetTextureScale("_BumpMap", uvScale);
		}
	}

	void setNeighborOffsets(int i, int j) {
		neiN = GetCell(i, j+1);
		neiS = GetCell(i, j-1);
		neiE = GetCell(i+1, j);
		neiW = GetCell(i-1, j);
		//neiNW = GetCell(i-1, j+1);
	}

	public void Build3D() {
		// flag corners that need half of a lip 
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) {
			Cell c = Cells[i, j];
			currH = c.CeilingHeight;
			setNeighborOffsets(i, j);


			// hmmmmmm....does this make sense?  we're making a partial height 
			// in adjacent cubes based on the current cell of the actual building stage 
			if (c.IsAir) {
			}
		}

		// now do actual building 
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) {
			Cell c = Cells[i, j];
			currH = c.CeilingHeight;
			setNeighborOffsets(i, j);


			// imagine this, as if the passageway "mouth" is facing you (it's not always an absolute axis aligned orientation) 

			// make walls 
			// (each orientation only handles the lip on the right edge of its face) 
			if (c.IsAir) {
				// west 
				if (neiW != null) {
					buildWall(new Vector3(-Scale.x, 0, 0), 
					          Vector3.left, Scale, // main wall 
					          neiW.CeilingHeight, i, j,				     
					          Vector2.zero);

					if (c.NeedsNWLip) {
						buildWall(
							new Vector3(
								-Scale.x + Scale.x*thinMAX*2, 
								0, 
								Scale.z - Scale.z*thickMAX + thickMAX*Scale.z), 
							Vector3.left, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiW.CeilingHeight, i, j,
							new Vector2(thickMAX, 0));

						buildWall(
							new Vector3(
								-Scale.x + Scale.x*thinMAX, 
								0, 
								Scale.z - Scale.z*thickMAX*2 + thickMAX*Scale.z), 
							Vector3.forward,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiW.CeilingHeight, i, j,					          
							new Vector2(thinMAX, 0));
					}

					if (c.NeedsSELip) { // (SE corner of NW cell!) must offset it to the correct cell 
						buildWall(
							new Vector3(
								-Scale.x - Scale.x*thickMAX + thickMAX*Scale.x, 
								Scale.y, 
								Scale.z + Scale.z*thickMAX),
							Vector3.back, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiW.CeilingHeight, i, j,
							new Vector2(thickMAX, 0));

						buildWall(
							new Vector3(
								-Scale.x - Scale.x*thickMAX*2 + thickMAX*Scale.x,
								Scale.y, 
								Scale.z + Scale.z*thickMAX/2),
							Vector3.right,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiW.CeilingHeight, i, j,
							new Vector2(thinMAX, 0));
					}
				}


				
				// east 
				if (neiE != null) {
					buildWall(new Vector3(+Scale.x, 0, 0), 
					          Vector3.right, Scale, 
					          neiE.CeilingHeight, i, j,				          
					          Vector2.zero);

					if (c.NeedsSELip) {
						buildWall(
							new Vector3(
								Scale.x - Scale.x*thinMAX*2, 
								0, 
								-Scale.z + Scale.z*thickMAX - thickMAX*Scale.z), 
							Vector3.right, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiE.CeilingHeight, i, j,
							new Vector2(thickMAX, 0));
						
						buildWall(
							new Vector3(
								Scale.x - Scale.x*thinMAX, 
								0, 
								-Scale.z + Scale.z*thickMAX*2 - thickMAX*Scale.z), 
							Vector3.back,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiE.CeilingHeight, i, j,					          
							new Vector2(thinMAX, 0));
					}
					
					if (c.NeedsNWLip) {  // this was copied from NW
						buildWall(
							new Vector3(
								Scale.x + Scale.x*thickMAX - thickMAX*Scale.x, 
								Scale.y, 
								-Scale.z - Scale.z*thickMAX),
							Vector3.forward, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiE.CeilingHeight, i, j,
							new Vector2(thickMAX, 0));
						
						buildWall(
							new Vector3(
								Scale.x + Scale.x*thickMAX*2 - thickMAX*Scale.x,
								Scale.y, 
								-Scale.z - Scale.z*thickMAX/2),
							Vector3.left,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiE.CeilingHeight, i, j,
							new Vector2(thinMAX, 0));
					}
				}



				// north 
				if (neiN != null) {
					buildWall(new Vector3(0, 0, +Scale.z), 
					          Vector3.forward, Scale, 
					          neiN.CeilingHeight, i, j,
					          Vector2.zero);

					if (c.NeedsNELip) {
						buildWall(
							new Vector3(
							Scale.x, 
							0, 
							Scale.z - Scale.z*thickMAX), 
							Vector3.forward, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiN.CeilingHeight, i, j,
							new Vector2(thickMAX, 0), 
							true);
						
						buildWall(
							new Vector3(
							Scale.x - Scale.x*thickMAX, 
							0, 
							Scale.z - Scale.z*thinMAX), 
							Vector3.right,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiN.CeilingHeight, i, j,					          
							new Vector2(thinMAX, 0), 
							true);
					}
					
					if (c.NeedsSWLip) { // (SE corner of NW cell!) must offset it to the correct cell 
						buildWall(
							new Vector3(
							Scale.x + Scale.x*thinMAX, 
							Scale.y, 
							Scale.z + Scale.z*thickMAX),
							Vector3.back, 
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiN.CeilingHeight, i, j,
							new Vector2(thickMAX, 0), 
							true);
						
						buildWall(
							new Vector3(
							Scale.x + Scale.x*thickMAX,
							Scale.y, 
							Scale.z),
							Vector3.left,
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiN.CeilingHeight, i, j,
							new Vector2(thinMAX, 0), 
							true);
					}
				}



				// south 
				if (neiS != null) {
					buildWall(new Vector3(0, 0, -Scale.z), 
					          Vector3.back, Scale, 
					          neiS.CeilingHeight, i, j,				          
					          Vector2.zero);

					if (c.NeedsSWLip) {
						buildWall(
							new Vector3(
							-Scale.x, 
							0, 
							-Scale.z + Scale.z*thickMAX), 
							Vector3.back, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiS.CeilingHeight, i, j,
							new Vector2(thickMAX, 0), 
							true);
						
						buildWall(
							new Vector3(
							-Scale.x + Scale.x*thickMAX, 
							0, 
							-Scale.z + Scale.z*thinMAX), 
							Vector3.left,
							new Vector3(Scale.x*thinMAX, Scale.y, Scale.z),
							neiS.CeilingHeight, i, j,					          
							new Vector2(thinMAX, 0), 
							true);
					}
					
					if (c.NeedsNELip) {
						buildWall(
							new Vector3(
							-Scale.x, 
							Scale.y, 
							-Scale.z - Scale.z*thickMAX),
							Vector3.forward, 
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiS.CeilingHeight, i, j,
							new Vector2(thickMAX, 0), 
							true);
						
						buildWall(
							new Vector3(
							-Scale.x - Scale.x*thickMAX,
							Scale.y, 
							-Scale.z),
							Vector3.right,
							new Vector3(Scale.x*thickMAX, Scale.y, Scale.z),
							neiS.CeilingHeight, i, j,
							new Vector2(thickMAX, 0), 
							true);
					}
				}
			}

			// floor 
			if (Cells[i, j].Floor) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
				np.transform.forward = Vector3.down;
				np.transform.position = Pos + new Vector3(
					(float)i * Scale.x, 
					0f, 
					(float)j * Scale.z);
				np.transform.localScale = Scale;
				np.renderer.material = Cells[i, j].MatFloor;
			}
			
			// ceiling 
			if (currH > 0) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
				np.transform.forward = Vector3.up;
				np.transform.position = Pos + new Vector3(
					Scale.x * (float)i, 
					Scale.y * (float)currH, 
					Scale.z * (float)j);
				np.transform.localScale = Scale;
				np.renderer.material = Cells[i, j].MatCeiling;
			}
		} // done processing cells 
	} // end of Build3D()
}
