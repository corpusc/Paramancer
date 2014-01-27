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

				case UserAction.Activate:
					BindData[i].KeyCode = KeyCode.Mouse0;
					break;
				case UserAction.SwapWeapon:
					BindData[i].KeyCode = KeyCode.Mouse1;
					break;
				case UserAction.Sprint:
					BindData[i].KeyCode = KeyCode.LeftShift;
					break;
				case UserAction.GrabItem:
					BindData[i].KeyCode = KeyCode.G;
					break;
				case UserAction.Chat:
					BindData[i].KeyCode = KeyCode.Return;
					break;
				case UserAction.Menu:
					BindData[i].KeyCode = KeyCode.Escape;
					break;
				case UserAction.Scores:
					BindData[i].KeyCode = KeyCode.Tab;
					break;
				case UserAction.SwapTeam:
					BindData[i].KeyCode = KeyCode.Alpha5;
					break;
				case UserAction.Suicide:
					BindData[i].KeyCode = KeyCode.K;
					break;
			}
		}
	}
	
	public static string GetKeyLabel(UserAction action) {
		for (int i = 0; i < BindData.Length; i++) {
			if (action == BindData[i].Action)
				return BindData[i].KeyCode.ToString();
		}
		
		return "THAT BIND DOESN'T EXIST";
	}
	
	public static bool Started(UserAction action) {
		for (int i = 0; i < BindData.Length; i++) {
			if (action == BindData[i].Action)
				if (Input.GetKeyDown(BindData[i].KeyCode) )
					return true;
		}
		
		return false;
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
