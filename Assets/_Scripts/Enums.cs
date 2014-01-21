using UnityEngine;
using System.Collections;

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