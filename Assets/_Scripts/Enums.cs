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

public enum Menu {
	// Offline
	Main,
	StartGame,
	JoinGame,
	Avatar,
	Credits,
	Connecting,
	ConnectionError,
	InitializingServer,
	Wait,
	
	// online
	Kick,
	Match, // MatchSetup
	
	// both
	Controls,
	
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
	
	Sprint,
	SwapWeapon,
	GrabItem,
	Chat,
	Menu,
	Scores,
	SwapTeam,
	Suicide,
	
	Count
//	Use, // fire/activate/use whatever you are holding
}
