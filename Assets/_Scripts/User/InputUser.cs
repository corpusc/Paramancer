using UnityEngine;
using System.Collections;

public static class InputUser {
	public static BindData[] BindData = new BindData[(int)UserAction.Count];
	
	
	
	static InputUser() {
		// load textures
		Object[] pics = Resources.LoadAll("Pic/Hud/Action");
		
		// use this temp list to setup permanent vars
		for (int i = 0; i < pics.Length; i++) {
			var s = pics[i].name;
			Debug.Log("Action: " + s);
		}
		
		
		
		
		
		// bind default keys to user actions
		for (int i = 0; i < (int)UserAction.Count; i++) {
			BindData[i] = new BindData();
			BindData[i].Action = (UserAction)i;
			
			// get pic from list
			Texture pic = null;
			for (int j = 0; j < pics.Length; j++) {
				if ((UserAction)i + "" == pics[j].name)
					pic = (Texture)pics[j];
			}
			
			BindData[i].Pic = pic;
//			///////////  FIXME      HOOK INTO PlayerPrefs.GetSetStuffz
			
			switch ((UserAction)i) {
				case UserAction.MoveForward:
					BindData[i].KeyCode = KeyCode.E;
					break;
				case UserAction.MoveBackward:
					BindData[i].KeyCode = KeyCode.D;
					break;
				case UserAction.MoveLeft:
					BindData[i].KeyCode = KeyCode.S;
					break;
				case UserAction.MoveRight:
					BindData[i].KeyCode = KeyCode.F;
					break;
				case UserAction.MoveUp:
					BindData[i].KeyCode = KeyCode.A;
					break;
				case UserAction.MoveDown:
					BindData[i].KeyCode = KeyCode.Z;
					break;
			}
		}
	}
	
	public static bool Holding(UserAction action) {
		for (int i = 0; i < BindData.Length; i++) {
			if (action == BindData[i].Action)
				if (Input.GetKey(BindData[i].KeyCode) )
					return true;
		}
		
		return false;
	}
}
