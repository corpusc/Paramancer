using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GridEntry : MonoBehaviour {
	GridGen grid;
	Awesonator aweso = new Awesonator();



	void Start() {
		// generate a grid 
		grid = new GridGen();
		//Debug.LogError("let's count these");
		var pic = new Texture2D(grid.Max.x, grid.Max.z);
		pic.filterMode = FilterMode.Point;
		renderer.material.mainTexture = pic;

		Debug.Log("about to SetPixel all the things");
		for (int x = 0; x < pic.width; x++)
		for (int y = 0; y < pic.height; y++) {
			Color color = (grid.Cells[x, y].IsAir ? Color.white : Color.black);
			pic.SetPixel(x, y, color);
			Debug.Log("SetPixel ONE");
		}
		Debug.Log("set!");

		pic.Apply();
		Debug.Log("applied!");

		grid.Scale = new Vector3(3f, 3f, 3f);
		Debug.Log("set scale");
		grid.Pos = new Vector3(
			grid.Scale.x, 
			-grid.Scale.y*2, 
			grid.Scale.z);
		Debug.Log("set pos");
		grid.GenerateSurfaces();
		Debug.Log("survived Build3D()");
	}

	void Update() {
		aweso.Update();
	}
}
