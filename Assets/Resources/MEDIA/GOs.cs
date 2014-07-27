using UnityEngine;
//using System.Collections;
using System.Collections.Generic;



public static class GOs {
	// private 
	static Dictionary<string, GameObject> gos = new Dictionary<string, GameObject>();


	
	static GOs() {
		Debug.Log("----------------- GOs constructor -----------------");
		handleFolder("Entity/Av");
		handleFolder("Game");
		handleFolder("Item");
		handleFolder("Theme");
	}

	public static GameObject Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (gos.ContainsKey(s))
			return gos[s];
		
		Debug.LogError("______ COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static GameObject GetFirstWith(string s) { // this is slow i think.  iterating thru key/value pairs 
		foreach (var pair in gos) {
			if (pair.Value.name.Contains(s))
				return pair.Value;
		}
		
		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}
	
	static void handleFolder(string s) {
		string feedback = "";
		var tGOs = Resources.LoadAll<GameObject>(s);
		
		// add to the master collection that includes files from all subfolders 
		foreach (var cl in tGOs) {
			gos.Add(cl.name, cl);
			feedback += cl.name + ",  ";
		}
		
		Debug.Log("______ GAMEOBJECTS --- " + s + " ______ " + feedback.TrimEnd(',', ' '));
		
		
		
		// CLEANME:     use this temp list to setup permanent vars 
		for (int i = 0; i < tGOs.Length; i++) {
			switch (tGOs[i].name) {
			case "Some string":
				// do some specific hardwired setup for that particular model 
				break;
			}
		}
	}
}
