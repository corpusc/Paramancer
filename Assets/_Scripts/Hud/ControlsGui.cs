using UnityEngine;
using System.Collections;

public class ControlsGui : MonoBehaviour {
	public int BottomMost;
	public Texture KeyCap;
	
	// privates
	int numX = 21;
	int numY = 6;
	// VVVVV matching indices VVVVVVV
	KeyCode[] codes; // first build this for cleaner looking initialization of values
	KeyData[] keyData; // this is the real structure built from "codes", that actually gets used elsewhere
	// ^^^^^ matching indices ^^^^^^^
	BindData[] bindData;
	
	
	
	void Start() {
		Object[] pics = Resources.LoadAll("Basics");
		
		for (int i = 0; i < pics.Length; i++) {
			bindData[i] = new BindData();
			var s = pics[i].ToString();
			Debug.Log("str of txtr: " + s);
//			bindData[i].Text = s;
//			//bindData[i].Action = (BindData.ActionType)s;
//			bindData[i].Pic = (Texture)pics[i];
//			///////////        STILL NEED TO FIGURE OUT HOW TO SET THE RIGHT .kEYcODE AND HOOK IT INTO PlayerPrefs.GetSetStuffz
		}
		
		codes = new KeyCode[] {
			KeyCode.Escape, 
			KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, 
			KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
			KeyCode.Print, KeyCode.ScrollLock, KeyCode.Pause, 
			KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 
			
			KeyCode.BackQuote, 
			KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, 
			KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, 
			KeyCode.Minus, KeyCode.Equals, KeyCode.Backspace,
			KeyCode.Insert, KeyCode.Home, KeyCode.PageUp,
			KeyCode.Numlock, KeyCode.KeypadDivide, KeyCode.KeypadMultiply, KeyCode.KeypadMinus, 
			
			KeyCode.Tab, 
			KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, 
			KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, 
			KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash,
			KeyCode.Delete, KeyCode.End, KeyCode.PageDown,
			KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.KeypadPlus, 
			
			KeyCode.CapsLock, 
			KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, 
			KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon, 
			KeyCode.Quote, KeyCode.Return, KeyCode.None,
			KeyCode.None, KeyCode.None, KeyCode.None,
			KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.None, 
			
			KeyCode.LeftShift, 
			KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, 
			KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, 
			KeyCode.Slash, KeyCode.RightShift, KeyCode.None, KeyCode.None,
			KeyCode.None, KeyCode.UpArrow, KeyCode.None,
			KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.KeypadEnter, 
			
			KeyCode.LeftControl, KeyCode.None,
			KeyCode.LeftWindows, 
			KeyCode.LeftAlt, 
			KeyCode.Space,
			KeyCode.None, KeyCode.None,
			KeyCode.None, KeyCode.None,
			KeyCode.RightAlt, 
			KeyCode.RightWindows,
			KeyCode.Menu,
			KeyCode.RightControl, KeyCode.None,
			KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow,
			KeyCode.Keypad0, KeyCode.None, KeyCode.KeypadPeriod, KeyCode.None, 
		};
		
		keyData = new KeyData[codes.Length];
		for (int i = 0; i < keyData.Length; i++)
			keyData[i] = new KeyData();
	}
	
	void Update() {
	}
	
	int oldW = 0; int oldH = 0;
	void OnGUI () {
		var mouPos = Input.mousePosition;
		mouPos.y = Screen.height - mouPos.y;
		
		if /* screen size changed */ (oldW != Screen.width || oldH != Screen.height)
			setupKeyData();
		
		oldW = Screen.width;
		oldH = Screen.height;
		
		// draw keys
		for (int i = 0; i < keyData.Length; i++) {
			// get the right color
			if (Input.GetKey(keyData[i].KeyCode) || keyData[i].Rect.Contains(mouPos) )
				GUI.color = Color.yellow;
			else
				GUI.color = Color.cyan;
				
			// draw
			GUI.DrawTexture(keyData[i].Rect, KeyCap);
			GUI.Box(keyData[i].Rect, keyData[i].Text);
		}
	}
	
