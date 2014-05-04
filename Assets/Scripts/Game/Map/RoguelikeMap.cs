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
	public Vec3i MapSize;
	public int Forms = 40; // the amount of rooms/hallways to create 
	public float MaxOverride = 0.2f; // only create a form if there aren't too many things already in there 
	public int MinFormWidth = 1;
	public int MaxFormWidth = 16;
	public int MaxArea = 100; // limits the creation of extremely large rooms, favorizes corridors
	public Vector3 Pos = Vector3.zero; // the position of the min-coordinate corner of the generated map
	public Vector3 Scale = Vector3.one; // the scale of all elements on the map
	public int MinHeight = 2; // the minimal height a room can have
	public int HeightRand = 3; // the maximal additional room height(minimal is 0)
	// the maximal height of a room is determined by the map height & the room size

	// private
	int numTries = 5000; // the amount of attempts to place a room to be done before giving up (used for safety to avoid an infinite loop)
	List<Material> MatPool = new List<Material>();

	public void Init () {
		Block = new bool[MapSize.x, MapSize.y, MapSize.z];
		Mat = new Material[MapSize.x, MapSize.y, MapSize.z];
		MatPool.Add((Material)Resources.Load("Mat/Allegorithmic/metal_floor_003", typeof(Material)));
	}
}
