using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Pics {
	// hardcoded/variable-based textures 
	static public Texture[] swapperCrosshair = new Texture[4];
	static public Texture Health;
	static public Texture White;
	static public Texture Black;
	static public Texture crossHair;
	static public Texture TeamRedFlag;
	static public Texture TeamBlueFlag;
	static public Texture gameLogo;
	static public Texture companyLogo;
	static public Texture Sprint;

	// private
	static Dictionary<string, Texture> pics = new Dictionary<string, Texture>();


	
	static Pics() {
		handleFolder("Hud");
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

//	public static Texture GetFirstWith(string s) {
//		for (int i = 0; i < pics.Length; i++) {
//			if (s == pics[i].name)
//				return (Texture)pics[i]; 
//		}
//
//		Debug.LogError("Can't find any picture with that name!");
//		return null;
//	}
	
	static void handleFolder(string s) {
		string feedback = "";
		var tPics = Resources.LoadAll<Texture>("Pic/" + s);
		
		// add to the master collection that includes files from all folders 
		foreach (var cl in tPics) {
			pics.Add(cl.name, cl);
			feedback += cl.name + ",  ";
		}
		
		Debug.Log("______ PIC/" + s + " ______ " + feedback.TrimEnd(',', ' '));



		// CLEANME:     use this temp list to setup permanent vars 
		for (int i = 0; i < tPics.Length; i++) {
			switch (tPics[i].name) {
			case "Sprint": 
				Sprint = tPics[i]; 
				break;
			case "Health": 
				Health = tPics[i]; 
				break;
			case "whiteTex": 
				White = tPics[i]; 
				break;
			case "blackTex": 
				Black = tPics[i]; 
				break;
			case "Crosshair": 
				crossHair = tPics[i]; 
				break;
			case "Logo - CazCore": 
				companyLogo = tPics[i]; 
				break;
			case "Logo - Paramancer": 
				gameLogo = tPics[i]; 
				break;
			case "FlagRed": 
				TeamRedFlag = tPics[i]; 
				break;
			case "FlagBlue": 
				TeamBlueFlag = tPics[i]; 
				break;
			case "swapper_crosshair":
				swapperCrosshair[0] = tPics[i];
				break;
			case "swapper_crosshair2":
				swapperCrosshair[1] = tPics[i];
				break;				
			case "swapper_crosshair3":
				swapperCrosshair[2] = tPics[i];
				break;				
			case "swapper_crosshair4":
				swapperCrosshair[3] = tPics[i];
				break;			
			}
		}
	}
}
