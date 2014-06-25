using UnityEngine;
using System.Collections;

public static class CcInput {
	public static BindData[] BindData = new BindData[(int)UserAction.Count];

	// private 
	static float currWheel;


	
	static CcInput() {
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
	
	public static bool Started(UserAction ua) {
		for (int i = 0; i < BindData.Length; i++) {
			if (ua == BindData[i].Action)
				if (Input.GetKeyDown(BindData[i].KeyCode) )
					return true;
		}
		
		return false;
	}
	public static bool Ended(UserAction ua) {
		for (int i = 0; i < BindData.Length; i++) {
			// if 
			if (ua == BindData[i].Action)
				if (Input.GetKeyUp(BindData[i].KeyCode) )
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

	public static void SetDefaultBinds() { // bind default keys/pics to user actions 
		for (int i = 0; i < (int)UserAction.Count; i++) {
			// weapon names have spaces in them currently, so specialcase those 
			var s = "" + (UserAction)i;
			if (i >= (int)UserAction.Pistol &&
			    i <= (int)UserAction.Spatula)
			{
				s = S.GetSpacedOut(s);
			}

			// the rest 
			BindData[i] = new BindData();
			BindData[i].Action = (UserAction)i;
			BindData[i].Pic = Pics.Get(s);
			
			switch ((UserAction)i) {
				#region movement
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
				#endregion
					
				case UserAction.Activate:
					bind(i, KeyCode.Mouse0);
					break;
				case UserAction.Alt:
					bind(i, KeyCode.Mouse1);
					break;
				case UserAction.Next:
					bind(i, KeyCode.LeftArrow);
					break;
				case UserAction.Previous:
					bind(i, KeyCode.RightArrow);
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
				case UserAction.TakePicture:
					bind(i, KeyCode.Print);
					break;
				case UserAction.Scores:
					bind(i, KeyCode.Tab);
					break;
				case UserAction.SwapTeam:
					bind(i, KeyCode.KeypadEnter);
					break;
				case UserAction.Suicide:
					bind(i, KeyCode.KeypadMinus);
					break;
					
					
					
				#region guns
				case UserAction.Pistol:
					bind(i, KeyCode.Alpha2);
					break;
				case UserAction.GrenadeLauncher:
					bind(i, KeyCode.X);
					break;
				case UserAction.MachineGun:
					bind(i, KeyCode.Alpha3);
					break;
				case UserAction.RailGun:
					bind(i, KeyCode.G);
					break;
				case UserAction.RocketLauncher:
					bind(i, KeyCode.V);
					break;
				case UserAction.Swapper:
					bind(i, KeyCode.W);
					break;
				case UserAction.Gravulator:
					bind(i, KeyCode.R);
					break;
				case UserAction.Bomb:
					bind(i, KeyCode.C);
					break;
				case UserAction.Spatula:
					bind(i, KeyCode.Q);
					break;
				#endregion
			}
		}
	}
}
