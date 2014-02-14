// global general purpose singleton

using UnityEngine;
using System;
using System.Globalization;



static public class S {
	static public int GetInt(string v) {
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}
	
	static public string GetSpacedOut(string s) {
		string ns = "";
		// add spaces between words, determined by capital letters
		for (int i = 0; i < s.Length; i++) {
			if (i != 0 && s[i] >= 'A' && s[i] <= 'Z') {
				ns += " ";
			}
			
			ns += s[i];
		}
		
		return ns;
	}

	static public void GUIDrawOutlinedTexture(Rect r, Texture pic) {
		Color prevColor = GUI.color;
		GUI.color = Color.black;

		GUI.DrawTexture(new Rect(r.x-1, r.y-1, r.width, r.height), pic);
		GUI.DrawTexture(new Rect(r.x-1, r.y+1, r.width, r.height), pic);
		GUI.DrawTexture(new Rect(r.x+1, r.y+1, r.width, r.height), pic);
		GUI.DrawTexture(new Rect(r.x+1, r.y-1, r.width, r.height), pic);
		GUI.color = prevColor;
		GUI.DrawTexture(new Rect(r.x, r.y, r.width, r.height), pic);
	}
}
