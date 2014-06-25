using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public struct Vec3i {
	public int X;
	public int y;
	public int z;
};



public class VoxGen : MonoBehaviour {
	public int Seed = 0;
	public bool[,,] Block; // true = the block is open, false = unreachable(wall etc) 
	public Material[,,] Mat;
	public int Forms = 40; // the amount of rooms/halls to create on the ground floor 
	public int FormsPerFloor = 10; // the amount of corridors/bridges that connect rooms reaching the given height, per floor 
	public float MaxOverride = 0.3f; // only create a form if there aren't too many things already in there 
	public int MinFormWidth = 2; // the minimal span of a form(this will end up being the width ussually) 
	public int MaxFormWidth = 16; // the maximal span of a form(this will end up being the length ussually) 
	public int MinArea = 20; // don't create tiny rooms that will not be noticed 
	public int MaxArea = 100; // limits the creation of extremely large rooms, favorizes corridors 
	public int MaxCorridorArea = 60; // for corridors/bridges above the ground floor 
	public Vector3 Pos = Vector3.zero; // the position of the min-coordinate corner of the generated map 
	public Vector3 Scale = Vector3.one; // the scale of all elements on the map 
	public float SizeToHeight = 0.85f; // this is used to give a larger height to rooms with a square-like shape.  Setting it to a lower value will cause rooms to appear flatter. 
	public int MinHeight = 2; // the minimal height a room can have 
	public int MaxFloorHeight = 1; // the maximal height of the floor of the rooms on the ground floor. Use 0 for no randomness. 
	public int HeightRand = 4; // the maximal additional room height(minimal is 0)(last-exclusive, so set to 1 for no randomness) 
	public int CorridorHeightRand = 2; // same as above, but for corridors 
	public int MinFloorLevelStep = 8; // the minimal height distance between two corridor levels on top of each other 
	public int MaxFloorLevelStep = 10; // the maximal height distance between two corridor levels on top of each other 
	public int MinCorridorStartHeight = 3; // the minimal height at which corridors & bridges are created 
	public int MaxCorridorStartHeight = 5; // the maximal height at which corridors & bridges are created 
	public bool MapIsOpen = false; // used to control whether corridors reaching the border of the map are to be closed 

	public Theme Theme = Theme.SciFi; // used to control the placing of art assets & the texture images 

	// assets to be placed on the map 
	//		lights 
	public GameObject Torch;
	public float TorchScale = 0.3f;
	public float TorchOffset = 0.1f; // the distance from the center of the torch to the wall/floor it is attached to 
	// 		jump pads 
	public int numJumpPads = 10;
	public GameObject JumpPad;
	public Vector3 JumpPadScale = new Vector3(1f, 0.3f, 1f);
	public int JumpHeight = 2; // the height that cannot be jumped over normally (needs a jump pad & must be lesser than MapSize.y) 
	public float JumpPadOffset = 0.05f; // the distance from the center of the jump pad to the floor it is on 
	// spawns 
	public Vector3 SpawnPointScale = Vector3.one;
	public GameObject SpawnPoint;
	public int SpawnPoints = 4; // the amount of spawn points to be placed(of each type[FFA/red/blue]) 
	public int MonsterSpawns = 6;
	// 		weapons 
	public GameObject WeaponSpawn;
	public Vector3 WeaponSpawnScale = new Vector3(0.5f, 0.1f, 0.5f);
	public float WeaponSpawnOffset = 0.05f; // the distance from the center of the weapon spawn to the floor it is spawned on 

	// the maximal height of a room is determined by the map height & the room size 
	// This assumes the values you passed make sense, ie you didn't make MinFormWidth > MaxFormWidth 
	// WARNING: MaxFormWidth must be lesser than MapSize.x and MapSize.z, and MinHeight + HeightRand must be lesser than MapSize.y 
	// To create a map, set all the values you need, and call Init(), Build() and Build3d() 



