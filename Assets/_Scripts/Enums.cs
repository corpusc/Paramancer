using UnityEngine;
using System.Collections;

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
	Main,
	StartGame,         // (don't need both Match AND StartGame anymore?)
	JoinGame,
	Avatar,
	Credits,
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// Online
	Kick,
	Match, // MatchSetup   (don't need both Match AND StartGame anymore?)
	
	// Both
	Controls,
	Playing, // guess its not really BOTH, until i implement single player
	
	Count
}

public enum Item {
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
	SwapWeapon,
	GrabItem,
	Sprint,
	Chat,
	Menu,
	Scores,
	SwapTeam,
	Suicide,
	
	Count
}
