using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public static class VoxGen {
	public static bool Generating = false;
	public static Theme Theme = Theme.SciFi; // determines what assets to use, and how 
	public static bool MapIsOpen = false; // used to control whether corridors reaching the border of the map are to be closed 
	public static int Seed = 0;
	// rooms 
	public static int NumLowerRooms = 40; // rooms/halls to create on the ground floor 
	public static float MaxOverride = 0.3f; // only create a room if there aren't too many things already in there 
	public static int MinRoomSpan = 2; // the minimal span of a form(this will end up being the width ussually) 
	public static int MaxRoomSpan = 16; // the maximal span of a form(this will end up being the length ussually) 
	public static int MinArea = 20; // don't create tiny rooms that will not be noticed 
	public static int MaxArea = 100; // limits the creation of extremely large rooms, favors corridors 
	public static int MaxCorridorArea = 60; // for corridors/bridges above the ground floor 
	public static Vector3 Pos = Vector3.zero; // the position of the min-coordinate corner of the generated map 
	public static Vector3 Scale = Vector3.one; // the scale of all elements on the map 
	public static float SizeToHeight = 0.85f; // this is used to give a larger height to rooms with a square-like shape.  
		//Setting it to a lower value will cause rooms to appear flatter. 
	public static int MinHeight = 2; // the minimal height.  needs to allow space for player 
	public static int MaxLowerY = 1; // the y position of the lower rooms.  use 0 for no randomness 
	public static int HeightRand = 4; // the maximal additional room height (minimal is 0) (last-exclusive, so set to 1 for no randomness) 
	// corridors 
	public static int NumUpperRooms = 10; // corridors/bridges that connect rooms reaching the given height 
	public static int CorridorHeightRand = 2; // same as above, but for corridors 
	public static int MinFloorLevelStep = 8; // the minimal height distance between two corridor levels on top of each other 
	public static int MaxFloorLevelStep = 10; // the maximal height distance between two corridor levels on top of each other 
	public static int MinCorridorStartHeight = 3; // the minimal height at which corridors & bridges are created 
	public static int MaxCorridorStartHeight = 5; // the maximal height at which corridors & bridges are created 
	// assets to be placed on the map 
	//		lights 
	public static float TorchScale = 1f;
	public static float TorchOffset = 0.1f; // the distance from the center of the torch to the wall/floor it is attached to 
	// 		jump pads 
	public static int numJumpPads = 10;
	public static Vector3 JumpPadScale = new Vector3(1f, 0.3f, 1f);
	public static int JumpHeight = 2; // the height that cannot be jumped over normally (needs a jump pad & must be lesser than MapSize.y) 
	public static float JumpPadOffset = 0.05f; // the distance from the center of the jump pad to the floor it is on 
	// spawns 
	//		entities 
	public static Vector3 SpawnPointScale = Vector3.one;
	public static int NumUserSpawns = 6; // (FFA/red/blue) 
	public static int NumMobSpawns = 8;
	// 		weapons 
	public static Vector3 WeaponSpawnScale = new Vector3(0.5f, 0.1f, 0.5f);
	public static float WeaponSpawnOffset = 0.05f; // the distance from the center of the weapon spawn to the floor it is spawned on 
	// bags act as containers. (since scenes don't have folders) 
	// a simple transform gameobject which children/content will mark as their parent as they are born 
	public static GameObject PrimBag; 
	public static GameObject SpawnBag;



	// private 
	static int numMade = 0; // curr tally of just about everything that gets made 
	const int numGunSpawns = 10;
	const int numTries = 20000; // ... at making a room 
	static Cell[,,] cells;
	static Vec3i numVoxAcross; // number of voxels across 3 dimensions 
	static ThemedCategories cat = new ThemedCategories();
	static List<VoxelRect> rooms = new List<VoxelRect>();
	// current pointers/indexes 
	//static int currX;
	static int currRoom;
	static Vector3 currDir; // direction/orientation/facing 
	static int sx, sy, sz, x, y, z, xMax, yMax, zMax; // start & current positions & max counts (of current room) 



	static VoxGen() {
		numVoxAcross.X = 64;
		numVoxAcross.Z = 64;
		numVoxAcross.Y = 32;
		
		cells = new Cell[numVoxAcross.X, numVoxAcross.Y, numVoxAcross.Z];
	}
	
	
	
	public static void OnGUI() {
		if (!Generating)
			return;

		int xSpan = Screen.width / /*numVoxAcross.X*/ rooms.Count;
		GUI.Box(new Rect(0, Screen.height/2, Screen.width, Screen.height), 
		        "Generating a map for you....... wait for it...........WAIT FOR IT");
		GUI.Label(new Rect(/*currX*/ currRoom*xSpan, 0, xSpan, Screen.height), Pics.Health);
	}



	public static void GenerateMap(int seed, Theme theme) {
		Random.seed = 
			Seed = 
			seed;
		Theme = theme;
		Scale = Vector3.one * 2f;

		// bags 
		// ...are containers.
		// since we can't have "folders" in a scene, we parent spammy quads and 
		// such to these objects, so they will be collapsed/hidden 
		PrimBag = GameObject.Find("[PRIM]");
		SpawnBag = GameObject.Find("[SPAWN]");

		Debug.Log("Generating map with seed: " + seed);
		// cleanup previous possible map 
		rooms.Clear();
		cellsClear();

		Debug.Log("makeFirstRoom     num rooms: " + rooms.Count);
		makeFirstRoom();
		makeGroundFloor();
		addBridgesAndCorridorsHigherUp();
		Debug.Log("finished with addBridgesAndCorridorsHigherUp()     num rooms: " + rooms.Count);

		//currX = 0;
		currRoom = 0;
		Generating = true;
	}
	
	
	
	public static void Update() {
		if (!Generating)
			return;

		//generateXSlice(currX);
		//currX++;
		generateOneRoom();

		// if we still need to generate more slices 
		if (currRoom < rooms.Count /*currX < numVoxAcross.X*/)
			return;
		else
			postGenerationProcesses();
	}



	private static void postGenerationProcesses() {
		Debug.Log("finished all rooms     num: " + rooms.Count);
		Generating = false;
		
		// spawn map features 
		Debug.Log("makeLights");
		makeLights();
		Debug.Log("makeJumpPads");
		makeJumpPads();
		makeSpawns(NumUserSpawns, GOs.Get("UserSpawn"), getChildTransform("FFA"));
		makeSpawns(NumUserSpawns, GOs.Get("UserSpawn"), getChildTransform("TeamBlue"));
		makeSpawns(NumUserSpawns, GOs.Get("UserSpawn"), getChildTransform("TeamRed"));
		makeSpawns(NumMobSpawns, GOs.Get("UserSpawn"), getChildTransform("Mob"));
		makeSpawns(numGunSpawns, GOs.Get("GunSpawn"), getChildTransform("Gun"), true);
	}



	// starting position needs to be reset for each quad (the max positions might as well be here,  just for grouping) 
	private static void setStart(Vector3 dir) { 
		currDir = dir;
		var r = rooms[currRoom];

		sx = r.Pos.X;
		sy = r.Pos.Y;
		sz = r.Pos.Z;
		x = sx;
		y = sy;
		z = sz;
		xMax = r.Pos.X + r.Size.X;
		yMax = r.Pos.Y + r.Size.Y;
		zMax = r.Pos.Z + r.Size.Z;

		//if (currRoom == 0)
			//Debug.Log("xMax: " + xMax + "  yMax: " + yMax + "  zMax: " + zMax);
//		Debug.Log("room: " + currRoom + " - " + r.Pos.X + " " + r.Pos.Y + " " + r.Pos.Z + 
//		          " - " + r.Size.X + " " + r.Size.Y + " " + r.Size.Z);
	}



	private static void generateOneRoom() {
		var hx = Scale.x * 0.5f;
		var hy = Scale.y * 0.5f;
		var hz = Scale.z * 0.5f;

		setStart(Vector3.left);
		for (y = sy; y < yMax; y++)
		for (z = sz; z < zMax; z++)
			maybeMakeQuad(x-1, y, z, new Vector3(Scale.x*x-hx, Scale.y*y, Scale.z*z));

		setStart(Vector3.right);
		for (y = sy; y < yMax; y++)
		for (z = sz; z < zMax; z++)
			maybeMakeQuad(xMax/*x+1*/, y, z, new Vector3(Scale.x*(xMax-1)/*x*/+hx, Scale.y*y, Scale.z*z));

		setStart(Vector3.forward);
		for (x = sx; x < xMax; x++)
		for (y = sy; y < yMax; y++)
			maybeMakeQuad(x, y, zMax/*z+1*/, new Vector3(Scale.x*x, Scale.y*y, Scale.z*(zMax-1)/*z*/+hz));

		setStart(Vector3.back);
		for (x = sx; x < xMax; x++)
		for (y = sy; y < yMax; y++) {
			if (currRoom == 0)
				Debug.Log("x: " + x + "y: " + y + " z: " + z);
			maybeMakeQuad(x, y, z-1, new Vector3(Scale.x*x, Scale.y*y, Scale.z*z-hz));
		}

		setStart(Vector3.up);
		for (x = sx; x < xMax; x++)
		for (z = sz; z < zMax; z++)
			maybeMakeQuad(x, yMax/*y+1*/, z, new Vector3(Scale.x*x, Scale.y*(yMax-1)/*y*/+hy, Scale.z*z));

		setStart(Vector3.down);
		for (x = sx; x < xMax; x++)
		for (z = sz; z < zMax; z++)
			maybeMakeQuad(x, y-1, z, new Vector3(Scale.x*x, Scale.y*y-hy, Scale.z*z));

		currRoom++;
	}



	private static void generateXSlice(int x) {
		for (int y = 0; y < numVoxAcross.Y; y++)
		for (int z = 0; z < numVoxAcross.Z; z++) {
			// if air... 
			if (cells[x, y, z].IsAir) {
				// ...need to make surface quads against neighboring void voxels 
				var hx = Scale.x * 0.5f;
				var hy = Scale.y * 0.5f;
				var hz = Scale.z * 0.5f;

				maybeMakeQuad(//Vector3.left, 
					x-1, y, z,
					new Vector3(Scale.x*x-hx, Scale.y*y, Scale.z*z));
				maybeMakeQuad(//Vector3.right, 
					x+1, y, z,
					new Vector3(Scale.x*x+hx, Scale.y*y, Scale.z*z));
				
				maybeMakeQuad(//Vector3.forward, 
					x, y, z+1,
					new Vector3(Scale.x*x, Scale.y*y, Scale.z*z+hz));
				maybeMakeQuad(//Vector3.back,
					x, y, z-1,
					new Vector3(Scale.x*x, Scale.y*y, Scale.z*z-hz));
				
				maybeMakeQuad(//Vector3.up, 
					x, y+1, z,
					new Vector3(Scale.x*x, Scale.y*y+hy, Scale.z*z));
				maybeMakeQuad(//Vector3.down, 
					x, y-1, z,
					new Vector3(Scale.x*x, Scale.y*y-hy, Scale.z*z));
			}
		}
	}






	// private methods 
	private static void cellsClear() {
		for (int i = 0; i < numVoxAcross.X; i++)
		for (int j = 0; j < numVoxAcross.Y; j++)
		for (int k = 0; k < numVoxAcross.Z; k++) {
			cells[i, j, k] = new Cell();
		}
	}



	private static void makeFirstRoom() {
		Vec3i a;
		a.X = Random.Range(0, numVoxAcross.X - MaxRoomSpan);
		a.Y = 0;
		a.Z = Random.Range(0, numVoxAcross.Z - MaxRoomSpan);
		
		Vec3i b;
		b.X = Random.Range(a.X + MinRoomSpan, a.X + MaxRoomSpan);
		b.Y = Random.Range(MinHeight, MinHeight + HeightRand);
		b.Z = Random.Range(a.Z + MinRoomSpan, a.Z + MaxRoomSpan);
		
		a.X = 0;
		a.Y = 0;
		a.Z = 0;
		
		b.X = 2; // not size/span....absolute end positions 
		b.Y = 3;
		b.Z = 4;
		
		carveOutRoom(a, b);
	}



	private static void carveOutRoom(Vec2i s, Vec2i e, int h = 0) { // start, end, height (of floor) 
		Vec3i a;
		a.X = s.x;
		a.Z = s.z;

		Vec3i b;
		b.X = e.x;
		b.Z = e.z;

		// if at ground floor 
		if (h == 0) {
			h = (int)((float)Mathf.Min(e.x - s.x, e.z - s.z) * SizeToHeight); // height(of the room)
			if (h < MinHeight) 
				h = MinHeight;
			h += Random.Range(0, HeightRand);

			a.Y = Random.Range(0, MaxLowerY + 1);
			b.Y = a.Y + h;
		}else{ // bridges & overground corridors 
			a.Y = h + 1;
			b.Y = h + Random.Range(MinHeight, MinHeight + HeightRand) + 1;
		}

		carveOutRoom(a, b);
	}
	private static void carveOutRoom(Vec3i s, Vec3i e) { // start, end 
		// the <= is there because it fills the space between the positions inclusively, 
		// so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block 
		for (int i = s.X; i <= e.X; i++) 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++)
			cells[i, j, k].IsAir = true;

		// cache room 
		var room = new VoxelRect();
		room.Surfaces = cat.GetRandomSurfaces();
		room.Pos = s;
		room.Size.X = e.X - s.X + 1;
		room.Size.Y = e.Y - s.Y + 1;
		room.Size.Z = e.Z - s.Z + 1;
		rooms.Add(room);
	}



	// fills with false, used for creating bridges 
	private static void putWall(Vec2i s, Vec2i e, int h) { // start, end, height() (must be sorted and not be out of borders) 
		for (int i = s.x; i <= e.x; i++) // the <= is there because it fills the space between the positions inclusively, 
										// so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block 
		for (int k = s.z; k <= e.z; k++)
			cells[i, h, k].IsAir = false;
	}



	// counts all true blocks in an area 
	private static int countBlocks(Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		int t = 0; // temporary 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it counts the blocks inclusively, so counting from 3, 3, 3 to 3, 3, 3 can cause it to return 1 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++) {
			if (cells[i, j, k].IsAir) 
				t++;
		}

		return t;
	}
	// gets the taken area of the floor 
	private static int countBlocks(Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders) 
		int m = 0; // the temporary var for storing the maximal amount of blocks in a level 
		Vec3i a;
		a.X = s.x;
		a.Z = s.z;
		Vec3i b;
		b.X = e.x;
		b.Z = e.z;
		for (int i = 0; i <= MaxLowerY; i++) {
			a.Y = i;
			b.Y = i;
			int t = countBlocks(a, b);
			if (t > m) m = t;
		}
		return m;
	}
	// gets the taken area of the floor
	private static int countBlocks(Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.Y = h;
		a.Z = s.z;
		Vec3i b;
		b.X = e.x;
		b.Y = h;
		b.Z = e.z;
		return countBlocks(a, b);
	}



	private static bool containsAnyAir(Vec2i s, Vec2i e, int h = 0) { // start, end, height 
		Vec3i a;
		a.X = s.x;
		a.Y = h;
		a.Z = s.z;

		Vec3i b;
		b.X = e.x;
		b.Y = h; // FIXME?!   how can this and a.Y both being the passed height work? 
		b.Z = e.z;

		if (h == 0) // must be ground floor check (no height passed) 
			b.Y = MaxLowerY;

		return containsAnyAir(a, b);
	}
	private static bool containsAnyAir(Vec3i s, Vec3i e) { // start, end 
		// the <= is there because it checks the blocks inclusively, 
		// so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true
		for (int i = s.X; i <= e.X; i++) 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++) {
			if (cells[i, j, k].IsAir) 
				return true;
		}
		
		return false; // if we got here, it means that there are no blocks in the area 
	}



	// only returns the floor area 
	private static int getArea (Vec3i s, Vec3i e) { // start, end, must be sorted 
		return (e.X - s.X + 1) * (e.Z - s.Z + 1);
	}
	private static int getArea(Vec2i s, Vec2i e) { // start, end, must be sorted 
		return (e.x - s.x + 1) * (e.z - s.z + 1);
	}



	private static bool air(Vec3i p) {
		if (p.X < 0 || p.X >= numVoxAcross.X) return MapIsOpen;
		if (p.Y < 0 || p.Y >= numVoxAcross.Y) return MapIsOpen;
		if (p.Z < 0 || p.Z >= numVoxAcross.Z) return MapIsOpen;

		return cells[p.X, p.Y, p.Z].IsAir;
	}
	private static bool air(int x, int y, int z) {
		Vec3i v;
		v.X = x;
		v.Y = y;
		v.Z = z;

		return air(v);
	}



	// checks if the area contains any blocks that are false (so that bridges are not suspended in mid-air) 
	private static bool containsWalls (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++)
			if (!cells[i, j, k].IsAir) return true;

		return false; // if we got here, it means that there are no walls in the area
	}
	private static bool containsWalls (Vec2i s, Vec2i e, int h) { // start, end, height(sorted, not out of borders)
		Vec3i a;
		a.X = s.x;
		a.Y = h;
		a.Z = s.z;

		Vec3i b;
		b.X = e.x;
		b.Y = h;
		b.Z = e.z;

		return containsWalls (a, b);
	}
	private static bool containsWalls(Vec2i s, Vec2i e) { // start, end(sorted, not out of borders)
		Vec3i a;
		a.X = s.x;
		a.Y = 0;
		a.Z = s.z;

		Vec3i b;
		b.X = e.x;
		b.Y = 0;
		b.Z = e.z;

		return containsWalls (a, b);
	}



	// "Tiny" is the tidiest way to refer to some compressed/quantized number 
	// like an int/index (which is much smaller, in data size, than a float) 
	// .... which would be the typical type for representing distances. 

	// *** RETURNS NEGATIVE *** if no floor was reached before the map ended 
	private static int getTinyDistance(Vec3i sv) { // starting voxel 
		// y offset (go down from starting vox) 
		for (int yOff = 0; yOff <= sv.Y + 1; yOff++) {
			// + 1 because it also scans the floor that is not coded as bloks, instead, is dependent on the MapIsOpen variable
			if (!air(sv.X, sv.Y-yOff, sv.Z)) 
				return yOff;
		}

		return -1;
	}
	private static int getTinyDistance(int x, int y, int z) {
		Vec3i v;
		v.X = x;
		v.Y = y;
		v.Z = z;
		return getTinyDistance(v);
	}



	// checks if all floors at given heights contain open blocks, 
	// used to check if you can get into a corridor at all 
	// (MinHeight is considered the player height) 
	private static bool eachFloorOpen(Vec2i s, Vec2i e, int hs, int he) { // start, end, starting height, ending height (inclusive) 
		for (int i = hs; i <= he; i++)
			if (!containsAnyAir(s, e, i)) 
				return false;

		return true; // if we got here, everything's fine 
	}



	// passed vars: start position, height from there 
	private static bool columnOpen(Vec3i s, int h) {
		for (int i = s.Y; i <= s.Y + h; i++)
			if (!air(s.X, i, s.Z)) 
				return false;

		return true;
	}
	private static bool columnOpen(int x, int y, int z, int h) {
		Vec3i v;

		v.X = x;
		v.Y = y;
		v.Z = z;
		return columnOpen(v, h);
	}



	private static void makeGroundFloor() {
		int numMade = 0;
		for (int i = 0; i < numTries && numMade < NumLowerRooms; i++) {
			Vec2i t;
			t.x = Random.Range(0, numVoxAcross.X);
			t.z = Random.Range(0, numVoxAcross.Z);
			
			Vec2i u;
			u.x = Random.Range(0, numVoxAcross.X);
			u.z = Random.Range(0, numVoxAcross.Z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
			
			Vec2i start;
			Vec2i end;
			start.x = Mathf.Min(t.x, u.x);
			start.z = Mathf.Min(t.z, u.z);
			end.x = Mathf.Max(t.x, u.x);
			end.z = Mathf.Max(t.z, u.z);
			
			if (containsAnyAir(start, end)) {
				if ((end.x >= start.x + MinRoomSpan) && (end.z >= start.z + MinRoomSpan))
					if ((end.x <= start.x + MaxRoomSpan) && (end.z <= start.z + MaxRoomSpan))
						if (getArea(start, end) <= MaxArea && getArea(start, end) >= MinArea)
						if (countBlocks(start, end) < getArea(start, end) * MaxOverride * (MaxLowerY + 1f)) {
							carveOutRoom(start, end);
							numMade++;
						}
			}
		}
	}



	private static void addBridgesAndCorridorsHigherUp() {
		// h is the height of the floor 
		for (int h = Random.Range(MinCorridorStartHeight, MaxCorridorStartHeight + 1); 
		     h < numVoxAcross.Y - MinHeight - HeightRand; 
		     h += Random.Range(MinFloorLevelStep, MaxFloorLevelStep + 1)
		) {
			numMade = 0;

			for (int i = 0; i < numTries && numMade < NumUpperRooms; i++) {
				Vec2i t;
				t.x = Random.Range(0, numVoxAcross.X);
				t.z = Random.Range(0, numVoxAcross.Z);
				
				Vec2i u;
				u.x = Random.Range(0, numVoxAcross.X);
				u.z = Random.Range(0, numVoxAcross.Z);
				
				Vec2i start;
				start.x = Mathf.Min(t.x, u.x);
				start.z = Mathf.Min(t.z, u.z);

				Vec2i end;
				end.x = Mathf.Max(t.x, u.x);
				end.z = Mathf.Max(t.z, u.z);

				if (eachFloorOpen(start, end, h + 1, h + MinHeight)) { // MinHeight is considered to be the player height, to check if the corridor can be accessed 
					if ((end.x >= start.x + MinRoomSpan) && (end.z >= start.z + MinRoomSpan)) {
						if ((end.x <= start.x + MaxRoomSpan) && (end.z <= start.z + MaxRoomSpan)) {
							if (containsWalls(start, end, h)) { // so that bridges don't generate in mid-air 
								// no MaxOverride check here because we want bridges to generate 
								if (getArea(start, end) <= MaxCorridorArea) {
									putWall(start, end, h);
									carveOutRoom(start, end, h);
									numMade++;
								}
							}
						}
					}
				}
			}
		}
	}



	private static void maybeMakeQuad(int x, int y, int z, Vector3 offset) {
		if (!air(x, y, z)) {
			Material mat;

			if /***/ (currDir == Vector3.up) {
				mat = rooms[currRoom].Surfaces.Ceiling;
			}else if (currDir == Vector3.down) {
				mat = rooms[currRoom].Surfaces.Floor;
			}else{
				mat = rooms[currRoom].Surfaces.Walls;
			}

			var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
			np.transform.position = Pos + offset;
			np.transform.localScale = Scale;
			np.transform.forward = currDir;
			np.renderer.material = mat;
			np.transform.parent = PrimBag.transform;
		}
	}



	private static void makeLights() {
		const int numLights = 25;
		numMade = 0;
		
		for (int i = 0; i < numTries && numMade < numLights; i++) {
			Vec3i v;
			v.X = Random.Range(0, numVoxAcross.X);
			v.Y = Random.Range(0, numVoxAcross.Y);
			v.Z = Random.Range(0, numVoxAcross.Z);
			
			if (cells[v.X, v.Y, v.Z].IsAir) {
				if (!air(v.X-1, v.Y, v.Z)) {
					var nt = (GameObject)GameObject.Instantiate(GOs.Get("Torch"));
					nt.transform.position = Pos + new Vector3 (Scale.x * v.X - Scale.x * 0.5f + TorchOffset, Scale.y * v.Y, Scale.z * v.Z);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = PrimBag.transform;
					numMade++;
				}else
				if (!air(v.X+1, v.Y, v.Z)) {
					var nt = (GameObject)GameObject.Instantiate(GOs.Get("Torch"));
					nt.transform.position = Pos + new Vector3 (Scale.x * v.X + Scale.x * 0.5f - TorchOffset, Scale.y * v.Y, Scale.z * v.Z);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = PrimBag.transform;
					numMade++;
				}else
				if (!air(v.X, v.Y, v.Z-1)) {
					var nt = (GameObject)GameObject.Instantiate(GOs.Get("Torch"));
					nt.transform.position = Pos + new Vector3 (Scale.x * v.X, Scale.y * v.Y, Scale.z * v.Z - Scale.z * 0.5f + TorchOffset);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = PrimBag.transform;
					numMade++;
				}else
				if (!air(v.X, v.Y, v.Z+1)) {
					var nt = (GameObject)GameObject.Instantiate(GOs.Get("Torch"));
					nt.transform.position = Pos + new Vector3 (Scale.x * v.X, Scale.y * v.Y, Scale.z * v.Z + Scale.z * 0.5f - TorchOffset);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = PrimBag.transform;
					numMade++;
				}
			} // end of scanning an individual block to see if a torch can be placed 
		}
	}
	
	
	
	private static void makeJumpPads() {
		numMade = 0;

		for (int i = 0; i < numTries && numMade < numJumpPads; i++) {
			// get random x/z at JumpHeight 
			Vec3i v;
			v.X = Random.Range(0, numVoxAcross.X);
			v.Y = Random.Range(JumpHeight, numVoxAcross.Y);
			v.Z = Random.Range(0, numVoxAcross.Z);
			
			// if valid air space & there's a surface/floor directly underneath it 
			if (cells[v.X, v.Y, v.Z].IsAir && 
			    !cells[v.X, v.Y-1, v.Z].IsAir
		    ) {
				int d = getTinyDistance(v.X - 1, v.Y, v.Z); // distance 
				
				if (d >= JumpHeight && columnOpen(v.X-1, v.Y, v.Z, MinHeight)) {
					var nj = (GameObject)GameObject.Instantiate(GOs.Get("JumpPad"));
					nj.transform.position = Pos + new Vector3(Scale.x * v.X - Scale.x, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z);
					nj.transform.localScale = JumpPadScale;
					nj.transform.parent = PrimBag.transform;
					numMade++;
				} else {
					d = getTinyDistance(v.X + 1, v.Y, v.Z);
					
					if (d >= JumpHeight && columnOpen(v.X + 1, v.Y, v.Z, MinHeight)) {
						var nj = (GameObject)GameObject.Instantiate(GOs.Get("JumpPad"));
						nj.transform.position = Pos + new Vector3(Scale.x * v.X + Scale.x, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z);
						nj.transform.localScale = JumpPadScale;
						nj.transform.parent = PrimBag.transform;
						numMade++;
					} else {
						d = getTinyDistance(v.X, v.Y, v.Z - 1);

						if (d >= JumpHeight && columnOpen(v.X, v.Y, v.Z - 1, MinHeight)) {
							var nj = (GameObject)GameObject.Instantiate(GOs.Get("JumpPad"));
							nj.transform.position = Pos + new Vector3(Scale.x * v.X, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z - Scale.z);
							nj.transform.localScale = JumpPadScale;
							nj.transform.parent = PrimBag.transform;
							numMade++;
						} else {
							d = getTinyDistance(v.X, v.Y, v.Z + 1);

							if (d >= JumpHeight && columnOpen(v.X, v.Y, v.Z + 1, MinHeight)) {
								var nj = (GameObject)GameObject.Instantiate(GOs.Get("JumpPad"));
								nj.transform.position = Pos + new Vector3(Scale.x * v.X, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z + Scale.z);
								nj.transform.localScale = JumpPadScale;
								nj.transform.parent = PrimBag.transform;
								numMade++;
							}
						}
					}
				}
			} // end of checking the block for possible jump pad placing 
		}
	}



	private static void makeSpawns(int num, GameObject o, Transform tr, bool isGun = false) {
		numMade = 0;
		float yOff = 0f;
		var scale = SpawnPointScale;
		
		if (isGun) {
			yOff = WeaponSpawnOffset - Scale.y * 0.5f;
			scale = WeaponSpawnScale;
		}
		
		for (int i = 0; i < numTries && numMade < num; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);
			
			if (columnOpen(t, MinHeight)) // if the place is accessible (ceiling height) 
			if (!air(t.X, t.Y-1, t.Z)) {
				var ns = (GameObject)GameObject.Instantiate(o);
				ns.transform.parent = tr;
				ns.transform.localScale = scale;
				ns.transform.position = Pos + new Vector3(
					Scale.x*t.X, 
					Scale.y*t.Y + yOff, 
					Scale.z*t.Z);
				if (isGun)
					ns.GetComponent<SpawnData>().Gun = numMade;
				
				numMade++;
			}
		}
	}
	
	
	
	public static Transform getChildTransform(string s) {
		var tr = SpawnBag.transform;

		for (int i = 0; i < tr.childCount; i++) {
			var v = tr.GetChild(i);

			if (v.name == s)
				return v.transform;
		}

		return null;
	}
}
