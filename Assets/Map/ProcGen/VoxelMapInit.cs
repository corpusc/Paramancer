using UnityEngine;
using System.Collections;

public class VoxelMapInit : MonoBehaviour {

	ProcGenVoxel vox;
	
	void Start () {
		vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		vox.MapSize.x = 64;
		vox.MapSize.y = 32;
		vox.MapSize.z = 64;
		vox.Pos = Vector3.zero;
		vox.Scale = Vector3.one * 2f;
		vox.Init();
		vox.Build();
		vox.Build3d();
		vox.RemoveOriginals();
	}
}
