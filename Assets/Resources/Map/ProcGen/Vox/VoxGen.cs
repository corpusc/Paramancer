using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public struct Vec3i {
	public int X;
	public int Y;
	public int Z;
};



public static class VoxGen {
	public static int Seed = 0;
	public static Material[,,] Mat;
	public static int Forms = 40; // the amount of rooms/halls to create on the ground floor 
	public static int FormsPerFloor = 10; // the amount of corridors/bridges that connect rooms reaching the given height, per floor 
	public static float MaxOverride = 0.3f; // only create a form if there aren't too many things already in there 
	public static int MinFormWidth = 2; // the minimal span of a form(this will end up being the width ussually) 
	public static int MaxFormWidth = 16; // the maximal span of a form(this will end up being the length ussually) 
	public static int MinArea = 20; // don't create tiny rooms that will not be noticed 
	public static int MaxArea = 100; // limits the creation of extremely large rooms, favorizes corridors 
	public static int MaxCorridorArea = 60; // for corridors/bridges above the ground floor 
	public static Vector3 Pos = Vector3.zero; // the position of the min-coordinate corner of the generated map 
	public static Vector3 Scale = Vector3.one; // the scale of all elements on the map 
	public static float SizeToHeight = 0.85f; // this is used to give a larger height to rooms with a square-like shape.  Setting it to a lower value will cause rooms to appear flatter. 
	public static int MinHeight = 2; // the minimal height a room can have 
	public static int MaxFloorHeight = 1; // the maximal height of the floor of the rooms on the ground floor. Use 0 for no randomness. 
	public static int HeightRand = 4; // the maximal additional room height(minimal is 0)(last-exclusive, so set to 1 for no randomness) 
	public static int CorridorHeightRand = 2; // same as above, but for corridors 
	public static int MinFloorLevelStep = 8; // the minimal height distance between two corridor levels on top of each other 
	public static int MaxFloorLevelStep = 10; // the maximal height distance between two corridor levels on top of each other 
	public static int MinCorridorStartHeight = 3; // the minimal height at which corridors & bridges are created 
	public static int MaxCorridorStartHeight = 5; // the maximal height at which corridors & bridges are created 
	public static bool MapIsOpen = false; // used to control whether corridors reaching the border of the map are to be closed 

	public static Theme Theme = Theme.SciFi; // used to control the placing of art assets & the texture images 

	// assets to be placed on the map 
	//		lights 
	public static GameObject Torch;
	public static float TorchScale = 0.3f;
	public static float TorchOffset = 0.1f; // the distance from the center of the torch to the wall/floor it is attached to 
	// 		jump pads 
	public static int numJumpPads = 10;
	public static GameObject JumpPad;
	public static Vector3 JumpPadScale = new Vector3(1f, 0.3f, 1f);
	public static int JumpHeight = 2; // the height that cannot be jumped over normally (needs a jump pad & must be lesser than MapSize.y) 
	public static float JumpPadOffset = 0.05f; // the distance from the center of the jump pad to the floor it is on 
	// spawns 
	//		entities 
	public static Vector3 SpawnPointScale = Vector3.one;
	public static GameObject SpawnPoint;
	public static int SpawnPoints = 4; // the amount of spawn points to be placed(of each type[FFA/red/blue]) 
	public static int MonsterSpawns = 6;
	// 		weapons 
	public static GameObject WeaponSpawn;
	public static Vector3 WeaponSpawnScale = new Vector3(0.5f, 0.1f, 0.5f);
	public static float WeaponSpawnOffset = 0.05f; // the distance from the center of the weapon spawn to the floor it is spawned on 

	// the maximal height of a room is determined by the map height & the room size 
	// This assumes the values you passed make sense, ie you didn't make MinFormWidth > MaxFormWidth 
	// WARNING: MaxFormWidth must be lesser than MapSize.x and MapSize.z, 
	//		and MinHeight + HeightRand must be lesser than MapSize.y 



	// private 
	const int numGunSpawns = 10;
	const int numTries = 20000; // ... at making a room 
	static bool building = false;
	static bool[,,] IsAir;
	static Vec3i numVoxAcross; // number of voxels across 3 dimensions 
	static List<Material> MatPool = new List<Material>();

