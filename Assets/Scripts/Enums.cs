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

public enum Barrel {
	Double,
	Muted,
	Large,

	Count
}

public enum UnderBarrel { //activate with z?
	Bayonet, //instakill anyone close enough and right in front of you
	Laser, //deal damage per second to enemies directly in front of you
	Flashlight,
	Kamikaze, //go boom! when you die, activatable
	Gravulator,
	Swapper,
	GrapplingHook,
	OozeLeaker, //an ooze that slows everyone who enters it

	Count
}

public enum OverBarrel { //activate with x?
	Scope, //zoom
	Forcefield, //damage all enemies that are close
	DamageReductor, //dmg *= 0.95f
	Unhitter, //activate to enter a mode where you can't move/shoot, but you can't be hit(only lasts 3 seconds)
	Flamethrower,

	Count
}

public enum FiringChamber {
	Large, //for grenades/rockets
	Vaporizing, //jets of glowing-hot gasses fly out of your gun to the sides, dealing damage
	Vacuum, //no shot sound
	Amortized, //no knockback

	Count
}

public enum MainBody {
	Light, //move 10% faster
	Electromagnetic, //railgun effect, consume energy for more damage
	Immortality, //if (health - dmg <= 0f && cooldown == 0f) health = 1f;
	Cushioned, //if (dmg > 30f) dmg = lerp(dmg, 30f, 0.1f);
	Ethereal, //instant respawn

	Count
}

public enum Ammo {
	Rocket,
	Grenade,
	SmokeGrenade,
	Tape, //for machine guns
	Pull, //...anyone hit towards you
	Push, //...-,,- away from you
	Tesla, //damage nearby enemies

	Count
}

public enum Handle {
	Standard,
	Auto,

	Count
}
