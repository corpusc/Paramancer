using UnityEngine;
using System.Collections;

public class VoxelMapInit : MonoBehaviour {

	ProcGenVoxel vox;
	static ProcGenVoxel s_vox;
	
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

	public static void CreateMap (int seed) {
		s_vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		s_vox.Seed = seed;
		s_vox.MapSize.x = 64;
		s_vox.MapSize.y = 32;
		s_vox.MapSize.z = 64;
		s_vox.Pos = Vector3.zero;
		s_vox.Scale = Vector3.one * 2f;
		s_vox.Init();
		s_vox.Build();
		s_vox.Build3d();
		s_vox.RemoveOriginals();
		Random.seed = (int)(Time.time * 100f);
	}

	public static void CreateMap (int seed, GameStyle style) {
		s_vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		s_vox.Seed = seed;
		s_vox.Style = style;
		s_vox.MapSize.x = 64;
		s_vox.MapSize.y = 32;
		s_vox.MapSize.z = 64;
		s_vox.Pos = Vector3.zero;
		s_vox.Scale = Vector3.one * 2f;
		s_vox.Init();
		s_vox.Build();
		s_vox.Build3d();
		s_vox.RemoveOriginals();
		Random.seed = (int)(Time.time * 100f);
	}
}
