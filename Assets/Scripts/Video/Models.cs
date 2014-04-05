using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Models {
	
	// private
	static Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();
	
	
	
	static Models() {
		Debug.Log("----------------- Models constructor -----------------");
		//handleFolder("Women");
	}
	
	
	public static GameObject Get(string s) { // hash lookups in a Dictionary are fast right? 
		if (models.ContainsKey(s))
			return models[s];
		
		Debug.LogError("______ COULDN'T FIND THE FILE NAMED '" + s + "'!!! ______");
		return null;
	}
	
	public static GameObject GetFirstWith(string s) { // this is slow i think.  iterating thru key/value pairs 
		foreach (var pair in models) {
			if (pair.Value.name.Contains(s))
				return pair.Value;
		}
		
		Debug.LogError("______ COULDN'T FIND ANY FILE WITH '" + s + "' IN THE NAME!!! ______");
		return null;
	}
	
	//	public static GameObject GetFirstWith(string s) {
	//		for (int i = 0; i < models.Length; i++) {
	//			if (s == models[i].name)
	//				return (GameObject)models[i]; 
	//		}
	//
	//		Debug.LogError("Can't find any picture with that name!");
	//		return null;
	//	}
	
	static void handleFolder(string s) {
		string feedback = "";
		var tmodels = Resources.LoadAll<GameObject>("Model/" + s);
		
		// add to the master collection that includes files from all folders 
		foreach (var cl in tmodels) {
			models.Add(cl.name, cl);
			feedback += cl.name + ",  ";
		}
		
		Debug.Log("______ MODEL/" + s + " ______ " + feedback.TrimEnd(',', ' '));
		
		
		
		// CLEANME:     use this temp list to setup permanent vars 
		for (int i = 0; i < tmodels.Length; i++) {
			switch (tmodels[i].name) {
			case "Some string":
				// do some specific hardwired setup for that particular model 
				break;
			}
		}
	}
}
