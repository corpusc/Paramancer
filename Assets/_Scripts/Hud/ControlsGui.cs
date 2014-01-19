using UnityEngine;
using System.Collections;

public class ControlsGui : MonoBehaviour {
	public int BottomMost;
	
	// privates
	Texture keyCap;
	int numX = 21;
	int numY = 6;
	BindData draggee = null;
	Vector3 mouPos; // = Input.mousePosition;
	// VVVVV matching indices VVVVVVV
	KeyCode[] codes; // first build this for cleaner looking initialization of values
	KeyData[] keyData; // this is the real structure built from "codes", that actually gets used elsewhere
	// ^^^^^ matching indices ^^^^^^^
	BindData[] bindData = new BindData[(int)BindData.ActionType.Count];
	
	
	
	void Start() {
		// load textures
		Object[] pics = Resources.LoadAll("Basics", typeof(Texture));
		
		// use this temp list to setup permanent vars
		for (int i = 0; i < pics.Length; i++) {
			var s = pics[i].name;
			
			if (s == "KeyCap")
				keyCap = pics[i] as Texture;
		}
		
		// setup temp structure for the physical layout of the keyboard
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
		
		// bind default keys to user actions
		for (int i = 0; i < (int)BindData.ActionType.Count; i++) {
			bindData[i] = new BindData();
			bindData[i].Action = (BindData.ActionType)i;
			
			// get pic from list
			Texture pic = null;
			for (int j = 0; j < pics.Length; j++) {
				if ((BindData.ActionType)i + "" == pics[j].name)
					pic = (Texture)pics[j];
			}
			
			bindData[i].Pic = pic;
//			///////////  FIXME      HOOK INTO PlayerPrefs.GetSetStuffz
			
			switch ((BindData.ActionType)i) {
				case BindData.ActionType.MoveForward:
					bindData[i].KeyCode = KeyCode.E;
					break;
				case BindData.ActionType.MoveBackward:
					bindData[i].KeyCode = KeyCode.D;
					break;
				case BindData.ActionType.MoveLeft:
					bindData[i].KeyCode = KeyCode.S;
					break;
				case BindData.ActionType.MoveRight:
					bindData[i].KeyCode = KeyCode.F;
					break;
				case BindData.ActionType.MoveUp:
					bindData[i].KeyCode = KeyCode.A;
					break;
				case BindData.ActionType.MoveDown:
					bindData[i].KeyCode = KeyCode.Z;
					break;
			}
			
			for (int j = 0; j < codes.Length; j++)
				if (bindData[i].KeyCode == codes[j]) {
					bindData[i].Id = j;
					//Debug.Log("set an id to: " + j);
				}
		}
		
		// the more complicated structure that's used from now on
		keyData = new KeyData[codes.Length];
		for (int i = 0; i < keyData.Length; i++)
			keyData[i] = new KeyData();
	}
	
	int latestUnboundKey;
	void Update() {
		// move user action icons by clicking on the onscreen keyboard
		// FIXME: we'll wanna clear draggee upon leaving the controls config screen, otherwise
		// when coming back, there will be an action on the pointer instead of showing up on
		// its proper key location
		if (Input.GetKeyDown(KeyCode.Mouse0) ) {
			int moId; // mouse over id
			var mo = mouseOver(out moId);
			if (mo == null) { // not a valid area, so discard draggee
				if (draggee != null) {
					draggee.Id = latestUnboundKey;
					draggee.KeyCode = codes[latestUnboundKey];
				}
				
				draggee = null; // 
			}else{ // over a valid key
				if (draggee == null) {
					// pick it up
					for (int i = 0; i < bindData.Length; i++) {
						if (moId == bindData[i].Id) {
							draggee = bindData[i];
							latestUnboundKey = bindData[i].Id;
						}
					}
				}else{ // we were dragging
					// change bind settings
					draggee.Id = moId;
					draggee.KeyCode = mo.KeyCode;
					
					// if there was something else there, that's the new draggee
					bool anotherWasBoundThere = false;
					BindData theOther = null;
					for (int i = 0; i < bindData.Length; i++) {
						if (draggee != bindData[i] && 
							draggee.Id == bindData[i].Id)
						{
							anotherWasBoundThere = true;
							theOther = bindData[i];
						}
					}
					
					if (anotherWasBoundThere)
						draggee = theOther;
					else
						draggee = null;
				}
			}
		}
	}
	
	int oldW = 0; int oldH = 0;
	void OnGUI () {
		mouPos = Input.mousePosition;
		mouPos.y = Screen.height - mouPos.y;
		
		if /* screen size changed */ (oldW != Screen.width || oldH != Screen.height)
			setupKeyData();
		
		oldW = Screen.width;
		oldH = Screen.height;
		
		// draw keys          (perhaps clean this up by using mouseOver())
		for (int i = 0; i < keyData.Length; i++) {
			// get the right color
			if (Input.GetKey(keyData[i].KeyCode) || keyData[i].Rect.Contains(mouPos) )
				GUI.color = Color.yellow;
			else
				GUI.color = Color.white;
				
			// draw
			var dest = keyData[i].Rect;
			float spanD = dest.width = dest.width / 3; // slice span for DESTINATION
			float spanT = 0.3333f; // slice span for TEXTURE COORDS
			var texCoords = new Rect(0f, 0f, spanT, 1f);
			
			GUI.color = Color.magenta;
			GUI.DrawTextureWithTexCoords(dest, keyCap, texCoords);
			
			dest.x += spanD;
			texCoords.x += spanT;
			GUI.color = Color.green;
			GUI.DrawTextureWithTexCoords(dest, keyCap, texCoords);
			
			dest.x += spanD;
			texCoords.x += spanT;
			GUI.color = Color.yellow;
			GUI.DrawTextureWithTexCoords(dest, keyCap, texCoords);
		}
		
		// draw actions
		GUI.color = Color.cyan;
		for (int i = 0; i < bindData.Length; i++) {
			if (bindData[i] != draggee)
				GUI.DrawTexture(keyData[bindData[i].Id].Rect, bindData[i].Pic, ScaleMode.ScaleToFit);
		}
		
		// draw key text          (perhaps clean this up by using mouseOver())
		for (int i = 0; i < keyData.Length; i++) {
			// get the right color
			if (Input.GetKey(keyData[i].KeyCode) || keyData[i].Rect.Contains(mouPos) )
				GUI.color = Color.yellow;
			else
				GUI.color = Color.white;
				
			// draw
			GUI.Box(keyData[i].Rect, keyData[i].Text);
		}
		
		if (draggee != null)
			GUI.DrawTexture(new Rect(mouPos.x, mouPos.y, 50, 50), draggee.Pic);
	}
	
	KeyData mouseOver(out int mouseOverId) {
		for (int i = 0; i < keyData.Length; i++) {
			if (keyData[i].Rect.Contains(mouPos) ) {
				mouseOverId = i;
				return keyData[i];
			}
		}
		
		mouseOverId = 999777; // junk number that should never be used, because null (paired)
		return null;
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
