using UnityEngine;
using System.Collections;

public class RlikePlane : MonoBehaviour {

	RoguelikeLevel lev;

	// Use this for initialization
	void Start () {
		lev = new RoguelikeLevel();
		lev.MapSize.x = 1024;
		lev.MapSize.y = 1024;
		lev.Block = new bool[1024, 1024]; //manually init this because lev.Start() will only be called after this start(), which would make texture.SetPixel harder to do
		lev.BlockType = new int[1024, 1024];
		lev.Build();
		Texture2D texture = new Texture2D(1024, 1024);
		renderer.material.mainTexture = texture;
		int y = 0;
		while (y < texture.height) {
			int x = 0;
			while (x < texture.width) {
				Color color = (lev.Block[x, y] ? Color.white : Color.gray);
				texture.SetPixel(x, y, color);
				++x;
			}
			++y;
		}
		texture.Apply();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