	string getConciseText(KeyCode kc) {
		string text = kc.ToString();
			
		modifyText(ref text, "Period", ".");
		modifyText(ref text, "Control", "Ctl");
		modifyText(ref text, "Page", "Pg");
		modifyText(ref text, "Down", "Dn");
		modifyText(ref text, "Backslash", @"\");
		modifyText(ref text, "Slash", "/");
		modifyText(ref text, "Plus", "+");
		modifyText(ref text, "Minus", "-");
		modifyText(ref text, "Divide", "/");
		modifyText(ref text, "Multiply", "*");
		modifyText(ref text, "Numlock", "Num");
		modifyText(ref text, "ScrollLock", "ScLk");
		modifyText(ref text, "Print", "PtSc");
		modifyText(ref text, "Insert", "Ins");
		modifyText(ref text, "Delete", "Del");
		modifyText(ref text, "Equals", "=");
		modifyText(ref text, "Backspace", "<--");
		modifyText(ref text, "CapsLock", "CpLk");
		modifyText(ref text, "LeftBracket", "[");
		modifyText(ref text, "RightBracket", "]");
		modifyText(ref text, "Windows", "Win");
		modifyText(ref text, "Comma", ",");
		modifyText(ref text, "BackQuote", "`");
		modifyText(ref text, "Escape", "Esc");
		modifyText(ref text, "Semicolon", ";");
		modifyText(ref text, "Quote", "'");
		modifyText(ref text, "Return", "Ret");
		modifyText(ref text, "Enter", "Ent");
			
		modifyText(ref text, "Left", "L");
		modifyText(ref text, "Right", "R");
		// just get rid of these (no substitution)
		modifyText(ref text, "Keypad", "");
		modifyText(ref text, "Alpha", "");
		modifyText(ref text, "Arrow", "");

		return text;
	}
	
	void modifyText(ref string s, string substring, string replacement) {
		if (s.Contains(substring) )
			s = s.Replace(substring, replacement);
	}
	
	int numNonesToTheRight(KeyCode kc) {
		switch (kc) {
			case KeyCode.Keypad0:
			case KeyCode.LeftControl:
			case KeyCode.RightControl:
				return 1;
			case KeyCode.RightShift:
				return 2;
			case KeyCode.Return:
				return 1;
			case KeyCode.Space:
				return 4;
			default:
				return 0;		
		}		
	}

	void setupKeyData() {
		int w = Screen.width/(numX+1); // need an extra space to put a bit of distance tween keypad, cursor keys & main alpha area
		//int h = Screen.height/(numY+1);
		int fKeyGap = w/3;
		
		int i = 0;
		for (int y = 0; y < numY; y++) {
			for (int x = 0; x < numX; x++) {
				if (codes[i] != KeyCode.None) {
					// setup extra wide or tall keys
					int numExtraXCells = numNonesToTheRight(codes[i]);
					int numExtraYCells = 0;
					switch (codes[i]) {
						case KeyCode.KeypadPlus:
						case KeyCode.KeypadEnter:
							numExtraYCells++;
							break;
					}
					
					int xOffs = 0;
					int yOffs = 0;
		
					if (y == 0) {
						if (x > 0) xOffs += fKeyGap;
						if (x > 4) xOffs += fKeyGap;
						if (x > 8) xOffs += fKeyGap;
						if (x > 12) xOffs += fKeyGap;
					} else {
						if (x > 13) xOffs += fKeyGap;
						if (x > 16) xOffs += fKeyGap;
						yOffs = w/2;
					}
		
					// set key data
					keyData[i].KeyCode = codes[i];
					keyData[i].Text = getConciseText(codes[i]);
					keyData[i].Rect = new Rect(
						x * w + xOffs,
						y * w + yOffs,
						w + w * numExtraXCells, 
						w + w * numExtraYCells);
				}
				
				i++;
			}
			
			BottomMost = y * w + w + w/2;
		}
	}
}
