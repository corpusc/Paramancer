using UnityEngine;
using System.Collections.Generic;


public static class Mats {
	// private
	static Dictionary<string, Material> mats = new Dictionary<string, Material>();
	
	
	
	static Mats() {
		handleFolder("Av/Color");
		handleFolder("Av/Head");
		handleFolder("Item/Weap/FX");
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

		// add to the master collection that includes files from all folders 
		foreach (var cl in tMats) {
			//Debug.Log("MATS - " + s + " --------------- cl.name: " + cl.name);
			mats.Add(cl.name, cl);
			feedback += cl.name + ",  ";
		}
		
		Debug.Log("______ Mat/" + s + " ______ " + feedback.TrimEnd(',', ' '));
	}
}
