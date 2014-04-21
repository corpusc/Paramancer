using UnityEngine;
using System.Collections;

public enum ControlDevice {
	Hydra,
	LeftyMouse,
	LeftyMMOMouse,
	RightyMMOMouse,
	RightyMouse,
	GamePad,

	Count,
}

public enum Head {
	NormalHead,
	CardboardBoxHead,
	FishHead,
	BananaHead,
	CreeperHead,
	ElephantHeadMesh,
	MoonHead,
	PyramidHead,
	ChocoboHead,
	SpikeHead,

	Count
}

public enum Crosshair {
	Normal,
	Shrinking,
	Growing,
	Count
}

public enum Match {
	Custom,
	BringYourOwnGravity,
	GrueFood,
	FFAFragMatch,
	BBall,
	TeamFragMatch,
	YouOnlyLiveThrice,
	InstaGib,
	WeaponLottery,
	Count
}

public enum HudMode {
	// Offline
	PreMenu,
	MainMenu,
	NewGame,         // see comment below VVVVVVVVVV 
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// Online
	KickAPlayer,
	MatchSetup, // MatchSetup   (don't need both MatchSetup AND NewGame anymore?) 
	
	// Both
	About,
	Controls,
	Settings,
	Credits,
	Playing, // guess its not really BOTH, until i implement single player
	Editing, // guess its not really BOTH, until i implement single player
	
	Count
}

public enum Item {
	RocketProjectile = -6, // this was "rocket" when stringized, NOT "rocketlauncher"
	Lava = -5, // not really an item, should be moved?
	Suicide = -4, // not really an item, should be moved?
	Health = -3,
	Random = -2,
	None = -1,

	// when changing weapon names, also change in "UserAction" enum 
	Pistol = 0,
	GrenadeLauncher,
	MachineGun, // 2
	RailGun,
	RocketLauncher, // 4
	Swapper,
	Gravulator, // 6
	Bomb,
	Spatula, // 8
	Count
}

public enum UserAction {
	// when changing weapon names, also change in "Item" enum 
	// these are for selecting/equipping with 1 touch 
	Pistol,
	GrenadeLauncher,
	MachineGun, // 2
	RailGun,
	RocketLauncher, // 4
	Swapper,
	Gravulator, // 6
	Bomb,
	Spatula, // 8
	
	MoveForward,
	MoveBackward,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	
	Activate, // fire/activate/use whatever you are holding
	Alt,
	Next,
	Previous,
	Sprint, // for sound: something like GO SPEED RACER GO?
	Chat,
	Menu, // for sound: "gotta change something... HOLD ON!"?
	Scores,
	SwapTeam, // for sound: "WASSUP GUYS, the other team SUCKS"?
	Suicide, // for sound: "there's nothing left for me here!"?
	
	Count
}
