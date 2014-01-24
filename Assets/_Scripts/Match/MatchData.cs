using UnityEngine;
using System.Collections;

public class MatchData {
	// 'mode select' stuff
	public string Name = "";
	public string Descript; // description
	public string[] allowedLevels;
	// specific mode/game settings stuff
	public string levelName; // ...to load/play in
	public int winScore = 0;
	public float Duration = 3f; // (negative for infinite)
	public float respawnWait = 10f;
	public bool deathsSubtractScore = true;
	public bool killsIncreaseScore = true;
	public bool teamBased = false;
	public bool allowFriendlyFire = false;
	public bool pitchBlack = false;
	public int spawnGunA = 0;
	public int spawnGunB = 1;
	public float restockTime = 10f;
	public int playerLives = 0;
	public bool basketball = false;
	// what is in the level pickupslots?
	public int pickupSlot1 = 0;
	public int pickupSlot2 = 1;
	public int pickupSlot3 = 2;
	public int pickupSlot4 = 3;
	public int pickupSlot5 = -3;
	
	
	
	public MatchData(Match match) {
		switch (match) {
			case Match.Custom:
				Name = "Custom";
				Descript = "Have it your way!  All the exact settings you prefer.";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				respawnWait = 5f;
				break;
			case Match.GravORama:
				Name = "Grav-O-Rama"; // Gravity Of The Matter/Situation?  Your Own Gravity? A Gravity Of Your Own?
				// Gravity Is/Gets Personal?, Personal Gravity?, Gravitaction?
				Descript = "Each player has their own, independent, changeable gravity";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
				respawnWait = 5f;
				spawnGunB = (int)Weapon.GravGun;
				pickupSlot5 = 4;
				break;
			case Match.GrueFood:
				Name = "Grue Food";
				Descript = "It is pitch black.  You are likely to be eaten by a grue.";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				respawnWait = 5f;
				pitchBlack = true;
				pickupSlot1 = 1;
				pickupSlot2 = 2;
				pickupSlot3 = 3;
				pickupSlot4 = 4;
				pickupSlot5 = 7;
				break;
			case Match.FFAFragMatch:
				Name = "FFA Fragmatch";
				Descript = "Frag count is ALL that counts in this freestyle Free For All!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				respawnWait = 5f;
				break;
			case Match.TeamFragMatch:
				Name = "Team Fragmatch";
				Descript = "Frag count is what counts, but don't hurt your mates!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				respawnWait = 5f;
				teamBased = true;
				pickupSlot5 = 4;
				break;
			case Match.BBall:
				Name = "BBall";
				Descript = "Shooting hoops...and GUNS!  GANGSTA!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
				respawnWait = 5f;
				deathsSubtractScore = false;
				killsIncreaseScore = false;
				teamBased = true;
				basketball = true;
				pickupSlot2 = 2;
				pickupSlot3 = 3;
				pickupSlot4 = 4;
				pickupSlot5 = 5;
				break;
			case Match.YouOnlyLiveThrice:
				Name = "YOLT! (You Only Live Thrice)";
				Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				Duration = 0f;
				respawnWait = 5f;
				killsIncreaseScore = false;
				pickupSlot5 = 4;
				break;
			case Match.SwapMeat:
				Name = "Swap Meat";
				Descript = "There is only the swapper gun, grenades and lava... have fun!";
				allowedLevels = new string[] { "Furnace" , "Tower"};
				respawnWait = 3f;
				killsIncreaseScore = false;
				spawnGunA = 5;
				spawnGunB = 1;
				pickupSlot1 = -1;
				pickupSlot2 = -1;
				pickupSlot3 = -1;
				pickupSlot4 = -1;
				pickupSlot5 = -1;
				break;
			case Match.WeaponLottery:
				Name = "Weapon Lottery";
				Descript = "Assigned weaponry is a crap shoot!  CRAP! SHOOT!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				winScore = 20;
				respawnWait = 5f;
				spawnGunA = -2;
				spawnGunB = -2;
				restockTime = 2f;
				pickupSlot1 = -2;
				pickupSlot2 = -2;
				pickupSlot3 = -2;
				pickupSlot4 = -2;
				pickupSlot5 = -2;
				break;
		}
	}
}
