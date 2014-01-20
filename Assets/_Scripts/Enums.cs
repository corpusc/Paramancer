using UnityEngine;
using System.Collections;

public enum UserAction {
	MoveForward,
	MoveBackward,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	
	
	Count
//	Use, // fire/activate/use whatever you are holding
//	Sprint,
//	SwapWeapon,
//	GrabItem,
//	Chat,
//	Menu,
//	Scores,
//	SwapTeam,
//	Suicide,
};

public enum Weapon {
	Pistol,
	Grenade,
	MachineGun,
	Rifle,
	RocketLauncher,
	Swapper,
	GravGun,
	Bomb,
	Spatula
}