using UnityEngine;
using System.Collections;

public class GameModeScript {
	// 'mode select' stuff
	public string gameModeName = "";
	public string gameModeDescription;
	public string[] allowedLevels;
	// specific mode/game settings stuff
	public string levelName; // ...to load/play in
	public int winScore = 0;
	public float MatchDuration = 3f; // (negative for infinite)
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
}
