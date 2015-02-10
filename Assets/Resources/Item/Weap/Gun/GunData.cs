using UnityEngine;
using System.Collections;

public class GunData {
	public string Name = "default name";
	public Texture Pic;
	public Material Mat;
	public GameObject Prefab;
	public GameObject Instance;
	public Renderer Renderer;
	public Vector3 PosOffset = Vector3.zero; // i don't think these 2 offsets are needed when prefabs are setup right 
	public Vector3 EulerOffset = Vector3.zero;
	public bool AutoFire = false;
	public float BlastRadius = 0f;
	public Color Color = Color.white;
	public float Delay = 0f;
	public float Cooldown = 0f; // current progress through the ^above^ Delay 
	public bool Carrying = false;
	public float Range = 999f;
	public float MarkScale = 1f;
}
