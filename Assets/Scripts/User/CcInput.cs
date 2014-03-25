using UnityEngine;
using System.Collections;

public static class CcInput {
	public static BindData[] BindData = new BindData[(int)UserAction.Count];

	// private 
	static float currWheel;
	static Texture[] pics;


	
	static CcInput() {
		pics = Resources.LoadAll<Texture>("Pic/Hud/Action");
		SetDefaultBinds();
	}

	public static void PollScrollWheel(out bool wheelNext, out bool wheelPrev) {
		wheelNext = wheelPrev = false;
		currWheel = Input.GetAxis("Mouse ScrollWheel");

		if (currWheel > 0f)
			wheelNext = true;
		else if (currWheel < 0f)
			wheelPrev = true;
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
	
	static void loadPic(int i, string s) {
		BindData[i].Pic = Resources.Load<Texture>("Pic/Weap/" + s);
	}
	
	static void bind(int i, KeyCode kc) {
		BindData[i].KeyCode = (KeyCode)PlayerPrefs.GetInt("" + (UserAction)i, (int)kc);
	}

	public static void SetDefaultBinds() {
		// bind default keys/pics to user actions
		for (int i = 0; i < (int)UserAction.Count; i++) {
			BindData[i] = new BindData();
			BindData[i].Action = (UserAction)i;
			
			// get pic from list
			Texture pic = null;
			for (int j = 0; j < pics.Length; j++) {
				if ((UserAction)i + "" == pics[j].name)
					pic = pics[j];
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
				case UserAction.Next:
					bind(i, KeyCode.Mouse1);
					break;
				case UserAction.AltFire:
					bind(i, KeyCode.Mouse2);
					break;
				case UserAction.Previous:
					bind(i, KeyCode.P);
					break;
				case UserAction.Sprint:
					bind(i, KeyCode.LeftShift);
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
					
					
					
				case UserAction.Pistol:
					bind(i, KeyCode.Alpha2);
					loadPic(i, "0 " + (UserAction)i);
					break;
				case UserAction.Grenade:
					bind(i, KeyCode.X);
					loadPic(i, "1 " + (UserAction)i);
					break;
				case UserAction.MachineGun:
					bind(i, KeyCode.Alpha3);
					loadPic(i, "2 " + (UserAction)i);
					break;
				case UserAction.Rifle:
					bind(i, KeyCode.G);
					loadPic(i, "3 " + (UserAction)i);
					break;
				case UserAction.RocketLauncher:
					bind(i, KeyCode.V);
					loadPic(i, "4 " + (UserAction)i);
					break;
				case UserAction.Swapper:
					bind(i, KeyCode.W);
					loadPic(i, "5 " + (UserAction)i);
					break;
				case UserAction.GravGun:
					bind(i, KeyCode.R);
					loadPic(i, "6 " + (UserAction)i);
					break;
				case UserAction.Bomb:
					bind(i, KeyCode.C);
					loadPic(i, "7 " + (UserAction)i);
					break;
				case UserAction.Spatula:
					bind(i, KeyCode.Alpha1);
					loadPic(i, "8 " + (UserAction)i);
					break;
			}
		}
	}
}