	static GameObject primObject;
	static GameObject ffaSpawnBag;
	static GameObject blueSpawnBag;
	static GameObject redSpawnBag;
	static GameObject weaponSpawnBag;
	static GameObject monsterSpawnBag;



	public static void OnGUI() {
		if (!building)
			return;

		int xSpan = Screen.width / numVoxAcross.X;

		for (int x = 0; x < numVoxAcross.X; x++) {
			GUI.Label(new Rect(x * xSpan, 0, xSpan, Screen.height), Pics.Health);
		}
	}



	public static void GenerateMap(int seed, Theme theme) {
		building = true;

		Seed = seed;
		Theme = theme;
		Scale = Vector3.one * 2f;

		Init();
		Build();
		Debug.Log("Build3d() -----  seed: " + seed + " ----- ");
		Build3D();
		Debug.Log("RemoveOriginals() -----  seed: " + seed + " ----- ");
		RemoveOriginals();

		building = false;
	}



	// setup (doesn't build) 
	// call Build() and Build3D() right after this for the random seed to work 
	public static void Init() {
		Random.seed = Seed;
		numVoxAcross.X = 64;
		numVoxAcross.Z = 64;
		numVoxAcross.Y = 32;

		IsAir = new bool[numVoxAcross.X, numVoxAcross.Y, numVoxAcross.Z];
		Mat = new Material[numVoxAcross.X, numVoxAcross.Y, numVoxAcross.Z];

		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_floor_003", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_plate_005", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_plate_008", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/sci_fi_003", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/Shutter_01", typeof(Material)));

		// stuff manually place into the scene 
		JumpPad = GameObject.Find("JumpPad");
		SpawnPoint = GameObject.Find("SpawnPoint");
		WeaponSpawn = GameObject.Find("WeaponSpawn");

		// bags 
		primObject = GameObject.Find("[PRIMS]");
		ffaSpawnBag = GameObject.Find("FFA Spawns");
		blueSpawnBag = GameObject.Find("Blue Team Spawns");
		redSpawnBag = GameObject.Find("Red Team Spawns");
		weaponSpawnBag = GameObject.Find("_PickupSpots");
		monsterSpawnBag = GameObject.Find("Monster Spawns");
	}



	// this will build a model of the level in memory 
	public static void Build() {
		emptyMap();
		preBuild();
		makeGroundFloor();
		addBridgesAndCorridorsHigherUp();
	}



