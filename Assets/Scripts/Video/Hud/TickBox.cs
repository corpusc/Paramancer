using UnityEngine;
using System.Collections;



public static class TickBox {
	public static bool Display(bool state, string text = "") {
		text += ": ";
		var bt = state ? "Yes" : "No"; // button text 

		GUIStyle GS = "Label";
		GUIContent GC = new GUIContent(text);
		GUILayout.Label(text, GUILayout.MaxWidth(GS.CalcSize(GC).x));
		
		GS = "Button";
		GC = new GUIContent(bt);
		var wid = GS.CalcSize(GC).x;
		
		if (GUILayout.Button(bt, GUILayout.Width(wid)))
			return !state;

		return state;
	}
}
