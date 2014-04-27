using UnityEngine;
using System.Collections;

public class RlikePlane : MonoBehaviour {

	RoguelikeLevel lev;

	// Use this for initialization
	void Start () {
		lev = ScriptableObject.CreateInstance<RoguelikeLevel>();
		lev.MapSize.x = 64;
		lev.MapSize.y = 64;
		lev.Forms = 40;
		lev.MaxArea = 50;
		lev.MaxFormWidth = 16;
		lev.MinFormWidth = 1;
		lev.MaxOverride = 0.1f;
		lev.Init();
		Texture2D texture = new Texture2D(64, 64);
		texture.filterMode = FilterMode.Point;
		renderer.material.mainTexture = texture;
		for (int x = 0; x < texture.width; x++)
		for(int y = 0; y < texture.height; y++) {
				Color color = (lev.Cells[x, y].IsAir ? Color.white : Color.black);
				texture.SetPixel(x, y, color);
		}
		texture.Apply();
		lev.Scale = new Vector3(3f, 3f, 3f);
		lev.Pos = new Vector3(
			lev.Scale.x, 
			-lev.Scale.y*2, 
			lev.Scale.z);
		lev.Build3D();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
