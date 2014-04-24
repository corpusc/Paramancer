using UnityEngine;
using System.Collections;

public struct Vec2i {
	public int x;
	public int y;
};

public class RoguelikeLevel : ScriptableObject {
	public bool[,] Block; // 2d array
	// if Block[i, j] is true, then there is empty space at i, j
	public TileType[,] Type;
	public bool[,] Floor; // for holes connecting levels
	public char[,] Ceiling;
	public Vec2i MapSize;
	public int Forms = 50; // the amount of rooms/hallways to create
	public float MaxOverride = 0.2f; // only create a form if there aren't too many things already in there
	public int MinFormWidth = 16;
	public int MaxFormWidth = 500;
	public int MaxArea = 10000; // limits the creation of extremely large rooms
	public Vector3 Pos = Vector3.zero;
	public Vector3 Scale = Vector3.one;

	Material BricksMat;
	Material ConcreteMat;
	Material GrayMat;
	Material MetalMat;
	Material MetalMat2;
	Material MetalMat3;
	Material SciFiMat;
	Material WoodMat;

	int safetyLimit = 50000; // the limit of tries Build() can do before surrendering

	public void Init () {
		Block = new bool[MapSize.x, MapSize.y];
		Floor = new bool[MapSize.x, MapSize.y];
		Ceiling = new char[MapSize.x, MapSize.y];
		Type = new TileType[MapSize.x, MapSize.y];

		BricksMat = (Material)Resources.Load("Mat/Allegorithmic/BrickWall_02", typeof(Material));
		ConcreteMat = (Material)Resources.Load("Mat/Allegorithmic/concrete_049", typeof(Material));
		GrayMat = (Material)Resources.Load("Mat/IceFlame/GrayTileBlueMat", typeof(Material));
		MetalMat = (Material)Resources.Load("Mat/Allegorithmic/metal_floor_003", typeof(Material));
		MetalMat2 = (Material)Resources.Load("Mat/Allegorithmic/metal_plate_005", typeof(Material));
		MetalMat3 = (Material)Resources.Load("Mat/Allegorithmic/metal_plate_008", typeof(Material));
		SciFiMat = (Material)Resources.Load("Mat/Allegorithmic/sci_fi_003", typeof(Material));
		WoodMat = (Material)Resources.Load("Mat/Allegorithmic/Wood_Planks_01", typeof(Material));

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
				if (Block[i, j]) return true;

		return false;
	}

	int blocksInRect (Vec2i start, Vec2i end) {
		int c = 0;
		for (int i = start.x; i <= end.x; i++)
			for (int j = start.y; j <= end.y; j++)
				if (Block[i, j]) c++;

		return c;
	}

	void fillRect (Vec2i start, Vec2i end) {
		start.x = Mathf.Clamp(start.x, 0, MapSize.x - 1);
		start.y = Mathf.Clamp(start.y, 0, MapSize.y - 1);
		end.x = Mathf.Clamp(end.x, 0, MapSize.x - 1);
		end.y = Mathf.Clamp(end.y, 0, MapSize.y - 1); // because sometimes MaxFormWidth > MapSize

		char height = (char)Random.Range(1, 3);
		TileType currTile = (TileType)Random.Range(0, (int)TileType.Count);
		for (int i = start.x; i <= end.x; i++)
		for (int j = start.y; j <= end.y; j++) {
			Block[i, j] = true;
			Ceiling[i, j] = height;
			Floor[i, j] = true;
			Type[i, j] = currTile;
		}
	}

