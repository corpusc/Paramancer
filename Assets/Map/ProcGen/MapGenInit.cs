using UnityEngine;
using System.Collections;

public class MapGenInit : MonoBehaviour {

	ProcGenVoxel vox;

	// Use this for initialization
	void Start () {
		vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		vox.MapSize.x = 64;
		vox.MapSize.y = 32;
		vox.MapSize.z = 64;
		vox.Pos = Vector3.zero;
		vox.Init();
		vox.Build();
		vox.Build3d();
		vox.RemoveOriginals();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
