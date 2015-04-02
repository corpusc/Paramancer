public enum UserAction {
	// when changing weapon names, also change in "Gun" enum 
	// these are for direct selecting/equipping with 1 button press 
	Pistol, // if these ever change, then modify testing for this range (thru Spatula) in SetDefaultBinds() 
	GrenadeLauncher,
	MachineGun,
	RailGun,
	NapalmLauncher,
	Swapper,
	Gravulator,
	Bomb,
	Spatula,
	
	MoveForward,
	MoveBackward,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	
	Activate, // fire/activate/use whatever you are holding/wielding 
	Alt,
	Next,
	Previous,
	Sprint, // for sound: something like GO SPEED RACER GO? 
	Chat,
	
	Menu, // for sound: "gotta change something... HOLD ON!"? 
	TakePicture,
	Scores,
	SwapTeam, // for sound: "WASSUP GUYS, the other team SUCKS"? 
	Suicide, // for sound: "there's nothing left for me here!"? 
	
	// sound suggestions are player located sounds (not omni) 
	
	// music (Melodician) 
	//	NoteC,
	//	NoteCSharp,
	//	NoteD,
	//	NoteDSharp,
	//	NoteE,
	//	NoteF,
	//	NoteFSharp,
	//	NoteG,
	//	NoteGSharp,
	//	NoteA,
	//	NoteASharp,
	//	NoteB,
	
	Count
}