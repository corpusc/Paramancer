using UnityEngine;
using System.Collections;



public class NetUser {
	// misc 
	public float health {
		get {   return Entity.bod.health;   }
		set {   Entity.bod.health = value;   }
	}

	// networking
	public EntityClass Entity = null;
	public NetworkPlayer netPlayer;
	public NetworkViewID viewID;
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
	public Ping ping;
	public float lastPong = 0f;
	public int team = 0;
	// 			frag related
	public GameObject FraggedBy = null;	
	public int kills;
	public int totalKills;
	public int deaths;
	public int totalDeaths;



	public void InstantiateEntity(GameObject entityPrefab) {
		if (Entity == null) {
			Debug.Log("NetUser.InstantiateEntity() --- 'Entity' was null at this point....all is well");
		}else{
			Debug.Log("'Entity' NOT null, so....EXITING InstantiateEntity() at the top");
			return;
		}
		
		var o = (GameObject)GameObject.Instantiate(entityPrefab);
		Entity = o.GetComponent<EntityClass>();
		Entity.colA = colA;
		Entity.colB = colB;
		Entity.colC = colC;
		Entity.headType = headType;
		//fpsEntity.viewID = viewID;
		Entity.isLocal = local;
		Entity.User = this;

		if (local && lives < 0) {
			currentScore = -99;
			Entity.Spectating = true;
		}
	}
}
