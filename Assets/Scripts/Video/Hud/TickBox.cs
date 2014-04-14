using UnityEngine;
using System.Collections;

public class TickBox : MonoBehaviour {

	Texture2D off;
	Texture2D on;

	bool Display(bool state) {
		off = (Texture2D)Pics.Get("Tickbox");
		on = (Texture2D)Pics.Get("TickboxTicked");
		return GUILayout.Button(state ? on : off);
	}
}
