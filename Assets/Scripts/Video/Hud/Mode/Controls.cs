// TODO

// ---name ideas
// Visual Controls/Config
// Juicy    "     /  "   (if name emphasizes appearance, stress that its not the point....
//					 		ease/fidelity of use is)
// Best. Controls. EVAR
// Finest Control Display & Config
// Ultimate Controls

// ---marketing
// * in-game controls redef (total replacement for InputManager?)
// * a snap, even with the most complicated setups that use tons of keys.
//		ALL ACTIONS GUARANTEED to be mapped, and closeby cuz of the swapping system
// *  	no need to look down and plan
// * Stress that they will have the ultimate in configuration that the gaming world has ever seen
//		that its one area where its CHEAP to have a best-in-class solution
// * fully configurable/customizeable
// * FULL SOURCE CODE
// * supports MMO thumbpads.....BUT POINTLESS TO MENTION THIS UNTIL ITS GOT HOTBAR FUNC!?!, altho maybe tease with it!
// * maybe use current icons and start price at $5 and say its so cheap cuz i plan to upgrade icons

// ------------------absolutely necessary for 1st release 
// replace "Left-Click" and such with mouse icon that shows LMB being pressed 
// put mostly transparent scrollwheel graphic behind gun icons panel 
// FIXME: high up actions like screenshot, currently has HoverPanels drawn under menu window 
// #1, use .SetPixel
		// if more than half transparent, turn to black if any neighbor is less than half transparent
// outline plain white sprites..... later versions would use RenderToTexture, but think we can handle
// 		all the draw calls fine, for a MENU screen
// reset to default keys (resetting mouse can be removed, seems senseless
// put mouse sens and inversion on bottom of mouse graphic?  too game specific?
// gamepad
// drag keys themselves around, for foreign keyboards
// space it to take up the whole width of screen.... offsets for groupings take the leftover pixels
// attention grabbing text such as "Click on actions/keys to change them"
//		left click to move actions, right click to move keys.  maybe mouse in the middle and l & r texts on either side
// ??  maybe extra text when one is picked up, telling them to place it?  prob not
// 		ACTUALLY, just replace the initial text with the latter, while dragging something...PROBABLY!
// visualize the spaces/possibilities for Naga/MMO thumb grids
// animations/sounds
//		smoothly move to mouse pointer on pickup, smoothly move to destination key on dropping
//		tilting graphic variable to how fast pointer is moving?  maybe even spinning at height of craziness
//			maybe its anchored along top edge, and physics could let it spin around
//		size/color changes upon grab/drop.... particle puff?  single particle texture like i wanted for
//			melodicians depressed onscreen "keys"?   like a spinning starlike particle pattern.
//			maybe several stacked on top of each other rotating at diff speeds.  so the actions twinkle/sparkle

// ------------------"hotbar" release
// changeable size of icon, so it doesn't get close to the edges of keys
// (V) Adjustable visibility rectangle, with horizontal split to shove the top and bottom rows to the top and bottom of the screen
// (^) changeable visibility panels for left and right hand, with horizontal splitter when more than 2 rows
// 			should it be invisible by default?  and have like a "Start" button to access all kindsa config configuration?  8)
// gameplay hotbar

// ------------------final release
// tabs for other categories (music playing, vehicular controls)
// lol!  levelling up in "Reconfig Mastery" the more keys you remap?  8)



