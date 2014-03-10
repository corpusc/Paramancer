using UnityEngine;
using System.Collections;

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

//	// NOT sophie's
//  TentacleRoot,
//	RobotHead,
//	head_spaceship,
//	enforcer_face,
//	SmileyHead,
//	Helmet,
//	PaperBag,
//	Mahead,

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
	GravORama,
	GrueFood,
	FFAFragMatch,
	BBall,
	TeamFragMatch,
	YouOnlyLiveThrice,
	SwapMeat,
	WeaponLottery,
	Count
}

public enum HudMode {
	// Offline
	MenuMain,
	StartGame,         // (don't need both Match AND StartGame anymore?)
	JoinGame,
	Avatar,
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// Online
	Kick,
	MatchSetup, // MatchSetup   (don't need both Match AND StartGame anymore?)
	
	// Both
	Controls,
	Settings,
	Credits,
	//FuturePlans,
	Playing, // guess its not really BOTH, until i implement single player
	Editing, // guess its not really BOTH, until i implement single player
	
	Count
}

public enum Item {
	RocketMaybeJustASingle = -6, // this was "rocket" when stringized, NOT "rocketlauncher"
	Lava = -5, // not really an item, should be moved?
	Suicide = -4, // not really an item, should be moved?
	Health = -3,
	Random = -2,
	None = -1,
	Pistol = 0,
	Grenade,
	MachineGun, // 2
	Rifle,
	RocketLauncher, // 4
	Swapper,
	GravGun, // 6
	Bomb,
	Spatula, // 8
	Count
}

public enum UserAction {
	MoveForward,
	MoveBackward,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	
	Activate, // fire/activate/use whatever you are holding
	Next,
	Previous,
	GrabItem, // for sound: YOINK?
	Sprint, // for sound: something like GO SPEED RACER GO?
	Chat,
	Menu, // for sound: "gotta change something... HOLD ON!"?
	Scores,
	SwapTeam, // for sound: "WASSUP GUYS, the other team SUCKS"?
	Suicide, // for sound: "there's nothing left for me here!"?
	
	Count
}
