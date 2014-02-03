using UnityEngine;
using System.Collections;

public static class Pics {
	// textures
	static public Texture[] swapperCrosshair = new Texture[4];
	static public Texture lifeIcon;
	static public Texture backTex;
	static public Texture blackTex;
	static public Texture crossHair;
	static public Texture teamRedFlag;
	static public Texture teamBlueFlag;
	static public Texture gameLogo;
	static public Texture companyLogo;
	
	

	static Pics() {
		UnityEngine.Object[] pics = Resources.LoadAll("Pic/Hud");
		
		// use this temp list to setup permanent vars
		for (int i = 0; i < pics.Length; i++) {
			var s = pics[i].name;
			
			switch (s) {
				case "Health": 
					lifeIcon = (Texture)pics[i]; 
					break;
				case "whiteTex": 
					backTex = (Texture)pics[i]; 
					break;
				case "blackTex": 
					blackTex = (Texture)pics[i]; 
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
					teamRedFlag = (Texture)pics[i]; 
					break;
				case "FlagBlue": 
					teamBlueFlag = (Texture)pics[i]; 
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
					break;			}
		}
	}
}
