using UnityEngine;
using System.Collections;



public class DebugVoxel : MonoBehaviour {
	ProcGenVoxel vox;
	static ProcGenVoxel s_vox;



	void Start() {
		vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		vox.Pos = Vector3.zero;
		vox.Scale = Vector3.one * 2f;
		vox.Init();
		vox.Build();
		vox.Build3d();
		vox.RemoveOriginals();
	}

	public static void CreateMap(int seed) {
		s_vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		s_vox.Seed = seed;
		s_vox.Pos = Vector3.zero;
		s_vox.Scale = Vector3.one * 2f;
		s_vox.Init();
		s_vox.Build();
		s_vox.Build3d();
		s_vox.RemoveOriginals();
		Random.seed = (int)(Time.time * 100f);
	}

	public static void CreateMap(int seed, Theme theme) {
		s_vox = ScriptableObject.CreateInstance<ProcGenVoxel>();
		s_vox.Seed = seed;
		s_vox.Theme = theme; //
		s_vox.Pos = Vector3.zero;
		s_vox.Scale = Vector3.one * 2f;
		s_vox.Init();
		s_vox.Build();
		s_vox.Build3d();
		s_vox.RemoveOriginals();
		Random.seed = (int)(Time.time * 100f);
	}
}
