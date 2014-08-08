using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public static class Pics {
	// hardcoded/variable-based textures 
	static public Texture Health;
	static public Texture White;
	static public Texture Black;
	static public Texture TeamRedFlag;
	static public Texture TeamBlueFlag;
	static public Texture Sprint;
	static public Texture CrossHair;
	static public Texture[] CrosshairSwapper = new Texture[4];

	// private 
	static Dictionary<string, Texture> pics = new Dictionary<string, Texture>();


	
	static Pics() {
		doFolder("Hud", false);
		doFolder("Item/Weap/Gun", false);
		doFolder("Map/Preview", false);
	}
	
	
	public static Texture Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (pics.ContainsKey(s))
			return pics[s];
		
		Debug.LogError("______ COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static Texture GetFirstWith(string s) { // this is slow i think.  iterating thru key/value pairs 
		foreach (var pair in pics) {
			if (pair.Value.name.Contains(s))
				return pair.Value;
		}
		
		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}

	static void doFolder(string s, bool inPicFolder = true) {
		string feedback = "";
		string path = s;

		if (inPicFolder)
			path = "Pic/" + s;
		
		var tPics = Resources.LoadAll<Texture>(path);
		
		// add to the master collection that includes files from all subfolders 
		foreach (var t in tPics) {
			if (pics.ContainsKey(t.name)) {
				Debug.LogError("______ The name: " + t.name + " is already a registered Texture!!! ______");
			} else {
				pics.Add(t.name, t);
				feedback += t.name + ",  ";
			}
		}
		
		Debug.Log("______ PIC/" + s + " ______ " + feedback.TrimEnd(',', ' '));



		// CLEANME:     use this temp list to setup permanent vars 
		for (int i = 0; i < tPics.Length; i++) {
			switch (tPics[i].name) {
			case "BlankWhite": 
				White = tPics[i]; 
				break;
			case "Sprint": 
				Sprint = tPics[i]; 
				break;
			case "GunIcon_health": 
				Health = tPics[i]; 
				break;
			case "Health": 
				Health = tPics[i]; 
				break;
			case "blackTex": 
				Black = tPics[i]; 
				break;
			case "Crosshair": 
				CrossHair = tPics[i]; 
				break;
			case "FlagRed": 
				TeamRedFlag = tPics[i]; 
				break;
			case "FlagBlue": 
				TeamBlueFlag = tPics[i]; 
				break;
			case "swapper_crosshair":
				CrosshairSwapper[0] = tPics[i];
				break;
			case "swapper_crosshair2":
				CrosshairSwapper[1] = tPics[i];
				break;				
			case "swapper_crosshair3":
				CrosshairSwapper[2] = tPics[i];
				break;				
			case "swapper_crosshair4":
				CrosshairSwapper[3] = tPics[i];
				break;			
			}
		}
	}
}
