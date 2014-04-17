using UnityEngine;
using System.Collections;

public static class TickBox {

	public static bool Display(bool state, string label = "") {
		GUIStyle GS = "Label";
		GUIContent GC = new GUIContent(label);
		GUILayout.Label(label, GUILayout.MaxWidth(GS.CalcSize(GC).x));
		if (GUILayout.Button(state ? "X" : " ", GUILayout.Width(20)))
			return !state;
		return state;
	}
}
