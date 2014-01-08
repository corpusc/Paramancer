using UnityEngine;
using System.Collections;

public class GameModeScript : MonoBehaviour {

	//General 'mode select' stuff
	public string gameModeName = "";
	public string gameModeDescription;
	public string[] allowedLevels;
	
	//specific mode/game settings stuff
	
	
	//name of level to load/play in
	public string levelName;
	
	//score a player hits to win/end game
	public int winScore;
	
	//time the game goes on for (negative for infinite)
	public float gameTime;
	
	//how long till respawn
	public float respawnWait = 10f;
	
	//do deaths make the score go down?
	public bool deathsSubtractScore = true;
	
	//do kills make the score go up?
	public bool killsIncreaseScore = true;
	
	//team based?
	public bool teamBased = false;
	
	//can you hurt your teammates?
	public bool allowFriendlyFire = false;
	
	//can you hurt your teammates?
	public bool pitchBlack = false;
	
	//what guns do we start with
	public int spawnGunA = 0;
	public int spawnGunB = 1;
	
	//and what is in the level pickupslots?
	public int pickupSlot1 = 0;
	public int pickupSlot2 = 1;
	public int pickupSlot3 = 2;
	public int pickupSlot4 = 3;
	public int pickupSlot5 = -3;
	
	//restock time
	public float restockTime = 10f;
	
	//number of lives
	public int playerLives = 0;
	
	public bool basketball = false;
}
