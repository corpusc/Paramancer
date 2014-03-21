using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchSetup {
	Vector2 scrollPos = Vector2.zero;
	int matchId = 1;
	int mapId = 0;
	MatchData[] matches;
	List<MapData> maps = new List<MapData>();

	// sizes, spans
	Rect screen;
	Rect middleVerticalStrip; // ... of the screen (minus the header and start button heights)
	float thirdOfWidth;



	public void Init() {
		// load map preview pics
		UnityEngine.Object[] pics = Resources.LoadAll("Pic/MapPreview");
		
		// setup map configs
		for (int i = 0; i < pics.Length; i++)
			maps.Add(new MapData(pics[i].name, (Texture)pics[i]) );
		
		setupMatchTypes();
	}

	public void Draw(bool serving, CcNet net, Hud hud, int vSpan) {
		int qSpan = Screen.width / 4; // quarter of screen width span
		Rect startButton = new Rect(qSpan, Screen.height - vSpan*4, qSpan*2, vSpan*4);
		screen = new Rect(0, 0, Screen.width, Screen.height);
		thirdOfWidth = screen.width / 3;
		int lpi = vSpan + vSpan/2; // line position increment

		// show map picture background
		for (int i=0; i<maps.Count; i++) {
			if (maps[i].Name == matches[matchId].allowedLevels[mapId]) {
				GUI.DrawTexture(screen, maps[i].Pic);
			}
		}

		// -------------------- gui --------------------
		GUI.BeginGroup(screen);
		
		// header
		middleVerticalStrip = screen;
		middleVerticalStrip.height = lpi;
		GUI.Box(middleVerticalStrip, "MATCH SETUP");

		// make column of 1/3 of horizontal space (down the middle)
		middleVerticalStrip.x = thirdOfWidth;
		middleVerticalStrip.y = lpi;
		middleVerticalStrip.width = thirdOfWidth;
		middleVerticalStrip.height = Screen.height - startButton.height - lpi;
		
		GUILayout.BeginArea(middleVerticalStrip);

		// extra STARTING options (not needed for match in progress)
		if (!serving) {
			serverSetup(net, hud, lpi);
		}
		
		// match type change might need us to show a different (allowed) map
		selectMatchType();

		// select map
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Level:");

		if (GUILayout.Button("<") ) {
			mapId--;
			if (mapId < 0) 
				mapId += matches[matchId].allowedLevels.Length;
		}
		
		GUILayout.Label(matches[matchId].allowedLevels[mapId]);

		if (GUILayout.Button(">") ) {
			mapId++;
			if (mapId >= matches[matchId].allowedLevels.Length) 
				mapId -= matches[matchId].allowedLevels.Length;
		}
		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		// customize button, or show all options
		if (matchId != 0) { // if not custom
			// description
			GUILayout.Label(matches[matchId].Descript);
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Customize Match") ) {
				int levelChangeIndex = 0;
				for (int i=0; i<matches[0].allowedLevels.Length; i++) {
					if (matches[0].allowedLevels[i] == matches[matchId].allowedLevels[mapId]) 
						levelChangeIndex = i;
				}
				
				mapId = levelChangeIndex;
				
				matches[0].killsIncreaseScore = matches[matchId].killsIncreaseScore;
				matches[0].deathsSubtractScore = matches[matchId].deathsSubtractScore;
				matches[0].respawnWait = matches[matchId].respawnWait;
				matches[0].teamBased = matches[matchId].teamBased;
				matches[0].allowFriendlyFire = matches[matchId].allowFriendlyFire;
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
				
				matchId = 0;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}else{ // custom, show options here
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			
			matches[matchId].killsIncreaseScore = GUILayout.Toggle(matches[matchId].killsIncreaseScore, "Kills Increase score");
			matches[matchId].deathsSubtractScore = GUILayout.Toggle(matches[matchId].deathsSubtractScore, "Deaths Reduce score");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Respawn Time: ");
			matches[matchId].respawnWait = S.GetInt( GUILayout.TextField(matches[matchId].respawnWait.ToString()) );
			GUILayout.EndHorizontal();
			
			matches[matchId].teamBased = GUILayout.Toggle(matches[matchId].teamBased, "Team Based");
			
			if (matches[matchId].teamBased)
				matches[matchId].basketball = GUILayout.Toggle(matches[matchId].basketball, "Basketball");
			else
				matches[matchId].basketball = false;
			
			
			matches[matchId].allowFriendlyFire = GUILayout.Toggle(matches[matchId].allowFriendlyFire, "Allow Friendly Fire");
			
			matches[matchId].pitchBlack = GUILayout.Toggle(matches[matchId].pitchBlack, "Pitch Black");
			
			GUILayout.Label(" --- Round end conditions (set to 0 to ignore) --- ");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Round Time (minutes): ");
			matches[matchId].Duration = S.GetInt( GUILayout.TextField(matches[matchId].Duration.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Winning score: ");
			matches[matchId].winScore = S.GetInt( GUILayout.TextField(matches[matchId].winScore.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Player Lives: ");
			matches[matchId].playerLives = S.GetInt( GUILayout.TextField(matches[matchId].playerLives.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- Weapon Settings --- ");
			
			// spawn gun A
			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("<")) 
				matches[matchId].spawnGunA--;
			if (matches[matchId].spawnGunA < Item.Random) 
				matches[matchId].spawnGunA = Item.Count-1;
			
			GUILayout.Label("Spawn Gun A: " + matches[matchId].spawnGunA);
			
			if (GUILayout.Button(">")) 
				matches[matchId].spawnGunA++;
			if (matches[matchId].spawnGunA >= Item.Count) 
				matches[matchId].spawnGunA = Item.Random;
			GUILayout.EndHorizontal();
			
			// spawn gun B
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].spawnGunB--;
			if (matches[matchId].spawnGunB < Item.Random) 
				matches[matchId].spawnGunB = Item.Count-1;
			
			GUILayout.Label("Spawn Gun B: " + matches[matchId].spawnGunB);
			
			if (GUILayout.Button(">")) 
				matches[matchId].spawnGunB++;
			if (matches[matchId].spawnGunB >= Item.Count) 
				matches[matchId].spawnGunB = Item.Random;
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- ");
			
			slotSelect(ref matches[matchId].pickupSlot1, 1);
			slotSelect(ref matches[matchId].pickupSlot2, 2);
			slotSelect(ref matches[matchId].pickupSlot3, 3);
			slotSelect(ref matches[matchId].pickupSlot4, 4);
			slotSelect(ref matches[matchId].pickupSlot5, 5);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Restock time (seconds): ");
			matches[matchId].restockTime = S.GetInt(GUILayout.TextField(matches[matchId].restockTime.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.EndScrollView();
		}
		
		if (GUILayout.Button("CANCEL")) {
			hud.Mode = HudMode.MenuMain;
		}
		
		GUILayout.EndArea();



		// START button
		if (!serving) {
			if (GUI.Button(startButton, "Start Game!")) {
				// init a server with the current game mode settings
				net.serverGameChange = true;
				Network.incomingPassword = net.password;
				net.lastGameWasTeamBased = false;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].allowedLevels[mapId]);
				net.MatchTypeAndMap = net.CurrMatch.Name + "\n" + net.CurrMatch.levelName;
				bool useNat = !Network.HavePublicAddress();
				Debug.Log("Initializing server, has public address: " + Network.HavePublicAddress().ToString());
				Network.InitializeServer(net.connections,net.listenPort, useNat);
				hud.Mode = HudMode.InitializingServer;
			}
		}else{
			if (GUI.Button(startButton, "Start Match!")) {
				net.serverGameChange = true;
				net.lastGameWasTeamBased = net.CurrMatch.teamBased;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].allowedLevels[mapId]);
				net.NetVI = Network.AllocateViewID();
				net.RequestGameData();
			}
		}

		GUI.EndGroup();
	}

	void serverSetup(CcNet net, Hud hud, int lpi) {
		GUILayout.BeginHorizontal(); {
			GUILayout.Label("Name: ");
			net.gameName = GUILayout.TextField(net.gameName);
			GUILayout.FlexibleSpace();
		} GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(); {
			GUILayout.Label("Password: ");
			net.password = GUILayout.TextField(net.password);
			GUILayout.FlexibleSpace();
		} GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Port: ");
		net.listenPort = S.GetInt(GUILayout.TextField(net.listenPort.ToString ()));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		hud.SizedLabel("Max Connections: " + net.connections);
		net.connections = (int)Mathf.Round(GUILayout.HorizontalSlider(net.connections, 2, 32));
		GUILayout.EndHorizontal();
	}

	void selectMatchType() {
		GUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace();
		GUILayout.Label("Mode: ");
		
		if (GUILayout.Button("<") ) {
			int lastInt = matchId;
			
			matchId--;
			if (matchId < 0) 
				matchId += matches.Length;
			
			int mapChangeId = 0;
			for (int i=0; i<matches[matchId].allowedLevels.Length; i++) {
				if (matches[matchId].allowedLevels[i] == 
				    matches[lastInt].allowedLevels[mapId]
				    ) 
					mapChangeId = i;
			}
			
			mapId = mapChangeId;
		}
		
		GUILayout.Label(matches[matchId].Name);
		
		if (GUILayout.Button(">") ) {
			int lastInt = matchId;
			
			matchId++;
			if (matchId >= matches.Length) 
				matchId -= matches.Length;
			
			int mapChangeId = 0;
			for (int i=0; i<matches[matchId].allowedLevels.Length; i++) {
				if (matches[matchId].allowedLevels[i] == matches[lastInt].allowedLevels[mapId]) 
					mapChangeId = i;
			}
			
			mapId = mapChangeId;
		}
		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();
	}

	void slotSelect(ref Item item, int slot) {
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<")) 
			item--;
		
		if (item < Item.Health) 
			item = Item.Count-1;
		
		GUILayout.Label("Pickup Slot " + slot + S.GetSpacedOut(": " + item));
		
		if (GUILayout.Button(">")) 
			item++;
		
		if (item >= Item.Count) 
			item = Item.Health;
		GUILayout.EndHorizontal();
	}
	
	void setupMatchTypes() {
		matches = new MatchData[(int)Match.Count];
		for (int i = 0; i < matches.Length; i++)
			matches[i] = new MatchData((Match)i);
	}
}
