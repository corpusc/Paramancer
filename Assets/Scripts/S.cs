// global general purpose singleton

using UnityEngine;
using System;
using System.Globalization;



static public class S {
	public const float GoldenRatio = 1.6180339887498948482f;
	
	// colors 
	public static Color PurpleTRANS = new Color(0.3f, 0f, 0.4f, 0.6f);
	public static Color Purple = new Color(0.8f, 0f, 1f, 1f);
	public static Color WhiteTRANS = new Color(1f, 1f, 1f, 0.35f);

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

	static public void GetShoutyColor() {
		if ((Time.time % 0.3f) > 0.15f)
			GUI.color = Color.cyan;
		else
			GUI.color = Color.blue;
	}
	
	static public void GUIOutlinedLabel(Rect r, string s) {
		Color prevColor = GUI.color;
		GUI.color = Color.black;
		
		GUI.Label(new Rect(r.x-1, r.y-1, r.width, r.height), s);
		GUI.Label(new Rect(r.x-1, r.y+1, r.width, r.height), s);
		GUI.Label(new Rect(r.x+1, r.y+1, r.width, r.height), s);
		GUI.Label(new Rect(r.x+1, r.y-1, r.width, r.height), s);
		GUI.color = prevColor;
		GUI.Label(new Rect(r.x, r.y, r.width, r.height), s);
	}
	
	static public void GUIDrawOutlinedTexture(Rect r, Texture pic, ScaleMode scaleMode = ScaleMode.StretchToFill) {
		Color prevColor = GUI.color;
		GUI.color = Color.black;
		
		GUI.DrawTexture(new Rect(r.x-1, r.y-1, r.width, r.height), pic, scaleMode);
		GUI.DrawTexture(new Rect(r.x-1, r.y+1, r.width, r.height), pic, scaleMode);
		GUI.DrawTexture(new Rect(r.x+1, r.y+1, r.width, r.height), pic, scaleMode);
		GUI.DrawTexture(new Rect(r.x+1, r.y-1, r.width, r.height), pic, scaleMode);
		GUI.color = prevColor;
		GUI.DrawTexture(new Rect(r.x, r.y, r.width, r.height), pic, scaleMode);
	}

	public static float GetPitch(this Vector3 v) {
		float len = Mathf.Sqrt( (v.x * v.x) + (v.z * v.z) ); // Length on xz plane.
		return -Mathf.Atan2(v.y, len);
	}
	
	public static float GetYaw(this Vector3 v) {
		return Mathf.Atan2(v.x, v.z);
	}

	public static void RotateX(this Vector3 v, float angle) {
		float sin = Mathf.Sin(angle);
		float cos = Mathf.Cos(angle);
		
		float ty = v.y;
		float tz = v.z;
		v.y = (cos * ty) - (sin * tz);
		v.z = (cos * tz) + (sin * ty);
	}
	
	public static void RotateY(this Vector3 v, float angle) {
		float sin = Mathf.Sin(angle);
		float cos = Mathf.Cos(angle);
		
		float tx = v.x;
		float tz = v.z;
		v.x = (cos * tx) + (sin * tz);
		v.z = (cos * tz) - (sin * tx);
	}
	
	public static void RotateZ(this Vector3 v, float angle) {
		float sin = Mathf.Sin(angle);
		float cos = Mathf.Cos(angle);
		
		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (cos * ty) + (sin * tx);
	}
}
