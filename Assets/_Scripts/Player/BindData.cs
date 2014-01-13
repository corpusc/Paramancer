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
	};
	

	public ActionType Action;
	public KeyCode KeyCode;
	public Texture Pic;
	public string Text;
	public int Id;
}
