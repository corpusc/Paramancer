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
 * 
 * no manual reloading
 * no classes
 * no clunky "cover system" interrupting your control flow
 * no perks
 * no kill streak "rewards" (they are THEIR OWN REWARD)
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
	public string Error = "'Error' string init value...NO ERRORS YET!";
	public NetworkViewID NetVI;
	public bool Connected = false;
	public bool InServerMode = false;
	// if you change the client, change this name. 
	// should be unique or different types of clients will clash 
	public string MasterServerFacingName = "Rogue-Lite FPS";
	public string MatchName = "Init";
	public int connections = 32;
	public int listenPort = 25000;
	public string MatchTypeAndMap = ""; // match type & map name (for server browser) 
	public string password = "";
	public List<NetEntity> Entities;
	public List<SpawnData> GunSpawns = new List<SpawnData>();
	public int team1Score = 0;
	public int team2Score = 0;
	public bool gameOver = false;
	public bool serverGameChange = false;
	public bool broadcastPos = false;
	public bool lastGameWasTeamBased = false;

	// match (game modes/types) 
	public MatchData CurrMatch;
	public float MatchTimeLeft = 0f;
	public float IntermissionTimeLeft = 0f;

	// personal / local user 
	public NetEntity LocEnt;
	public bool gunBobbing = true;
	public bool JumpAuto = true;



	// private 
	float nextPingTime = 0f;
	float latestPacket = 0f;
	float latestServerHeartbeat = 0f;
	string nameOfOfflineBackdrop = "OfflineBackdrop";
	// bball 
	GameObject basketball;
	// map 
	bool playableMapIsReady;
	Announcements announce = new Announcements();
	VoxGen vGen;
	// scripts 
	CcLog log;
	Hud hud;
	Arsenal arse;


	
	void Start() {
		Mats.Init();
		vGen = new VoxGen();
		vGen.Init();

		DontDestroyOnLoad(this);
		//Application.targetFrameRate = 60; // -1 (the default) makes standalone games render as fast as they can, 
		// and web player games to render at 50-60 frames/second depending on the platform. 
		// If vsync is set in quality setting, the target framerate is ignored 
		
		// scripts 
		hud = GetComponent<Hud>();
		log = GetComponent<CcLog>();
		arse = GetComponent<Arsenal>();
		CurrMatch = new MatchData(Match.FFAFragMatch);
		
		Application.LoadLevel(nameOfOfflineBackdrop);
		Entities = new List<NetEntity>();
	}



	void OnGUI() {
		vGen.OnGUI();
	}

	void Update() {
		Sfx.Melodician.Update();

		vGen.Update();
		setupMapIfReady();

		// ping periodically
		if (Connected && Time.time > nextPingTime) {
			nextPingTime = Time.time + 5f;
			for (int i=0; i<Entities.Count; i++) {
				if (!Entities[i].local) {
					new Ping(Entities[i].netPlayer.ipAddress);
				}
			}
		}
		
		// let players know they are still connected 
		if (Connected && InServerMode) {
			if (Time.time > latestServerHeartbeat + 9f) {
				latestServerHeartbeat = Time.time + 9f;
				networkView.RPC("HeartbeatFromServer", RPCMode.All);
			}
		}
		
		// are we still connected to the server? 
		if (Connected && !InServerMode) {
			if (Time.time > latestPacket + 30f) {
				DisconnectNow();
				hud.Mode = HudMode.ConnectionError;
				Error = "Client hasn't heard from host for 30 seconds.\n" +
					"Probably because someone's connection sucks.";
			}
			
			// remind the server we here 
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].local && Time.time > Entities[i].lastPong + 5f) {
					networkView.RPC("PONG", RPCMode.Server,Entities[i].viewID);
					Entities[i].lastPong = Time.time;
				}	
			}
		}
		
		if (Connected && InServerMode) {
			for (int i=0; i<Entities.Count; i++) {
				if (!Entities[i].local) {
					if (Time.time>Entities[i].lastPong + 20f) {
						// gone 20 secs without hearing from client, auto-kick 
						Kick(i,true);
					}
				}
			}
		}
		
		if (InServerMode) {
			// respawn dead players 
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].Actor != null && Entities[i].Health <= 0f) {
					if (Time.time > Entities[i].respawnTime) {
						if (CurrMatch.playerLives == 0 || Entities[i].lives > 0)
							Entities[i].Health = 100f;
						
						networkView.RPC("AssignPlayerStats", RPCMode.All, 
						                Entities[i].viewID, 
						                Entities[i].Health, 
						                Entities[i].kills, 
						                Entities[i].deaths, 
						                Entities[i].currentScore);
						networkView.RPC("RespawnPlayer", RPCMode.All, Entities[i].viewID);
					}
				}else{ // set spawn countdown 
					Entities[i].respawnTime = Time.time + CurrMatch.respawnWait;
				}
			}
		}
		
		// change team 
		if (Connected && CurrMatch.teamBased) {
			if (CcInput.Started(UserAction.SwapTeam)) {
				if (LocEnt.team == 1) {
					LocEnt.team = 2;
				}else{
					LocEnt.team = 1;
				}
				
				networkView.RPC("PlayerChangedTeams", RPCMode.AllBuffered, LocEnt.viewID, LocEnt.team);
				
				for (int i=0; i<Entities.Count; i++) {
					if (Entities[i].viewID == LocEnt.viewID && Entities[i].Health > 0f) 
						Entities[i].Actor.Respawn();
				}
			}
		}
		
		MatchTimeLeft -= Time.deltaTime;
		// time announcements 
		announce.Update(MatchTimeLeft);

		// game time up? 
		if (Connected && !gameOver) {
			if (MatchTimeLeft <= 0f && CurrMatch.Duration > 0f){
				MatchTimeLeft = 0f;
				gameOver = true;
				
				IntermissionTimeLeft = 15f;
			}
		}
		
		// if game over, count in next match 
		if (Connected && gameOver) {
			IntermissionTimeLeft -= Time.deltaTime;
			if (IntermissionTimeLeft <= 0f) {
				IntermissionTimeLeft = 0f;
				
				if (InServerMode) {
					//begin next match using current settings
					serverGameChange = true;
					lastGameWasTeamBased = CurrMatch.teamBased;
					NetVI = Network.AllocateViewID();
					RequestGameData();
				}
			}
		}
		
		// pickups 
		if (Connected && InServerMode && !gameOver) {
			for (int i=0; i<GunSpawns.Count; i++) {
				if (!GunSpawns[i].stocked) {
					GunSpawns[i].RestockTime -= Time.deltaTime;
					if (GunSpawns[i].RestockTime <= 0f) {
						Gun item = (Gun)GunSpawns[i].Gun;
						networkView.RPC("RestockPickup", RPCMode.All, GunSpawns[i].Gun, (int)item);
					}
				}
			}
		}
	}



	public BasketballScript GetBball() {
		return basketball.GetComponent<BasketballScript>();
	}

	//-------- network gameplay ---------- 
	public void SendTINYUserUpdate(NetworkViewID nvi, UserAction action) {
		networkView.RPC("SendTINYUserUpdateRPC", RPCMode.Others, nvi, (int)action);
	}
	[RPC]
	void SendTINYUserUpdateRPC(NetworkViewID nvi, int action) {
		latestPacket = Time.time;
		
		for (int i=0; i<Entities.Count; i++) {
			var u = Entities[i]; // user 

			if (u.viewID == nvi && 
			    u.Actor != null
		    ) {
				u.lastPong = Time.time;
				u.Actor.PlaySound((UserAction)action);
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
		
		for (int i=0; i<Entities.Count; i++) {
			if (viewID == Entities[i].viewID && Entities[i].Actor != null) {
				Entities[i].lastPong = Time.time;
				Entities[i].Actor.UpdatePlayer(pos, ang, crouch, moveVec, yMove, 
					info.timestamp, (Gun)gunA, (Gun)gunB, playerUp, playerForward);
			}
		}
	}
	
	public void ConsumeHealth(NetworkViewID viewID) {
		// we just used a health pack, tell the server our health is maxed out
		if (!InServerMode) {
			networkView.RPC("ConsumeHealthRPC", RPCMode.Server, viewID);
		}else{
			ConsumeHealthRPC(viewID);
		}
	}
	[RPC]
	void ConsumeHealthRPC(NetworkViewID viewID) {
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) {
				Entities[i].Health = 100f;
			}
		}
		if (LocEnt.viewID == viewID) 
			LocEnt.Health = 100f;
	}

	public void DetonateRocket(Vector3 detPos, Vector3 hitNorm, NetworkViewID bulletID) {
		arse.DetonateRocket(detPos, hitNorm, bulletID);
	}
	
	// this can be called by RL, GL, & Bomb 
	public void Detonate(Gun weapon, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID) {
		// we are server and something detonated, tell everyone
		networkView.RPC("DetonateRPC", RPCMode.All, (int)weapon, position, shooterID, bulletID);
	}
	[RPC]
	void DetonateRPC(int weapon, Vector3 position, NetworkViewID shooterID, NetworkViewID bulletID) {
		latestPacket = Time.time;
		if ((Gun)weapon != Gun.RocketLauncher) // rocket explosions are partially simulated client-side 
			arse.Detonate((Gun)weapon, position, bulletID);
		
		if (InServerMode) {
			// see if anyone gets hurt 
			for (int i=0; i<Entities.Count; i++){
				if (Vector3.Distance(position, Entities[i].Actor.transform.position) 
					< 
				    arse.Guns[weapon].BlastRadius + 0.5f /* FIXME: it's the entity lateral radius, shouldn't be hardcoded */
				) {
					// player in range 
					bool skip = false;
					// ignore if on the same team as the person who fired (unless bomb) 
					if (CurrMatch.teamBased && !CurrMatch.FriendlyFire) {
						int shooterIndex = -1;
						for (int k=0; k<Entities.Count; k++){
							if (Entities[k].viewID == shooterID) 
								shooterIndex = k;
						}
						
						if (shooterIndex != -1 && Entities[i].team == Entities[shooterIndex].team) 
							skip = true;
						if (shooterIndex != -1 && i == shooterIndex && (Gun)weapon == Gun.Bomb) 
							skip = false;
					}
					
					if (Entities[i].Health > 0f && !skip) {
						RegisterHitRPC(weapon, shooterID, Entities[i].viewID, Entities[i].Actor.transform.position);
					}
				}
			}
		}
	}
	
	public void Shoot(Gun weapon, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, bool hit, bool alt, Vector3 hitNorm, bool sprint = false) {
		// we have fired a shot, let's tell everyone about it so they can see it 
		//print ("Shot info received by net.Shoot(), alt = " + (alt ? "1" : "0"));
		NetworkViewID bulletID = Network.AllocateViewID();
		networkView.RPC("ShootRPC", RPCMode.All, (int)weapon, origin, direction, end, shooterID, bulletID, hit, sprint, alt, hitNorm);
	}
	[RPC]
	void ShootRPC(int weapon, Vector3 origin, Vector3 direction, Vector3 end, NetworkViewID shooterID, NetworkViewID bulletID, bool hit, bool sprint, bool alt, Vector3 hitNorm, NetworkMessageInfo info) {
		// somebody fired a shot, let's show it 
		latestPacket = Time.time;
		arse.Shoot((Gun)weapon, origin, direction, end, shooterID, bulletID, info.timestamp, hit, alt, hitNorm, sprint);
	}
	
	public void RegisterHit(Gun weapon, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos) {
		if (gameOver) 
			return;

		// we hit somebody, tell the server! 
		if (!InServerMode) {
			networkView.RPC("RegisterHitRPC", RPCMode.Server, (int)weapon, shooterID, victimID, hitPos);
		}else{
			RegisterHitRPC((int)weapon, shooterID, victimID, hitPos);
		}
	}
	[RPC]
	public void RegisterHitRPC(int weapon, NetworkViewID shooterID, NetworkViewID victimID, Vector3 hitPos) {
		if (gameOver) 
			return; // no damage after game over 
		
		latestPacket = Time.time;
		
		int si = -1; // shooter index 
		int vi = -1; // victim index 
		bool killShot = false;
		
		//Debug.Log("hit registered");
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == shooterID) si = i;
			if (Entities[i].viewID == victimID) vi = i;
		}
		
		if (si == -1 || vi == -1 || Entities[vi].Health <= 0f) 
			return;
		
		if ((Gun)weapon == Gun.Swapper){
			networkView.RPC("SwapPlayers",RPCMode.All, shooterID, Entities[si].Actor.transform.position, victimID, Entities[vi].Actor.transform.position);
			return;
		}
		
		
		// subtract health 
		if (si == vi && (Gun)weapon == Gun.RocketLauncher) {
			// rocket jumping 
			Entities[vi].Health -= 30f;
		}else{
			// normal damage 
			if ((Gun)weapon == Gun.GrenadeLauncher || (Gun)weapon == Gun.RocketLauncher) { // less damage when farther from the explosion 
				float d = Vector3.Distance(hitPos, Entities[vi].Actor.transform.position) + 1f;
				Entities[vi].Health -= arse.GetWeaponDamage((Gun)weapon) / d;

			} else Entities[vi].Health -= arse.GetWeaponDamage((Gun)weapon);
		}
		
		if (Entities[vi].Health <= 0f) {
			// player died 
			Entities[vi].Health = 0f;
			killShot = true;
		}
		
		// scores
		if (killShot) {
			Entities[vi].deaths++;
			
			if (CurrMatch.deathsSubtractScore) 
				Entities[vi].currentScore--;
			if (si == vi) {
				Sfx.PlayOmni("humiliation");
			} else {
				Entities[si].kills++;
				if (CurrMatch.killsIncreaseScore) 
					Entities[si].currentScore++;
			}
		}
		
		// team stuff
		if (killShot && CurrMatch.teamBased) {
			if (Entities[vi].team == 1 && Entities[si].team == 2 && CurrMatch.deathsSubtractScore) team1Score--;
			if (Entities[vi].team == 2 && Entities[si].team == 1 && CurrMatch.deathsSubtractScore) team2Score--;
			if (Entities[si].team == 1 && Entities[vi].team == 2 && CurrMatch.killsIncreaseScore) team1Score++;
			if (Entities[si].team == 2 && Entities[vi].team == 1 && CurrMatch.killsIncreaseScore) team2Score++;
		}
		
		// assign results
		networkView.RPC("AssignPlayerStats", RPCMode.All, victimID, Entities[vi].Health, Entities[vi].kills, Entities[vi].deaths, Entities[vi].currentScore);
		networkView.RPC("AssignPlayerStats", RPCMode.All, shooterID, Entities[si].Health, Entities[si].kills, Entities[si].deaths, Entities[si].currentScore);

		if (killShot) {
			networkView.RPC("AnnounceKill", RPCMode.All, weapon, shooterID, victimID);
			
			if (CurrMatch.teamBased) 
				networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		}
		
		// do hit effects 
		networkView.RPC("DoHitEffects", RPCMode.All, weapon, hitPos, victimID);
		
		// check for game over
		if (CurrMatch.winScore > 0) {
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].currentScore>=CurrMatch.winScore) {
					networkView.RPC("AnnounceGameOver", RPCMode.All);
				}
			}
		}
	}
	
	[RPC]
	void SwapPlayers(NetworkViewID shooterID, Vector3 shooterPos, NetworkViewID victimID, Vector3 victimPos) {
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].local) {
				if (Entities[i].viewID == shooterID) {
					Entities[i].Actor.transform.position = victimPos;
					Entities[i].Actor.sendRPCUpdate = true;
					Entities[i].Actor.PlaySound("Swapped");
					Entities[i].Actor.ForceLook(shooterPos);
				}else if (Entities[i].viewID == victimID) {
					Entities[i].Actor.transform.position = shooterPos;
					Entities[i].Actor.sendRPCUpdate = true;
					Entities[i].Actor.PlaySound("Swapped");
					Entities[i].Actor.ForceLook(victimPos);
				}
			}
		}
	}
	
	[RPC]
	void DoHitEffects(int weapon, Vector3 hitPos, NetworkViewID viewID) {
		int numGibs = 4;
		switch ((Gun)weapon) {
			case Gun.GrenadeLauncher:        numGibs = 15; break;
			case Gun.MachineGun:             numGibs = 2; break;
			case Gun.RailGun:                numGibs = 30; break;
			case Gun.RocketLauncher:         numGibs = 20; break;
			case Gun.Bomb:                   numGibs = 20; break;
			
			case Gun.Suicide:                numGibs = 30; break;
		}
		numGibs *= 4; // increase giblets across the board 
		
		for (int i=0; i<numGibs; i++) {
			GameObject g = (GameObject)GameObject.Instantiate(GOs.Get("Giblet"));
			g.transform.position = hitPos;
			g.GetComponent<Giblet>().Gravity = CurrMatch.Gravity;
		}
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID && 
				//players[i].local && 
				Entities[i].Health > 0f
			) {
				Entities[i].Actor.PlaySound("girldamage"); // positioned at the hit player 
				Sfx.PlayOmni("TakeDamage"); // heard at same volume (& centered panning), regardless of where the hit player is 
			}
		}
	}
	
	[RPC]
	void AnnounceGameOver() {
		MatchTimeLeft = 0f;
		gameOver = true;
		IntermissionTimeLeft = 15f;
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
		for (int i=0; i<Entities.Count; i++){
			if (Entities[i].viewID==viewID){
				Entities[i].kills = kills;
				Entities[i].deaths = deaths;
				Entities[i].currentScore = currentScore;
			}
		}
	}
	
	[RPC]
	void AssignPlayerStats(NetworkViewID viewID, float health, int kills, int deaths, int score) {
		latestPacket = Time.time;
		if (LocEnt.viewID == viewID) {
			LocEnt.Health = health;
			LocEnt.kills = kills;
			LocEnt.deaths = deaths;
			LocEnt.currentScore = score;
		}
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) {
				Entities[i].Health = health;
				Entities[i].kills = kills;
				Entities[i].deaths = deaths;
				Entities[i].currentScore = score;
			}
		}
	}
	
	[RPC]
	void AnnounceKill(int weapon, NetworkViewID fraggerID, NetworkViewID victimID) {
		latestPacket = Time.time;
		
		// setup names & ids for relevant players 
		int fraggerIdx = 0;
		int victimIdx = 0;
		string fraggerName = "Someone";
		string victimName = "Someone";
		for(int i = 0; i < Entities.Count; i++) {
			if (Entities[i].viewID == fraggerID) { // if this is the fragger 
				if (victimID != fraggerID) {
					Entities[i].Actor.MultiFragCount++;
					Entities[i].Actor.PrevFrag = Time.time;
					handleMultiFrag(i, Entities[i].name);
				}
				fraggerName = Entities[i].name;
				fraggerIdx = i;
			}
			
			if (Entities[i].viewID == victimID) { // if this is the victim 
				victimName = Entities[i].name;
				victimIdx = i;
				if (victimID != fraggerID)
					Entities[i].Actor.PlaySound("Die");

				// lives 
				Entities[i].lives--;
				if (CurrMatch.playerLives > 0 &&
				    Entities[i].lives <= 0 && 
				    Entities[i].local)
				{
					Entities[i].Actor.Spectating = true;
				}

				// basketball 
				if (CurrMatch.basketball && Entities[i].hasBall) {
					Entities[i].hasBall = false;

					if (InServerMode)
						ThrowBall(Entities[i].Actor.transform.position, -Vector3.up, 2f);
				}
			}
		}
		
		// set some unused?! totals 
		if (LocEnt.viewID == fraggerID) 
			LocEnt.totalKills++; // what the fuck are totalKills and totalDeaths for?????  just FIXME and delete this shiz? NOTE: they could be used for statistics
		if (LocEnt.viewID == victimID) { // if local player was the victim 
			LocEnt.totalDeaths++;
			LocEnt.FraggedBy = Entities[fraggerIdx].Actor.gameObject;
		}
		
		// lives 
		if (InServerMode && 
			Connected && 
			!gameOver && 
			CurrMatch.playerLives > 0 && 
			Entities.Count > 1)
		{
			int livingPlayers = 0;
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].lives > 0) 
					livingPlayers++;
			}
			
			if (livingPlayers <= 1) {
				MatchTimeLeft = 0f;
				gameOver = true;
				IntermissionTimeLeft = 15f;
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}
		}
		
		// obituary text 
		var le = new LogEntry();
		le.Maker = "";
		le.Color = Color.red;
		le.Text = getObituary(fraggerName, fraggerIdx, victimName, victimIdx, (Gun)weapon);
		log.Entries.Add(le);
		log.TimeToHideEntireLog = Time.time+log.FadeTime;
		
	}

	void handleMultiFrag(int i, string f) { // f is for fragger
		switch (Entities[i].Actor.MultiFragCount) {
			case 0: break;
			case 1: break;
			case 2: Entities[i].Actor.PlaySound("DoubleKill"); break;
			case 3: Entities[i].Actor.PlaySound("TripleKill"); break;
			case 4: Entities[i].Actor.PlaySound("QuadraKill"); break;
			case 5: Entities[i].Actor.PlaySound("PentaKill"); break;
			case 6: Entities[i].Actor.PlaySound("HexaKill"); break;
			default:

			Entities[i].Actor.PlaySound("GodLike");
			var le = new LogEntry();
			le.Maker = "";
			le.Color = Color.magenta;
			le.Text = f + " is godlike!";
			log.Entries.Add(le);
			log.TimeToHideEntireLog = Time.time+log.FadeTime;

			break;
		}
	}

	private string getObituary(string f, int fId, string v, int vId, Gun weapon) { // fragger, victim 
		// suicides 
		if (fId == vId) {
			switch (Random.Range(0, 5)) {
				case 0:	return f + " bought the farm!";
				case 1:	return f + " changed career... to Daisy Pusher!";
				case 2:	return f + " really bit the dust!";
				case 3:	return f + " really shot himself in the foot!";
				case 4:	return f + " did some nice kamikaze work!";
				default: return "....";
			}
		}

		// special per-weapon 
		if (weapon == Gun.Spatula) {
			switch (Random.Range(0, 4)) {
				case 0:	return f + " really gave " + v + " the SPAAA-TCHOOO-LAAH treatment!";
				case 1:	return f + " spatulated " + v + "!";
				case 2:	return f + " really flipped " + v + " out!";
				case 3: return f + " showed spatula power to " + v + "!";
				default: return "....";
			}
		}

		// normal frags 
		switch (Random.Range(0, 7)) {
			case 0:	return f + " really gave " + v + " what for!";
			case 1:	return f + " fixed " + v + "'s little red wagon!";
			case 2:	return f + " messed up " + v + " real bad!";
			case 3:	return f + " sent " + v + " to a better place!";
			case 4:	return f + " released " + v + "'s spirit from these mortal chains!";
			case 5: return v + " was shown the light by " + f + "!";
			case 6: return f + " showed the justice to " + v + "!";
			default: return "....";
		}
	}



	[RPC]
	void PONG(NetworkViewID viewID) {
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) 
				Entities[i].lastPong = Time.time;
		}
	}



	#region Gun Spawns

	[RPC]
	void RespawnPlayer(NetworkViewID viewID) {
		latestPacket = Time.time;
		
		if (viewID == LocEnt.viewID) {
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].viewID == viewID) {
					if ((CurrMatch.playerLives > 0 && 
					     Entities[i].lives > 0) ||
					    CurrMatch.playerLives == 0
					    ) {
						Entities[i].Actor.Respawn();
					}
				}
			}
		}
	}
	


	[RPC]
	void RestockPickup(int pointID, int item) {
		for (int i=0; i<GunSpawns.Count; i++) {
			if (GunSpawns[i].Gun == pointID) {
				if (GunSpawns[i].stocked) 
					return;
				
				GunSpawns[i].stocked = true;

				// health will be a kit model with an icon 
				var o = GOs.Get("Kit");
				if (item >= (int)Gun.Pistol) 
					o = arse.Guns[item].Prefab;

				var inst = (GameObject)GameObject.Instantiate(o);
				GunSpawns[i].Takeable = inst;
				inst.transform.position = GunSpawns[i].transform.position;
				inst.transform.localScale = Vector3.one * 0.5f;

				GunPickup gs;
				// if gun of some type 
				if (item >= (int)Gun.Pistol) {
					gs = inst.AddComponent<GunPickup>();
					gs.Model = inst;
					gs.Name = arse.Guns[item].Name;
				}else{ // it's health 
					gs = inst.GetComponent<GunPickup>();
					gs.Name = "Health";
				}

				gs.SpawnData = GunSpawns[i];
			}
		}
	}



	public void UnstockPickupPoint(SpawnData point){
		for (int i=0; i<GunSpawns.Count; i++) {
			if (point == GunSpawns[i]) {
				networkView.RPC("UnstockRPC", RPCMode.All, GunSpawns[i].Gun);
			}
		}
	}
	[RPC]
	void UnstockRPC(int pointID){
		for (int i=0; i<GunSpawns.Count; i++) {
			if (pointID == GunSpawns[i].Gun) {
				GunSpawns[i].stocked = false;
				
				if (GunSpawns[i].Takeable != null) 
					Destroy(GunSpawns[i].Takeable);
				
				if (InServerMode) 
					GunSpawns[i].RestockTime = CurrMatch.restockTime;
			}
		}
	}
	
	[RPC]
	void RequestPickupStocks() {
		// a client has requested the current pickup stock info
		for (int i=0; i<GunSpawns.Count; i++) {
			if (GunSpawns[i].stocked) {
				Gun item = (Gun)Random.Range((int)Gun.None, arse.Guns.Length);

				if (item == Gun.None)
					item = Gun.Health;

				networkView.RPC("RestockPickup", RPCMode.All, GunSpawns[i].Gun, (int)item);
			}
		}
	}

	#endregion



	#region BBall

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

	#endregion



	[RPC]
	void HeartbeatFromServer() {
		latestPacket = Time.time;
	}
	
	[RPC]
	void PlayerChangedTeams(NetworkViewID viewID, int team) {
		latestPacket = Time.time;
		for (int i=0; i<Entities.Count; i++) {
			//always set model visibility on team change, that way if *you* change teams, all lights are changed
			Entities[i].Actor.SetModelVisibility(!Entities[i].local);
			
			if (viewID == Entities[i].viewID) {
				Entities[i].team = team;
				//Entities[i].Visuals.SetModelVisibility(!players[i].local);
				
				var l = new LogEntry();
				l.Maker = "";
				l.Text = "";
				
				if (team == 1) {
					if (LocEnt.viewID == viewID){
						l.Color = Color.red;
						l.Text = "<< you defected! >>";
					}else if (LocEnt.team == 1) {
						l.Color = Color.red;
						l.Text = "<< " + Entities[i].name + " defected to your team! >>";
					}else{
						l.Color = Color.cyan;
						l.Text = "<< " + Entities[i].name + " turned their back on the team! >>";
					}
				}else{
					if (LocEnt.viewID == viewID) {
						l.Color = Color.cyan;
						l.Text = "<< you defected! >>";
					}else if (LocEnt.team == 2) {
						l.Color = Color.cyan;
						l.Text = "<< " + Entities[i].name + " defected to your team! >>";
					}else{
						l.Color = Color.red;
						l.Text = "<< " + Entities[i].name + " turned their back on the team! >>";
					}
				}
				
				log.Entries.Add(l);
				log.TimeToHideEntireLog = Time.time + log.FadeTime;
			}
		}
	}



	//-------- player joining stuff ----------
	[RPC]
	void NewPlayer(NetworkViewID viewID, string name, Vector3 cA, Vector3 cB, Vector3 cC, int head, 
		NetworkPlayer np, int targetTeam, int lives
	) {
		latestPacket = Time.time;
		
		if (Entities.Count == 1 && CurrMatch.playerLives > 0) {
			if (InServerMode && Connected && !gameOver) {
				// this is a lives match, and now we have enough players 
				MatchTimeLeft = 0f;
				gameOver = true;
				IntermissionTimeLeft = 5f; // CHANGE ME
				networkView.RPC("AnnounceGameOver", RPCMode.Others);
			}	
		}
		
		// this could just be a team change update at the start of the level 
		bool weExist = false;
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) {
				weExist = true;
				Entities[i].team = targetTeam;
				Entities[i].lives = lives;
			}
		}
		
		if (weExist) 
			return;
		
		// another player has joined, lets add them to our view of the game 
		bool localShopForLocalPeople = false;
		if (viewID == LocEnt.viewID) 
			localShopForLocalPeople = true;
		
		AddPlayer(localShopForLocalPeople, viewID, 
		          S.VecToCol(cA), 
		          S.VecToCol(cB), 
		          S.VecToCol(cC), head, name, np, targetTeam, lives);
		
		if (playableMapIsReady) {
			// only instantiate the actual GameObject of the player if we are in the right map. 
			// uninstantiated players are added when the map finishes loading 
			Entities[Entities.Count-1].InstantiateGO(GOs.Get("FPSEntity"));
		}
		
		broadcastPos = true;

		if (InServerMode) {
			// let new players know the scores 
			for (int i=0; i<Entities.Count; i++) {
				networkView.RPC("SharePlayerScores", RPCMode.Others, 
	                Entities[i].viewID, 
	                Entities[i].kills, 
	                Entities[i].deaths, 
					Entities[i].currentScore);
			}
			
			networkView.RPC("AnnounceTeamScores", RPCMode.Others, team1Score, team2Score);
		}
		
		if (playableMapIsReady) {
			var le = new LogEntry();
			le.Maker = "";
			le.Color = Color.grey;
			le.Text = "<< " + name + " has joined >>";
			log.Entries.Add(le);
			log.TimeToHideEntireLog = Time.time+log.FadeTime;
		}
	}
	
	void AddPlayer(bool local, NetworkViewID anID, Color cA, Color cB, Color cC, int head, string name, 
		NetworkPlayer np, int targetTeam, int lives
	) {
		var ent = new NetEntity();
		ent.colA = cA;
		ent.colB = cB;
		ent.colC = cC;
		ent.headType = head;
		ent.viewID = anID;
		ent.local = local;
		ent.name = name;
		ent.netPlayer = np;
		ent.ping = new Ping(ent.netPlayer.ipAddress);
		ent.team = targetTeam;
		ent.kills = 0;
		ent.deaths = 0;
		ent.currentScore = 0;
		ent.lives = lives;
		ent.lastPong = Time.time;
		Entities.Add(ent);
	}
	
	public void AssignGameModeConfig(MatchData md, string mapName) {
		CurrMatch.MapName = mapName;
		
		// FIXME?: no need for all these lines?   we could just do 'CurrMatch = md;'?
		// allowedLevels is the only thing missing from here?
		CurrMatch.Name = md.Name;
		CurrMatch.Descript = md.Descript;
		CurrMatch.winScore = md.winScore;
		CurrMatch.Duration = md.Duration;
		CurrMatch.respawnWait = md.respawnWait;
		CurrMatch.deathsSubtractScore = md.deathsSubtractScore;
		CurrMatch.killsIncreaseScore = md.killsIncreaseScore;
		CurrMatch.teamBased = md.teamBased;
		CurrMatch.FriendlyFire = md.FriendlyFire;
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
		CurrMatch.NeedsGenerating = md.NeedsGenerating;
		CurrMatch.Seed = md.Seed;
		CurrMatch.MoveSpeedMult = md.MoveSpeedMult;
		CurrMatch.Gravity = md.Gravity;
		CurrMatch.Theme = md.Theme;
	}
	
	[RPC]
	public void RequestGameData() {
		// a player has requested game data, pass it out 
		latestPacket = Time.time;
		
		// also figure out which team to drop them in 
		int team1count = 0;
		int team2count = 0;
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].team == 1) 
				team1count++;
			if (Entities[i].team == 2) 
				team2count++;
		}
		
		int targetTeam = 0;
		if (CurrMatch.teamBased) 
			targetTeam = 1;
		if (CurrMatch.teamBased && team1count>team2count) 
			targetTeam = 2;
		if (CurrMatch.teamBased && team2count>team1count) 
			targetTeam = 1;
		
		// keep teams if we are already assigned to teams 
		if (serverGameChange && Entities.Count > 0) {
			if (lastGameWasTeamBased) {
				targetTeam = -1;
			}else{
				targetTeam = -2;
			}
		}
		lastGameWasTeamBased = false;
		
		int livesBroadcast = 0;
		if (serverGameChange) {
			MatchTimeLeft = CurrMatch.Duration * 60f;
			gameOver = false;
			announce.SetupTimeCountdowns();
			livesBroadcast = CurrMatch.playerLives;
		}else{
			if (CurrMatch.playerLives > 0) {
				livesBroadcast = -1;
			}
		}
		
		networkView.RPC("BroadcastNewGame", RPCMode.All, NetVI, 
			CurrMatch.Name, CurrMatch.MapName, CurrMatch.Descript, CurrMatch.winScore, CurrMatch.Duration, 
			CurrMatch.respawnWait, CurrMatch.deathsSubtractScore, CurrMatch.killsIncreaseScore, CurrMatch.teamBased, 
			targetTeam, CurrMatch.FriendlyFire, CurrMatch.pitchBlack, gameOver, MatchTimeLeft, 
			(int)CurrMatch.spawnGunA, (int)CurrMatch.spawnGunB, (int)CurrMatch.pickupSlot1, (int)CurrMatch.pickupSlot2, 
			(int)CurrMatch.pickupSlot3, (int)CurrMatch.pickupSlot4, (int)CurrMatch.pickupSlot5, livesBroadcast, serverGameChange, 
			CurrMatch.basketball, CurrMatch.MoveSpeedMult, CurrMatch.Gravity, CurrMatch.NeedsGenerating, CurrMatch.Seed, (int)CurrMatch.Theme);
	}



	[RPC]
	void BroadcastNewGame(NetworkViewID viewID, string matchName, string mapName, string matchDescript, int winScore, 
		float duration, float respawnWait, bool deathsSubtractScore, bool killsIncreaseScore, bool teamBased, 
		int targetTeam, bool allowFriendlyFire, bool pitchBlack, bool gameIsOver, float serverGameTime, int spawnGunA, 
		int spawnGunB, int pickupSlot1, int pickupSlot2, int pickupSlot3, int pickupSlot4, int pickupSlot5, 
		int playerLives, bool newGame, bool basketball, float speedUp, float GForce, bool needsGen, int seed, int style,
	    NetworkMessageInfo info
	) {
		// we've received game info from the server 
		latestPacket = Time.time;
		
		Time.timeScale = speedUp;
		Physics.gravity = new Vector3(0f, -GForce, 0f);
		
		if (NetVI != null && 
			NetVI == viewID && 
			!serverGameChange) 
			return;
		
		if (!InServerMode) 
			MatchTimeLeft = serverGameTime - (float)(Network.time - info.timestamp);
		
		// make sure all connected players have lives 
		if (newGame)
			for (int i=0; i<Entities.Count; i++)
				Entities[i].lives = playerLives;
		
		if (!InServerMode)
			gameOver = gameIsOver;
		
		IntermissionTimeLeft = 15f;
		
		// if we get to this point, it's a new game, so let's get it together! 
		NetVI = viewID;
		serverGameChange = false;

		announce.SetupTimeCountdowns();
		
		if (!InServerMode) {
			// update the local game settings 
			CurrMatch.MapName = mapName;
			CurrMatch.Name = matchName;
			CurrMatch.Descript = matchDescript;
			CurrMatch.winScore = winScore;
			CurrMatch.Duration = duration;
			CurrMatch.respawnWait = respawnWait;
			CurrMatch.deathsSubtractScore = deathsSubtractScore;
			CurrMatch.killsIncreaseScore = killsIncreaseScore;
			CurrMatch.teamBased = teamBased;
			CurrMatch.FriendlyFire = allowFriendlyFire;
			CurrMatch.pitchBlack = pitchBlack;
			CurrMatch.playerLives = playerLives;
			CurrMatch.basketball = basketball;
			CurrMatch.spawnGunA = (Gun)spawnGunA;
			CurrMatch.spawnGunB = (Gun)spawnGunB;
			CurrMatch.pickupSlot1 = (Gun)pickupSlot1;
			CurrMatch.pickupSlot2 = (Gun)pickupSlot2;
			CurrMatch.pickupSlot3 = (Gun)pickupSlot3;
			CurrMatch.pickupSlot4 = (Gun)pickupSlot4;
			CurrMatch.pickupSlot5 = (Gun)pickupSlot5;
			CurrMatch.NeedsGenerating = needsGen;
			CurrMatch.Seed = seed;
			CurrMatch.MoveSpeedMult = speedUp;
			CurrMatch.Gravity = GForce;
			CurrMatch.Theme = (Theme)style;
		}
		
		if (targetTeam != -1) {
			// don't keep current teams
			LocEnt.team = targetTeam;
			for (int i=0; i<Entities.Count; i++){
				if (Entities[i].viewID == LocEnt.viewID){
					Entities[i].team = targetTeam;
				}
			}
		}
		if (targetTeam == -1) {
			// last game was team based, leave teams as they are 
		}
		if (targetTeam == -2) {
			// last game wasn't team based, this is, set team 
			LocEnt.team = 1;
			if (Random.Range(0,10) < 5) 
				LocEnt.team = 2;
			
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].viewID == LocEnt.viewID) {
					Entities[i].team = LocEnt.team;
				}
			}
		}
		
		team1Score = 0;
		team2Score = 0;
		
		// clear stuff out if we are already playing 
		playableMapIsReady = false;
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].Actor != null) 
				Destroy(Entities[i].Actor.gameObject);
			Entities[i].Actor = null;
			
			Entities[i].kills = 0;
			Entities[i].deaths = 0;
			Entities[i].currentScore = 0;
		}
		
		LocEnt.kills = 0;
		LocEnt.deaths = 0;
		LocEnt.currentScore = 0;
		arse.Clear();
		
		// load map 
		Application.LoadLevel(mapName);
		
		// if procgen map 
		if (mapName == MatchData.VoxelName)
			vGen.GenerateMap(CurrMatch.Seed, CurrMatch.Theme);
	}
	
	void OnLevelWasLoaded(int level) {
		Debug.Log("OnLevelWasLoaded() - level: " + level);

		// skip Init(0) and OfflineBackdrop(1) scenes 
		if (level > 1 && !vGen.Generating)
			setupSpawns();
	}

	private void setupSpawns() {
		playableMapIsReady = true;
		
		handleBBall();
		
		// add entities for all known users 
		for (int i=0; i<Entities.Count; i++) {
			Entities[i].InstantiateGO(GOs.Get("FPSEntity"));
		}
		
		// tell everyone we're here 
		networkView.RPC("NewPlayer", RPCMode.AllBuffered, LocEnt.viewID, LocEnt.name, 
		                S.ColToVec(LocEnt.colA), S.ColToVec(LocEnt.colB), S.ColToVec(LocEnt.colC), 
		                LocEnt.headType, Network.player, LocEnt.team, CurrMatch.playerLives);
		
		setupGunSpawns();
		
		networkView.RPC("RequestPickupStocks", RPCMode.Server);
		hud.Mode = HudMode.Playing;
	}

	private void handleBBall() {
		if (CurrMatch.basketball) {
			basketball = (GameObject)GameObject.Instantiate(GOs.Get("BasketBall"));
			
			if (!InServerMode) 
				networkView.RPC("RequestBallStatus", RPCMode.Server);
		}else{
			if (GameObject.Find("_BasketRed") != null) 
				Destroy(GameObject.Find("_BasketRed"));
			if (GameObject.Find("_BasketBlue") != null) 
				Destroy(GameObject.Find("_BasketBlue"));
		}
	}

	private void setupGunSpawns() {
		GunSpawns = new List<SpawnData>();
		var par = GameObject.Find("Gun"); // parent of all the gun spawns 
		if (par != null) {
			string s = "items: ";
			// initial temp list to draw from..... so guns only appear once, and the rest of the spawns are healthpacks 
			var guns = new List<Gun>();
			for (int i = 0; i < (int)Gun.Count; i++)
				guns.Add((Gun)i);
			while (guns.Count < par.transform.childCount) // fill up the rest of spawns with health pickups 
				guns.Add(Gun.Health);
			
			foreach (Transform child in par.transform) {
				//Gun item = (Gun)Random.Range((int)Gun.None, arse.Guns.Length);
				int i = (int)Random.Range(0, guns.Count);
				Gun gun = guns[i];
				guns.RemoveAt(i);
				
				var gs = (GameObject)GameObject.Instantiate(GOs.Get("GunSpawn"));
				gs.transform.position = 
				child.transform.position;

				var sd = gs.GetComponent<SpawnData>();
				sd.Gun = (int)gun;

				s += gun + ", ";
				
				if (gun == Gun.None) { // don't think this can ever happen anymore 
					Destroy(child.gameObject);
				}else{
					GunSpawns.Add(sd);
				}
			}
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
	
	// ------------- Connecting/Server setup -------------- 
	void OnServerInitialized() {
        //Debug.Log("Server initialized, now registering...");
		MasterServer.RegisterHost(MasterServerFacingName, MatchName, MatchTypeAndMap);
    }
	
    void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.RegistrationSucceeded) {
            //Debug.Log("Server registered");
			InServerMode = true;
			
			if (!Connected) {
				// we've just joined a game as host, lets create the local player & add it to the RPC buffer 
				LocEnt.viewID = Network.AllocateViewID();
				hud.Mode = HudMode.Wait;
				NetVI = Network.AllocateViewID();
				RequestGameData();
			}
			
			Connected = true;
		}else if (msEvent == MasterServerEvent.RegistrationFailedNoServer || 
		          msEvent == MasterServerEvent.RegistrationFailedGameType || 
		          msEvent == MasterServerEvent.RegistrationFailedGameName)
		{
			Debug.LogError("server registration failed, disconnecting");
			Error = "server registration failed";
			hud.Mode = HudMode.ConnectionError;
			LocEnt.viewID = new NetworkViewID();
			NetVI = new NetworkViewID();
			Network.Disconnect();
		}
    }
	
	void OnConnectedToServer() {
		//Debug.Log("Connected to a server");
		Connected = true;
		// we just connected to a host, let's RPC the host and ask for the game info
		networkView.RPC("RequestGameData", RPCMode.Server);
		hud.Mode = HudMode.Wait;
		latestPacket = Time.time;
		LocEnt.viewID = Network.AllocateViewID();
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
			Error = "Invalid/incorrect password!";
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
		LocEnt.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
		hud.Mode = HudMode.ConnectionError;
	}
	
	//--------- Disconnecting, quitting, kicking -----------
	void OnApplicationQuit() {
		DisconnectNow();
	}
	
	public void DisconnectNow() {
		if (Connected) {
			if (!InServerMode) {
				networkView.RPC("PlayerLeave", RPCMode.OthersBuffered, LocEnt.viewID, LocEnt.name);
			}else{
				networkView.RPC("ServerLeave", RPCMode.OthersBuffered);
			}
			
			LocEnt.viewID = new NetworkViewID();
			NetVI = new NetworkViewID();
			
			Network.Disconnect();
			if (InServerMode) 
				MasterServer.UnregisterHost();
			Connected = false;
			InServerMode = false;
			
			Camera.main.transform.parent = null;
			for (int i=0; i<Entities.Count; i++) {
				if (Entities[i].Actor != null) 
					Destroy(Entities[i].Actor.gameObject);
			}

			Entities = new List<NetEntity>();
			
			hud.Mode = HudMode.MainMenu;
			Application.LoadLevel(nameOfOfflineBackdrop);
			playableMapIsReady = false;
		}
	}
	
	
	[RPC]
	void PlayerLeave(NetworkViewID viewID, string name) {
		latestPacket = Time.time;
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) {
				
				if (basketball && Entities[i].hasBall) {
					basketball.transform.parent = null;
					if (InServerMode) {
						ThrowBall(Entities[i].Actor.transform.position, -Vector3.up, 2f);
					}
				}
				
				
				if (Entities[i].Actor != null) 
					Destroy(Entities[i].Actor.gameObject);
				Entities.RemoveAt(i);
			}
		}
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.grey;
		newMsg.Text = "<< " + name + " left >>";
		log.Entries.Add(newMsg);
		log.TimeToHideEntireLog = Time.time+log.FadeTime;
	}
	
	public void Kick(int playerIndex, bool autokick){
		Network.CloseConnection(Entities[playerIndex].netPlayer, true);
		networkView.RPC("KickedPlayer", RPCMode.AllBuffered, Entities[playerIndex].viewID, autokick);
	}
	
	[RPC]
	void KickedPlayer(NetworkViewID viewID, bool autokick) {
		latestPacket = Time.time;
		Debug.Log("A player was kicked");
		string name = "???";
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].viewID == viewID) {
				
				if (basketball && Entities[i].hasBall) {
					basketball.transform.parent = null;
					
					if (InServerMode) {
						ThrowBall(Entities[i].Actor.transform.position, -Vector3.up, 2f);
					}
				}
				
				name = Entities[i].name;
				if (Entities[i].Actor!= null) 
					Destroy(Entities[i].Actor.gameObject);
				Entities.RemoveAt(i);
			}
		}
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.grey;
		newMsg.Text = "<< ! " + name + " was kicked ! >>";
		if (autokick) 
			newMsg.Text = "<< ! " + name + " was auto-kicked ! >>";
		log.Entries.Add(newMsg);
		log.TimeToHideEntireLog = Time.time + log.FadeTime;
		
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection netDis) {
		Debug.Log("Disconnected from server");
		Time.timeScale = 1f;
		Physics.gravity = new Vector3(0f, -10f, 0f);
		if (InServerMode) 
			return;
		
		LogEntry newMsg = new LogEntry();
		newMsg.Maker = "";
		newMsg.Color = Color.white;
		newMsg.Text = "<< You have been disconnected from host >>";
		log.Entries.Add(newMsg);
		log.TimeToHideEntireLog = Time.time+log.FadeTime;
		
		//Network.Disconnect();
		//if (isServer) MasterServer.UnregisterHost();
		Connected = false;
		InServerMode = false;
		
		LocEnt.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
				
		Camera.main.transform.parent = null;
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].Actor!= null) 
				Destroy(Entities[i].Actor.gameObject);
		}

		Entities = new List<NetEntity>();
		
		hud.Mode = HudMode.MainMenu;
		Application.LoadLevel(nameOfOfflineBackdrop);
		playableMapIsReady = false;
	}
	
	[RPC]
	void ServerLeave() {
		latestPacket = Time.time;
		Debug.Log("THE HOST LEFT!!!");
		
		Network.Disconnect();
		if (InServerMode) 
			MasterServer.UnregisterHost();
		
		Connected = false;
		InServerMode = false;
		LocEnt.viewID = new NetworkViewID();
		NetVI = new NetworkViewID();
		Camera.main.transform.parent = null;
		
		for (int i=0; i<Entities.Count; i++) {
			if (Entities[i].Actor != null) 
				Destroy(Entities[i].Actor.gameObject);
		}
		
		Entities = new List<NetEntity>();
		hud.Mode = HudMode.MainMenu;
		Application.LoadLevel(nameOfOfflineBackdrop);
		playableMapIsReady = false;
		var le = new LogEntry();
		le.Maker = "";
		le.Color = Color.grey;
		le.Text = "<< Host has left >>";
		log.Entries.Add(le);
		log.TimeToHideEntireLog = Time.time + log.FadeTime;
	}


	
	#region Map
	
	bool prevWas = false; // previously was generating map? 
	private void setupMapIfReady() {
		// if done generating map 
		if (prevWas && !vGen.Generating)
			setupSpawns();
		
		prevWas = vGen.Generating;
	}

	#endregion
}
