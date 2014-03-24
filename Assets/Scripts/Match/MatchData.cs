using UnityEngine;
using System.Collections;

public class MatchData {
	// 'mode select' stuff
	public string Name = "";
	public string Descript; // description
	public string[] allowedLevels;
	// specific mode/game settings stuff
	public string levelName; // ...to load/play in
	public int winScore = 30;
	public float Duration = 2.2f; // (negative for infinite)
	public float respawnWait = 2f;
	public bool deathsSubtractScore = true;
	public bool killsIncreaseScore = true;
	public bool teamBased = false;
	public bool allowFriendlyFire = false;
	public bool pitchBlack = false;
	public Item spawnGunA = Item.Pistol;
	public Item spawnGunB = Item.Grenade;
	public float restockTime = 12f;
	public int playerLives = 0;
	public bool basketball = false;
	// what is in the level pickupslots?
	public Item pickupSlot1 = Item.Health;
	public Item pickupSlot2 = Item.Grenade;
	public Item pickupSlot3 = Item.MachineGun;
	public Item pickupSlot4 = Item.Rifle;
	public Item pickupSlot5 = Item.Pistol;
	
	
	
	public MatchData(Match match) {
		switch (match) {
			case Match.Custom:
				Name = "Custom";
				Descript = "Have it your way!  All the exact settings you prefer.";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				break;
			case Match.GravORama:
				Name = "Bring Your Own Gravity"; // Gravity Of The Matter/Situation?  Your Own Gravity? A Gravity Of Your Own?
				// Gravity Is/Gets Personal?, Personal Gravity?, Gravitaction?
				Descript = "Each player has their own, independent, changeable gravity";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
				spawnGunA = Item.GravGun;
				spawnGunB = Item.Pistol;
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.GrueFood:
				Name = "Blackout";
				Descript = "Careful when you spark one up.....your GUN that is";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				pitchBlack = true;
				pickupSlot1 = Item.Grenade;
				pickupSlot2 = Item.MachineGun;
				pickupSlot3 = Item.Rifle;
				pickupSlot4 = Item.RocketLauncher;
				pickupSlot5 = Item.Bomb;
				break;
			case Match.FFAFragMatch:
				Name = "FFA Fragmatch";
				Descript = "Frag count is ALL that counts in this freestyle Free For All!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				respawnWait = 1f;
				break;
			case Match.TeamFragMatch:
				Name = "Team Fragmatch";
				Descript = "Frag count is what counts, but don't hurt your mates!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", 
					"TestLevelB", "Tower" };
				teamBased = true;
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.BBall:
				Name = "BBall";
				Descript = "Shooting hoops...and GUNS!  GANGSTA!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
				deathsSubtractScore = false;
				killsIncreaseScore = false;
				teamBased = true;
				basketball = true;
				pickupSlot2 = Item.MachineGun;
				pickupSlot3 = Item.Rifle;
				pickupSlot4 = Item.RocketLauncher;
				pickupSlot5 = Item.Swapper;
				break;
			case Match.YouOnlyLiveThrice:
				Name = "YOLT! (You Only Live Thrice)";
				Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				Duration = 0f;
				killsIncreaseScore = false;
				pickupSlot5 = Item.RocketLauncher;
				break;
			case Match.SwapMeat:
				Name = "Swap Meat";
				Descript = "There is only the swapper gun, grenades and lava... have fun!";
				allowedLevels = new string[] { "Furnace" , "Tower"};
				killsIncreaseScore = false;
				spawnGunA = Item.Swapper;
				spawnGunB = Item.Grenade;
				pickupSlot1 = Item.None;
				pickupSlot2 = Item.None;
				pickupSlot3 = Item.None;
				pickupSlot4 = Item.None;
				pickupSlot5 = Item.None;
				break;
			case Match.WeaponLottery:
				Name = "Weapon Lottery";
				Descript = "Assigned weaponry is a crap shoot!  CRAP! SHOOT!";
				allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
				winScore = 20;
				spawnGunA = Item.Random;
				spawnGunB = Item.Random;
				restockTime = 2f;
				pickupSlot1 = Item.Random;
				pickupSlot2 = Item.Random;
				pickupSlot3 = Item.Random;
				pickupSlot4 = Item.Random;
				pickupSlot5 = Item.Random;
				break;
		}
	}
}
