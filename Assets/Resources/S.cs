// global general purpose singleton

using UnityEngine;
using System;
using System.Globalization;



static public class S {
	public const float GoldenRatio = 1.6180339887498948482f;
	public const int K = 1000; // kilo in common usage (a kilo is actually 1024) 
	public static Hud Hud = null;

	// colors 
	// shouty/shimmery colors that cycle back & forth tween 2 colors 
	static float point; // ...between them 
	static bool increasing = true; // ... towards the limit of 1f 
	static public Color ShoutyBlue { 
		get { 
			moveTween0and1();
			return Color.Lerp(Color.cyan, Color.blue, point);
		} 
	}
	static public Color ShoutyPurple { 
		get { 
			moveTween0and1();
			return Color.Lerp(/*new Color(0f, 0.3f, 0f, 1f)*/ Color.magenta, Purple, point);
		} 
	}
	// 		transparent 
	public static Color WhiteTRANS = new Color(1f, 1f, 1f, 0.35f);
	public static Color RedTRANS = new Color(1f, 0f, 0f, 0.35f);
	public static Color BlueTRANS = new Color(0f, 0f, 1f, 0.35f);
	public static Color PurpleTRANS = new Color(0.2f, 0f, 0.3f, 0.8f);
	// 		opaque
	public static Color Purple = new Color(0.4f, 0f, 0.5f, 1f);
	public static Color PurpleLight = Color.Lerp(Purple, Color.white, 0.5f);
	public static Color Orange = Color.Lerp(Color.red, Color.yellow, 0.4f);

	// private 
	static float ccs = 1.2f; // color change speed 


	static public int GetInt(string v) {
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}
	
	static public double GetDouble(string v) {
		return Convert.ToDouble(v.Trim(), new CultureInfo("en-US"));
	}

	static public void Log(string s) {
		Log(s, Color.grey);
	}
	static public void Log(string s, Color c) {
		if (Hud == null) {
			Debug.Log(s);
		}else{
			Hud.Log.AddEntry("R14:", s, S.ColToVec(c));
		}
	}

	static void moveTween0and1() {
		if (increasing) {
			point += Time.deltaTime * ccs;
			
			if (point > 1f) {
				point = 1f;
				increasing = false;
			}
		}else{ // decreasing 
			point -= Time.deltaTime * ccs;
			
			if (point < 0f) {
				point = 0f;
				increasing = true;
			}
		}
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

	static public void SetShoutyColor() {
		GUI.color = ShoutyBlue;
	}
	
	static public void OutlinedLabel(Rect r, string s) {
		Color prevColor = GUI.color;
		GUI.color = Color.black;
		
		GUI.Label(new Rect(r.x-1, r.y-1, r.width, r.height), s);
		GUI.Label(new Rect(r.x-1, r.y+1, r.width, r.height), s);
		GUI.Label(new Rect(r.x+1, r.y+1, r.width, r.height), s);
		GUI.Label(new Rect(r.x+1, r.y-1, r.width, r.height), s);
		GUI.color = prevColor;
		GUI.Label(new Rect(r.x, r.y, r.width, r.height), s);
	}
	
	static public void DrawOutlinedTexture(Rect r, Texture pic, ScaleMode scaleMode = ScaleMode.StretchToFill) {
		if (pic == null) {
			Debug.LogError("******** Trying to draw with null Texture: \"" + pic.name + "\"! ********");
			return;
		}

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

	public static Vector3 ColToVec(Color c) {
		// convert colour to a vector 
		var v = Vector3.zero;
		v.x = c.r;
		v.y = c.g;
		v.z = c.b;
		return v;
	}
	public static Color VecToCol(Vector3 v) {
		// convert vector to a colour 
		var c = Color.white;
		c.r = v.x;
		c.g = v.y;
		c.b = v.z;
		return c;
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
	
	public static void DrawHoriStretchedAndCappedRect(Rect or, Texture pic)	{
		// setup border widths for 3 panel vertical slicing, so center can stretch, leaving normal borders
		Rect dest = or;
		int numSl = 3;
		// number of slices per normal key (1x1 aspect ratio / perfectly square)
		float numDiv = dest.width / dest.height * numSl;
		// num of divisions
		float pixBorderW = dest.width / numDiv;
		// pixel width
		float perBorderW = 1f / numSl;
		// percent width
		Rect texC = new Rect (0f, 0f, perBorderW, 1f);
		// texture coordinates
		// draw left border
		dest.width = pixBorderW;
		GUI.DrawTextureWithTexCoords (dest, pic, texC);
		// draw right border
		dest.x = or.xMax - pixBorderW;
		texC.x = 1f - perBorderW;
		GUI.DrawTextureWithTexCoords (dest, pic, texC);
		// draw middle slice
		dest.x = or.x + pixBorderW;
		texC.x = 0f + perBorderW;
		dest.width = or.width - pixBorderW * 2;
		texC.width = 1f - perBorderW * 2;
		GUI.DrawTextureWithTexCoords (dest, pic, texC);
	}
}
