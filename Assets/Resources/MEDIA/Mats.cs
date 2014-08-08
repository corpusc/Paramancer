using UnityEngine;
using System.Collections.Generic;


public static class Mats {
	// private
	static Dictionary<string, Material> mats = new Dictionary<string, Material>();
	
	
	
	public static void Init() {
		handleFolder("Entity/Av/Color");
		handleFolder("Entity/Av/Head");
		handleFolder("Item/Weap/FX");
		handleFolder("Theme");
	}

	public static Material Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (mats.ContainsKey(s))
			return mats[s];
		
		Debug.LogError("______ COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static Material GetFirstWith(string s) { // this is slow i think.  iterating thru key/value pairs 
		foreach (var pair in mats) {
			if (pair.Value.name.Contains(s))
				return pair.Value;
		}
		
		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}
	
	static void handleFolder(string s) {
		string feedback = "";
		
		var tMats = Resources.LoadAll<Material>(s);

		// add to the master collection that includes files from all subfolders 
		foreach (var o in tMats) {
			if (mats.ContainsKey(o.name)) {
				Debug.LogError("______ The name: " + o.name + " is already a registered Material!!! ______");
			} else {
				mats.Add(o.name, o);
				feedback += o.name + ",  ";
				Debug.Log("__" + o.name + " __ ");
			}
		}
		
		Debug.Log("______ Mat/" + s + " ______ " + feedback.TrimEnd(',', ' '));
	}
}
