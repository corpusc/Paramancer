using UnityEngine;
using System.Collections;

public class RlikePlane : MonoBehaviour {

	RoguelikeLevel lev;

	// Use this for initialization
	void Start () {
		lev = (RoguelikeLevel)this.GetComponent("RoguelikeLevel");
		lev.MapSize.x = 128;
		lev.MapSize.y = 128;
		lev.Forms = 1;
		lev.Block = new bool[128, 128];
		lev.Build();
		/*
		Vec2i a;
		Vec2i b;
		a.x = 0;
		a.y = 0;
		b.x = 13;
		b.y = 16;
		lev.fillRect(a, b);
		*/
		Texture2D texture = new Texture2D(128, 128);
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