	// private 
	Vec3i numVoxAcross;
	int numGunSpawns = 10;
	int numTries = 20000; // the amount of attempts to place a room,before giving up 
	List<Material> MatPool = new List<Material>();

	GameObject primObject;
	GameObject ffaSpawnBag;
	GameObject blueSpawnBag;
	GameObject redSpawnBag;
	GameObject weaponSpawnBag;
	GameObject monsterSpawnBag;



	static VoxGen s_vox;
	public static void GenerateMap(int seed, Theme theme) {
		s_vox = ScriptableObject.CreateInstance<VoxGen>();
		s_vox.Seed = seed;
		s_vox.Theme = theme;
		s_vox.Scale = Vector3.one * 2f;
		s_vox.Init();
		s_vox.Build();
		s_vox.Build3d();
		s_vox.RemoveOriginals();
		Random.seed = (int)(Time.time * 100f);
	}

	// setup (doesn't build) 
	// call Build() and Build3D() right after this for the random seed to work 
	public void Init() {
		Random.seed = Seed;
		numVoxAcross.X = 64;
		numVoxAcross.y = 32;
		numVoxAcross.z = 64;

		Block = new bool[numVoxAcross.X, numVoxAcross.y, numVoxAcross.z];
		Mat = new Material[numVoxAcross.X, numVoxAcross.y, numVoxAcross.z];

		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_floor_003", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_plate_005", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/metal_plate_008", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/sci_fi_003", typeof(Material)));
		MatPool.Add((Material)Resources.Load("MEDIA/Mat/SciFi/Shutter_01", typeof(Material)));

		Torch = GameObject.Find("Torch"); // a GameObject called Torch must already be in the scene for this to work, it will be removed after everything is done by the script
		JumpPad = GameObject.Find("JumpPad"); // a GameObject called JumpPad must already be in the scene for this to work, it will be removed after everything is done by the script
		SpawnPoint = GameObject.Find("SpawnPoint"); // a GameObject called SpawnPoint must already be in the scene for this to work, it will be removed after everything is done by the script
		WeaponSpawn = GameObject.Find("WeaponSpawn"); // a GameObject called WeaponSpawn must already be in the scene, it will be removed afterwards by calling RemoveOriginals()
		primObject = GameObject.Find("[PRIMS]"); // an empty GameObject container, already in the scene 
		ffaSpawnBag = GameObject.Find("FFA Spawns"); // an empty GameObject container, already in the scene 
		blueSpawnBag = GameObject.Find("Blue Team Spawns"); // an empty GameObject container, already in the scene 
		redSpawnBag = GameObject.Find("Red Team Spawns"); // an empty GameObject container, already in the scene 
		weaponSpawnBag = GameObject.Find("_PickupSpots"); // an empty GameObject container, must already be in the scene
		monsterSpawnBag = GameObject.Find("Monster Spawns"); // an empty GameObject container, must already be in the scene
	}

	// this will build a model of the level in memory, to generate the 3d model in the scene, use Build3d() 
	public void Build() {
		emptyMap();
		preBuild();

		// start off by creating the ground floor (the main map) 
		int formsMade = 0;
		for (int i = 0; i < numTries && formsMade < Forms; i++) {
			Vec2i t;
			t.x = Random.Range(0, numVoxAcross.X);
			t.z = Random.Range(0, numVoxAcross.z);
			
			Vec2i u;
			u.x = Random.Range(0, numVoxAcross.X);
			u.z = Random.Range(0, numVoxAcross.z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
			
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
							formsMade++;
						}
			}
		} // end of ground floor creation

		//...and then add bridges and corridors higher up
		for (int h = Random.Range(MinCorridorStartHeight, MaxCorridorStartHeight + 1); h < numVoxAcross.y - MinHeight - HeightRand; h += Random.Range(MinFloorLevelStep, MaxFloorLevelStep + 1)) { // h is the height of the floor
			formsMade = 0;
			for (int i = 0; i < numTries && formsMade < FormsPerFloor; i++) {
				Vec2i t;
				t.x = Random.Range(0, numVoxAcross.X);
				t.z = Random.Range(0, numVoxAcross.z);
				
				Vec2i u;
				u.x = Random.Range(0, numVoxAcross.X);
				u.z = Random.Range(0, numVoxAcross.z); // this is last-exclusive, hence all the <= and ...+ 1 later on 
				
				Vec2i start;
				Vec2i end;
				start.x = Mathf.Min(t.x, u.x);
				start.z = Mathf.Min(t.z, u.z);
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
							formsMade++;
						}
			} // end of the creation of a single corridor/bridge
		} // end of corridor/bridge creation

	} // end of Build ()

	// this generates the level in 3d(puts it in the scene) based what Build() created - call Build() before this
	int numMade = 0;
	public void Build3d () {
		for (int i = 0; i < numVoxAcross.X; i++)
		for (int j = 0; j < numVoxAcross.y; j++)
		for (int k = 0; k < numVoxAcross.z; k++) {
			// only build walls around a block if it's not a wall itself
			if (Block[i, j, k]) {
					if (!getBlock(i - 1, j, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i - Scale.x * 0.5f, Scale.y * j, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.left;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
					if (!getBlock(i + 1, j, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i + Scale.x * 0.5f, Scale.y * j, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.right;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
					if (!getBlock(i, j, k - 1)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j, Scale.z * k - Scale.z * 0.5f);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.back;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
					if (!getBlock(i, j, k + 1)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j, Scale.z * k + Scale.z * 0.5f);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.forward;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
					if (!getBlock(i, j - 1, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j - Scale.y * 0.5f, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.down;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
					if (!getBlock(i, j + 1, k)) {
						var np = GameObject.CreatePrimitive(PrimitiveType.Quad);
						np.transform.position = Pos + new Vector3(Scale.x * i, Scale.y * j + Scale.y * 0.5f, Scale.z * k);
						np.transform.localScale = Scale;
						np.transform.forward = Vector3.up;
						np.renderer.material = Mat[i, j, k];
						np.transform.parent = primObject.transform;
					}
			} // end of wall creation for the current block
		} // end of block scan
		
		// place assets 
		numMade = 0;
		int numLights = 25;
		for (int i = 0; i < numTries && numMade < numLights; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (Block[t.X, t.y, t.z]) {
				if (!getBlock(t.X - 1, t.y, t.z)) {
					var nt = (GameObject)GameObject.Instantiate(Torch);
					nt.transform.position = Pos + new Vector3(Scale.x * t.X - Scale.x * 0.5f + TorchOffset, Scale.y * t.y, Scale.z * t.z);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = primObject.transform;
					numMade++;
				}else 
				if (!getBlock(t.X + 1, t.y, t.z)) {
					var nt = (GameObject)GameObject.Instantiate(Torch);
					nt.transform.position = Pos + new Vector3(Scale.x * t.X + Scale.x * 0.5f - TorchOffset, Scale.y * t.y, Scale.z * t.z);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = primObject.transform;
					numMade++;
				}else 
				if (!getBlock(t.X, t.y, t.z - 1)) {
					var nt = (GameObject)GameObject.Instantiate(Torch);
					nt.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z - Scale.z * 0.5f + TorchOffset);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = primObject.transform;
					numMade++;
				}else 
				if (!getBlock(t.X, t.y, t.z + 1)) {
					var nt = (GameObject)GameObject.Instantiate(Torch);
					nt.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z + Scale.z * 0.5f - TorchOffset);
					nt.transform.localScale = Scale * TorchScale;
					nt.transform.parent = primObject.transform;
					numMade++;
				}
				// no torches on ceilings and floors! 
			} // end of scanning an individual block to see if a torch can be placed 
		} // end of placing torches 
		
		// place jump pads 
		numMade = 0;
		for (int i = 0; i < numTries && numMade < numJumpPads; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(JumpHeight, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (Block[t.X, t.y, t.z] && !Block[t.X, t.y - 1, t.z]) { // if this is the floor of any form above jump height 
				int d = floorScan(t.X - 1, t.y, t.z); // distance 
				if (d >= JumpHeight && columnOpen(t.X - 1, t.y, t.z, MinHeight)) {
					var nj = (GameObject)GameObject.Instantiate(JumpPad);
					nj.transform.position = Pos + new Vector3(Scale.x * t.X - Scale.x, Scale.y * (t.y - d + 0.5f + JumpPadOffset), Scale.z * t.z);
					nj.transform.localScale = JumpPadScale;
					nj.transform.parent = primObject.transform;
					numMade++;
				} else {
					d = floorScan(t.X + 1, t.y, t.z);

					if (d >= JumpHeight && columnOpen(t.X + 1, t.y, t.z, MinHeight)) {
						var nj = (GameObject)GameObject.Instantiate(JumpPad);
						nj.transform.position = Pos + new Vector3(Scale.x * t.X + Scale.x, Scale.y * (t.y - d + 0.5f + JumpPadOffset), Scale.z * t.z);
						nj.transform.localScale = JumpPadScale;
						nj.transform.parent = primObject.transform;
						numMade++;
					} else {
						d = floorScan(t.X, t.y, t.z - 1);
						if (d >= JumpHeight && columnOpen(t.X, t.y, t.z - 1, MinHeight)) {
							var nj = (GameObject)GameObject.Instantiate(JumpPad);
							nj.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * (t.y - d + 0.5f + JumpPadOffset), Scale.z * t.z - Scale.z);
							nj.transform.localScale = JumpPadScale;
							nj.transform.parent = primObject.transform;
							numMade++;
						} else {
							d = floorScan(t.X, t.y, t.z + 1);
							if (d >= JumpHeight && columnOpen(t.X, t.y, t.z + 1, MinHeight)) {
								var nj = (GameObject)GameObject.Instantiate(JumpPad);
								nj.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * (t.y - d + 0.5f + JumpPadOffset), Scale.z * t.z + Scale.z);
								nj.transform.localScale = JumpPadScale;
								nj.transform.parent = primObject.transform;
								numMade++;
							}
						}
					}
				}
			} // end of checking the block for possible jump pad placing 
		} // end of placing jump pads 
		
		// place FFA spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.y - 1, t.z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = ffaSpawnBag.transform;
				numMade++;
			}
		}

		// place blue spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.y - 1, t.z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = blueSpawnBag.transform;
				numMade++;
			}
		}

		// place red spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < SpawnPoints; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.y - 1, t.z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = redSpawnBag.transform;
				numMade++;
			}
		}

		// place monster spawn points 
		numMade = 0; // count spawn points as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < MonsterSpawns; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);
			if (columnOpen(t, MinHeight)) // if the place is accessible(ceiling height)
			if (!getBlock(t.X, t.y - 1, t.z)) {
				var ns = (GameObject)GameObject.Instantiate(SpawnPoint); // new spawn point 
				ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y, Scale.z * t.z);
				ns.transform.localScale = SpawnPointScale;
				ns.transform.parent = monsterSpawnBag.transform;
				numMade++;
			}
		}

		// place weapon spawns 
		numMade = 0; // count weapon spawns as forms, no need for a separate var 
		for (int i = 0; i < numTries && numMade < numGunSpawns; i++) {
			Vec3i t;
			t.X = Random.Range(0, numVoxAcross.X);
			t.y = Random.Range(0, numVoxAcross.y);
			t.z = Random.Range(0, numVoxAcross.z);

			// if the place is accessible (ceiling height) 
			if (columnOpen(t, MinHeight)) {
				if (!getBlock(t.X, t.y - 1, t.z)) {
					var ns = (GameObject)GameObject.Instantiate(WeaponSpawn); // new weapon spawn 
					ns.transform.position = Pos + new Vector3(Scale.x * t.X, Scale.y * t.y + WeaponSpawnOffset - Scale.y * 0.5f, Scale.z * t.z);
					ns.transform.localScale = WeaponSpawnScale;
					ns.transform.parent = weaponSpawnBag.transform;
					ns.GetComponent<PickupPoint>().pickupPointID = numMade;
					numMade++;
				}
			}
		}
	} // end of Build3d() 
	
	public void RemoveOriginals () { // ....so they don't cause any trouble like spawning players outside the map 
		GameObject.Destroy(Torch);
		GameObject.Destroy(JumpPad);
		GameObject.Destroy(SpawnPoint);
		GameObject.Destroy(WeaponSpawn);
	}



	// private methods 

	void emptyMap() {
		for (int i = 0; i < numVoxAcross.X; i++)
		for (int j = 0; j < numVoxAcross.y; j++)
		for (int k = 0; k < numVoxAcross.z; k++) {
				Block[i, j, k] = false;
		}
	}

	// used so that there is a single room in the level 
	void preBuild() {
		Vec3i a;
		a.X = Random.Range(0, numVoxAcross.X - MaxFormWidth);
		a.y = 0;
		a.z = Random.Range(0, numVoxAcross.z - MaxFormWidth);
		Vec3i b;
		b.X = Random.Range(a.X + MinFormWidth, a.X + MaxFormWidth);
		b.y = Random.Range(MinHeight, MinHeight + HeightRand);
		b.z = Random.Range(a.z + MinFormWidth, a.z + MaxFormWidth);
		fillRect(a, b);
	}

	// sets blocks to true(opens them to create rooms) and sets their material 
	void fillRect(Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders) 
		Material cMat = MatPool[Random.Range(0, MatPool.Count)]; // current mat 
		for (int i = s.X; i <= e.X; i++) // the <= is there because it fills the space between the positions inclusively, so filling 3, 3, 3 to 3, 3, 3 will result in filling 1 block 
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			Block[i, j, k] = true;
			Mat[i, j, k] = cMat;
		}
	}

	// at ground floor 
	void fillRect (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		int h = (int)((float)Mathf.Min(e.x - s.x, e.z - s.z) * SizeToHeight); // height(of the room)
		if (h < MinHeight) h = MinHeight;
		h += Random.Range(0, HeightRand);
		if (h >= numVoxAcross.y) {
			MonoBehaviour.print("Voxel map generation error: room height is too large! Please set MapSize.y to a higher value.");
			return;
		}
		Vec3i a;
		a.X = s.x;
		a.y = Random.Range(0, MaxFloorHeight + 1);
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = a.y + h;
		b.z = e.z;
		fillRect(a, b);
	}

	// this is only called for bridges & overground corridors
	void fillRect (Vec2i s, Vec2i e, int h) { // start, end, height(of the floor)(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = h + 1; // h + 1 because floors generate at h(for bridges)
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = h + Random.Range(MinHeight, MinHeight + HeightRand) + 1;
		b.z = e.z;
		fillRect(a, b);
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
		for (int i = s.X; i <= e.X; i++) // the <= is there because it counts the blocks inclusively, so counting from 3, 3, 3 to 3, 3, 3 can cause it to return 1
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			if (Block[i, j, k]) t++;
		}
		return t;
	}

	// gets the taken area of the floor
	int countBlocks (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		int m = 0; // the temporary var for storing the maximal amount of blocks in a level
		Vec3i a;
		a.X = s.x;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.z = e.z;
		for (int i = 0; i <= MaxFloorHeight; i++) {
			a.y = i;
			b.y = i;
			int t = countBlocks(a, b);
			if (t > m) m = t;
		}
		return m;
	}

	// gets the taken area of the floor
	int countBlocks (Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = h;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = h;
		b.z = e.z;
		return countBlocks(a, b);
	}

	bool containsBlocks (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		for (int i = s.X; i <= e.X; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++) {
			if (Block[i, j, k]) return true;
		}
		return false; // if we got here, it means that there are no blocks in the area
	}

	// checks for blocks on the ground floor
	bool containsBlocks (Vec2i s, Vec2i e) { // start, end(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = 0;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = MaxFloorHeight;
		b.z = e.z;
		return containsBlocks (a, b);
	}
	
	bool containsBlocks (Vec2i s, Vec2i e, int h) { // start, end, height(must be sorted and not be out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = h;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = h;
		b.z = e.z;
		return containsBlocks (a, b);
	}

	// only returns the floor area
	int getArea (Vec3i s, Vec3i e) { // start, end, must be sorted
		return (e.X - s.X + 1) * (e.z - s.z + 1);
	}

	int getArea (Vec2i s, Vec2i e) { // start, end, must be sorted
		return (e.x - s.x + 1) * (e.z - s.z + 1);
	}

	//used to safely get a block(ie when you're not sure whether it is out of the map)
	bool getBlock (Vec3i p) { // position
		if (p.X < 0 || p.X >= numVoxAcross.X) return MapIsOpen;
		if (p.y < 0 || p.y >= numVoxAcross.y) return MapIsOpen;
		if (p.z < 0 || p.z >= numVoxAcross.z) return MapIsOpen;
		return Block[p.X, p.y, p.z];
	}

	bool getBlock (int x, int y, int z) {
		Vec3i t; // temporary
		t.X = x;
		t.y = y;
		t.z = z;
		return getBlock(t);
	}

	// checks if the area contains any blocks that are false(used so that bridges are not suspended in mid-air)
	bool containsWalls (Vec3i s, Vec3i e) { // start, end(must be sorted and not be out of borders)
		for (int i = s.X; i <= e.X; i++) // the <= is there because it checks the blocks inclusively, so checking from 3, 3, 3 to 3, 3, 3 can cause it to return true
		for (int j = s.y; j <= e.y; j++)
		for (int k = s.z; k <= e.z; k++)
			if (!Block[i, j, k]) return true;

		return false; // if we got here, it means that there are no walls in the area
	}

	bool containsWalls (Vec2i s, Vec2i e, int h) { // start, end, height(sorted, not out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = h;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = h;
		b.z = e.z;
		return containsWalls (a, b);
	}

	bool containsWalls (Vec2i s, Vec2i e) { // start, end(sorted, not out of borders)
		Vec3i a;
		a.X = s.x;
		a.y = 0;
		a.z = s.z;
		Vec3i b;
		b.X = e.x;
		b.y = 0;
		b.z = e.z;
		return containsWalls (a, b);
	}

	// scans for walls starting from a given point, going down, returns the distance to the nearest floor
	int floorScan (Vec3i p) { // position
		for (int i = 0; i <= p.y + 1; i++) { // <= p.y + 1 because it also scans the floor that is not coded as bloks, instead, is dependent on the MapIsOpen variable
			if (!getBlock(p.X, p.y - i, p.z)) return i;
		}
		return -1; // if no floor was reached before the map ended
	}

	int floorScan (int x, int y, int z) {
		Vec3i t; // temporary
		t.X = x;
		t.y = y;
		t.z = z;
		return floorScan(t);
	}

	// checks if all floors at given heights contain open blocks, used to check if you can get into a corridor at all(MinHeight is considerd the player height)
	bool eachFloorOpen (Vec2i s, Vec2i e, int hs, int he) { // start, end, starting height, ending height(inclusive, must be sorted and not be out of borders)
		for (int i = hs; i <= he; i++)
			if (!containsBlocks(s, e, i)) return false;

		return true; // if we got here, everything's fine
	}

	// used for checking if a spawn can be placed, only checks a column of blocks
	bool columnOpen (Vec3i s, int h) { // starting position, height from starting position
		for (int i = s.y; i <= s.y + h; i++)
			if (!getBlock(s.X, i, s.z)) return false;

		return true; // if we got here, it means all blocks we checked are open
	}

	bool columnOpen (int x, int y, int z, int h) { // position, height
		Vec3i t;
		t.X = x;
		t.y = y;
		t.z = z;
		return columnOpen(t, h);
	}
}