	public void EmptyMap () {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) {
			Block[i, j] = false;
			Ceiling[i, j] = (char)0;
			Floor[i, j] = false;
			Type[i, j] = 0;
		}
	}

	public bool GetBlock(int x, int y) { 
		// only difference between adressing blocks as an array is that this will return false if out of the map 
		if (x < 0 || x >= MapSize.x) return false;
		if (y < 0 || y >= MapSize.y) return false;
		//MonoBehaviour.print("Test passed, x = " + x.ToString() + " y = " + y.ToString()); 
		return Block[x, y];
	}

	Material GetMat (TileType cType) { // current 
		switch (cType) {
		case TileType.Bricks:
			return BricksMat;
			break;
		case TileType.Concrete:
			return ConcreteMat;
			break;
		case TileType.Gray:
			return GrayMat;
			break;
		case TileType.Metal:
			return MetalMat;
			break;
		case TileType.Metal2:
			return MetalMat2;
			break;
		case TileType.Metal3:
			return MetalMat3;
			break;
		case TileType.SciFi:
			return SciFiMat;
			break;
		case TileType.Wood:
			return WoodMat;
			break;
		default:
			return BricksMat;
			break;
		}
	}

	public void Build3D () {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++) {
			if (Floor[i, j]) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
				np.transform.up = Vector3.up; // a weird thing with planes, their forward is not the direction they're facing
				np.transform.position = Pos + new Vector3((float)i * Scale.x, 0f, (float)j * Scale.z);
				np.transform.localScale = Scale * 0.1f; // for some reason, planes start as a 10x10 square, hence the 0.1
				np.renderer.material = GetMat(Type[i, j]);
			}

			char height = Ceiling[i, j];
			if (height > 0) {
				var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
				np.transform.up = Vector3.down;
				np.transform.position = Pos + new Vector3(
					Scale.x * (float)i, 
					Scale.y * (float)height, 
					Scale.z * (float)j);
				np.transform.localScale = Scale * 0.1f;
				np.renderer.material = GetMat(Type[i, j]);
			}

			if (Block[i, j]) { // walls 
				if (!GetBlock(i - 1, j)) {
//					var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
//					np.transform.up = Vector3.right;
//					np.transform.position = Pos + new Vector3((float)i * Scale.x - Scale.x / 2f, (float)Scale.y / 2f, (float)j * Scale.z);
//					np.transform.localScale = new Vector3(Scale.y, 1f, Scale.z) * 0.1f;
//					np.renderer.material = GetMat(Type[i, j]);
					var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
					np.transform.forward = Vector3.left;
					np.transform.position = Pos + new Vector3(
						(float)i * Scale.x - Scale.x / 2f, 
						(float)height * Scale.y / 2f, 
						(float)j * Scale.z);
					np.transform.localScale = new Vector3(Scale.x, Scale.y * height, Scale.z);//new Vector3(Scale.x, 1f, Scale.y) * 0.1f;
					np.renderer.material = GetMat(Type[i, j]);
				}
				
				if (!GetBlock(i + 1, j)) {
//					var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
//					np.transform.up = Vector3.left;
//					np.transform.position = Pos + new Vector3((float)i * Scale.x + Scale.x / 2f, (float)Scale.y / 2f, (float)j * Scale.z);
//					np.transform.localScale = new Vector3(Scale.y, 1f, Scale.z) * 0.1f;
//					np.renderer.material = GetMat(Type[i, j]);
					var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
					np.transform.forward = Vector3.right;
					np.transform.position = Pos + new Vector3(
						(float)i * Scale.x + Scale.x / 2f, 
						(float)height * Scale.y / 2f, 
						(float)j * Scale.z);
					np.transform.localScale = new Vector3(Scale.x, Scale.y * height, Scale.z);//new Vector3(Scale.x, 1f, Scale.y) * 0.1f;
					np.renderer.material = GetMat(Type[i, j]);
				}
				
				if (!GetBlock(i, j - 1)) {
					var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
					np.transform.forward = Vector3.back;
					np.transform.position = Pos + new Vector3(
						(float)i * Scale.x, 
						(float)height * Scale.y / 2f, 
						(float)j * Scale.z - Scale.z / 2f);
					np.transform.localScale = new Vector3(Scale.x, Scale.y * height, Scale.z);//new Vector3(Scale.x, 1f, Scale.y) * 0.1f;
					np.renderer.material = GetMat(Type[i, j]);
				}
				
				if (!GetBlock(i, j + 1)) {
//					var np = GameObject.CreatePrimitive(PrimitiveType.Plane);
//					np.transform.up = Vector3.back;
//					np.transform.position = Pos + new Vector3((float)i * Scale.x, (float)Scale.y / 2f, (float)j * Scale.z + Scale.z / 2f);
//					np.transform.localScale = new Vector3(Scale.x, 1f, Scale.y) * 0.1f;
//					np.renderer.material = GetMat(Type[i, j]);
					var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
					//np.transform.forward = Vector3.forward;
					np.transform.position = Pos + new Vector3(
						(float)i * Scale.x, 
						(float)height * Scale.y / 2f, 
						(float)j * Scale.z + Scale.z / 2f);
					np.transform.localScale = new Vector3(Scale.x, Scale.y * height, Scale.z);//new Vector3(Scale.x, 1f, Scale.y) * 0.1f;
					np.renderer.material = GetMat(Type[i, j]);
				}
			} // end of walls
		} // end of loop
	} // end of Build3D()
}
