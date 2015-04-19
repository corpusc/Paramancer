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
	Coop, // multiplayer campaign (PVE/co-operative)... players cannot hurt each other 
	Invasion, // multiplayer campaign (PVP)... players CAN hurt each other 
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
	NewGame,    // (don't need both MatchSetup AND NewGame anymore?) 
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// online 
	MatchSetup, // (don't need both MatchSetup AND NewGame anymore?) 
	KickAPlayer,

	// both 
	MainMenu,

	About,
	Controls,
	Settings,
	Credits,
	Playing,
	Editing,
	
	Count
}


public enum Gun { 
	// FIXME: this used to be "Item" 
	// things that aren't guns need their own enum or system 
	Lava = -5,
	Suicide = -4,
	Health = -3,
	None = -1,

	// when changing weapon names, also change in "UserAction" enum 
	Pistol = 0,
	GrenadeLauncher,
	MachineGun,
	RailGun,
	NapalmLauncher,
	Swapper,
	Gravulator,
	Bomb,
	Spatula,

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
	Fantasy,
	SteamPunk,

	Count
}