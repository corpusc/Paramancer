using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchData {
	public float MoveSpeedMult = 1f; // the mtime speed multiplier 
	public float Gravity = 10f;
	public Theme Theme = Theme.SciFi;
	// proc gen 
	public bool NeedsGenerating = false;
	public int Seed = 0;
	public static string ProcGenName = "Generated with seed"; // user-facing name of procedurally generated voxel-style maps 
	// 'mode select' 
	public string Name = "";
	public string Descript; // description 
	public List<string> Maps; // valid maps 
	// specific match/game settings 
	public string levelName; // ...to load/play in 
	public int winScore = 30;
	public float Duration = 13f; // (negative for infinite) 
	public float respawnWait = 2f;
	public bool deathsSubtractScore = true;
	public bool killsIncreaseScore = true;
	public bool teamBased = false;
	public bool FriendlyFire = false;
	public bool pitchBlack = false;
	public Gun spawnGunA = Gun.Pistol;
	public Gun spawnGunB = Gun.GrenadeLauncher;
	public float restockTime = 12f;
	public int playerLives = 0;
	public bool basketball = false;
	// pickupslots 
	public Gun pickupSlot1 = Gun.Health;
	public Gun pickupSlot2 = Gun.GrenadeLauncher;
	public Gun pickupSlot3 = Gun.MachineGun;
	public Gun pickupSlot4 = Gun.RailGun;
	public Gun pickupSlot5 = Gun.Pistol;

	
	
	public MatchData(Match match) {
		var all = new List<string>() { ProcGenName, "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "Tower" };
		var hasGoalsAndCeilings = new List<string>() { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		Maps = all;

		switch (match) {
			case Match.Custom:
				Name = "Custom";
				Descript = "Have it your way";
				break;
			case Match.BringYourOwnGravity:
				Name = "Bring Your Own Gravity"; //  A Gravity Of Your Own?  Gravity Is/Gets Personal? 
				Descript = "Each player has their own, independent, changeable gravity";
				Maps = hasGoalsAndCeilings;
				Maps.Insert(0, ProcGenName);
				spawnGunA = Gun.Gravulator;
				spawnGunB = Gun.Pistol;
				pickupSlot5 = Gun.RocketLauncher;
				break;
			case Match.Blackout:
				Name = "Blackout";
				Descript = "Careful when you spark one up! (Your gun, that is)";
				pitchBlack = true;
				pickupSlot1 = Gun.GrenadeLauncher;
				pickupSlot2 = Gun.MachineGun;
				pickupSlot3 = Gun.RailGun;
				pickupSlot4 = Gun.RocketLauncher;
				pickupSlot5 = Gun.Bomb;
				break;
			case Match.FFAFragMatch:
				Name = "FFA Fragmatch";
				Descript = "Frag count is ALL that counts in this freestyle Free For All";
				respawnWait = 1f;
				break;
			case Match.TeamFragMatch:
				Name = "Team Fragmatch";
				Descript = "Frag count is what counts, but don't hurt your mates!";
				teamBased = true;
				pickupSlot5 = Gun.RocketLauncher;
				break;
			case Match.BBall:
				Name = "BBall";
				Descript = "Shooting hoops...and GUNS!  GANGSTA!";
				Maps = hasGoalsAndCeilings;
				deathsSubtractScore = false;
				killsIncreaseScore = false;
				teamBased = true;
				basketball = true;
				pickupSlot2 = Gun.MachineGun;
				pickupSlot3 = Gun.RailGun;
				pickupSlot4 = Gun.RocketLauncher;
				pickupSlot5 = Gun.Swapper;
				break;
			case Match.YouOnlyLiveThrice:
				Name = "YOLT! (You Only Live Thrice)";
				Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
				Duration = 0f;
				killsIncreaseScore = false;
				pickupSlot5 = Gun.RocketLauncher;
				break;
			case Match.InstaGib:
				Name = "InstaGib";
				Descript = "Rail Guns & Spatulas.  One hit & you're dead";
				spawnGunA = Gun.RailGun;
				spawnGunB = Gun.Spatula;
				pickupSlot1 = Gun.None;
				pickupSlot2 = Gun.None;
				pickupSlot3 = Gun.None;
				pickupSlot4 = Gun.None;
				pickupSlot5 = Gun.None;
				break;
			case Match.LowGravity:
				Name = "Low Gravity";
				Descript = "...";
				Gravity = 2f;
				break;
			case Match.SlowMotion:
				Name = "Slow Motion";
				Descript = "Who used the time machine?!";
				MoveSpeedMult = 0.2f;
				spawnGunA = Gun.RocketLauncher;
				spawnGunB = Gun.GrenadeLauncher;
				pickupSlot1 = Gun.Gravulator;
				pickupSlot2 = Gun.Health;
				pickupSlot3 = Gun.Spatula;
				pickupSlot4 = Gun.Swapper;
				pickupSlot5 = Gun.Bomb;
				break;
			case Match.HighSpeed:
				Name = "High Speed";
				Descript = "Who used the time machine... again?!";
				MoveSpeedMult = 1.8f;
				spawnGunA = Gun.RocketLauncher;
				spawnGunB = Gun.GrenadeLauncher;
				pickupSlot1 = Gun.Gravulator;
				pickupSlot2 = Gun.Health;
				pickupSlot3 = Gun.MachineGun;
				pickupSlot4 = Gun.Pistol;
				pickupSlot5 = Gun.RailGun;
				break;
		}

		Seed = Random.Range(0, 1000000);
	}
}
