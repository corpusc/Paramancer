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
		
		// bind default keys/pics to user actions
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
			
			switch ((UserAction)i) {
				case UserAction.MoveForward: 
					bind(i, KeyCode.E);
					break;
				case UserAction.MoveBackward:
					bind(i, KeyCode.D);
					break;
				case UserAction.MoveLeft:
					bind(i, KeyCode.S);
					break;
				case UserAction.MoveRight:
					bind(i, KeyCode.F);
					break;
				case UserAction.MoveUp:
					bind(i, KeyCode.A);
					break;
				case UserAction.MoveDown:
					bind(i, KeyCode.Z);
					break;

				case UserAction.Activate:
					bind(i, KeyCode.Mouse0);
					break;
				case UserAction.SwapWeapon:
					bind(i, KeyCode.Mouse1);
					break;
				case UserAction.Sprint:
					bind(i, KeyCode.LeftShift);
					break;
				case UserAction.GrabItem:
					bind(i, KeyCode.G);
					break;
				case UserAction.Chat:
					bind(i, KeyCode.Return);
					break;
				case UserAction.Menu:
					bind(i, KeyCode.Escape);
					break;
				case UserAction.Scores:
					bind(i, KeyCode.Tab);
					break;
				case UserAction.SwapTeam:
					bind(i, KeyCode.T);
					break;
				case UserAction.Suicide:
					bind(i, KeyCode.K);
					break;
			}
		}
	}
	
	public static void SaveKeyConfig() {
		for (int i = 0; i < (int)UserAction.Count; i++) {
			PlayerPrefs.SetInt("" + (UserAction)i, (int)BindData[i].KeyCode);
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
	
	static void bind(int i, KeyCode kc) {
		BindData[i].KeyCode = (KeyCode)PlayerPrefs.GetInt("" + (UserAction)i, (int)kc);
	}
}
