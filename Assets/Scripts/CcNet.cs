/*
 * ----marketing
 * 
 * instead of telling you how many features we got, let's talk about
 * all the modern bullshit that we've STRIPPED AWAY FROM a formerly fun genre!
 * 
 * no blind, patriotic gung ho guv progoganda
 * no virtual LARPing
 * no pretensions of being a military pawn
 * no limits on number of guns you can carry
 * no mechanics/aiming penalties for fast & constant motion and jumping
 * no slowing down to hold gun up to your eyeballs
 * no realistic shallow hops.  you can jump/LEAP instead
 * no restricting our weapon dynamics/mechanics/designs to be slavish 
 * 		to real life weapons.  many more types of weapons which give lots of mechanics variety
 * no forced spectating
 * no forced timeout/punishment/respawn-timers....stay in the action!
 * no loadout/selection screens..... pickup everything IN THE GAME, not in menus
 * no forced waiting (for longer than 10 seconds?) between matches
 * no working towards weapon unlocks
 * ....no grinding!   you don't play games to prove your virtual work ethic!

-----roguelike layer
here's an idea that would work nice in single player....and if not being religiously slavish to the original rogue....
	have a town with a shop in an overworld, and when you come to the surface to sell junk to him, have it persist
	between games, so later characters can buy that stuff


 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CcNet : MonoBehaviour {
	// avatar
	
	// networky stuff
	public string Error = "'Error' string init value...NO ERRORS YET!";
	public NetworkViewID NetVI;
	public bool Connected = false;
	public bool isServer = false;
	// if you change the client, change this.
	// should be unique or different types of clients will clash
	public string uniqueGameName = "Y U REFLECT ME!!!!";
	public string gameName = "TestSession";
	public int connections = 32;
	public int listenPort = 25000;
	public string MatchTypeAndMap = ""; // match type & map name (for server browser)
	public string password = "";
	public List<NetUser> players;
	public List<PickupPoint> pickupPoints = new List<PickupPoint>();
	private float nextPingTime = 0f;
	public int team1Score = 0;
	public int team2Score = 0;
	public bool gameOver = false;
	public bool serverGameChange = false;
	public bool broadcastPos = false;
	public bool lastGameWasTeamBased = false;
	private float latestPacket = 0f;
	private float latestServerHeartbeat = 0f;
	//		level
	private bool preppingLevel = false;
	private bool levelLoaded = false;
	
	// game modes/types
	public MatchData CurrMatch;
	private GameObject basketball;

	// personal stuff
	public NetUser localPlayer;
	public bool gunBobbing = true;
	public bool autoPickup = false;
	public float gameVolume = 1f;

	// private
	// scripts
	CcLog log;
	Hud hud;
	Arsenal arse;
	GameObject entityPrefab;
	GameObject pickupBoxPrefab;
	GameObject splatPrefab;
	GameObject basketballPrefab;
	
	
	
	void Start() {
		DontDestroyOnLoad(this);
		//Application.targetFrameRate = 60; // -1 (the default) makes standalone games render as fast as they can, 
		// and web player games to render at 50-60 frames/second depending on the platform.
		// If vsync is set in quality setting, the target framerate is ignored
		
		// scripts
		hud = GetComponent<Hud>();
		log = GetComponent<CcLog>();
		arse = GetComponent<Arsenal>();
		CurrMatch = new MatchData(Match.FFAFragMatch);
		
		// load prefabs
		Object[] prefabs = Resources.LoadAll("Prefab/CcNet");
		foreach (var p in prefabs) {
			switch (p.name) {
				case "Basketball": basketballPrefab = (GameObject)p; break;	
				case "FPSEntity": entityPrefab = (GameObject)p; break;				
				case "PickupBox": pickupBoxPrefab = (GameObject)p; break;	
				case "SplatCube": splatPrefab = (GameObject)p; break;	
			}
			
		}
		
		Application.LoadLevel("MenuMain");
		players = new List<NetUser>();
	}

	public BasketballScript GetBball() {
		return basketball.GetComponent<BasketballScript>();
	}

	//-------- Network gameplay stuff ----------
	public void SendTINYUserUpdate(NetworkViewID viewID, UserAction action) {
		networkView.RPC("SendTINYUserUpdateRPC", RPCMode.Others, viewID, (int)action);
	}
	[RPC]
	void SendTINYUserUpdateRPC(NetworkViewID viewID, int action) {
		latestPacket = Time.time;
		
		for (int i=0; i<players.Count; i++) {
			if (viewID == players[i].viewID && players[i].Entity != null) {
				players[i].lastPong = Time.time;
				players[i].Entity.PlaySound((UserAction)action);
			}
		}
	}
	
	public void SendUserUpdate(NetworkViewID viewID, Vector3 pos, Vector3 ang, bool crouch, Vector3 moveVec, float yMove, 
		int gunA, int gunB, Vector3 playerUp, Vector3 playerForward
	) {
		// send out a player's current properties to everyone, so they know where we are
		networkView.RPC("SendUserUpdateRPC", RPCMode.Others, viewID, pos, ang, crouch, moveVec, yMove, gunA, gunB, playerUp, playerForward);
	}
	[RPC]
	void SendUserUpdateRPC(NetworkViewID viewID, Vector3 pos, Vector3 ang, bool crouch, Vector3 moveVec, float yMove, 
		int gunA, int gunB, Vector3 playerUp, Vector3 playerForward, NetworkMessageInfo info
	) {
		// received a player's properties, let's update the local view of that player
		latestPacket = Time.time;
		
		for (int i=0; i<players.Count; i++) {
			if (viewID == players[i].viewID && players[i].Entity != null) {
				players[i].lastPong = Time.time;
				players[i].Entity.UpdatePlayer(pos, ang, crouch, moveVec, yMove, 
					info.timestamp, (Item)gunA, (Item)gunB, playerUp, playerForward);
			}
		}
	}
	
	public void ConsumeHealth(NetworkViewID viewID) {
		// we just used a health pack, tell the server our health is maxed out
		if (!isServer) {
			networkView.RPC("ConsumeHealthRPC", RPCMode.Server, viewID);
		}else{
			ConsumeHealthRPC(viewID);
		}
	}
	[RPC]
	void ConsumeHealthRPC(NetworkViewID viewID) {
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) {
				players[i].health = 100f;
			}
		}
		if (localPlayer.viewID == viewID) 
			localPlayer.health = 100f;
	}
	
	public void Detonate(Item weapon, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID) {
		// we are server and something detonated, tell everyone
		networkView.RPC("DetonateRPC", RPCMode.All, (int)weapon, position, shooterID, bulletID);
	}
	[RPC]
	void DetonateRPC(int weapon, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID) {
		latestPacket = Time.time;
		arse.Detonate((Item)weapon, position, bulletID);
		
		if (isServer) {
			// see if anyone gets hurt
			for (int i=0; i<players.Count; i++){
				if (Vector3.Distance(position, players[i].Entity.transform.position) 
					< arse.GetDetonationRadius((Item)weapon) + 0.5f
				) {
					// player in range
					bool skip = false;
					// ignore if on the same team as the person who fired (unless bomb)
					if (CurrMatch.teamBased && !CurrMatch.allowFriendlyFire) {
						int shooterIndex = -1;
						for (int k=0; k<players.Count; k++){
							if (players[k].viewID == shooterID) 
								shooterIndex = k;
						}
						
						if (shooterIndex != -1 && players[i].team == players[shooterIndex].team) 
							skip = true;
						if (shooterIndex != -1 && i == shooterIndex && (Item)weapon == Item.Bomb) 
							skip = false;
					}
					
					
					if (players[i].health > 0f && !skip) {
						RegisterHitRPC(weapon, shooterID, players[i].viewID, players[i].Entity.transform.position);
					}
				}
			}
		}
	}
	
	public void Shoot(Item weapon, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, bool hit, bool sprint = false) {
		// we have fired a shot, let's tell everyone about it so they can see it
		NetworkViewID bulletID = Network.AllocateViewID();
		networkView.RPC("ShootRPC", RPCMode.All, (int)weapon, origin, direction, end, shooterID, bulletID, hit, sprint);
	}
	[RPC]
	void ShootRPC(int weapon, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, NetworkViewID bulletID, bool hit, bool sprint, NetworkMessageInfo info) {
		// somebody fired a shot, let's show it
		latestPacket = Time.time;
		arse.Shoot((Item)weapon, origin, direction, end, shooterID, bulletID, info.timestamp, hit, sprint);
	}
	
	public void RegisterHit(Item weapon, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos) {
		if (gameOver) 
			return;
		
		// we hit somebody, tell the server!
		if (!isServer) {
			networkView.RPC("RegisterHitRPC", RPCMode.Server, (int)weapon, shooterID, victimID, hitPos);
		}else{
			RegisterHitRPC((int)weapon, shooterID, victimID, hitPos);
		}
	}
	[RPC]
	public void RegisterHitRPC(int weapon, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos) {
		// one player hit another
		latestPacket = Time.time;
		
		if (gameOver) 
			return; // no damage after game over
		
		int si = -1; // shooter index
		int vi = -1; // victim index
		bool killShot = false;
		
		//Debug.Log("hit registered");
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == shooterID) si = i;
			if (players[i].viewID == victimID) vi = i;
		}
		
		if (si == -1 || vi == -1 || players[vi].health <= 0f) 
			return;
		
		if ((Item)weapon == Item.Swapper){
			networkView.RPC("SwapPlayers",RPCMode.All, shooterID, players[si].Entity.transform.position, victimID, players[vi].Entity.transform.position);
			return;
		}
		
		
		// subtract health
		if (si == vi && (Item)weapon == Item.RocketMaybeJustASingle) {
			// rocket jumping
			players[vi].health -= 30f;
		}else{
			// normal damage
			players[vi].health -= arse.GetWeaponDamage((Item)weapon);
		}
		
		if (players[vi].health <= 0f) {
			//player died
			players[vi].health = 0f;
			killShot = true;
		}
		
		// scores
		if (killShot) {
			players[vi].deaths++;
			
			if (CurrMatch.deathsSubtractScore) 
				players[vi].currentScore--;
			
			players[si].kills++;
			
			if (CurrMatch.killsIncreaseScore) 
				players[si].currentScore++;
		}
		
		// team stuff
		if (killShot && CurrMatch.teamBased) {
			if (players[vi].team == 1 && players[si].team == 2 && CurrMatch.deathsSubtractScore) team1Score--;
			if (players[vi].team == 2 && players[si].team == 1 && CurrMatch.deathsSubtractScore) team2Score--;
			if (players[si].team == 1 && players[vi].team == 2 && CurrMatch.killsIncreaseScore) team1Score++;
			if (players[si].team == 2 && players[vi].team == 1 && CurrMatch.killsIncreaseScore) team2Score++;
		}
		
		// assign results
		networkView.RPC("AssignPlayerStats", RPCMode.All, victimID, players[vi].health, players[vi].kills, players[vi].deaths, players[vi].currentScore);
		networkView.RPC("AssignPlayerStats", RPCMode.All, shooterID, players[si].health, players[si].kills, players[si].deaths, players[si].currentScore);

		if (killShot) {
			networkView.RPC("AnnounceKill", RPCMode.All, weapon, shooterID, victimID);
			
			if (CurrMatch.teamBased) 
				networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		}
		
		// let players see hit
		networkView.RPC("ShowHit", RPCMode.All, weapon, hitPos, victimID);
		
		// check for game over
		if (CurrMatch.winScore > 0) {
			for (int i=0; i<players.Count; i++) {
				if (players[i].currentScore>=CurrMatch.winScore) {
					networkView.RPC("AnnounceGameOver", RPCMode.All);
				}
			}
		}
	}
	
	[RPC]
	void SwapPlayers(NetworkViewID shooterID, Vector3 shooterPos, NetworkViewID victimID, Vector3 victimPos) {
		for (int i=0; i<players.Count; i++) {
			if (players[i].local) {
				if (players[i].viewID == shooterID) {
					players[i].Entity.transform.position = victimPos;
					players[i].Entity.sendRPCUpdate = true;
					players[i].Entity.PlaySound("Swapped");
					players[i].Entity.ForceLook(shooterPos);
				}else if(players[i].viewID == victimID) {
					players[i].Entity.transform.position = shooterPos;
					players[i].Entity.sendRPCUpdate = true;
					players[i].Entity.PlaySound("Swapped");
					players[i].Entity.ForceLook(victimPos);
				}
			}
		}
	}
	
	[RPC]
	void ShowHit(int weapon, Vector3 hitPos, NetworkViewID viewID) {
		int numGibs = 4;
		
		switch ((Item)weapon) {
			case Item.Grenade:                numGibs = 15; break;
			case Item.MachineGun:             numGibs = 2; break;
			case Item.Rifle:                  numGibs = 30; break;
			case Item.RocketMaybeJustASingle: numGibs = 20; break;
			case Item.Bomb:                   numGibs = 20; break;
			
			case Item.Suicide:                numGibs = 30; break;
		}
		numGibs *= 4;
		
		for (int i=0; i<numGibs; i++) {
			GameObject newSplat = (GameObject)GameObject.Instantiate(splatPrefab);
			newSplat.transform.position = hitPos;
		}
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID && 
				players[i].local && 
				players[i].health > 0f
			) 
				players[i].Entity.PlaySound("takeHit");
		}
	}
	
	[RPC]
	void AnnounceGameOver() {
		gameTimeLeft = 0f;
		gameOver = true;
		NextMatchTime = 15f;
	}
	
	[RPC]
	void AnnounceTeamScores(int score1, int score2) {
		latestPacket = Time.time;
		team1Score = score1;
		team2Score = score2;
	}
	
	[RPC]
	void SharePlayerScores(NetworkViewID viewID, int kills, int deaths, int currentScore) {
		latestPacket = Time.time;
		for (int i=0; i<players.Count; i++){
			if (players[i].viewID==viewID){
				players[i].kills = kills;
				players[i].deaths = deaths;
				players[i].currentScore = currentScore;
			}
		}
	}
	
	[RPC]
	void AssignPlayerStats(NetworkViewID viewID, float health, int kills, int deaths, int score) {
		latestPacket = Time.time;
		if (localPlayer.viewID == viewID) {
			localPlayer.health = health;
			localPlayer.kills = kills;
			localPlayer.deaths = deaths;
			localPlayer.currentScore = score;
		}
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) {
				players[i].health = health;
				players[i].kills = kills;
				players[i].deaths = deaths;
				players[i].currentScore = score;
			}
		}
	}
	
	[RPC]
	void AnnounceKill(int weapon, NetworkViewID shooterID, NetworkViewID victimID) {
		latestPacket = Time.time;
		
		if (localPlayer.viewID == shooterID) 
			localPlayer.totalKills ++;
		if (localPlayer.viewID == victimID) 
			localPlayer.totalDeaths ++;
		
		if (localPlayer.viewID == victimID) {
			for (int i=0; i<players.Count; i++) {
				if (players[i].viewID == shooterID) {
					for (int k=0; k<players.Count; k++) {
						if (players[k].viewID == localPlayer.viewID)
							players[k].Entity.ourKiller = players[i].Entity.gameObject;
					}
				}
			}
		}
		
		string shooterName = "Someone";
		string victimName = "Someone";
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == shooterID) shooterName = players[i].name;
			if (players[i].viewID == victimID) victimName = players[i].name;
			if (players[i].viewID == victimID) players[i].Entity.PlaySound("die");
			
			// lives
			if (players[i].viewID == victimID) {
				players[i].lives--;
				if (players[i].lives <= 0 && 
					players[i].local && 
					CurrMatch.playerLives > 0)
				{
					// spectate
					players[i].Entity.Spectating = true;
				}
			}
			
			// basketball
			if (players[i].viewID == victimID) {
				if (CurrMatch.basketball && players[i].hasBall) {
					players[i].hasBall = false;
					if (isServer)
						ThrowBall(players[i].Entity.transform.position, -Vector3.up, 2f);
				}
			}
		}
		
		// lives
		if (isServer && 
			Connected && 
			!gameOver && 
			CurrMatch.playerLives > 0 && 
			players.Count > 1)
		{
			int livingPlayers = 0;
			for (int i=0; i<players.Count; i++) {
				if (players[i].lives > 0) 
					livingPlayers++;
			}
			
			if (livingPlayers <= 1) {
				gameTimeLeft = 0f;
				gameOver = true;
				NextMatchTime = 15f;
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}
		}
		
		
		var le = new LogEntry();
		le.Maker = "";
		le.Color = Color.red;
		le.Text = getObituary(shooterName, victimName);
		log.Entries.Add(le);
		log.DisplayTime = Time.time+log.FadeTime;
		
	}
	
	private string getObituary(string f, string v) { // fragger, victim
		// suicides
		if (f == v) { // fixme.... cuz if 2 players have the same name, it will read like a suicide when it wasn't
			switch (Random.Range(0, 5)) {
				case 0:	return f + " bought the farm!";
				case 1:	return f + " changed career... to Daisy Pusher!";
				case 2:	return f + " really bit the dust!";
				case 3:	return f + " really shot themself in the foot!";
				case 4:	return f + " did some nice kamikaze work!";
				default: return "....";
			}
		}
		
		// normal frags
		switch (Random.Range(0, 5)) {
			case 0:	return f + " really gave " + v + " what for!";
			case 1:	return f + " fixed " + v + "'s little red wagon!";
			case 2:	return f + " messed up " + v + " real bad!";
			case 3:	return f + " sent " + v + " to a better place!";
			case 4:	return f + " released " + v + "'s spirit from these mortal chains!";
			default: return "....";
		}
	}
	
	[RPC]
	void RespawnPlayer(NetworkViewID viewID) {
		latestPacket = Time.time;
		
		if (viewID == localPlayer.viewID) {
			for (int i=0; i<players.Count; i++) {
				if (players[i].viewID == viewID) {
					if ((CurrMatch.playerLives > 0 && 
						players[i].lives > 0) ||
						CurrMatch.playerLives == 0
					) {
						players[i].Entity.Respawn();
					}
				}
			}
		}
	}
	
	[RPC]
	void PONG(NetworkViewID viewID) {
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) 
				players[i].lastPong = Time.time;
		}
	}
	
	void Update() {
		// ping periodically
		if (Connected && Time.time > nextPingTime) {
			nextPingTime = Time.time + 5f;
			for (int i=0; i<players.Count; i++) {
				if (!players[i].local) {
					new Ping(players[i].netPlayer.ipAddress);
				}
			}
		}
		
		// let players know they are still connected
		if (Connected && isServer) {
			if (Time.time > latestServerHeartbeat + 9f) {
				latestServerHeartbeat = Time.time + 9f;
				networkView.RPC("HeartbeatFromServer", RPCMode.All);
			}
		}
		
		// are we still connected to the server?
		if (Connected && !isServer) {
			if (Time.time > latestPacket + 30f) {
				DisconnectNow();
				hud.Mode = HudMode.ConnectionError;
				Error = "Client hasn't heard from host for 30 seconds.\n" +
					"Probably because someone's connection sucks.";
			}
			
			// remind the server we here
			for (int i=0; i<players.Count; i++) {
				if (players[i].local && Time.time > players[i].lastPong + 5f) {
					networkView.RPC("PONG", RPCMode.Server,players[i].viewID);
					players[i].lastPong = Time.time;
				}	
			}
		}
		
		if (Connected && isServer) {
			for (int i=0; i<players.Count; i++) {
				if (!players[i].local) {
					if (Time.time>players[i].lastPong + 20f) {
						//gone 20 secs without hearing from client, auto-kick
						Kick(i,true);
					}
				}
			}
		}
		
		// respawn dead players
		if (isServer) {
			for (int i=0; i<players.Count; i++) {
				if (players[i].health <= 0f){
					if (Time.time > players[i].respawnTime) {
						if (CurrMatch.playerLives == 0 || players[i].lives > 0)
							players[i].health = 100f;
						
						networkView.RPC("AssignPlayerStats", RPCMode.All, players[i].viewID, players[i].health, players[i].kills, players[i].deaths, players[i].currentScore);
						networkView.RPC("RespawnPlayer", RPCMode.All, players[i].viewID);
					}
				}else{
					players[i].respawnTime = Time.time + CurrMatch.respawnWait;
				}
			}
		}
		
		// change team
		if (Connected && CurrMatch.teamBased) {
			if (CcInput.Started(UserAction.SwapTeam)) {
				if (localPlayer.team == 1) {
					localPlayer.team = 2;
				}else{
					localPlayer.team = 1;
				}
				
				networkView.RPC("PlayerChangedTeams",RPCMode.AllBuffered, localPlayer.viewID, localPlayer.team);
				
				for (int i=0; i<players.Count; i++) {
					if (players[i].viewID == localPlayer.viewID && players[i].health > 0f) 
						players[i].Entity.Respawn();
				}
			}
		}
		
		// count time!
		gameTimeLeft -= Time.deltaTime;
		
		// game time up?
		if (Connected && !gameOver) {
			if (gameTimeLeft <= 0f && CurrMatch.Duration > 0f){
				gameTimeLeft = 0f;
				gameOver = true;
				
				NextMatchTime = 15f;
			}
		}
		
		// if game over, count in next match
		if (Connected && gameOver) {
			NextMatchTime -= Time.deltaTime;
			if (NextMatchTime <= 0f){
				NextMatchTime = 0f;
				
				if (isServer) {
					//begin next match using current settings
					serverGameChange = true;
					lastGameWasTeamBased = CurrMatch.teamBased;
					NetVI = Network.AllocateViewID();
					RequestGameData();
				}
			}
		}
		
		// pickups
		if (Connected && isServer && !gameOver) {
			for (int i=0; i<pickupPoints.Count; i++) {
				if (!pickupPoints[i].stocked) {
					pickupPoints[i].RestockTime -= Time.deltaTime;
					if (pickupPoints[i].RestockTime <= 0f) {
						Item item = Item.None;
						if (pickupPoints[i].pickupType == 1) item = CurrMatch.pickupSlot1;
						if (pickupPoints[i].pickupType == 2) item = CurrMatch.pickupSlot2;
						if (pickupPoints[i].pickupType == 3) item = CurrMatch.pickupSlot3;
						if (pickupPoints[i].pickupType == 4) item = CurrMatch.pickupSlot4;
						if (pickupPoints[i].pickupType == 5) item = CurrMatch.pickupSlot5;
						if (item == Item.Random) {
							item = (Item)Random.Range(-1, arse.Guns.Length);
							if (item == Item.None) 
								item--;
						}
						
						if (item != Item.None) {
							networkView.RPC("RestockPickup", RPCMode.All, pickupPoints[i].pickupPointID, (int)item);
						}
					}
				}
			}
		}
	}
	
	
	
	[RPC]
	void RestockPickup(int pointID, int item) {
		for (int i=0; i<pickupPoints.Count; i++) {
			if (pickupPoints[i].pickupPointID == pointID) {
				if (pickupPoints[i].stocked) 
					return;
				
				pickupPoints[i].stocked = true;

				// health will be a box with its 'Health' pic on it
				var o = pickupBoxPrefab;
				if (item >= (int)Item.Pistol) 
					o = arse.Guns[item].Prefab;

				var n = (GameObject)GameObject.Instantiate(o);
				pickupPoints[i].currentAvailablePickup = n;
				n.transform.position = pickupPoints[i].transform.position;
				n.transform.localScale = Vector3.one * 0.5f;

				PickupBoxScript box;
				if (item >= (int)Item.Pistol) {
					box = n.AddComponent<PickupBoxScript>();
					box.boxObj = n;
				}else{
					box = n.GetComponent<PickupBoxScript>();
				}
				box.pickupPoint = pickupPoints[i];

				if (item < (int)Item.Pistol) {
					// health
					box.pickupName = "Health";
					box.iconObj.renderer.material.SetTexture("_MainTex", Pics.Health);
					Material[] mats = box.boxObj.renderer.materials;
					mats[0].color = Color.green;
					box.boxObj.renderer.materials = mats;
				}else{
					// gun of some type
					box.pickupName = arse.Guns[item].Name;
					//box.iconObj.renderer.material.SetTexture("_MainTex", arse.Guns[item].Pic);
					//Material[] mats = box.boxObj.renderer.materials;
					//mats[0] = arse.Guns[item].Mat;
					//box.boxObj.renderer.materials = mats;
				}
			}
		}
	}
	
	public void UnstockPickupPoint(PickupPoint point){
		for (int i=0; i<pickupPoints.Count; i++) {
			if (point == pickupPoints[i]) {
				networkView.RPC("UnstockRPC", RPCMode.All, pickupPoints[i].pickupPointID);
			}
		}
	}
	[RPC]
	void UnstockRPC(int pointID){
		for (int i=0; i<pickupPoints.Count; i++) {
			if (pointID == pickupPoints[i].pickupPointID) {
				pickupPoints[i].stocked = false;
				
				if (pickupPoints[i].currentAvailablePickup != null) 
					Destroy(pickupPoints[i].currentAvailablePickup);
				
				if (isServer) 
					pickupPoints[i].RestockTime = CurrMatch.restockTime;
			}
		}
	}
	
	[RPC]
	void RequestPickupStocks() {
		// a client has requested the current pickup stock info
		for (int i=0; i<pickupPoints.Count; i++) {
			if (pickupPoints[i].stocked) {
				Item item = Item.None;
				if (pickupPoints[i].pickupType == 1) item = CurrMatch.pickupSlot1;
				if (pickupPoints[i].pickupType == 2) item = CurrMatch.pickupSlot2;
				if (pickupPoints[i].pickupType == 3) item = CurrMatch.pickupSlot3;
				if (pickupPoints[i].pickupType == 4) item = CurrMatch.pickupSlot4;
				if (pickupPoints[i].pickupType == 5) item = CurrMatch.pickupSlot5;
				
				if (item == Item.Random) {
					item = (Item)Random.Range(-1, arse.Guns.Length);
					
					if (item == Item.None) 
						item--;
				}
				
				if (item != Item.None) {
					networkView.RPC("RestockPickup", RPCMode.All, pickupPoints[i].pickupPointID, (int)item);
				}
				
			}
		}
	}
	
	public void AnnounceBallCapture(NetworkViewID viewID) {
		networkView.RPC("AnnounceBallCaptureRPC", RPCMode.All, viewID);
	}
	
	[RPC]
	void AnnounceBallCaptureRPC(NetworkViewID viewID) {
		basketball.GetComponent<BasketballScript>().HoldBall(viewID);
	}
	
	public void ThrowBall(Vector3 fromPos, Vector3 direction, float strength) {
		networkView.RPC("ThrowBallRPC", RPCMode.All, fromPos, direction, strength);
	}
	
	[RPC]
	void ThrowBallRPC(Vector3 fromPos, Vector3 direction, float strength) {
		basketball.GetComponent<BasketballScript>().Throw(fromPos,direction,strength);
	}
	
	[RPC]
	void HeartbeatFromServer() {
		latestPacket = Time.time;
	}
	
	[RPC]
	void PlayerChangedTeams(NetworkViewID viewID, int team) {
		latestPacket = Time.time;
		for (int i=0; i<players.Count; i++) {
			//always set model visibility on team change, that way if *you* change teams, all lights are changed
			players[i].Entity.SetModelVisibility(!players[i].local);
			
			if (viewID == players[i].viewID) {
				players[i].team = team;
				//players[i].Entity.SetModelVisibility(!players[i].local);
				
				LogEntry l = new LogEntry();
				l.Maker = "";
				l.Text = "";
				
				if (team == 1) {
					if (localPlayer.viewID == viewID){
						l.Color = Color.red;
						l.Text = "<< you defected! >>";
					}else if (localPlayer.team == 1) {
						l.Color = Color.red;
						l.Text = "<< " + players[i].name + " defected to your team! >>";
					}else{
						l.Color = Color.cyan;
						l.Text = "<< " + players[i].name + " turned their back on the team! >>";
					}
				}else{
					if (localPlayer.viewID == viewID) {
						l.Color = Color.cyan;
						l.Text = "<< you defected! >>";
					}else if (localPlayer.team == 2) {
						l.Color = Color.cyan;
						l.Text = "<< " + players[i].name + " defected to your team! >>";
					}else{
						l.Color = Color.red;
						l.Text = "<< " + players[i].name + " turned their back on the team! >>";
					}
				}
				
				log.Entries.Add(l);
				log.DisplayTime = Time.time + log.FadeTime;
			}
		}
	}
	
	//-------- player joining stuff ----------
	[RPC]
	void NewPlayer(NetworkViewID viewID, string name, Vector3 cA, Vector3 cB, Vector3 cC, int head, 
		NetworkPlayer np, int targetTeam, int lives
	) {
		latestPacket = Time.time;
		
		if (players.Count == 1 && CurrMatch.playerLives > 0) {
			if (isServer && Connected && !gameOver) {
				// this is a lives match, and now we have enough players
				gameTimeLeft = 0f;
				gameOver = true;
				NextMatchTime = 5f; // CHANGE ME
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}	
		}
		
		// this could just be a team change update at the start of the level
		bool weExist = false;
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) {
				weExist = true;
				players[i].team = targetTeam;
				players[i].lives = lives;
				players[i].health = 100f;
			}
		}
		
		if (weExist) 
			return;
		
		// another player has joined, lets add them to our view of the game
		bool localShopforLocalPeople = false;
		if (viewID == localPlayer.viewID) 
			localShopforLocalPeople = true;
		
		AddPlayer(localShopforLocalPeople, viewID, VecToCol(cA), VecToCol(cB), VecToCol(cC), head, name, np, targetTeam, lives);
		
		if (levelLoaded) {
			// only instantiate the actual object of the player if we are in the right level
			// uninstantiated players are added when the level finished loading
			players[players.Count-1].InstantiateEntity(entityPrefab);
		}
		
		
		// tell local Entity to broadcast position so new players know
		broadcastPos = true;
		// also let new players know the scores
		if (isServer){
			for (int i=0; i<players.Count; i++) {
				networkView.RPC("SharePlayerScores", RPCMode.Others, players[i].viewID, players[i].kills, players[i].deaths, 
					players[i].currentScore);
			}
			
			networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		}
		
		if (levelLoaded){
			LogEntry newMsg = new LogEntry();
			newMsg.Maker = "";
			newMsg.Color = Color.grey;
			newMsg.Text = "<< " + name + " has joined >>";
			log.Entries.Add(newMsg);
			log.DisplayTime = Time.time+log.FadeTime;
		}
	}
	
	void AddPlayer(bool isLocal, NetworkViewID anID, Color cA, Color cB, Color cC, int head, string name, 
		NetworkPlayer np, int targetTeam, int lives
	) {
		var u = new NetUser();
		u.colA = cA;
		u.colB = cB;
		u.colC = cC;
		u.headType = head;
		u.viewID = anID;
		u.local = isLocal;
		u.name = name;
		u.netPlayer = np;
		u.ping = new Ping(u.netPlayer.ipAddress);
		u.team = targetTeam;
		u.kills = 0;
		u.deaths = 0;
		u.currentScore = 0;
		u.lives = lives;
		u.lastPong = Time.time;
		players.Add(u);
	}
	
	public void AssignGameModeConfig(MatchData md, string levelName) {
		CurrMatch.levelName = levelName;
		
		CurrMatch.Name = md.Name;
		CurrMatch.Descript = md.Descript;
		CurrMatch.winScore = md.winScore;
		CurrMatch.Duration = md.Duration;
		CurrMatch.respawnWait = md.respawnWait;
		CurrMatch.deathsSubtractScore = md.deathsSubtractScore;
		CurrMatch.killsIncreaseScore = md.killsIncreaseScore;
		CurrMatch.teamBased = md.teamBased;
		CurrMatch.allowFriendlyFire = md.allowFriendlyFire;
		CurrMatch.pitchBlack = md.pitchBlack;
		CurrMatch.restockTime = md.restockTime;
		CurrMatch.playerLives = md.playerLives;
		CurrMatch.basketball = md.basketball;
		CurrMatch.spawnGunA = md.spawnGunA;
		CurrMatch.spawnGunB = md.spawnGunB;
		CurrMatch.pickupSlot1 = md.pickupSlot1;
		CurrMatch.pickupSlot2 = md.pickupSlot2;
		CurrMatch.pickupSlot3 = md.pickupSlot3;
		CurrMatch.pickupSlot4 = md.pickupSlot4;
		CurrMatch.pickupSlot5 = md.pickupSlot5;
	}
	
	[RPC]
	public void RequestGameData() {
		//a player has requested game data, pass it out.
		latestPacket = Time.time;
		
		//also figure out which team to drop them in
		int team1count = 0;
		int team2count = 0;
		for (int i=0; i<players.Count; i++) {
			if (players[i].team == 1) 
				team1count++;
			if (players[i].team == 2) 
				team2count++;
		}
		
		int targetTeam = 0;
		if (CurrMatch.teamBased) 
			targetTeam = 1;
		if (CurrMatch.teamBased && team1count>team2count) 
			targetTeam = 2;
		if (CurrMatch.teamBased && team2count>team1count) 
			targetTeam = 1;
		
		//keep teams if we are already assigned to teams
		if (serverGameChange && players.Count > 0) {
			if (lastGameWasTeamBased) {
				targetTeam = -1;
			}else{
				targetTeam = -2;
			}
		}
		lastGameWasTeamBased = false;
		
		int livesBroadcast = 0;
		if (serverGameChange) {
			gameTimeLeft = CurrMatch.Duration * 60f;
			gameOver = false;
			livesBroadcast = CurrMatch.playerLives;
		}else{
			if (CurrMatch.playerLives > 0) {
				livesBroadcast = -1;
			}
		}
		
		networkView.RPC("BroadcastNewGame", RPCMode.All, NetVI, 
			CurrMatch.Name, CurrMatch.levelName, CurrMatch.Descript, CurrMatch.winScore, CurrMatch.Duration, 
			CurrMatch.respawnWait, CurrMatch.deathsSubtractScore, CurrMatch.killsIncreaseScore, CurrMatch.teamBased, 
			targetTeam, CurrMatch.allowFriendlyFire, CurrMatch.pitchBlack, gameOver, gameTimeLeft, 
			(int)CurrMatch.spawnGunA, (int)CurrMatch.spawnGunB, (int)CurrMatch.pickupSlot1, (int)CurrMatch.pickupSlot2, 
			(int)CurrMatch.pickupSlot3, (int)CurrMatch.pickupSlot4, (int)CurrMatch.pickupSlot5, livesBroadcast, serverGameChange, 
			CurrMatch.basketball);
	}
	
	public float gameTimeLeft = 0f;
	public float NextMatchTime = 0f;
	
	[RPC]
	void BroadcastNewGame(NetworkViewID viewID, string matchName, string levelName, string matchDescript, int winScore, 
		float duration, float respawnWait, bool deathsSubtractScore, bool killsIncreaseScore, bool teamBased, 
		int targetTeam, bool allowFriendlyFire, bool pitchBlack, bool gameIsOver, float serverGameTime, int spawnGunA, 
		int spawnGunB, int pickupSlot1, int pickupSlot2, int pickupSlot3, int pickupSlot4, int pickupSlot5, 
		int playerLives, bool newGame, bool basketball, NetworkMessageInfo info
	) {
		// we've received game info from the server
		latestPacket = Time.time;
		
		if (NetVI != null && 
			NetVI == viewID && 
			!serverGameChange) 
			return;
		
		if (!isServer) 
			gameTimeLeft = serverGameTime - (float)(Network.time - info.timestamp);
		
		// make sure all connected players have lives
		if (newGame)
			for (int i=0; i<players.Count; i++)
				players[i].lives = playerLives;
		
		if (!isServer) 
			gameOver = gameIsOver;
		
		NextMatchTime = 15f;
		
		// if we get to this point, it's a new game, so let's get it together!
		NetVI = viewID;
		serverGameChange = false;
		
		if (!isServer) {
			// lets update the local game settings
			CurrMatch.levelName = levelName;
			CurrMatch.Name = matchName;
			CurrMatch.Descript = matchDescript;
			CurrMatch.winScore = winScore;
			CurrMatch.Duration = duration;
			CurrMatch.respawnWait = respawnWait;
			CurrMatch.deathsSubtractScore = deathsSubtractScore;
			CurrMatch.killsIncreaseScore = killsIncreaseScore;
			CurrMatch.teamBased = teamBased;
			CurrMatch.allowFriendlyFire = allowFriendlyFire;
			CurrMatch.pitchBlack = pitchBlack;
			CurrMatch.playerLives = playerLives;
			CurrMatch.basketball = basketball;
			CurrMatch.spawnGunA = (Item)spawnGunA;
			CurrMatch.spawnGunB = (Item)spawnGunB;
			CurrMatch.pickupSlot1 = (Item)pickupSlot1;
			CurrMatch.pickupSlot2 = (Item)pickupSlot2;
			CurrMatch.pickupSlot3 = (Item)pickupSlot3;
			CurrMatch.pickupSlot4 = (Item)pickupSlot4;
			CurrMatch.pickupSlot5 = (Item)pickupSlot5;
		}
		
		if (targetTeam != -1) {
			// don't keep current teams
			localPlayer.team = targetTeam;
			for (int i=0; i<players.Count; i++){
				if (players[i].viewID == localPlayer.viewID){
					players[i].team = targetTeam;
				}
			}
		}
		if (targetTeam == -1) {
			// last game was team based, leave teams as they are
		}
		if (targetTeam == -2) {
			// last game wasn't team based, this is, set team.
			localPlayer.team = 1;
			if (Random.Range(0,10) < 5) 
				localPlayer.team = 2;
			
			for (int i=0; i<players.Count; i++) {
				if (players[i].viewID == localPlayer.viewID) {
					players[i].team = localPlayer.team;
				}
			}
		}
		
		team1Score = 0;
		team2Score = 0;
		
		// let's clear stuff out if we are already playing
		preppingLevel = false;
		levelLoaded = false;
		for (int i=0; i<players.Count; i++) {
			if (players[i].Entity != null) Destroy(players[i].Entity.gameObject);
			players[i].Entity = null;
			
			players[i].health = 100;
			players[i].kills = 0;
			players[i].deaths = 0;
			players[i].currentScore = 0;
		}
		
		localPlayer.health = 100;
		localPlayer.kills = 0;
		localPlayer.deaths = 0;
		localPlayer.currentScore = 0;
		arse.Clear();
		
		// now let's load the level
		preppingLevel = true;
		Application.LoadLevel(levelName);
	}
	
	void OnLevelWasLoaded() {
		if (preppingLevel) {
			// level set up, let's play!
			preppingLevel = false;
			levelLoaded = true;
			
			// drop the basket ball in
			if (CurrMatch.basketball) {
				basketball = (GameObject)GameObject.Instantiate(basketballPrefab);
				
				if (!isServer) 
					networkView.RPC("RequestBallStatus", RPCMode.Server);
			}else{
				if (GameObject.Find("_BasketRed") != null) 
					Destroy(GameObject.Find("_BasketRed"));
				if (GameObject.Find("_BasketBlue") != null) 
					Destroy(GameObject.Find("_BasketBlue"));
			}
			
			// add fps entities for all known players
			for (int i=0; i<players.Count; i++) {
				players[i].InstantiateEntity(entityPrefab);
			}
			
			// tell everyone we're here
			networkView.RPC("NewPlayer", RPCMode.AllBuffered, localPlayer.viewID, localPlayer.name, 
				ColToVec(localPlayer.colA), ColToVec(localPlayer.colB), ColToVec(localPlayer.colC), 
				localPlayer.headType, Network.player, localPlayer.team, CurrMatch.playerLives);
			
			// make sure we know about pickup spawn points
			pickupPoints = new List<PickupPoint>();
			GameObject p = GameObject.Find("_PickupSpots");
			if (p != null) {
				string s = "items: ";
				foreach (Transform child in p.transform) {
					Item item = Item.None;
					PickupPoint pp = child.GetComponent<PickupPoint>();
					if (pp.pickupType == 1) item = CurrMatch.pickupSlot1;
					if (pp.pickupType == 2) item = CurrMatch.pickupSlot2;
					if (pp.pickupType == 3) item = CurrMatch.pickupSlot3;
					if (pp.pickupType == 4) item = CurrMatch.pickupSlot4;
					if (pp.pickupType == 5) item = CurrMatch.pickupSlot5;
					
					s += item + ", ";
					
					if (item != Item.None) {
						pickupPoints.Add(pp);
					}else{
						Destroy(child.gameObject);
					}
				}
				Debug.Log(s);
			}
			
			networkView.RPC("RequestPickupStocks", RPCMode.Server);
			hud.Mode = HudMode.Playing;
		}
	}
	
	[RPC]
	void RequestBallStatus() {
		// player has joined and doesn't yet know the status of the basketball, lets share it
		var bballScript = basketball.GetComponent<BasketballScript>();
		networkView.RPC("ShareBallStatus",RPCMode.Others, basketball.transform.position, bballScript.moveVector, bballScript.throwerID, bballScript.held);
	}
	
	[RPC]
	void ShareBallStatus(Vector3 ballPos, Vector3 ballMovement, NetworkViewID ballThrower, bool ballHeld) {
		var bballScript = basketball.GetComponent<BasketballScript>();
		bballScript.throwerID = ballThrower;
		
		if (ballHeld) {
			bballScript.HoldBall(ballThrower);
		}else{
			bballScript.Throw(ballPos, ballMovement.normalized, ballMovement.magnitude);
		}
	}
	
	//------------- Connecting/Server setup --------------
	void OnServerInitialized() {
        Debug.Log("Server initialized, now registering...");
		MasterServer.RegisterHost(uniqueGameName, gameName, MatchTypeAndMap);
    }
	
    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.RegistrationSucceeded) {
            Debug.Log("Server registered");
			isServer = true;
			
			if (!Connected) {
				// we've just joined a game as host, lets create the local player & add it to the RPC buffer
				localPlayer.viewID = Network.AllocateViewID();
				hud.Mode = HudMode.Wait;
				NetVI = Network.AllocateViewID();
				RequestGameData();
			}
			
			Connected = true;
		}else if (msEvent == MasterServerEvent.RegistrationFailedNoServer || 
		          msEvent == MasterServerEvent.RegistrationFailedGameType || 
		          msEvent == MasterServerEvent.RegistrationFailedGameName)
		{
			Debug.Log("server registration failed, disconnecting");
			Error = "server registration failed";
			hud.Mode = HudMode.ConnectionError;
			localPlayer.viewID = new NetworkViewID();
			NetVI = new NetworkViewID();
			Network.Disconnect();
		}
    }
	
	void OnConnectedToServer() {
		Debug.Log("Connected to a server");
		Connected = true;
		// we just connected to a host, let's RPC the host and ask for the game info
		networkView.RPC("RequestGameData", RPCMode.Server);
		hud.Mode = HudMode.Wait;
		latestPacket = Time.time;
		localPlayer.viewID = Network.AllocateViewID();
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
    }
	
	void OnFailedToConnect(NetworkConnectionError e) {
		Error = "";
		if (e == NetworkConnectionError.NoError) 
			Error = "No Error Reported.";
		if (e == NetworkConnectionError.RSAPublicKeyMismatch) 
			Error = "We presented an RSA public key which does not match what the system we connected to is using.";
		if (e == NetworkConnectionError.InvalidPassword) 
			Error = "The server is using a password and has refused our connection because we did not set the correct password.";
		if (e == NetworkConnectionError.ConnectionFailed) 
			Error = "Connection attempt failed, possibly because of internal connectivity problems.";
		if (e == NetworkConnectionError.TooManyConnectedPlayers) 
			Error = "The server is at full capacity, failed to connect.";
		if (e == NetworkConnectionError.ConnectionBanned) 
			Error = "We are banned from the system we attempted to connect to (likely temporarily).";
		if (e == NetworkConnectionError.AlreadyConnectedToServer) 
			Error = "We are already connected to this particular server (can happen after fast disconnect/reconnect).";
		if (e == NetworkConnectionError.AlreadyConnectedToAnotherServer) 
			Error = "Cannot connect to two servers at once. Close the connection before connecting again.";
		if (e == NetworkConnectionError.CreateSocketOrThreadFailure) 
			Error = "Internal error while attempting to initialize network interface. Socket possibly already in use.";
		if (e == NetworkConnectionError.IncorrectParameters) 
			Error = "Incorrect parameters given to Connect function.";
		if (e == NetworkConnectionError.EmptyConnectTarget) 
			Error = "No host target given in Connect.";
		if (e == NetworkConnectionError.InternalDirectConnectFailed) 
			Error = "Client could not connect internally to same network NAT enabled server.";
		if (e == NetworkConnectionError.NATTargetNotConnected) 
			Error = "The NAT target we are trying to connect to is not connected to the facilitator server.";
		if (e == NetworkConnectionError.NATTargetConnectionLost) 
			Error = "Connection lost while attempting to connect to NAT target.";
		if (e == NetworkConnectionError.NATPunchthroughFailed) 
			Error = "NAT punchthrough attempt has failed. The cause could be a too restrictive NAT implementation on either endpoints.";
		
		Debug.Log("Failed to Connect: " + Error);
		Network.Disconnect();
		localPlayer.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
		hud.Mode = HudMode.ConnectionError;
	}
	
	//--------- Disconnecting, quitting, kicking -----------
	void OnApplicationQuit() {
		DisconnectNow();
	}
	
	public void DisconnectNow() {
		if (Connected) {
			if (!isServer) {
				networkView.RPC("PlayerLeave", RPCMode.OthersBuffered, localPlayer.viewID, localPlayer.name);
			}else{
				networkView.RPC("ServerLeave", RPCMode.OthersBuffered);
			}
			
			localPlayer.viewID = new NetworkViewID();
			NetVI = new NetworkViewID();
			
			Network.Disconnect();
			if (isServer) 
				MasterServer.UnregisterHost();
			Connected = false;
			isServer = false;
			
			Camera.main.transform.parent = null;
			for (int i=0; i<players.Count; i++) {
				if (players[i].Entity != null) 
					Destroy(players[i].Entity.gameObject);
			}
			players = new List<NetUser>();
			
			hud.Mode = HudMode.MenuMain;
			Application.LoadLevel("MenuMain");
			levelLoaded = false;
		}
	}
	
	
	[RPC]
	void PlayerLeave(NetworkViewID viewID, string name) {
		latestPacket = Time.time;
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) {
				
				if (basketball && players[i].hasBall) {
					basketball.transform.parent = null;
					if (isServer) {
						ThrowBall(players[i].Entity.transform.position, -Vector3.up, 2f);
					}
				}
				
				
				if (players[i].Entity != null) 
					Destroy(players[i].Entity.gameObject);
				players.RemoveAt(i);
			}
		}
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.grey;
		newMsg.Text = "<< " + name + " left >>";
		log.Entries.Add(newMsg);
		log.DisplayTime = Time.time+log.FadeTime;
	}
	
	public void Kick(int playerIndex, bool autokick){
		Network.CloseConnection(players[playerIndex].netPlayer, true);
		networkView.RPC("KickedPlayer", RPCMode.AllBuffered, players[playerIndex].viewID, autokick);
	}
	
	[RPC]
	void KickedPlayer(NetworkViewID viewID, bool autokick) {
		latestPacket = Time.time;
		Debug.Log("A player was kicked");
		string name = "???";
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].viewID == viewID) {
				
				if (basketball && players[i].hasBall) {
					basketball.transform.parent = null;
					
					if (isServer) {
						ThrowBall(players[i].Entity.transform.position, -Vector3.up, 2f);
					}
				}
				
				name = players[i].name;
				if (players[i].Entity!= null) 
					Destroy(players[i].Entity.gameObject);
				players.RemoveAt(i);
			}
		}
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.grey;
		newMsg.Text = "<< ! " + name + " was kicked ! >>";
		if (autokick) 
			newMsg.Text = "<< ! " + name + " was auto-kicked ! >>";
		log.Entries.Add(newMsg);
		log.DisplayTime = Time.time + log.FadeTime;
		
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection netDis) {
		Debug.Log("Disconnected from server");
		if (isServer) 
			return;
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.white;
		newMsg.Text = "<< You have been disconnected from host >>";
		log.Entries.Add(newMsg);
		log.DisplayTime = Time.time+log.FadeTime;
		
		//Network.Disconnect();
		//if (isServer) MasterServer.UnregisterHost();
		Connected = false;
		isServer = false;
		
		localPlayer.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
				
		Camera.main.transform.parent = null;
		for (int i=0; i<players.Count; i++) {
			if (players[i].Entity!= null) 
				Destroy(players[i].Entity.gameObject);
		}
		players = new List<NetUser>();
		
		hud.Mode = HudMode.MenuMain;
		Application.LoadLevel("MenuMain");
		levelLoaded = false;
	}
	
	[RPC]
	void ServerLeave() {
		latestPacket = Time.time;
		Debug.Log("THE HOST LEFT!!!");
		
		Network.Disconnect();
		if (isServer) 
			MasterServer.UnregisterHost();
		
		Connected = false;
		isServer = false;
		localPlayer.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
		Camera.main.transform.parent = null;
		
		for (int i=0; i<players.Count; i++) {
			if (players[i].Entity != null) 
				Destroy(players[i].Entity.gameObject);
		}
		
		players = new List<NetUser>();
		hud.Mode = HudMode.MenuMain;
		Application.LoadLevel("MenuMain");
		levelLoaded = false;
		var newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.grey;
		newMsg.Text = "<< Host has left >>";
		log.Entries.Add(newMsg);
		log.DisplayTime = Time.time + log.FadeTime;
	}
	
	
	
	// misc
	public Vector3 ColToVec(Color colIn) {
		// convert colour to a vector
		Vector3 retVec = Vector3.zero;
		retVec.x = colIn.r;
		retVec.y = colIn.g;
		retVec.z = colIn.b;
		return retVec;
	}
	public Color VecToCol(Vector3 vecIn) {
		// convert vector to a color
		Color retCol = Color.white;
		retCol.r = vecIn.x;
		retCol.g = vecIn.y;
		retCol.b = vecIn.z;
		return retCol;
	}
}