	static int numMade = 0;
	public static void Build3D () {
		for (int x = 0; x < numVoxAcross.X/4; x++)
		for (int y = 0; y < numVoxAcross.Y/4; y++)
		for (int z = 0; z < numVoxAcross.Z/4; z++) {
			// if air... 
			if (IsAir[x, y, z]) {
				// ...need to make surface quads against neighboring void voxels 
				var hx = Scale.x * 0.5f;
				var hy = Scale.y * 0.5f;
				var hz = Scale.z * 0.5f;

				maybeMakeQuad(Vector3.left, 
					x-1, y, z,
					new Vector3(Scale.x*x-hx, Scale.y*y, Scale.z*z));
				maybeMakeQuad(Vector3.right, 
					x+1, y, z,
					new Vector3(Scale.x*x+hx, Scale.y*y, Scale.z*z));
				
				maybeMakeQuad(Vector3.forward, 
					x, y, z+1,
					new Vector3(Scale.x*x, Scale.y*y, Scale.z*z+hz));
				maybeMakeQuad(Vector3.back,
					x, y, z-1,
					new Vector3(Scale.x*x, Scale.y*y, Scale.z*z-hz));
				
				maybeMakeQuad(Vector3.up, 
					x, y+1, z,
					new Vector3(Scale.x*x, Scale.y*y+hy, Scale.z*z));
				maybeMakeQuad(Vector3.down, 
					x, y-1, z,
					new Vector3(Scale.x*x, Scale.y*y-hy, Scale.z*z));

				Debug.Log("AIR " + x);
			}else{ // NOT air 
				Debug.Log("VOID " + x);
			}
		} 
		Debug.Log("end of vox scan");






		// spawn map features 
		makeLights();
		Debug.Log("end of makeLights");

		// place jump pads 
		numMade = 0;
		for (int i = 0; i < numTries && numMade < numJumpPads; i++) {
			Vec3i v;
			v.X = Random.Range(0, numVoxAcross.X);
			v.Y = Random.Range(JumpHeight, numVoxAcross.Y);
			v.Z = Random.Range(0, numVoxAcross.Z);

			// if this is the floor of any room above jump height 
			if (IsAir[v.X, v.Y, v.Z] && !IsAir[v.X, v.Y - 1, v.Z]) {
				int d = floorScan(v.X - 1, v.Y, v.Z); // distance 
				if (d >= JumpHeight && columnOpen(v.X - 1, v.Y, v.Z, MinHeight)) {
					var nj = (GameObject)GameObject.Instantiate(JumpPad);
					nj.transform.position = Pos + new Vector3(Scale.x * v.X - Scale.x, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z);
					nj.transform.localScale = JumpPadScale;
					nj.transform.parent = primObject.transform;
					numMade++;
				} else {
					d = floorScan(v.X + 1, v.Y, v.Z);

					if (d >= JumpHeight && columnOpen(v.X + 1, v.Y, v.Z, MinHeight)) {
						var nj = (GameObject)GameObject.Instantiate(JumpPad);
						nj.transform.position = Pos + new Vector3(Scale.x * v.X + Scale.x, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z);
						nj.transform.localScale = JumpPadScale;
						nj.transform.parent = primObject.transform;
						numMade++;
					} else {
						d = floorScan(v.X, v.Y, v.Z - 1);
						if (d >= JumpHeight && columnOpen(v.X, v.Y, v.Z - 1, MinHeight)) {
							var nj = (GameObject)GameObject.Instantiate(JumpPad);
							nj.transform.position = Pos + new Vector3(Scale.x * v.X, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z - Scale.z);
							nj.transform.localScale = JumpPadScale;
							nj.transform.parent = primObject.transform;
							numMade++;
						} else {
							d = floorScan(v.X, v.Y, v.Z + 1);
							if (d >= JumpHeight && columnOpen(v.X, v.Y, v.Z + 1, MinHeight)) {
								var nj = (GameObject)GameObject.Instantiate(JumpPad);
								nj.transform.position = Pos + new Vector3(Scale.x * v.X, Scale.y * (v.Y - d + 0.5f + JumpPadOffset), Scale.z * v.Z + Scale.z);
								nj.transform.localScale = JumpPadScale;
								nj.transform.parent = primObject.transform;
								numMade++;
							}
						}
					}
				}
			} // end of checking the block for possible jump pad placing 
		}
		Debug.Log("end of placing jump pads");

		// place FFA spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.Y - 1, t.Z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.Y, Scale.z * t.Z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = ffaSpawnBag.transform;
				numMade++;
			}
		}
		Debug.Log("ffa");

		// place blue spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.Y - 1, t.Z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.Y, Scale.z * t.Z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = blueSpawnBag.transform;
				numMade++;
			}
		}
		Debug.Log("blue");

		// place red spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.Y - 1, t.Z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.Y, Scale.z * t.Z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = redSpawnBag.transform;
				numMade++;
			}
		}
		Debug.Log("red");

		// place monster spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < MonsterSpawns; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.Y - 1, t.Z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.Y, Scale.z * t.Z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = monsterSpawnBag.transform;
				numMade++;
			}
		}
		Debug.Log("mob");

		// place weapon spawns 
		numMade = 0; // count weapon spawns as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < numGunSpawns; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.Y = Random.Range(0, numVoxAcross.Y);
			t.Z = Random.Range(0, numVoxAcross.Z);

			// if the place is accessible (ceiling height) 
			if (columnOpen(t, MinHeight)) {
				if (!getBlock(t.X, t.Y - 1, t.Z)) {
					var ns = (GameObject)GameObject.Instantiate(WeaponSpawn); // new weapon spawn 
					ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.Y + WeaponSpawnOffset - Scale.y * 0.5f, Scale.z * t.Z);
					ns.transform.localScale = WeaponSpawnScale;
					ns.transform.parent = weaponSpawnBag.transform;
					ns.GetComponent<PickupPoint>().pickupPointID = numMade;
					numMade++;
				}
			}
		}
		Debug.Log("weap");
	} // end of Build3d() 



	public static void RemoveOriginals () { // ....so they don't cause any trouble like spawning players outside the map 
		GameObject.Destroy(JumpPad);
		GameObject.Destroy(SpawnPoint);
		GameObject.Destroy(WeaponSpawn);
	}
	





	// private methods 
	private static void makeLights() {
		numMade = 0;
		int numLights = 25;
		for (int i = 0; i < numTries && numMade < numLights; i++) {
			Vec3i v;
			v.X = Random.Range (0, numVoxAcross.X);
			v.Y = Random.Range (0, numVoxAcross.Y);
			v.Z = Random.Range (0, numVoxAcross.Z);

			if (IsAir [v.X, v.Y, v.Z]) {
				if (!getBlock (v.X - 1, v.Y, v.Z)) {
					var nt = (GameObject)GameObject.Instantiate(Torch);
					nt.transform.position = Pos + new Vector3 (Scale.x * v.X - Scale.x * 0.5f + TorchOffset, Scale.y * v.Y, Scale.z * v.Z);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = primObject.transform;
					numMade++;
				}
				else
					if (!getBlock (v.X + 1, v.Y, v.Z)) {
						var nt = (GameObject)GameObject.Instantiate(Torch);
						nt.transform.position = Pos + new Vector3 (Scale.x * v.X + Scale.x * 0.5f - TorchOffset, Scale.y * v.Y, Scale.z * v.Z);
						nt.transform.localScale = Scale * TorchScale;
						nt.transform.parent = primObject.transform;
						numMade++;
					}
					else
						if (!getBlock (v.X, v.Y, v.Z - 1)) {
							var nt = (GameObject)GameObject.Instantiate(Torch);
							nt.transform.position = Pos + new Vector3 (Scale.x * v.X, Scale.y * v.Y, Scale.z * v.Z - Scale.z * 0.5f + TorchOffset);
							nt.transform.localScale = Scale * TorchScale;
							nt.transform.parent = primObject.transform;
							numMade++;
						}
						else
							if (!getBlock (v.X, v.Y, v.Z + 1)) {
								var nt = (GameObject)GameObject.Instantiate(Torch);
								nt.transform.position = Pos + new Vector3 (Scale.x * v.X, Scale.y * v.Y, Scale.z * v.Z + Scale.z * 0.5f - TorchOffset);
								nt.transform.localScale = Scale * TorchScale;
								nt.transform.parent = primObject.transform;
								numMade++;
							}
				// no torches on ceilings and floors! 
			}
			// end of scanning an individual block to see if a torch can be placed 
		}
	}



	private static void emptyMap() {
		for (int i = 0; i < numVoxAcross.X; i++)
		for (int j = 0; j < numVoxAcross.Y; j++)
		for (int k = 0; k < numVoxAcross.Z; k++) {
			IsAir[i, j, k] = false;
		}
	}

	// used so that there is a single room in the level 
	private static void preBuild() {
		Vec3i a;
		a.X = Random.Range(0, numVoxAcross.X - MaxFormWidth);
		a.Y = 0;
		a.Z = Random.Range(0, numVoxAcross.Z - MaxFormWidth);
		Vec3i b;
		b.X = Random.Range(a.X + MinFormWidth, a.X + MaxFormWidth);
		b.Y = Random.Range(MinHeight, MinHeight + HeightRand);
		b.Z = Random.Range(a.Z + MinFormWidth, a.Z + MaxFormWidth);
		fillRect(a, b);
	}

	// sets blocks to true (opens them to create rooms) and sets their material 
	private static void fillRect(Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		var curr = MatPool[Random.Range(0, MatPool.Count)]; // current mat 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it fills the space between the positions inclusively, so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++) {
			IsAir[i, j, k] = true;
			Mat[i, j, k] = curr;
		}
	}
	// at ground floor 
	private static void fillRect (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		int h = (int)((float)Mathf.Min(e.x - s.x, e.z - s.z) * SizeToHeight); // height(of the room)
		if (h < MinHeight) h = MinHeight;
		h += Random.Range(0, HeightRand);
		if (h >= numVoxAcross.Y) {
			MonoBehaviour.print("Voxel map generation error: room height is too large! Please set MapSize.y to a higher value.");
			return;
		}
		Vec3i a;
		a.X = s.x;
		a.Y = Random.Range(0, MaxFloorHeight + 1);
		a.Z = s.z;
		Vec3i b;
		b.X = e.x;
		b.Y = a.Y + h;
		b.Z = e.z;
		fillRect(a, b);
	}
	// this is only called for bridges & overground corridors 
	private static void fillRect(Vec2i s, Vec2i e, int h) { // start, end, height (of floor).  must be sorted and not be out of borders 
		Vec3i a;
		a.X = s.x;
		a.Y = h + 1; // h + 1 because floors generate at h(for bridges)
		a.Z = s.z;

		Vec3i b;
		b.X = e.x;
		b.Y = h + Random.Range(MinHeight, MinHeight + HeightRand) + 1;
		b.Z = e.z;

		fillRect(a, b);
	}



	// fills with false, used for creating bridges 
	private static void putWall(Vec2i s, Vec2i e, int h) { // start, end, height() (must be sorted and not be out of borders) 
		for (int i = s.x; i <= e.x; i++) // the <= is there because it fills the space between the positions inclusively, 
										// so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block 
		for (int k = s.z; k <= e.z; k++)
			IsAir[i, h, k] = false;
	}



	// counts all true blocks in an area 
	private static int countBlocks(Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		int t = 0; // temporary 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it counts the blocks inclusively, so counting from 3, 3, 3 to 3, 3, 3 can cause it to return 1 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++) {
			if (IsAir[i, j, k]) 
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
		for (int i = 0; i <= MaxFloorHeight; i++) {
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

	private static bool containsBlocks(Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		for (int i = s.X; i <= e.X; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++) {
			if (IsAir[i, j, k]) 
				return true;
		}

		return false; // if we got here, it means that there are no blocks in the area
	}
	// checks for blocks on the ground floor
	private static bool containsBlocks(Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.Y = 0;
		a.Z = s.z;
		Vec3i b;
		b.X = e.x;
		b.Y = MaxFloorHeight;
		b.Z = e.z;
		return containsBlocks (a, b);
	}
	private static bool containsBlocks(Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.Y = h;
		a.Z = s.z;
		Vec3i b;
		b.X = e.x;
		b.Y = h;
		b.Z = e.z;
		return containsBlocks (a, b);
	}

	// only returns the floor area 
	private static int getArea (Vec3i s, Vec3i e) { // start, end, must be sorted 
		return (e.X - s.X + 1) * (e.Z - s.Z + 1);
	}
	private static int getArea(Vec2i s, Vec2i e) { // start, end, must be sorted 
		return (e.x - s.x + 1) * (e.z - s.z + 1);
	}



	// used to safely get a block (ie when you're not sure whether it is out of the map) 
	private static bool getBlock(Vec3i p /* position */) {
		if (p.X < 0 || p.X >= numVoxAcross.X) return MapIsOpen;
		if (p.Y < 0 || p.Y >= numVoxAcross.Y) return MapIsOpen;
		if (p.Z < 0 || p.Z >= numVoxAcross.Z) return MapIsOpen;

		return IsAir[p.X, p.Y, p.Z];
	}
	private static bool getBlock(int x, int y, int z) {
		Vec3i v;
		v.X = x;
		v.Y = y;
		v.Z = z;

		return getBlock(v);
	}



	// checks if the area contains any blocks that are false (so that bridges are not suspended in mid-air) 
	private static bool containsWalls (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true 
		for (int j = s.Y; j <= e.Y; j++)
		for (int k = s.Z; k <= e.Z; k++)
			if (!IsAir[i, j, k]) return true;

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



	private static int floorScan(Vec3i sv) { // starting voxel 
		// y offset (go down from starting vox) 
		for (int yOff = 0; yOff <= sv.Y + 1; yOff++) {
			// <= p.y + 1 because it also scans the floor that is not coded as bloks, instead, is dependent on the MapIsOpen variable
			if (!getBlock(sv.X, sv.Y-yOff, sv.Z)) 
				return yOff;
		}

		return -1; // if no floor was reached before the map ended 
	}
	private static int floorScan(int x, int y, int z) {
		Vec3i v;
		v.X = x;
		v.Y = y;
		v.Z = z;
		return floorScan(v);
	}



	// checks if all floors at given heights contain open blocks, used to check if you can get into a corridor at all(MinHeight is considerd the player height)
	private static bool eachFloorOpen (Vec2i s, Vec2i e, int hs, int he) { // start, end, starting height, ending height(inclusive, must be sorted and not be out of borders)
		for (int i = hs; i <= he; i++)
			if (!containsBlocks(s, e, i)) return false;

		return true; // if we got here, everything's fine 
	}



	// used for checking if a spawn can be placed, only checks a column of blocks 
	private static bool columnOpen(Vec3i s, int h) { // starting position, height from starting position 
		for (int i = s.Y; i <= s.Y + h; i++)
			if (!getBlock(s.X, i, s.Z)) 
				return false;

		return true; // if we got here, it means all checked are air voxels 
	}
	private static bool columnOpen(int x, int y, int z, int h) { // position, height 
		Vec3i v;
		v.X = x;
		v.Y = y;
		v.Z = z;
		return columnOpen(v, h);
	}



	private static void makeGroundFloor() {
		int numMade = 0;
		for (int i = 0; i < numTries && numMade < Forms; i++) {
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
			
			if (containsBlocks(start, end)) {
				if ((end.x >= start.x + MinFormWidth) && (end.z >= start.z + MinFormWidth))
					if ((end.x <= start.x + MaxFormWidth) && (end.z <= start.z + MaxFormWidth))
						if (getArea(start, end) <= MaxArea && getArea(start, end) >= MinArea)
						if (countBlocks(start, end) < getArea(start, end) * MaxOverride * (MaxFloorHeight + 1f)) {
							fillRect(start, end);
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

			// make single corridor/bridge 
			for (int i = 0; i < numTries && numMade < FormsPerFloor; i++) {
				Vec2i t;
				t.x = Random.Range(0, numVoxAcross.X);
				t.z = Random.Range(0, numVoxAcross.Z);
				
				Vec2i u;
				u.x = Random.Range(0, numVoxAcross.X);
				u.z = Random.Range(0, numVoxAcross.Z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
				
				Vec2i start;
				start.x = Mathf.Min(t.x, u.x);
				start.z = Mathf.Min(t.z, u.z);

				Vec2i end;
				end.x = Mathf.Max(t.x, u.x);
				end.z = Mathf.Max(t.z, u.z);

				if (eachFloorOpen(start, end, h + 1, h + MinHeight)) // MinHeight is considered to be the player height, to check if the corridor can be accessed 
					if ((end.x >= start.x + MinFormWidth) && (end.z >= start.z + MinFormWidth))
						if ((end.x <= start.x + MaxFormWidth) && (end.z <= start.z + MaxFormWidth))
							if (containsWalls(start, end, h)) // so that bridges don't generate in mid-air 
								// no MaxOverride check here because we want bridges to generate 
								if (getArea(start, end) <= MaxCorridorArea) {
									putWall(start, end, h);
									fillRect(start, end, h);
									numMade++;
								}
			}
		} // end of corridor/bridge creation
	}



	private static void maybeMakeQuad(Vector3 dir, int x, int y, int z, Vector3 offset) {
		if (!getBlock(x, y, z)) {
			var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
			np.transform.position = Pos + offset;
			np.transform.localScale = Scale;
			np.transform.forward = dir;
			//np.renderer.material = Mat[x, y, z];
			np.transform.parent = primObject.transform;
		}
	}
}
