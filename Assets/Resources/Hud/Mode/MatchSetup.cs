using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MatchSetup {
	Vector2 scrollPos = Vector2.zero;
	int matchId = 1;
	int mapId = 0;
	MatchData[] matches;

	// sizes, spans 
	Rect screen;
	Rect middleVerticalStrip; // ... of the screen (minus the header and start button heights) 
	float thirdOfWidth;



	public void Init() {
		setupMatchTypes();
	}

	public void Draw(bool serving, CcNet net, Hud hud, int vSpan) {
		int qSpan = Screen.width / 4; // quarter of screen width span 
		Rect startButton = new Rect(qSpan, Screen.height - vSpan*4, qSpan*2, vSpan*4);
		screen = new Rect(0, 0, Screen.width, Screen.height);
		thirdOfWidth = screen.width / 3;

		// show map picture background 
		var map = matches[matchId].Maps[mapId];
		GUI.DrawTexture(screen, Pics.Get(map));

		// -------------------- gui -------------------- 
		GUI.BeginGroup(screen);
		
		middleVerticalStrip = screen;

		// make column of 1/3 of horizontal space (down the middle)
		middleVerticalStrip.x = thirdOfWidth;
		middleVerticalStrip.width = thirdOfWidth;
		middleVerticalStrip.height = Screen.height - startButton.height;
		
		GUI.color = S.WhiteTRANS;
		GUI.DrawTexture(middleVerticalStrip, Pics.White);
		GUI.color = Color.white;
		GUILayout.BeginArea(middleVerticalStrip);

		// header
		GUILayout.Box("MATCH SETUP");

		// extra STARTING options (not needed for match in progress)
		if (!serving) {
			serverSetup(net, hud);
		}
		
		// match type change might need us to show a different (allowed) map
		selectMatchType();

		// if not custom 
		if (matchId != 0) {
			showMapSelector();
		}



		// show customize button, OR show all custom options 
		// if not custom 
		if (matchId != 0) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Customize Match") ) {
				int levelChangeIndex = 0;
				for (int i=0; i<matches[0].Maps.Count; i++) {
					if (matches[0].Maps[i] == matches[matchId].Maps[mapId]) 
						levelChangeIndex = i;
				}
				
				mapId = levelChangeIndex;
				
				matches[0].killsIncreaseScore = matches[matchId].killsIncreaseScore;
				matches[0].deathsSubtractScore = matches[matchId].deathsSubtractScore;
				matches[0].respawnWait = matches[matchId].respawnWait;
				matches[0].teamBased = matches[matchId].teamBased;
				matches[0].FriendlyFire = matches[matchId].FriendlyFire;
				matches[0].pitchBlack = matches[matchId].pitchBlack;
				matches[0].Duration = matches[matchId].Duration;
				matches[0].winScore = matches[matchId].winScore;
				matches[0].spawnGunA = matches[matchId].spawnGunA;
				matches[0].spawnGunB = matches[matchId].spawnGunB;
				matches[0].pickupSlot1 = matches[matchId].pickupSlot1;
				matches[0].pickupSlot2 = matches[matchId].pickupSlot2;
				matches[0].pickupSlot3 = matches[matchId].pickupSlot3;
				matches[0].pickupSlot4 = matches[matchId].pickupSlot4;
				matches[0].pickupSlot5 = matches[matchId].pickupSlot5;
				matches[0].restockTime = matches[matchId].restockTime;
				matches[0].playerLives = matches[matchId].playerLives;
				matches[0].basketball = matches[matchId].basketball;
				matches[0].MoveSpeedMult = matches[matchId].MoveSpeedMult;
				
				matchId = 0;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}else{ // custom, show options here 
			GUILayout.Box("Custom Settings");

			scrollPos = GUILayout.BeginScrollView(scrollPos);
			
			showMapSelector();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Box("Limits");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("(0 ignores)");
			GUILayout.Label("");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Frags: ");
			matches[matchId].winScore = S.GetInt( GUILayout.TextField(matches[matchId].winScore.ToString()) );
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Minutes:");
			matches[matchId].Duration = (float)S.GetDouble( GUILayout.TextField(matches[matchId].Duration.ToString()) );
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Lives:");
			matches[matchId].playerLives = S.GetInt( GUILayout.TextField(matches[matchId].playerLives.ToString()) );
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Label("");
			GUILayout.Label("");

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			//hud.CategoryHeader("Misc");
			GUILayout.Box("Misc");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// checkbox toggles 
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			matches[matchId].killsIncreaseScore = TickBox.Display(matches[matchId].killsIncreaseScore, "Kills Increase score");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			matches[matchId].deathsSubtractScore = TickBox.Display(matches[matchId].deathsSubtractScore, "Deaths Reduce score");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			matches[matchId].pitchBlack = TickBox.Display(matches[matchId].pitchBlack, "Pitch Black");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			matches[matchId].teamBased = TickBox.Display(matches[matchId].teamBased, "Team Based");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if (matches[matchId].teamBased) {
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				matches[matchId].FriendlyFire = TickBox.Display(matches[matchId].FriendlyFire, "Friendly Fire");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				matches[matchId].basketball = TickBox.Display(matches[matchId].basketball, "Basketball");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Player Respawn:");
			matches[matchId].respawnWait = S.GetInt( GUILayout.TextField(matches[matchId].respawnWait.ToString()) );
			GUILayout.Label("(secs)");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Item Repawn:");
			matches[matchId].restockTime = S.GetInt(GUILayout.TextField(matches[matchId].restockTime.ToString()) );
			GUILayout.Label("(secs)");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Time Multiplier:");
			GUILayout.Label(matches[matchId].MoveSpeedMult.ToString());
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			matches[matchId].MoveSpeedMult = GUILayout.HorizontalSlider(matches[matchId].MoveSpeedMult, 0.1f, 10f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Gravity Force:");
			GUILayout.Label(matches[matchId].Gravity.ToString());
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			matches[matchId].Gravity = GUILayout.HorizontalSlider(matches[matchId].Gravity, 0.1f, 30f);
			GUILayout.EndHorizontal();

			GUILayout.Label("");
			GUILayout.Label("");

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Box("Items");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// spawn gun A 
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("<"))
				wrapALeft();
			if (GUILayout.Button(">"))
				wrapARight();

			if (gunsAreTheSame())
				wrapBRight();

			GUILayout.Label("Inventory A: ");
			GUILayout.FlexibleSpace();
			GUILayout.Label(S.GetSpacedOut("" + matches[matchId].spawnGunA));

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			// spawn gun B 
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("<")) {
				wrapBLeft();

				if (gunsAreTheSame())
					wrapBLeft();
			}

			if (GUILayout.Button(">")) {
				wrapBRight();

				if (gunsAreTheSame())
					wrapBRight();
			}


			// ****************************************************************





			GUILayout.Label("Inventory B: ");
			GUILayout.FlexibleSpace();
			GUILayout.Label(S.GetSpacedOut("" + matches[matchId].spawnGunB));

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Label("");
			
			slotSelect(ref matches[matchId].pickupSlot1, 1);
			slotSelect(ref matches[matchId].pickupSlot2, 2);
			slotSelect(ref matches[matchId].pickupSlot3, 3);
			slotSelect(ref matches[matchId].pickupSlot4, 4);
			slotSelect(ref matches[matchId].pickupSlot5, 5);

			GUILayout.EndScrollView();
		}
		
		//print("Current map name: " + maps[mapId].Name);

		if (matches[matchId].Maps[mapId] == MatchData.VoxelName) {
			matches[matchId].NeedsGenerating = true;
		} else {
			matches[matchId].NeedsGenerating = false;
		}

		GUILayout.FlexibleSpace();
		if (GUILayout.Button(hud.GoToMainMenu/*, GUILayout.ExpandWidth(false)*/)) {
			hud.Mode = HudMode.MainMenu;
		}
		
		GUILayout.EndArea();


		// START button 
		if (!serving) {
			if (GUI.Button(startButton, "Start Game!")) {
				// init a server with the current game mode settings 
				net.serverGameChange = true;
				Network.incomingPassword = net.password;
				net.lastGameWasTeamBased = false;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].Maps[mapId]);
				net.MatchTypeAndMap = net.CurrMatch.Name + "\n" + net.CurrMatch.MapName;
				bool useNat = !Network.HavePublicAddress();
				//Debug.Log("Initializing server, has public address: " + Network.HavePublicAddress().ToString());
				Network.InitializeServer(net.connections, net.listenPort, useNat);
				hud.Mode = HudMode.InitializingServer;
			}
		}else{
			if (GUI.Button(startButton, "Start Match!")) {
				net.serverGameChange = true;
				net.lastGameWasTeamBased = net.CurrMatch.teamBased;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].Maps[mapId]);
				net.NetVI = Network.AllocateViewID();
				net.RequestGameData();
			}
		}

		GUI.EndGroup();
	}

	void serverSetup(CcNet net, Hud hud) {
		// server name 
		GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Name: ");
			net.MatchName = GUILayout.TextField(net.MatchName);
			GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// num users 
		var maxUsers = "Max Users: ";
		float w = hud.GetWidthLabel(maxUsers + " 32");

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(maxUsers + net.connections, GUILayout.MinWidth(w));
		net.connections = (int)Mathf.Round(GUILayout.HorizontalSlider(net.connections, 2, 32, GUILayout.MinWidth(64)));
		GUILayout.FlexibleSpace();
		//GUILayout.EndHorizontal();

		// pw 
		//GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();
		GUILayout.Label("Password: ");
		net.password = GUILayout.TextField(net.password, GUILayout.MinWidth(16));
		GUILayout.FlexibleSpace();

		// port 
		GUILayout.FlexibleSpace();
		GUILayout.Label("Port: ");
		net.listenPort = S.GetInt(GUILayout.TextField(net.listenPort.ToString ()));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	void selectMatchType() {
		GUILayout.Label("");

		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("<") ) {
			int lastInt = matchId;
			
			matchId--;
			if (matchId < 0) 
				matchId = matches.Length - 1;
			
			int mapChangeId = 0;
			for (int i=0; i<matches[matchId].Maps.Count; i++) {
				if (matches[matchId].Maps[i] == 
				    matches[lastInt].Maps[mapId]
				    ) 
					mapChangeId = i;
			}
			
			mapId = mapChangeId;
		}
		
		if (GUILayout.Button(">") ) {
			int lastInt = matchId;
			
			matchId++;
			if (matchId >= matches.Length) 
				matchId = 0;
			
			int mapChangeId = 0;
			for (int i=0; i<matches[matchId].Maps.Count; i++) {
				if (matches[matchId].Maps[i] == matches[lastInt].Maps[mapId]) 
					mapChangeId = i;
			}
			
			mapId = mapChangeId;
		}


		GUILayout.Label("Mode: ");
		GUILayout.FlexibleSpace();
		GUILayout.Label(matches[matchId].Name);
		GUILayout.FlexibleSpace();
		


		GUILayout.EndHorizontal();

		// description 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(matches[matchId].Descript);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Label("");
	}

	void slotSelect(ref Gun item, int slot) {
		GUILayout.BeginHorizontal();

		// cycle 
		if (GUILayout.Button("<")) 
			item--;
		if (GUILayout.Button(">")) 
			item++;

		// wrap 
		if (item < Gun.Health) 
			item = Gun.Count-1;
		if (item >= Gun.Count) 
			item = Gun.Health;

		GUILayout.Label("Pickup " + slot + ": ");
		GUILayout.FlexibleSpace();
		GUILayout.Label(S.GetSpacedOut("" + item));
		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();
	}

	void showMapSelector() {
		// [<] [>]   buttons 
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("<") ) {
			mapId--;
			if (mapId < 0) 
				mapId += matches[matchId].Maps.Count;
		}
		
		if (GUILayout.Button(">") ) {
			mapId++;
			if (mapId >= matches[matchId].Maps.Count) 
				mapId -= matches[matchId].Maps.Count;
		}

		GUILayout.Label ("Map:");
		GUILayout.FlexibleSpace ();
		// set friendly map text for player 
		var n = matches [matchId].Maps [mapId];
		// name of map/scene 
		var pgName = MatchData.VoxelName;
		if (n == pgName) {
			GUILayout.Label ("(" + pgName + ": ");
			matches [matchId].Seed = int.Parse (GUILayout.TextArea (matches [matchId].Seed.ToString ()));
			GUILayout.Label (") ");
		}
		else {
			// handmade map 
			GUILayout.Label (n);
		}
		GUILayout.FlexibleSpace ();

		GUILayout.EndHorizontal();
		GUILayout.Label("");
	}
	
	void setupMatchTypes() {
		matches = new MatchData[(int)Match.Count];
		for (int i = 0; i < matches.Length; i++)
			matches[i] = new MatchData((Match)i);

		if (Debug.isDebugBuild) {
			matchId = (int)Match.BringYourOwnGravity;
		}
	}
	
	bool gunsAreTheSame() {
		if (matches[matchId].spawnGunA == 
			matches[matchId].spawnGunB
	    )
			return true;

		return false;
	}

	void wrapALeft() {
		matches[matchId].spawnGunA--;
		
		if (matches[matchId].spawnGunA < Gun.Pistol) 
			matches[matchId].spawnGunA = Gun.Count-1;
	}
	void wrapARight() {
		matches[matchId].spawnGunA++;
		
		if (matches[matchId].spawnGunA >= Gun.Count) 
			matches[matchId].spawnGunA = Gun.Pistol;
	}
	void wrapBLeft() {
		matches[matchId].spawnGunB--;
		
		if (matches[matchId].spawnGunB < Gun.Pistol) 
			matches[matchId].spawnGunB = Gun.Count-1;
	}
	void wrapBRight() {
		matches[matchId].spawnGunB++;

		if (matches[matchId].spawnGunB >= Gun.Count) 
			matches[matchId].spawnGunB = Gun.Pistol;
	}
}
