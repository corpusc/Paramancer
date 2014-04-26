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
		public bool Block = false; // if is true, then there is empty space at i, j 
		public bool Floor = false; // for holes connecting levels 
		public char Ceiling = (char)0;
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
	public int MaxArea = 10000; // limits the creation of extremely large rooms 
	public Vector3 Pos = Vector3.zero;
	public Vector3 Scale = Vector3.one;

	Material MetalFloor;
	Material MetalGroovedEdges;
	Material MetalWithRivets;
	Material SciFiMat;

	// private 
	List<Material> floors = new List<Material>();
	List<Material> walls = new List<Material>();
	List<Material> ceilings = new List<Material>();
	List<Material> lips = new List<Material>();
	char height; // current cell height 
	int safetyLimit = 50000; // the limit of tries Build() can do before giving up 



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
		for (int i = 0; i < safetyLimit && formsMade < Forms; i++) {
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
				if (Cells[i, j].Block) return true;

		return false;
	}

	int blocksInRect (Vec2i start, Vec2i end) {
		int c = 0;
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Cells[i, j].Block) c++;

		return c;
	}

	void fillRect(Vec2i start, Vec2i end) {
		start.x = Mathf.Clamp(start.x, 0, MapSize.x - 1);
		start.y = Mathf.Clamp(start.y, 0, MapSize.y - 1);
		end.x = Mathf.Clamp(end.x, 0, MapSize.x - 1);
		end.y = Mathf.Clamp(end.y, 0, MapSize.y - 1); // because sometimes MaxFormWidth > MapSize

		// random heights and materials 
		char height = (char)Random.Range(1, 5);
		var wall = walls[Random.Range(0, walls.Count)];
		var floo = floors[Random.Range(0, floors.Count)];
		var ceil = ceilings[Random.Range(0, ceilings.Count)];

		// set all cells in rect 
		for (int i = start.x; i <= end.x; i++)
		for (int j = start.y; j <= end.y; j++) {
			Cells[i, j].Block = true;
			Cells[i, j].Ceiling = height;
			Cells[i, j].Floor = true;
			Cells[i, j].MatWalls = wall;
			Cells[i, j].MatFloor = floo;
			Cells[i, j].MatCeiling = ceil;
		}
	}

	public void EmptyMap() {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) 
			Cells[i, j] = new Cell();
	}
	
	public bool GetBlock(int x, int y) { 
		// only difference between adressing blocks as an array is that this will return false if out of the map 
		if (x < 0 || x >= MapSize.x) return false;
		if (y < 0 || y >= MapSize.y) return false;
		//MonoBehaviour.print("Test passed, x = " + x.ToString() + " y = " + y.ToString()); 
		return Cells[x, y].Block;
	}
	
	
	public char GetCeilingHeight(int x, int y) { 
		// only difference between adressing blocks as an array is that this will return false if out of the map 
		if (x < 0 || x >= MapSize.x) return (char)0;
		if (y < 0 || y >= MapSize.y) return (char)0;
		return Cells[x, y].Ceiling;
	}
	
	void buildWall(Vector3 ori, // orientation/direction 
	               Vector3 scale, 
	               char orH, // other room ceiling height 
	               int i,
	               int j,
	               Vector3 offset,
	               bool uvOffset = false) {
		char pwH; // partial wall height 
		char sH; // space height 
		Vector2 uvScale;

		if (orH < height) {
			pwH = (char)(height - orH);
			sH = (char)(height - pwH);
			uvScale = new Vector2(1, pwH);
			var v = new Vector3(
				(float)i * Scale.x + offset.x / 2f, 
				(float)pwH * Scale.y / 2f + Scale.y * sH,
				(float)j * Scale.z + offset.z / 2f);

			var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
			np.transform.localScale = new Vector3(scale.x, scale.y * pwH, scale.z);
			np.transform.position = Pos + v;
			np.transform.forward = ori;
			np.renderer.material = Cells[i, j].MatWalls;
			if (uvOffset) {
				var uo = new Vector2(0.4f, 0f);
				np.renderer.material = lips[Random.Range(0, lips.Count)];
				np.renderer.material.mainTextureOffset = uo;
				np.renderer.material.SetTextureOffset("_BumpMap", uo);
				uvScale = new Vector2(0.2f, pwH);
			}
			np.renderer.material.mainTextureScale = uvScale;
			np.renderer.material.SetTextureScale("_BumpMap", uvScale);
		}
	}

	public void Build3D() {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) {
			Cell c = Cells[i, j];
			height = c.Ceiling;
			// imagine this, as if the passageway "mouth" is facing you (it's not always an absolute axis aligned orientation)
			float lwd = 8f; // lip width divisor, for passageway frame 
			float ldd = 16f; // lip depth divisor, for passageway frame 

			// make walls 
			// each orientation only handles the lip on the right edge of its face 
			if (c.Block) {
				// west 
				buildWall(Vector3.left, Scale, // main wall 
				          GetCeilingHeight(i-1, j),  i, j,
				          new Vector3(-Scale.x, 0, 0));

				if (c.NeedsNWLip) {
					buildWall(Vector3.left, 
					          new Vector3(Scale.x/lwd, Scale.y, Scale.z),
					          GetCeilingHeight(i-1, j), i, j,
					          new Vector3(-Scale.x + Scale.x/ldd*2, 0, Scale.z - Scale.z/lwd), 
					          true);
					buildWall(Vector3.forward,
					          new Vector3(Scale.x/ldd, Scale.y, Scale.z),
					          GetCeilingHeight(i-1, j), i, j,
					          new Vector3(-Scale.x + Scale.x/ldd, 0, Scale.z - Scale.z/lwd*2), 
					          true);
				}
				if (c.NeedsSELip) { // COPY OF NW ATM!!!!!!!!!!!!!!!!!!!!!!!
					buildWall(Vector3.back, 
					          new Vector3(Scale.x/lwd, Scale.y, Scale.z),
					          GetCeilingHeight(i-1, j), i, j,
					          new Vector3(Scale.x - Scale.x/lwd, 0, -Scale.z + Scale.z/lwd), 
					          true);
					buildWall(Vector3.right,
					          new Vector3(Scale.x/ldd, Scale.y, Scale.z),
					          GetCeilingHeight(i-1, j), i, j,
					          new Vector3(Scale.x -Scale.x/lwd*2, 0, -Scale.z + Scale.z/lwd/2), 
					          true);
				}

				
				// east 
				buildWall(Vector3.right, Scale, 
				          GetCeilingHeight(i+1, j), i, j,
				          new Vector3(+Scale.x, 0, 0));


				// south 
				buildWall(Vector3.back, Scale, 
				          GetCeilingHeight(i, j-1), i, j,
				          new Vector3(0, 0, -Scale.z));


				// north 
				buildWall(Vector3.forward, Scale, 
				          GetCeilingHeight(i, j+1), i, j,
				          new Vector3(0, 0, +Scale.z));
			}

			// floor 
			if (Cells[i, j].Floor) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
				np.transform.up = Vector3.up; // a weird thing with planes, their forward is not the direction they're facing 
				np.transform.position = Pos + new Vector3(
					(float)i * Scale.x, 
					0f, 
					(float)j * Scale.z);
				np.transform.localScale = Scale * 0.1f; // for some reason, planes start as a 10x10 square, hence the 0.1 
				np.renderer.material = Cells[i, j].MatFloor;
			}
			
			// ceiling 
			if (height > 0) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
				np.transform.up = Vector3.down;
				np.transform.position = Pos + new Vector3(
					Scale.x * (float)i, 
					Scale.y * (float)height, 
					Scale.z * (float)j);
				np.transform.localScale = Scale * 0.1f;
				np.renderer.material = Cells[i, j].MatCeiling;
			}
		} // done processing cells 
	} // end of Build3D()
}
