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
	public Item spawnGunA = Item.Pistol;
	public Item spawnGunB = Item.GrenadeLauncher;
	public float restockTime = 12f;
	public int playerLives = 0;
	public bool basketball = false;
	// pickupslots 
	public Item pickupSlot1 = Item.Health;
	public Item pickupSlot2 = Item.GrenadeLauncher;
	public Item pickupSlot3 = Item.MachineGun;
	public Item pickupSlot4 = Item.RailGun;
	public Item pickupSlot5 = Item.Pistol;

	
	
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
				spawnGunA = Item.Gravulator;
				spawnGunB = Item.Pistol;
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.Blackout:
				Name = "Blackout";
				Descript = "Careful when you spark one up! (Your gun, that is)";
				pitchBlack = true;
				pickupSlot1 = Item.GrenadeLauncher;
				pickupSlot2 = Item.MachineGun;
				pickupSlot3 = Item.RailGun;
				pickupSlot4 = Item.RocketLauncher;
				pickupSlot5 = Item.Bomb;
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
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.BBall:
				Name = "BBall";
				Descript = "Shooting hoops...and GUNS!  GANGSTA!";
				Maps = hasGoalsAndCeilings;
				deathsSubtractScore = false;
				killsIncreaseScore = false;
				teamBased = true;
				basketball = true;
				pickupSlot2 = Item.MachineGun;
				pickupSlot3 = Item.RailGun;
				pickupSlot4 = Item.RocketLauncher;
				pickupSlot5 = Item.Swapper;
				break;
			case Match.YouOnlyLiveThrice:
				Name = "YOLT! (You Only Live Thrice)";
				Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
				Duration = 0f;
				killsIncreaseScore = false;
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.InstaGib:
				Name = "InstaGib";
				Descript = "Rail Guns & Spatulas.  One hit & you're dead";
				spawnGunA = Item.RailGun;
				spawnGunB = Item.Spatula;
				pickupSlot1 = Item.None;
				pickupSlot2 = Item.None;
				pickupSlot3 = Item.None;
				pickupSlot4 = Item.None;
				pickupSlot5 = Item.None;
				break;
			case Match.WeaponLottery:
				Name = "Weapon Lottery";
				Descript = "Assigned weaponry is a crapshoot.  CRAP! SHOOT!";
				winScore = 20;
				restockTime = 2f;
				spawnGunA = Item.Random;
				spawnGunB = Item.Random;
				pickupSlot1 = Item.Random;
				pickupSlot2 = Item.Random;
				pickupSlot3 = Item.Random;
				pickupSlot4 = Item.Random;
				pickupSlot5 = Item.Random;
				break;
			case Match.LowGravity:
				Name = "Low Gravity";
				Descript = "I believe I can fly!";
				Gravity = 2f;
				break;
			case Match.SlowMo:
				Name = "Slow Motion";
				Descript = "Who used the time machine?!";
				MoveSpeedMult = 0.2f;
				spawnGunA = Item.RocketLauncher;
				spawnGunB = Item.GrenadeLauncher;
				pickupSlot1 = Item.Gravulator;
				pickupSlot2 = Item.Health;
				pickupSlot3 = Item.Spatula;
				pickupSlot4 = Item.Swapper;
				pickupSlot5 = Item.Bomb;
				break;
			case Match.HighSpeed:
				Name = "High Speed";
				Descript = "Who used the time machine... again?!";
				MoveSpeedMult = 1.8f;
				spawnGunA = Item.RocketLauncher;
				spawnGunB = Item.GrenadeLauncher;
				pickupSlot1 = Item.Gravulator;
				pickupSlot2 = Item.Health;
				pickupSlot3 = Item.MachineGun;
				pickupSlot4 = Item.Pistol;
				pickupSlot5 = Item.RailGun;
				break;
		}

		Seed = Random.Range(0, 1000000);
	}
}
