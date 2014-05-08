using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Vec3i {
	public int x;
	public int y;
	public int z;
};

public class RoguelikeMap : ScriptableObject {

	public bool[,,] Block; // true = the block is open, false = unreachable(wall etc)
	public Material[,,] Mat;
	public Vec3i MapSize; // this is the resolution
	public int Forms = 40; // the amount of rooms/halls to create on the ground floor
	public int FormsPerFloor = 5; // the amount of corridors/halls that connect alll rooms reaching the given height, per floor
	public float MaxOverride = 0.2f; // only create a form if there aren't too many things already in there 
	public int MinFormWidth = 1;
	public int MaxFormWidth = 16;
	public int MaxArea = 100; // limits the creation of extremely large rooms, favorizes corridors
	public int MaxCorridorArea = 50; // for corridors/bridges above the ground floor
	public Vector3 Pos = Vector3.zero; // the position of the min-coordinate corner of the generated map
	public Vector3 Scale = Vector3.one; // the scale of all elements on the map
	public int MinHeight = 2; // the minimal height a room can have
	public int HeightRand = 4; // the maximal additional room height(minimal is 0)(last-exclusive, so set to 1 for no randomness)
	public int CorridorHeightRand = 2; // same as above, but for corridors
	public bool MapIsOpen = false; // used to control whether corridors reaching the border of the map are to be closed
	// the maximal height of a room is determined by the map height & the room size
	// This assumes the values you passed make sense, ie you didn't make MinFormWidth > MaxFormWidth
	// WARNING: MaxFormWidth must be lesser than MapSize.x and MapSize.z, and MinHeight + HeightRand must be lesser than MapSize.y
	// To create a map, set all the values you need, and call Init(), Build() and Build3d()

	// private
	int numTries = 5000; // the amount of attempts to place a room to be done before giving up (used for safety to avoid an infinite loop)
	List<Material> MatPool = new List<Material>();

	// only sets everything up, doesn't build the level - call Build () and Build3d () manually
	public void Init () {
		Block = new bool[MapSize.x, MapSize.y, MapSize.z];
		Mat = new Material[MapSize.x, MapSize.y, MapSize.z];
		MatPool.Add ((Material)Resources.Load("Mat/Allegorithmic/metal_floor_003", typeof(Material)));
		MatPool.Add ((Material)Resources.Load("Mat/Allegorithmic/metal_plate_005", typeof(Material)));
		MatPool.Add ((Material)Resources.Load("Mat/Allegorithmic/metal_plate_008", typeof(Material)));
		MatPool.Add ((Material)Resources.Load("Mat/Allegorithmic/sci_fi_003", typeof(Material)));
		MatPool.Add ((Material)Resources.Load("Mat/Allegorithmic/Stones_01", typeof(Material)));
	}

