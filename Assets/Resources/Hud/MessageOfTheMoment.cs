using UnityEngine;
using System.Collections;



public class MessageOfTheMoment {
	float msgDura = 9f; // message/tip duration 
	float nextTip;
	string msg = ""; // the currently displayed splash 
	Rect r;
	
	string[] tipText = { // less than 2 entries would cause endless loop 
		"  This is PURE OLD SCHOOL ARENA ACTION!  \n  Don't ask for MODERN/MILITARY FPS features!  ",
		"TIP: In BYOG matches, use the Gravulator to confuse your enemies",
		"TIP: Download the stand-alone version for better performance",
	};



	public void Draw(Hud hud) {
		if (Time.time > nextTip) {
			nextTip = Time.time + msgDura;

			string newtip = "";

			// make sure we don't give the same message twice in a row 
			do {
				newtip = tipText[Random.Range(0, tipText.Length)];
			} while (newtip == msg);

			msg = newtip;
			var w = hud.GetWidthBox(msg);
			var h = hud.GetHeightBox(msg);
			r = new Rect((Screen.width - w) / 2f, 0, w, h);
		}
		
		GUI.Box(r, msg);
	}
}
