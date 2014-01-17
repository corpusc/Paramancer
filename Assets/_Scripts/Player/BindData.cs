using UnityEngine;
using System.Collections;

public class BindData {
	public enum ActionType {
		MoveForward,
		MoveBackward,
		MoveLeft,
		MoveRight,
		MoveUp,
		MoveDown,
		Count
	};
	
	

	public ActionType Action;
	public KeyCode KeyCode;
	public Texture Pic;
	
	
	
//	public BindData(ActionType action, KeyCode key, Texture pic) {
//		Action = action;
//		KeyCode = key;
//		Pic = pic;
//	}
}