	//this will build a model of the level in memory, to generate the 3d model in the scene, use Build3d ()
	public void Build () {
		//MonoBehaviour.print("Build() has been called.");
		emptyMap();
		preBuild();

		// start off by creating the ground floor(the main map)
		int formsMade = 0;
		for (int i = 0; i < numTries && formsMade < Forms; i++) {
			Vec2i t;
			t.x = Random.Range(0, MapSize.x);
			t.z = Random.Range(0, MapSize.z);
			
			Vec2i u;
			u.x = Random.Range(0, MapSize.x);
			u.z = Random.Range(0, MapSize.z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
			
			Vec2i start;
			Vec2i end;
			start.x = Mathf.Min(t.x, u.x);
			start.z = Mathf.Min(t.z, u.z);
			end.x = Mathf.Max(t.x, u.x);
			end.z = Mathf.Max(t.z, u.z);
			if (containsBlocks(start, end)) {
				if ((end.x >= start.x + MinFormWidth) && (end.z >= start.z + MinFormWidth))
					if ((end.x <= start.x + MaxFormWidth) && (end.z <= start.z + MaxFormWidth))
						if (getArea(start, end) <= MaxArea)

						if (countBlocks(start, end) < getArea(start, end) * MaxOverride) {
							fillRect(start, end);
							formsMade++;
						}
			}
		} // end of ground floor creation

		//MonoBehaviour.print("Created " + formsMade.ToString() + " forms.");

		//...and then add bridges and corridors higher up
		for (int h = Random.Range(MinHeight, MinHeight + HeightRand); h < MapSize.y - MinHeight - HeightRand; h += MinHeight + HeightRand + 1) { // h is the height of the floor
			formsMade = 0;
			for (int i = 0; i < numTries && formsMade < FormsPerFloor; i++) {
				Vec2i t;
				t.x = Random.Range(0, MapSize.x);
				t.z = Random.Range(0, MapSize.z);
				
				Vec2i u;
				u.x = Random.Range(0, MapSize.x);
				u.z = Random.Range(0, MapSize.z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
				
				Vec2i start;
				Vec2i end;
				start.x = Mathf.Min(t.x, u.x);
				start.z = Mathf.Min(t.z, u.z);
				end.x = Mathf.Max(t.x, u.x);
				end.z = Mathf.Max(t.z, u.z);
				if (containsBlocks(start, end, h + 1)) {
					if ((end.x >= start.x + MinFormWidth) && (end.z >= start.z + MinFormWidth))
						if ((end.x <= start.x + MaxFormWidth) && (end.z <= start.z + MaxFormWidth))

						// no MaxOverride check here because we want bridges to generate
						if (getArea(start, end) <= MaxCorridorArea) {
							putWall(start, end, h);
							fillRect(start, end, h);
							formsMade++;
						}
				}
				MonoBehaviour.print("Created " + formsMade.ToString() + " corridors");
			} // end of the creation of a single corridor/bridge
		} // end of corridor/bridge creation

	} // end of Build ()

	// this generates the level in 3d(puts it in the scene) based what Build() created - call Build() before this
	public void Build3d () {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++)
		for (int k = 0; k < MapSize.z; k++) {
			// only build walls around a block if it's not a wall itself
			if (Block[i, j, k]) {
					if (!getBlock(i - 1, j, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i - Scale.x * 0.5f, Scale.y * j, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.left;
						np.renderer.material = Mat[i, j, k];
					}
					if (!getBlock(i + 1, j, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i + Scale.x * 0.5f, Scale.y * j, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.right;
						np.renderer.material = Mat[i, j, k];
					}
					if (!getBlock(i, j, k - 1)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j, Scale.z * k - Scale.z * 0.5f);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.back;
						np.renderer.material = Mat[i, j, k];
					}
					if (!getBlock(i, j, k + 1)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j, Scale.z * k + Scale.z * 0.5f);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.forward;
						np.renderer.material = Mat[i, j, k];
					}
					if (!getBlock(i, j - 1, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j - Scale.y * 0.5f, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.down;
						np.renderer.material = Mat[i, j, k];
					}
					if (!getBlock(i, j + 1, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j + Scale.y * 0.5f, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.up;
						np.renderer.material = Mat[i, j, k];
					}
			} // end of wall creation for the current block
		} // end of block scan
	} // end of Build3d()

	// private methods

	void emptyMap () {
		for (int i = 0; i < MapSize.x; i++)
		for (int j = 0; j < MapSize.y; j++)
		for (int k = 0; k < MapSize.z; k++) {
				Block[i, j, k] = false;
		}
	}

	// used so that there is a single room in the level
	void preBuild () {
		Vec3i a;
		a.x = Random.Range(0, MapSize.x - MaxFormWidth);
		a.y = 0;
		a.z = Random.Range(0, MapSize.z - MaxFormWidth);
		//a.x = 0;
		//a.y = 0;
		//a.z = 0;
		Vec3i b;
		b.x = Random.Range(a.x + MinFormWidth, a.x + MaxFormWidth);
		b.y = Random.Range(MinHeight, MinHeight + HeightRand);
		b.z = Random.Range(a.z + MinFormWidth, a.z + MaxFormWidth);
		//b.x = 0;
		//b.y = 0;
		//b.z = 0;
		//MonoBehaviour.print("area of room in preBuild() = " + getArea(a, b));
		fillRect(a, b);
	}

	// sets blocks to true(opens them to create rooms) and sets their material
	void fillRect (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		//int t = 0;
		//MonoBehaviour.print("e.z = " + e.z.ToString());
		Material cMat = MatPool[Random.Range(0, MatPool.Count)]; // current mat
		for (int i = s.x; i <= e.x; i++) // the <= is there because it fills the space between the positions inclusively, so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			//MonoBehaviour.print("i = " + i.ToString());
			//MonoBehaviour.print("j = " + j.ToString());
			//MonoBehaviour.print("k = " + k.ToString());
			Block[i, j, k] = true;
			Mat[i, j, k] = cMat;
			//t++;
		}
		//MonoBehaviour.print("fillRect() created " + t.ToString() + " blocks.");
	}

	// at ground floor
	void fillRect (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		Vec3i a;
		a.x = s.x;
		a.y = 0;
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = Random.Range(MinHeight, MinHeight + HeightRand);
		b.z = e.z;
		fillRect(a, b);
	}

	void fillRect (Vec2i s, Vec2i e, int h) { // start, end, height(of the floor)(must be sorted and not be out of borders)
		MonoBehaviour.print("fillRect was called for a corridor");
		Vec3i a;
		a.x = s.x;
		a.y = h + 1; // h + 1 because floors generate at h(for bridges)
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = h + Random.Range(MinHeight, MinHeight + HeightRand) + 1;
		b.z = e.z;
		fillRect(a, b);
		MonoBehaviour.print("a.y = " + a.y.ToString());
		MonoBehaviour.print("b.y = " + b.y.ToString());
	}

	//fills with false, used for creating bridges
	void putWall (Vec2i s, Vec2i e, int h) { // start, end, height()(must be sorted and not be out of borders)
		for (int i = s.x; i <= e.x; i++) // the <= is there because it fills the space between the positions inclusively, so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block
		for (int k = s.z; k <= e.z; k++)
			Block[i, h, k] = false;
	}

	// counts all true blocks in an area
	int countBlocks (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		int t = 0; // temporary
		for (int i = s.x; i <= e.x; i++) // the <= is there because it counts the blocks inclusively, so counting from 3, 3, 3 to 3, 3, 3 can cause it to return 1
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			if (Block[i, j, k]) t++;
		}
		return t;
	}

	// gets the taken area of the floor
	int countBlocks (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		Vec3i a;
		a.x = s.x;
		a.y = 0;
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = 0;
		b.z = e.z;
		return countBlocks(a, b);
	}

	// gets the taken area of the floor
	int countBlocks (Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.x = s.x;
		a.y = h;
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = h;
		b.z = e.z;
		return countBlocks(a, b);
	}

	bool containsBlocks (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		for (int i = s.x; i <= e.x; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			if (Block[i, j, k]) return true;
		}
		return false; // if we got here, it means that there are no blocks in the area
	}

	// checks for blocks on the ground floor
	bool containsBlocks (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		Vec3i a;
		a.x = s.x;
		a.y = 0;
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = 0;
		b.z = e.z;
		return containsBlocks (a, b);
	}
	
	bool containsBlocks (Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.x = s.x;
		a.y = h;
		a.z = s.z;
		Vec3i b;
		b.x = e.x;
		b.y = h;
		b.z = e.z;
		return containsBlocks (a, b);
	}

	// only returns the floor area
	int getArea (Vec3i s, Vec3i e) { // start, end, must be sorted
		return (e.x - s.x + 1) * (e.z - s.z + 1);
	}

	int getArea (Vec2i s, Vec2i e) { // start, end, must be sorted
		return (e.x - s.x + 1) * (e.z - s.z + 1);
	}

	//used to safely get a block(ie when you're not sure whether it is out of the map)
	bool getBlock (Vec3i p) { // position
		if (p.x < 0 || p.x >= MapSize.x) return MapIsOpen;
		if (p.y < 0 || p.y >= MapSize.y) return MapIsOpen;
		if (p.z < 0 || p.z >= MapSize.z) return MapIsOpen;
		return Block[p.x, p.y, p.z];
	}

	bool getBlock (int x, int y, int z) {
		Vec3i t; // temporary
		t.x = x;
		t.y = y;
		t.z = z;
		return getBlock(t);
	}
}
