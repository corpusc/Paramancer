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

public enum Head { // ...to wear over avatars' normal heads (currently, replaces Sophie's stick figure avatar, which is a seperate model) 
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
	Blackout,
	FFAFragMatch,
	BBall,
	TeamFragMatch,
	YouOnlyLiveThrice,
	InstaGib,
	WeaponLottery,
	LowGravity,
	SlowMo,
	HighSpeed,
	Count
}

public enum HudMode {
	// Offline
	SplashLogos,
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
	Pistol, // if these ever change, then modify testing for this range (thru Spatula) in SetDefaultBinds() 
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
	
	Count
}

public enum ParticleType {
	Circle,
	Puff,
	Multiple, //these are many particles stacked upon each other in 1 image(for performance reasons)

	Count
}

public enum GameStyle {
	SciFi,
	Medieval,
	SteamPunk,

	Count
}
