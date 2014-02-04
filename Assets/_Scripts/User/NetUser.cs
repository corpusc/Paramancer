using UnityEngine;
using System.Collections;



public class NetUser {
	// dunno
	public NetworkViewID viewID;
	
	// networking
	public EntityClass Entity = null;
	public NetworkPlayer netPlayer;
	public bool local;
	
	// game/match
	public float respawnTime = 0f;
	public bool hasBall = false;
	
	// avatar
	//		settings
	public string name = "";
	public Color colA;
	public Color colB;
	public Color colC;
	// 		model
	public int headType;
	// 		status
	public int currentScore;
	public int lives = 0;
	public float health = 100f;
	public Ping ping;
	public float lastPong = 0f;
	public int team = 0;
	// 			frag tallies
	public int kills;
	public int totalKills;
	public int deaths;
	public int totalDeaths;

	
	
	public void InstantiateEntity(GameObject fpsEntityPrefab) {
		Debug.Log("InstantiateEntity()");
		if (Entity != null) 
			return;
		
		var o = (GameObject)GameObject.Instantiate(fpsEntityPrefab);
		Entity = o.GetComponent<EntityClass>();
		Entity.colA = colA;
		Entity.colB = colB;
		Entity.colC = colC;
		Entity.headType = headType;
		//fpsEntity.viewID = viewID;
		Entity.isLocal = local;
		Entity.User = this;
		Debug.Log("just setup NetUser.Entity");
		
		if (local && lives < 0) {
			currentScore = -99;
			Entity.Spectating = true;
		}
	}
}
