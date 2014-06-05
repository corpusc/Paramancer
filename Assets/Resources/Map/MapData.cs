using UnityEngine;
using System.Collections;

public class MapData {
	public string Name = "";
	public Texture Pic;
	public bool ProcGen = false;
	
	public MapData(string name, Texture pic, bool procGen = false) {
		Name = name;
		Pic = pic;
		ProcGen = procGen;
	}
}
