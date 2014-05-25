using UnityEngine;
using System.Collections;



public class MessageOfTheMoment {
	float msgDura = 9f; // message/tip duration 
	float nextTip;
	string msg = ""; // the currently displayed splash 
	Rect r;
	
	string[] tipText = { // less than 2 entries would cause endless loop 
		"This is PURE OLD SCHOOL ARENA ACTION!  Don't ask for MODERN/MILITARY FPS features!",
		"TIP: In BYOG matches, use the Gravulator often, to confuse your enemies",
		"TIP: Download the standalone version for better performance",
		//"TIP: Sometimes offense is the best defense"
	};



	public void Draw(Hud hud) {
		if /* time for next tip */ (Time.time > nextTip) {
			nextTip = Time.time + msgDura;

			string newtip = "";

			// make sure we don't give the same message twice in a row 
			do {
				newtip = tipText[Random.Range(0, tipText.Length)];
			} while (newtip == msg);

			msg = newtip;
			var wid = hud.GetWidthBox(msg);
			r = new Rect((Screen.width - wid) / 2f, 0, wid, hud.GetHeightBox(msg));
		}
		
		GUI.Box(r, msg);
	}
}
