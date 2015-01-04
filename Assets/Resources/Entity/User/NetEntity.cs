using UnityEngine;
using System.Collections;



public class NetEntity {
	// misc 
	public float Health = 100f;
//	public float Health {
//		get {   
//			if (Visuals == null) 
//				return -1f;   
//			else
//				return Visuals.bod.Health;   
//		}
//
//		set {   
//			if (Visuals != null) 
//				Visuals.bod.Health = value;   
//		}
//	}

	// networking 
	public Actor Actor = null;
	public NetworkPlayer netPlayer;
	public NetworkViewID viewID;
	public bool local;
	
	// game/match 
	public float respawnTime = 0f;
	public bool hasBall = false;
	
	// avatar 
	//		settings 
	public string name = "";
	public Color colA; // colours are needed both in here and in EntityVisuals, cuz the latter doesn't always exist, 
	public Color colB; // yet we'll want per-user chat colors, for chatting while dead/spectating 
	public Color colC;
	// 		visuals/model 
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



	public void InstantiateGO(GameObject go) {
		if (Actor == null) {
			Debug.Log("NetEntity.InstantiateGO() --- 'Visuals' was null at this point....all is well");
		}else{
			Debug.Log("'Visuals' NOT null, so....EXITING InstantiateGO() at the top");
			return;
		}
		
		var o = (GameObject)GameObject.Instantiate(go);
		Actor = o.GetComponent<Actor>();
		Actor.colA = colA;
		Actor.colB = colB;
		Actor.colC = colC;
		Actor.headType = headType;
		//Visuals.viewID = viewID;
		Actor.isLocal = local;
		Actor.User = this;

		if (local && lives < 0) {
			currentScore = -99;
			Actor.Spectating = true;
		}
	}
}
