using UnityEngine;
using System.Collections;

public class GunTypeScript : MonoBehaviour {
	public string gunName = "pistol";
	public Texture iconTex;
	public GameObject modelPrefab;
	public Material gunMaterial;
	public bool isAutomatic = false;
	public float fireCooldown = 0f;
	
}
