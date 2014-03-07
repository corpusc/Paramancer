using UnityEngine;
using System.Collections;

public static class Pics {
	//static public Object[] pics;

	// textures
	static public Texture[] swapperCrosshair = new Texture[4];
	static public Texture Health;
	static public Texture White;
	static public Texture Black;
	static public Texture crossHair;
	static public Texture TeamRedFlag;
	static public Texture TeamBlueFlag;
	static public Texture gameLogo;
	static public Texture companyLogo;
	static public Texture Sprint;

	
	
	static Pics() {
		Object[] pics = Resources.LoadAll("Pic/Hud");

		for (int i = 0; i < pics.Length; i++) {
			var s = pics[i].name;
			
			switch (s) {
				case "Sprint": 
					Sprint = (Texture)pics[i]; 
					break;
				case "Health": 
					Health = (Texture)pics[i]; 
					break;
				case "whiteTex": 
					White = (Texture)pics[i]; 
					break;
				case "blackTex": 
					Black = (Texture)pics[i]; 
					break;
				case "Crosshair": 
					crossHair = (Texture)pics[i]; 
					break;
				case "Logo - CazCore": 
					companyLogo = (Texture)pics[i]; 
					break;
				case "Logo - Paramancer": 
					gameLogo = (Texture)pics[i]; 
					break;
				case "FlagRed": 
					TeamRedFlag = (Texture)pics[i]; 
					break;
				case "FlagBlue": 
					TeamBlueFlag = (Texture)pics[i]; 
					break;
				case "swapper_crosshair":
					swapperCrosshair[0] = (Texture)pics[i];
					break;
				case "swapper_crosshair2":
					swapperCrosshair[1] = (Texture)pics[i];
					break;				
				case "swapper_crosshair3":
					swapperCrosshair[2] = (Texture)pics[i];
					break;				
				case "swapper_crosshair4":
					swapperCrosshair[3] = (Texture)pics[i];
					break;			
			}
		}
	}
	
//	public static Texture GetFirstWith(string s) {
//		for (int i = 0; i < pics.Length; i++) {
//			if (s == pics[i].name)
//				return (Texture)pics[i]; 
//		}
//
//		Debug.LogError("Can't find any picture with that name!");
//		return null;
//	}
}