using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour {
	public int HeightOfKeyboard;
	public Texture currDevPic;

	// private 
	Hud hud;
	Texture keyCap;
	Rect mouseRect;
	ControlDevice currDev;
	const int numMouseButtonColumns = 4;
	const int numMouseButtonRows = 5;
	const int numKeyRows = 6;
	int maxX = 21;
	int maxY = numMouseButtonRows + numKeyRows;
	int span;
	Vector3 mouPos; // converted Input.mousePosition to sensible coordinates
	// VVVVV matching indices VVVVVVV
	KeyCode[] codes; // first build this for cleaner looking initialization of values
	KeyData[] keyData; // this is the real structure built from "codes", that is mostly used elsewhere
	// ^^^^^ matching indices ^^^^^^^
	BindData draggee = null; // user action that is attached to pointer 
	

	
	public void SetCurrDevice(ControlDevice cd) {
		currDev = cd;

		if (currDev == ControlDevice.GamePad)
			currDevPic = Pics.Get("PerspectiveMin");
		else
			currDevPic = Pics.Get(currDev + "");
	}

	void Start() {
		hud = GetComponent<Hud>();
		keyCap = Pics.Get("KeyCap");
		SetCurrDevice(ControlDevice.RightyMouse);

		// setup temp structure for the physical layout of the keyboard
		var n = KeyCode.None;
		codes = new KeyCode[] {
			// mouse keys/buttons 
			n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, KeyCode.Mouse0, KeyCode.Mouse2, KeyCode.Mouse1, 
			n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, 
			n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, KeyCode.Mouse3,              n, KeyCode.Mouse5, n, 
			n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, KeyCode.Mouse4, n, KeyCode.Mouse6, n, 
			n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n, n,              n, 

			// keyboard 
			KeyCode.Escape, 
			KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, 
			KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
			KeyCode.Print, KeyCode.ScrollLock, KeyCode.Pause, 
			n, n, n, n, n, 
			
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
			KeyCode.Quote, KeyCode.Return, n,
			n, n, n,
			KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, n, 
			
			KeyCode.LeftShift, 
			KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, 
			KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, 
			KeyCode.Slash, KeyCode.RightShift, n, n,
			n, KeyCode.UpArrow, n,
			KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.KeypadEnter, 
			
			KeyCode.LeftControl, n,
			KeyCode.LeftWindows, 
			KeyCode.LeftAlt, 
			KeyCode.Space,
			n, n,
			n, n,
			KeyCode.RightAlt, 
			KeyCode.RightWindows,
			KeyCode.Menu,
			KeyCode.RightControl, n,
			KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow,
			KeyCode.Keypad0, n, KeyCode.KeypadPeriod, n,
			
//			switch (currDev) {
//			case ControlDevice.l:
//				break;
//			case ControlDevice.Right:
//				break;
//			case ControlDevice.Right:
//				break;
//			case ControlDevice.Right:
//				break;
//			}
		};
		
		// the more complicated mirrored structure that holds all the OTHER key data
		keyData = new KeyData[codes.Length];
		for (int i = 0; i < keyData.Length; i++)
			keyData[i] = new KeyData();
		
		// bind action/keycode bindings to key
		var bd = CcInput.BindData;
		for (int i = 0; i < bd.Length; i++) {
		for (int j = 0; j < codes.Length; j++) {
			if (bd[i].KeyCode == codes[j]) {
				bd[i].Id = j;
				//Debug.Log("set an id to: " + j);
			}
		}
		}
	}
	
	int latestUnboundKey;
	void Update() {
		// move user action icons by clicking on the onscreen keyboard
		// FIXME: we'll wanna clear draggee upon leaving the controls config screen, otherwise
		// when coming back, there will be an action on the pointer instead of showing up on
		// its proper key location (altho atm, its impossible to leave without clicking on menu 
		// option, thus dropping the action) 
		if (Input.GetKeyDown(KeyCode.Mouse0) ) {
			int tkId; // targeted key id
			var tk = targetedKey(out tkId);
			if (tk == null) { // not a valid area, so discard draggee 
				if (draggee != null) {
					draggee.Id = latestUnboundKey;
					draggee.KeyCode = codes[latestUnboundKey];
				}
				
				draggee = null; // 
			}else{ // over a valid key
				if (draggee == null) {
					// pick it up
					for (int i = 0; i < CcInput.BindData.Length; i++) {
						if (tkId == CcInput.BindData[i].Id) {
							latestUnboundKey = CcInput.BindData[i].Id;
							//CcInput.BindData[i].Id = 9999;       FIXME?   why the fuck did i do this?  some concern that was never finished i guess 
							draggee = CcInput.BindData[i];
						}
					}
				}else{ // we were dragging
					// change bind settings
					draggee.Id = tkId;
					draggee.KeyCode = tk.KeyCode;
					
					// if there was something else there, that's the new draggee
					bool anotherWasBoundThere = false;
					BindData theOther = null;
					for (int i = 0; i < CcInput.BindData.Length; i++) {
						if (draggee != CcInput.BindData[i] && 
							draggee.Id == CcInput.BindData[i].Id)
						{
							anotherWasBoundThere = true;
							theOther = CcInput.BindData[i];
						}
					}
					
					if (anotherWasBoundThere) {
						theOther.Id = 9999;
						draggee = theOther;
					}else
						draggee = null;
				}
			}
		}
	}
	
	int oldW = 0; int oldH = 0;
	void OnGUI () {
		hud.DrawControlsAdjunct();

		// handle change of screen dimensions
		mouPos = Input.mousePosition;
		mouPos.y = Screen.height - mouPos.y;
		
		if /* screen size changed */ (oldW != Screen.width || oldH != Screen.height)
			setupKeyData();
		
		oldW = Screen.width;
		oldH = Screen.height;
		
		var r = new Rect(0, 0, Screen.width, Screen.height);
		GUI.BeginGroup(r);

		r.y = Screen.height - HeightOfKeyboard;
		r.height = HeightOfKeyboard;
		//GUI.DrawTexture(r, Pics.White);
		GUI.DrawTexture(mouseRect, currDevPic);

		// draw keys          (perhaps clean this up by using mouseOver()) 
		for (int i = 0; i < keyData.Length; i++) {
			// get the right color 
			if (Input.GetKey(keyData[i].KeyCode) || keyData[i].Rect.Contains(mouPos) ) {
				GUI.color = S.PurpleLight;
			}else
				GUI.color = Color.white;
				
			Rect or = keyData[i].Rect; // original rect 
			
			// if slicing not needed, just draw in single call and move on to next key
			if (or.width <= or.height) {
				if (i < numMouseButtonRows*maxX) {
					var c = GUI.color;
					c.a = 0.5f;
					GUI.color = c;
				}

				GUI.DrawTexture(or, keyCap);
				continue;
			}
			
			S.DrawHoriStretchedAndCappedRect(or, keyCap);
		}
		
		// draw action icons 
		var bd = CcInput.BindData;
		for (int i = 0; i < bd.Length; i++) {
			if (bd[i] != draggee) {
				// get the right color
				if (pushingOrHoveringOver(bd[i].Id))
					GUI.color = Color.cyan;
				else
					GUI.color = S.Purple;
				
				//GUI.DrawTexture(keyData[bd[i].Id].Rect, bd[i].Pic, ScaleMode.ScaleToFit);
				S.GUIDrawOutlinedTexture(keyData[bd[i].Id].Rect, bd[i].Pic, ScaleMode.ScaleToFit);
			}
		}
		
		// draw key labels          (perhaps clean this up by using mouseOver())
		for (int i = 0; i < keyData.Length; i++) {
			var rect = keyData[i].Rect;

			// get the right color
			if (pushingOrHoveringOver(i))
				GUI.color = S.Purple;
			else {
				float l = 0.75f; // level 
				GUI.color = new Color(l, l, l);
			}
				
			// draw 
			rect.x += rect.width/5;
			S.GUIOutlinedLabel(rect, keyData[i].Text);
		}


		GUI.EndGroup();


		// text for hovered action icon 
		string s = targetedBind();
		if (s != null) {
			float wid = hud.GetWidthLabel(s);
			var h = hud.VSpanLabel;
			var rect = new Rect(
				mouPos.x-wid/2-h, 
				mouPos.y-h*3,
				wid+h*2,                     
				h*2);

			GUI.color = S.PurpleTRANS;
			GUI.DrawTexture(rect, Pics.Get("BlankWhite"));

			rect.x = mouPos.x-wid/2;
			rect.y += h/2;
			// don't really need to shrink this actually......otherwise: rect.width = wid*2 
			S.SetShoutyColor();
			S.GUIOutlinedLabel(rect, s);
		}
		
		// if carrying an action icon near the cursor 
		if (draggee != null) {
			s = "Click new key for " + S.GetSpacedOut(draggee.Action + "");
			float wid = hud.GetWidthLabel(s);
			var h = hud.VSpanLabel;
			var rect = new Rect(
				mouPos.x-wid/2, 
				mouPos.y+h*2,
				wid+h*2,                     
				h*2);
			
			GUI.color = S.ShoutyPurple;
			S.GUIOutlinedLabel(rect, s);
		}

		// draw action icon near pointer if it's being moved 
		if (draggee != null) {
			GUI.color = S.ShoutyPurple;
			S.GUIDrawOutlinedTexture(new Rect(mouPos.x-span/2, mouPos.y/*+span/2*/, span, span), draggee.Pic, ScaleMode.ScaleToFit);
		}

		GUI.color = Color.white;
	}

	bool pushingOrHoveringOver(int i) {
		if (i >= (int)KeyCode.Mouse6)
			Debug.Log("pushingOrHoveringOver() - i: " + i + "      (KeyCode)i: " + (KeyCode)i);

		return Input.GetKey(keyData[i].KeyCode) || keyData[i].Rect.Contains(mouPos);
	}

	string targetedBind() {
		for (int i = 0; i < CcInput.BindData.Length; i++) {
			if ((draggee == null || CcInput.BindData[i].Id != draggee.Id) &&
				keyData[CcInput.BindData[i].Id].Rect.Contains(mouPos) ) {
				return S.GetSpacedOut(CcInput.BindData[i].Action.ToString());
			}
		}
		
		return null;
	}
	
	KeyData targetedKey(out int mouseOverId) {
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
			
		modifyText(ref text, "Mouse", "m");
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
		int lOvW; // leftover width 
		span = Screen.width / (maxX+1); // need an extra space to put a bit of distance tween keypad, cursor keys & main alpha area
		lOvW = Screen.width - span*(maxX+1) + span;
		int fKeyGap = span/3;
		int gap1 = lOvW/2; // gap tween cursor/pgup/pgdn   &   main text area 
		lOvW -= gap1;
		int gap2 = lOvW; // gap tween keypad   &   cursor/pgup/pgdn 
		HeightOfKeyboard = span*numKeyRows + span/2;
		int topOfMouseButtons = Screen.height - HeightOfKeyboard - span*5;
		
		int i = 0;
		for (int y = 0; y < maxY; y++) {
			for (int x = 0; x < maxX; x++) {
				if (codes[i] != KeyCode.None) {
					// setup extra wide or tall keys
					int numExtraXCells = numNonesToTheRight(codes[i]);
					int numExtraYCells = 0;
					switch (codes[i]) {
						case KeyCode.Mouse0:
						case KeyCode.Mouse1:
						case KeyCode.Mouse2:
						case KeyCode.KeypadPlus:
						case KeyCode.KeypadEnter:
							numExtraYCells++;
							break;
					}
					
					int xOffs = 0;
					int yOffs = 0;
		
					if (y == numMouseButtonRows) { // row of function keys 
						if (x > 0) xOffs += fKeyGap;
						if (x > 4) xOffs += fKeyGap;
						if (x > 8) xOffs += fKeyGap;
						if (x > 12) xOffs += gap1;
					} else {
						if (y > numMouseButtonRows) {
							if (x > 13) 
								xOffs += gap1;
							if (x > 16) 
								xOffs += gap2;

							yOffs = span/2;
						}else{
							xOffs = gap1+gap2;
						}
					}
		
					// set key data 
					keyData[i].KeyCode = codes[i];
					keyData[i].Text = getConciseText(codes[i]);
					keyData[i].Rect = new Rect(
						x * span + xOffs,
						y * span + yOffs + topOfMouseButtons,
						span + span * numExtraXCells, 
						span + span * numExtraYCells);
				}
				
				i++;
			}
		}
		
		mouseRect = new Rect(
			Screen.width-3*span, topOfMouseButtons, 
			3*span, numMouseButtonRows*span);
	}
}
