using UnityEngine;
using System.Collections;

public enum ControlDevice {
	LeftyMouse,
	LeftyMMOMouse,
	RightyMMOMouse,
	RightyMouse,

	Count,

	// these are unused, cuz we have no textures for them yet (Voxamancy has gamepad)
	GamePad,
	Hydra,
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
	MainMenu,
	NewGame,         // (don't need both Match AND StartGame anymore?)
	//JoinGame,
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// Online
	KickAPlayer,
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
	RocketProjectile = -6, // this was "rocket" when stringized, NOT "rocketlauncher"
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
	Pistol,
	Grenade,
	MachineGun, // 2
	Rifle,
	RocketLauncher, // 4
	Swapper,
	GravGun, // 6
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
