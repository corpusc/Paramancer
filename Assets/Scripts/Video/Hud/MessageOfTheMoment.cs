using UnityEngine;
using System.Collections;



public class MessageOfTheMoment {
	float msgDura = 7f; // message/tip duration 
	float nextTip;
	string msg = ""; // the currently displayed splash 
	Rect splashRect;
	
	string[] tipText = { // less than 2 entries would cause endless loop 
		"MODERN/MILITARY FPS PLAYERS: Exit please, I do not value your feedback.  :)",
		"TIP: Use the Gravulator as often as possible to confuse your enemies",
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
			splashRect = new Rect((Screen.width - wid) / 2f, 0, wid, hud.GetHeightBox(msg));
		}
		
		GUI.Box(splashRect, msg);
	}
}
