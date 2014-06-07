using UnityEngine;
using System.Collections;



public enum RuleSet {
	// anything < RuleSet.Arena includes a campaign.
	// where you battle your way thru a series of maps (or dungeon floors), 
	// fighting monsters along the way.  there will be a variety of items 
	// to collect in your inventory, and to use at strategic moments, 
	// to help you in your quest.  at the end, you must defeat a boss monster, 
	// and get/save the MacGuffin/damsel (or perform some heroic act) 
	
	Solo, // singleplayer 
	Coop, // multiplayer (PVE/co-operative)... players cannot hurt each other 
	Invasion, // multiplayer (PVP)... players CAN hurt each other 
	Arena, // pure PVP.  no mobs/monsters, no campaign, and probably VERY FEW items/pickups (outside of guns) 

	Count,
}

public enum ControlDevice {	// anything with "mouse" in it also includes the keyboard 
	Hydra,
	LeftyMouse,
	LeftyMMOMouse,
	RightyMMOMouse,
	RightyMouse,
	GamePad,

	Count,
}

public enum Head { // ...to wear over normal avatars' heads 
	// (currently, REPLACES Sophie's stick figure avatar head, because it's a seperate model) 
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
	LowGravity,
	SlowMotion,
	HighSpeed,
	Count
}

public enum HudMode {
	// offline 
	SplashLogos,
	MainMenu,
	NewGame,    // (don't need both MatchSetup AND NewGame anymore?) 
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// online 
	MatchSetup, // (don't need both MatchSetup AND NewGame anymore?) 
	KickAPlayer,

	// both 
	About,
	Controls,
	Settings,
	Credits,
	Playing, // guess its not really BOTH, until i implement single player
	Editing, // guess its not really BOTH, until i implement single player
	
	Count
}

public enum Gun { // FIXME: this used to be "Item", but they need their own enum or system 
	Lava = -5, // not really an item, should be moved? 
	Suicide = -4, // not really an item, should be moved? 
	Health = -3,
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

public enum Theme {
	SciFi,
	Medieval,
	SteamPunk,

	Count
}