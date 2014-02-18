using UnityEngine;
using System.Collections;

public class BindData {
	public UserAction Action;
	public KeyCode KeyCode;
	public Texture Pic;
	public int Id; // idx into the keyData, which stores the physical layout of keys
	
	// unused atm
	//fadeMax
	//fadeMin
	//fadeInTime
	//   ------- all of the above could be params for each of these:
	// action icon
	// keycap/dpad/thumbstick/button
	// 		or as alternative to above, an outline, or a U-shaped/cup like SotA...maybe 2 sided angle
	// keylabel (prob not needed for gamepad?)
	
	
	
//	public BindData(ActionType action, KeyCode key, Texture pic) {
//		Action = action;
//		KeyCode = key;
//		Pic = pic;
//	}
}
