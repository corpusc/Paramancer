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
	public static string VoxelName = "Generated with seed"; // user-facing name of procedurally generated voxel-style maps 
	// 'mode select' 
	public string Name = "";
	public string Descript; // description 
	public List<string> Maps; // valid maps 
	// specific match/game settings 
	public string MapName; // ...to load/play in 
	public int winScore = 30;
	public float Duration = 13f; // (negative for infinite) 
	public float respawnWait = 2f;
	public bool deathsSubtractScore = true;
	public bool killsIncreaseScore = true;
	public bool teamBased = false;
	public bool FriendlyFire = false;
	public bool pitchBlack = false;
	public Gun spawnGunA = Gun.Pistol;
	public Gun spawnGunB = Gun.Spatula;
	public float restockTime = 12f;
	public int playerLives = 0;
	public bool basketball = false;
	public RuleSet RuleSet = RuleSet.Arena;
	// pickupslots 
	public Gun pickupSlot1 = Gun.Health;
	public Gun pickupSlot2 = Gun.GrenadeLauncher;
	public Gun pickupSlot3 = Gun.MachineGun;
	public Gun pickupSlot4 = Gun.RailGun;
	public Gun pickupSlot5 = Gun.Pistol;


	
	public MatchData(Match match) {
		var all = new List<string>() { VoxelName, "Furnace", "Conflict Room", "The OctaDrome", "Overpass", "Tower" };
		var hasGoalsAndCeilings = new List<string>() { "Furnace", "Conflict Room", "The OctaDrome" }; // voxel maps & Overpass have no red/blue goals.  if you add some,
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
				Maps.Insert(0, VoxelName);  // has ceiling (no goals needed)
				Maps.Insert(0, "Overpass"); // has ceiling (no goals needed)
				spawnGunA = Gun.Gravulator;
				spawnGunB = Gun.Pistol;
				break;
			case Match.Blackout:
				Name = "Blackout";
				Descript = "Careful when you spark one up! (Your gun, that is)";
				pitchBlack = true;
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
				break;
			case Match.BBall:
				Name = "BBall";
				Descript = "Shooting hoops...and GUNS!  GANGSTA!";
				Maps = hasGoalsAndCeilings;
				deathsSubtractScore = false;
				killsIncreaseScore = false;
				teamBased = true;
				basketball = true;
				break;
			case Match.YouOnlyLiveThrice:
				Name = "YOLT! (You Only Live Thrice)";
				Descript = "Last Person Standing...but you have 3 lives";
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
				Descript = "...";
				MoveSpeedMult = 0.2f;
				spawnGunA = Gun.RocketLauncher;
				spawnGunB = Gun.GrenadeLauncher;
				break;
			case Match.HighSpeed:
				Name = "High Speed";
				Descript = "...";
				MoveSpeedMult = 1.8f;
				spawnGunA = Gun.RocketLauncher;
				spawnGunB = Gun.GrenadeLauncher;
				break;
		}

		Seed = Random.Range(0, 1000000);

		if (Debug.isDebugBuild)
			restockTime = 2f;
	}
}
