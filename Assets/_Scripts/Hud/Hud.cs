using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class Hud : MonoBehaviour {
	// swapper
	public int swapperCrossX = 0;
	public int swapperCrossY = 0;
	public bool swapperLocked = false;
	
	// textures
	public Texture[] swapperCrosshair;
	public Texture lifeIcon;
	public Texture backTex;
	public Texture blackTex;
	public Texture crossHair;
	public Texture teamRedFlag;
	public Texture teamBlueFlag;
	public Texture gameLogo;
	public Texture companyLogo;
	
	// misc
	public string offeredPickup = "";
	public Menu OfflineMenu = Menu.Main;
	public Menu OnlineMenu = Menu.Controls;
	
	public bool Spectating = false;
	public int Spectatee = 0;
	
	public float gunACooldown = 0f;
	public int gunA = 0;
	public int gunB = 0;
	
	public float EnergyLeft = 1f; //0-1
	
	// private
	Rect screen;
	bool viewingScores = false;
	string defaultName = "Lazy Noob";
	Scoreboard scores = new Scoreboard();
	
	// UI element sizes
	Vector2 scrollPos = Vector2.zero;
	Rect window = new Rect(0, 0, 600, 400);
	Rect button = new Rect(0, 0, 100, 40); // fixme: are buttons always 40 in height?
	int vSpan = 20; // fixme: hardwired vertical span of the font.  doubled in places for buttons
	
	// match & map
	int matchId = 1;
	int mapId = 0;
	MatchData[] matches;
	List<MapData> maps = new List<MapData>();

	// scripts
	CcNet net;
	CcLog log;
	Arsenal arse;
	ControlsGui controGui;
	LocalUser locUser;

	
	
	void Start() {
		// scripts
		net = GetComponent<CcNet>();
		log = GetComponent<CcLog>();
		arse = GetComponent<Arsenal>();
		controGui = GetComponent<ControlsGui>();
		locUser = GetComponent<LocalUser>();
		
		// load map preview pics
		UnityEngine.Object[] pics = Resources.LoadAll("Pic/Map");
		
		// setup map configs
		for (int i = 0; i < pics.Length; i++)
			maps.Add(new MapData(pics[i].name, (Texture)pics[i]) );
		
		setupMatchTypes();
		loadHudPics();
		
		// make local player
		net.localPlayer = new NetUser();
		net.localPlayer.local = true;
		net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
		net.localPlayer.headType = PlayerPrefs.GetInt("PlayerHead", 0);
		net.localPlayer.colA.r = PlayerPrefs.GetFloat("PlayerColA_R", Color.yellow.r);
		net.localPlayer.colA.g = PlayerPrefs.GetFloat("PlayerColA_G", Color.yellow.g);
		net.localPlayer.colA.b = PlayerPrefs.GetFloat("PlayerColA_B", Color.yellow.b);
		net.localPlayer.colA.a = 1;
		net.localPlayer.colB.r = PlayerPrefs.GetFloat("PlayerColB_R", Color.green.r);
		net.localPlayer.colB.g = PlayerPrefs.GetFloat("PlayerColB_G", Color.green.g);
		net.localPlayer.colB.b = PlayerPrefs.GetFloat("PlayerColB_B", Color.green.b);
		net.localPlayer.colB.a = 1;
		net.localPlayer.colC.r = PlayerPrefs.GetFloat("PlayerColC_R", Color.cyan.r);
		net.localPlayer.colC.g = PlayerPrefs.GetFloat("PlayerColC_G", Color.cyan.g);
		net.localPlayer.colC.b = PlayerPrefs.GetFloat("PlayerColC_B", Color.cyan.b);
		net.localPlayer.colC.a = 1;
		
		// load settings
		log.FadeTime = PlayerPrefs.GetFloat("textFadeTime", 10f);
		net.gunBobbing = PlayerPrefs.GetInt("GunBobbing", 1) == 1;
		net.autoPickup = PlayerPrefs.GetInt("autoPickup", 0) == 1;
		net.gameVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
	}
	
	void Update() {
		if (Screen.lockCursor == true && InputUser.Started(UserAction.Menu)) 
			Screen.lockCursor = false;
		
		if (InputUser.Started(UserAction.Scores))
			viewingScores = !viewingScores;
	}

	void OnGUI() {
		controGui.enabled = false;
		
		// sizes of UI elements
		screen = new Rect(0, 0, Screen.width, Screen.height);
		int midX = Screen.width/2;
		int midY = Screen.height/2;
		window.y = Screen.height - window.height;
		var br = window; // back button rectangle
		br.y -= vSpan * 2;
		br.width = 100; // fixme for actual string width?
		br.height = vSpan * 2;
		
		// show map picture, if setting up match
		bool settingUpMatch;
		if ( (net.connected && OnlineMenu == Menu.Match) ||
			(!net.connected && OfflineMenu == Menu.StartGame)
		) {
			for (int i=0; i<maps.Count; i++) {
				if (maps[i].Name == matches[matchId].allowedLevels[mapId]) {
					GUI.DrawTexture(screen, maps[i].Pic);
				}
			}
			
			settingUpMatch = true;
		}else
			settingUpMatch = false;
	

		
		if (OfflineMenu == Menu.Main || OfflineMenu == Menu.Avatar)
			avatarView();
			
		if (net.connected)
			onlineMenus(midX, midY, settingUpMatch);
		else
			offlineMenus(br);
	}
	
	
	
	
	
	
	
	
	
	
	
	
	string TimeStringFromSecs(float totalSecs) {
		string timeString = "";
		int seconds = Mathf.FloorToInt(totalSecs);
		
		int minutes = 0;
		while(seconds > 60) {
			seconds -= 60;
			minutes++;
		}
		
		int hours = 0;
		while (minutes > 60) {
			minutes -= 60;
			hours++;
		}
		
		if (hours > 0) {
			timeString += hours.ToString() + ":";
			if (minutes < 10) timeString += "0";
		}
		timeString += minutes.ToString() + ":";
		if (seconds < 10) timeString += "0";
		timeString += seconds.ToString();
		
		return timeString;
	}
	
	
	
	
	
	
	
	
	void DrawWindowBackground(bool halfWidth = false) {
		DrawWindowBackground(window, halfWidth);
	}
	void DrawWindowBackground(Rect r, bool halfWidth = false) {
		GUI.color = new Color(0.3f, 0f, 0.4f, 0.7f);
		
		if (halfWidth)
			r.width /= 2;
		
		GUI.DrawTexture(r, backTex);
		GUI.color = new Color(0.8f, 0f, 1f, 1f);
	}
	
	
	
	
	
	
	
	
	
	
	string FormatName(string s) {
		string ret = "";
		
		for (int i=0; i<s.Length; i++) {
			bool pass = true;
			if (s.Substring(i, 1) == " ")
				if (i<s.Length-1)
					if (s.Substring(i+1,1) == " ")
						pass = false;
			
			if (s.Substring(i, 1) == "\n") pass = false;
			if (s.Substring(i, 1) == "	") pass = false;
			if (pass) 
				ret += s.Substring(i, 1);
		}
		
		return ret;
	}
	
	
	
	
	
	
	
	
	void DrawMenuConfigAvatar() {
		GUI.BeginGroup(window);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: ");
		net.localPlayer.name = GUILayout.TextField(net.localPlayer.name);
		if (net.localPlayer.name.Length>20) 
			net.localPlayer.name = net.localPlayer.name.Substring(0,20);
		net.localPlayer.name = FormatName(net.localPlayer.name);
		GUILayout.EndHorizontal();
			
		GUILayout.Label("Colour A:");
		net.localPlayer.colA.r = GUILayout.HorizontalSlider(net.localPlayer.colA.r,0f,1f);
		net.localPlayer.colA.g = GUILayout.HorizontalSlider(net.localPlayer.colA.g,0f,1f);
		net.localPlayer.colA.b = GUILayout.HorizontalSlider(net.localPlayer.colA.b,0f,1f);
				
		GUILayout.Label("Colour B:");
		net.localPlayer.colB.r = GUILayout.HorizontalSlider(net.localPlayer.colB.r,0f,1f);
		net.localPlayer.colB.g = GUILayout.HorizontalSlider(net.localPlayer.colB.g,0f,1f);
		net.localPlayer.colB.b = GUILayout.HorizontalSlider(net.localPlayer.colB.b,0f,1f);
		
		GUILayout.Label("Colour C:");
		net.localPlayer.colC.r = GUILayout.HorizontalSlider(net.localPlayer.colC.r,0f,1f);
		net.localPlayer.colC.g = GUILayout.HorizontalSlider(net.localPlayer.colC.g,0f,1f);
		net.localPlayer.colC.b = GUILayout.HorizontalSlider(net.localPlayer.colC.b,0f,1f);
				
		GUILayout.Label("");
				
		if (GUILayout.Button("Head type: " + net.localPlayer.headType.ToString())) {
			net.localPlayer.headType++;
			if (net.localPlayer.headType>17) net.localPlayer.headType = 0;
		}
		
		if (net.localPlayer.headType == 11) GUILayout.Label("Head Credit: @Ast3c");
		if (net.localPlayer.headType == 12) GUILayout.Label("Head Credit: @IcarusTyler");
		if (net.localPlayer.headType == 13) GUILayout.Label("Head Credit: @LeanderCorp");
		if (net.localPlayer.headType == 14) GUILayout.Label("Head Credit: @kagai_shan");
		if (net.localPlayer.headType == 15) GUILayout.Label("Head Credit: @Ast3c");
		if (net.localPlayer.headType == 16) GUILayout.Label("Head Credit: @Ast3c");
		if (net.localPlayer.headType == 17) GUILayout.Label("Head Credit: @Ast3c");
				
		// save player
		if (net.localPlayer.name != "" && net.localPlayer.name != " ") {
			PlayerPrefs.SetString("PlayerName", net.localPlayer.name);
		}else{
			PlayerPrefs.SetString("PlayerName", defaultName);
		}
		
		PlayerPrefs.SetInt("PlayerHead", net.localPlayer.headType);
		PlayerPrefs.SetFloat("PlayerColA_R", net.localPlayer.colA.r);
		PlayerPrefs.SetFloat("PlayerColA_G", net.localPlayer.colA.g);
		PlayerPrefs.SetFloat("PlayerColA_B", net.localPlayer.colA.b);
		PlayerPrefs.SetFloat("PlayerColB_R", net.localPlayer.colB.r);
		PlayerPrefs.SetFloat("PlayerColB_G", net.localPlayer.colB.g);
		PlayerPrefs.SetFloat("PlayerColB_B", net.localPlayer.colB.b);
		PlayerPrefs.SetFloat("PlayerColC_R", net.localPlayer.colC.r);
		PlayerPrefs.SetFloat("PlayerColC_G", net.localPlayer.colC.g);
		PlayerPrefs.SetFloat("PlayerColC_B", net.localPlayer.colC.b);
		
		GUI.EndGroup();
	}
	
	
	
	
	
	
	
	
	
	void KickMenu() {
		GUI.BeginGroup(window);
		
		GUI.Label(new Rect(250,0,100,20), "Kick a player:");
		GUILayout.Label("\n\n\n");
		
		for (int i=0; i<net.players.Count; i++) {
			if (net.players[i].viewID != net.localPlayer.viewID) {
				GUILayout.BeginHorizontal();
				
				if (GUILayout.Button(Menu.Kick.ToString()))
					net.Kick(i, false);

				string pingString = "?";
				if (net.players[i].ping.isDone) 
					pingString = net.players[i].ping.time.ToString();
				
				GUILayout.Label("- " + net.players[i].name + " - [Ping: " + pingString + "]");
				
				GUILayout.EndHorizontal();
			}
		}
		
		GUI.EndGroup();
	}
	
	
	
	
	
	
	
	
	
	private int fsWidth = 1280;
	private int fsHeight = 720;
	void MenuControls() {
		controGui.enabled = true;
		Rect r = window;
		r.y = controGui.BottomMost;
		DrawWindowBackground(r, true);
		r.height = vSpan + vSpan / 2;
		GUI.Box(r, "Config:");
		
		r.height = window.height;
		GUI.BeginGroup(r);
		
		GUILayout.BeginArea(new Rect(5,vSpan*2,280,380));
		
		locUser.LookInvert = GUILayout.Toggle(locUser.LookInvert, "Mouselook inversion");
		GUILayout.Label("Mouse Sensitivity:");
		locUser.mouseSensitivity = GUILayout.HorizontalSlider(locUser.mouseSensitivity,0.1f,10f);
		
		if (GUILayout.Button("Reset Mouse")){
			locUser.LookInvert = false;
			locUser.mouseSensitivity = 2f;
			log.FadeTime = 10f;
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Chat messages fade time: ");
		log.FadeTime = (float)MakeInt(GUILayout.TextField(log.FadeTime.ToString()));
		GUILayout.EndHorizontal();
		
		net.gunBobbing = GUILayout.Toggle(net.gunBobbing, "Gun Bobbing");
		net.autoPickup = GUILayout.Toggle(net.autoPickup, "Auto-Pickup");
		
		GUILayout.BeginHorizontal();
		fsWidth = MakeInt(GUILayout.TextField(fsWidth.ToString()));
		fsHeight = MakeInt(GUILayout.TextField(fsHeight.ToString()));
		if (GUILayout.Button("Fullscreen")){
			Screen.SetResolution(fsWidth, fsHeight, true);
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Audio Volume:");
		net.gameVolume = GUILayout.HorizontalSlider(net.gameVolume,0.0f,1f);
		
		
		if (locUser.LookInvert) PlayerPrefs.SetInt("InvertY", 1);
		else /*``````````````*/ PlayerPrefs.SetInt("InvertY", 0);
		PlayerPrefs.SetFloat("MouseSensitivity", locUser.mouseSensitivity);
		PlayerPrefs.SetFloat("textFadeTime", log.FadeTime);
		
		if (net.gunBobbing)
			PlayerPrefs.SetInt("GunBobbing", 1);
		else
			PlayerPrefs.SetInt("GunBobbing", 0);
		
		if (net.autoPickup)
			PlayerPrefs.SetInt("autoPickup", 1);
		else
			PlayerPrefs.SetInt("autoPickup", 0);
		
		PlayerPrefs.SetFloat("GameVolume", net.gameVolume);
		
		GUILayout.EndArea();
		
		GUI.EndGroup();
	}
	
	
	
	
	
	
	
	
	
	void MatchSetup(bool serving) {
		GUI.BeginGroup(screen);
		
		if (serving)
			GUI.Label(new Rect(250,0,100,20), "Change game:");
		else {
			GUI.Label(new Rect(250,0,100,20), "Host a game:");
				
			// set up server
			GUILayout.BeginArea(new Rect(5,20,290,400)); {
				GUILayout.BeginHorizontal(); {
					GUILayout.Label("Game Name: ");
					net.gameName = GUILayout.TextField(net.gameName);
				} GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal(); {
					GUILayout.Label("Game Password: ");
					net.password = GUILayout.TextField(net.password);
				} GUILayout.EndHorizontal();
			} GUILayout.EndArea();
					
			// set up server connections/port
			GUILayout.BeginArea(new Rect(305,20,290,400));
			
			GUILayout.Label("Max Connections: " + net.connections.ToString());
			net.connections = (int)Mathf.Round(GUILayout.HorizontalSlider(net.connections, 2, 32));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Port: ");
			net.listenPort = MakeInt(GUILayout.TextField(net.listenPort.ToString()));
			GUILayout.EndHorizontal();
			
			GUILayout.EndArea();
		}
				
		// game mode
		if (GUI.Button(new Rect(5,100,30,30), "<") ) {
			int lastInt = matchId;
			
			matchId--;
			if (matchId < 0) 
				matchId += matches.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<matches[matchId].allowedLevels.Length; i++) {
				if (matches[matchId].allowedLevels[i] == 
					matches[lastInt].allowedLevels[mapId]
				) 
					levelChangeIndex = i;
			}
			
			mapId = levelChangeIndex;
		}
		
		if (GUI.Button(new Rect(255,100,30,30), ">") ) {
			int lastInt = matchId;
			
			matchId++;
			if (matchId >= matches.Length) 
				matchId -= matches.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<matches[matchId].allowedLevels.Length; i++) {
				if (matches[matchId].allowedLevels[i] == matches[lastInt].allowedLevels[mapId]) 
					levelChangeIndex = i;
			}
			
			mapId = levelChangeIndex;
		}
		
		GUI.Label(new Rect(60,100,200,30), "Mode: " + matches[matchId].Name);
				
		// game level
		if (GUI.Button(new Rect(305,100,30,30), "<") ) {
			mapId--;
			if (mapId < 0) 
				mapId += matches[matchId].allowedLevels.Length;
		}
		if (GUI.Button(new Rect(555,100,30,30), ">") ) {
			mapId++;
			if (mapId >= matches[matchId].allowedLevels.Length) 
				mapId -= matches[matchId].allowedLevels.Length;
		}
		
		GUI.Label(new Rect(360,100,200,30), "Level: " + matches[matchId].allowedLevels[mapId]);
				
		if (matchId != 0) { // not custom
			// description
			GUI.Label(new Rect(5,240,590,200), matches[matchId].Name + ":\n" + matches[matchId].Descript);
			
			if (GUI.Button(new Rect(495,240,100,25), "Customise...") ) {
				
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
		}else{ // custom, show options here
			scrollPos = GUI.BeginScrollView(new Rect(5,135,590,160), scrollPos, new Rect(0,0,570,700));
			
			matches[matchId].killsIncreaseScore = GUILayout.Toggle(matches[matchId].killsIncreaseScore, "Kills Increase score");
			matches[matchId].deathsSubtractScore = GUILayout.Toggle(matches[matchId].deathsSubtractScore, "Deaths Reduce score");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Respawn Time: ");
			matches[matchId].respawnWait = MakeInt( GUILayout.TextField(matches[matchId].respawnWait.ToString()) );
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
			matches[matchId].Duration = MakeInt( GUILayout.TextField(matches[matchId].Duration.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Winning score: ");
			matches[matchId].winScore = MakeInt( GUILayout.TextField(matches[matchId].winScore.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Player Lives: ");
			matches[matchId].playerLives = MakeInt( GUILayout.TextField(matches[matchId].playerLives.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- Weapon Settings --- ");
			
			// spawn gun A
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].spawnGunA--;
			if (matches[matchId].spawnGunA<-2) 
				matches[matchId].spawnGunA = arse.Guns.Length-1;
			
			string gunName = "none";
			if (matches[matchId].spawnGunA == -2) 
				gunName = "random";
			if (matches[matchId].spawnGunA >= 0) 
				gunName = arse.Guns[matches[matchId].spawnGunA].Name;
			GUILayout.Label("Spawn Gun A: " + gunName);
			if (GUILayout.Button(">")) 
				matches[matchId].spawnGunA++;
			if (matches[matchId].spawnGunA >= arse.Guns.Length) 
				matches[matchId].spawnGunA = -2;
			GUILayout.EndHorizontal();
			
			// spawn gun B
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].spawnGunB--;
			if (matches[matchId].spawnGunB < -2) 
				matches[matchId].spawnGunB = arse.Guns.Length-1;
			gunName = "none";
			if (matches[matchId].spawnGunB == -2) 
				gunName = "random";
			if (matches[matchId].spawnGunB >= 0) 
				gunName = arse.Guns[matches[matchId].spawnGunB].Name;
			GUILayout.Label("Spawn Gun B: " + gunName);
			if (GUILayout.Button(">")) 
				matches[matchId].spawnGunB++;
			if (matches[matchId].spawnGunB >= arse.Guns.Length) 
				matches[matchId].spawnGunB = -2;
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- ");
			
			// gun slot 1
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) matches[matchId].pickupSlot1--;
			if (matches[matchId].pickupSlot1<-3) matches[matchId].pickupSlot1 = arse.Guns.Length-1;
			gunName = "none";
			if (matches[matchId].pickupSlot1 == -2) 
				gunName = "random";
			if (matches[matchId].pickupSlot1 == -3) 
				gunName = "health";
			if (matches[matchId].pickupSlot1 >= 0) 
				gunName = arse.Guns[matches[matchId].pickupSlot1].Name;
			GUILayout.Label("Pickup Slot 1: " + gunName);
			if (GUILayout.Button(">")) 
				matches[matchId].pickupSlot1++;
			if (matches[matchId].pickupSlot1>=arse.Guns.Length) 
				matches[matchId].pickupSlot1 = -3;
			GUILayout.EndHorizontal();
			
			// gun slot 2
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) matches[matchId].pickupSlot2--;
			if (matches[matchId].pickupSlot2<-3) matches[matchId].pickupSlot2 = arse.Guns.Length-1;
			gunName = "none";
			if (matches[matchId].pickupSlot2==-2) gunName = "random";
			if (matches[matchId].pickupSlot2==-3) gunName = "health";
			if (matches[matchId].pickupSlot2>=0) gunName = arse.Guns[matches[matchId].pickupSlot2].Name;
			GUILayout.Label("Pickup Slot 2: " + gunName);
			if (GUILayout.Button(">")) matches[matchId].pickupSlot2++;
			if (matches[matchId].pickupSlot2>=arse.Guns.Length) matches[matchId].pickupSlot2 = -3;
			GUILayout.EndHorizontal();
			
			//gun slot 3
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].pickupSlot3--;
			if (matches[matchId].pickupSlot3 < -3) 
				matches[matchId].pickupSlot3 = arse.Guns.Length-1;
			
			gunName = "none";
			if (matches[matchId].pickupSlot3==-2) 
				gunName = "random";
			if (matches[matchId].pickupSlot3==-3) 
				gunName = "health";
			if (matches[matchId].pickupSlot3>=0) 
				gunName = arse.Guns[matches[matchId].pickupSlot3].Name;
			GUILayout.Label("Pickup Slot 3: " + gunName);
			if (GUILayout.Button(">")) 
				matches[matchId].pickupSlot3++;
			if (matches[matchId].pickupSlot3>=arse.Guns.Length) 
				matches[matchId].pickupSlot3 = -3;
			GUILayout.EndHorizontal();
			
			//gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].pickupSlot4--;
			if (matches[matchId].pickupSlot4 < -3) 
				matches[matchId].pickupSlot4 = arse.Guns.Length-1;
			
			gunName = "none";
			if (matches[matchId].pickupSlot4 == -2) 
				gunName = "random";
			if (matches[matchId].pickupSlot4 == -3) 
				gunName = "health";
			if (matches[matchId].pickupSlot4 >= 0) 
				gunName = arse.Guns[matches[matchId].pickupSlot4].Name;
			
			GUILayout.Label("Pickup Slot 4: " + gunName);
			if (GUILayout.Button(">")) 
				matches[matchId].pickupSlot4++;
			if (matches[matchId].pickupSlot4 >= arse.Guns.Length) 
				matches[matchId].pickupSlot4 = -3;
			GUILayout.EndHorizontal();
			
			// gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) 
				matches[matchId].pickupSlot5--;
			if (matches[matchId].pickupSlot5<-3) 
				matches[matchId].pickupSlot5 = arse.Guns.Length-1;
			
			gunName = "none";
			if (matches[matchId].pickupSlot5==-2) 
				gunName = "random";
			if (matches[matchId].pickupSlot5==-3) 
				gunName = "health";
			if (matches[matchId].pickupSlot5>=0) 
				gunName = arse.Guns[matches[matchId].pickupSlot5].Name;
			GUILayout.Label("Pickup Slot 5: " + gunName);
			
			if (GUILayout.Button(">")) 
				matches[matchId].pickupSlot5++;
			if (matches[matchId].pickupSlot5>=arse.Guns.Length) 
				matches[matchId].pickupSlot5 = -3;
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Restock time (seconds): ");
			matches[matchId].restockTime = MakeInt(GUILayout.TextField(matches[matchId].restockTime.ToString()) );
			GUILayout.EndHorizontal();
			
			GUI.EndScrollView();
		}
				
		int qSpan = Screen.width / 4; // quarter of screen width span
		var start = new Rect(qSpan, Screen.height - vSpan*4, qSpan*2, vSpan*4);
		// init button
		if (!serving) {
			if (GUI.Button(start, "Start Game!")) {
				// init a server with the current game mode settings
				net.serverGameChange = true;
				Network.incomingPassword = net.password;
				net.lastGameWasTeamBased = false;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].allowedLevels[mapId]);
				net.comment = net.CurrMatch.Name + "\n" + net.CurrMatch.levelName;
				bool useNat = !Network.HavePublicAddress();
				Debug.Log("Initialising server, has public address: " + Network.HavePublicAddress().ToString());
				Network.InitializeServer(net.connections,net.listenPort, useNat);
				OfflineMenu = Menu.InitializingServer;
			}
		}else{
			if (GUI.Button(start, "Start Match!")) {
				net.serverGameChange = true;
				net.lastGameWasTeamBased = net.CurrMatch.teamBased;
				net.AssignGameModeConfig(matches[matchId], matches[matchId].allowedLevels[mapId]);
				net.NetVI = Network.AllocateViewID();
				net.RequestGameData();
				OnlineMenu = Menu.Controls;
			}
		}
				
		GUI.EndGroup();
	}
	
	Ping[] hostPings;// = new Ping[0];
	void JoinMenu() {
				GUI.BeginGroup(window);
				
				GUI.Label(new Rect(250,0,100,20), "Join a game:");
				
				GUILayout.BeginArea(new Rect(5,20,290,400));
				if (GUILayout.Button("Refresh Host List")){
					MasterServer.RequestHostList(net.uniqueGameName);
					hostPings = new Ping[0];
				}
				GUILayout.EndArea();
		
				GUILayout.BeginArea(new Rect(305,20,290,400));
					GUILayout.BeginHorizontal();
					GUILayout.Label("Game Password: ");
					net.password = GUILayout.TextField(net.password);
					GUILayout.EndHorizontal();
				GUILayout.EndArea();
				
				
				HostData[] hostData = MasterServer.PollHostList();
				
				int scrollHeight = hostData.Length * 40;
				if (scrollHeight < 350) 
					scrollHeight = 350;
				
				scrollPos = GUI.BeginScrollView(new Rect(0,50,600,350), scrollPos, new Rect(0,0,550,scrollHeight+40));
				
				if (hostData.Length == 0) {
					GUILayout.Label("No hosts found!");
				}else{
					if (hostPings.Length == 0) {
						//create new pings for all hosts
						hostPings = new Ping[hostData.Length];
						for (int i=0; i<hostData.Length; i++){
							string ipString = "";
							for (int k=0; k<hostData[i].ip.Length; k++){
								ipString += hostData[i].ip[k];
								if (k<hostData[i].ip.Length-1) ipString += ".";
							}
					
							Debug.Log("GettingPing: " + ipString);
							hostPings[i] = new Ping(ipString);
						}
					}
				}
				
				for (int i=0; i<hostData.Length; i++) {
					GUI.DrawTexture(new Rect(5, (i*40), 550, 1), backTex);
					
					if (GUI.Button(new Rect(5,(i*40)+2, 80,36), "Connect")){
						Network.Connect(hostData[i],net.password);
						OfflineMenu = Menu.Connecting;
					}
			
					GUI.Label(new Rect(100,(i*40)+2,150, 30), hostData[i].gameName);
					GUI.Label(new Rect(100,(i*40)+17,150, 30), "[" + hostData[i].connectedPlayers.ToString() + "/" + hostData[i].playerLimit.ToString() + "]");
					GUI.Label(new Rect(300,(i*40)+2,150, 60), hostData[i].comment);
					
					if (hostData[i].passwordProtected)
						GUI.Label(new Rect(160,(i*40)+17,150, 30), "[PASSWORDED]");
					
					if (hostPings[i].isDone)
						GUI.Label(new Rect(450,(i*40)+17,150, 30), "Ping: " + hostPings[i].time.ToString());
					else
						GUI.Label(new Rect(450,(i*40)+17,150, 30), "Ping: ?");
					
					if (i==hostData.Length-1)
						GUI.DrawTexture(new Rect(5, (i*40)+40, 550, 1), backTex);
				}
				
				GUI.EndScrollView();
				
				GUI.EndGroup();	
	}
	
	private int MakeInt(string v) {
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}

	void setupMatchTypes ()
	{
		matches = new MatchData[9];
		for (int i = 0; i < matches.Length; i++)
			matches[i] = new MatchData();
		
		matches[0].Name = "Custom";
		matches[0].Descript = "Have it your way!  All the exact settings you prefer.";
		matches[0].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", "TestLevelB", "Tower" };
		matches[0].respawnWait = 5f;
		
		matches[1].Name = "Grav-O-Rama"; // Gravity Of The Matter/Situation?  Your Own Gravity? A Gravity Of Your Own?
		// Gravity Is/Gets Personal?, Personal Gravity?, Gravitaction?
		matches[1].Descript = "Each player has their own, independent, changeable gravity";
		matches[1].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		matches[1].respawnWait = 5f;
		matches[1].spawnGunB = (int)Weapon.GravGun;
		
		matches[2].Name = "Grue Food";
		matches[2].Descript = "It is pitch black.  You are likely to be eaten by a grue.";
		matches[2].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
		matches[2].respawnWait = 5f;
		matches[2].pitchBlack = true;
		matches[2].pickupSlot1 = 1;
		matches[2].pickupSlot2 = 2;
		matches[2].pickupSlot3 = 3;
		matches[2].pickupSlot4 = 4;
		matches[2].pickupSlot5 = 7;
		
		matches[3].Name = "FFA Fragmatch";
		matches[3].Descript = "Frag count is ALL that counts in this freestyle Free For All!";
		matches[3].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", "TestLevelB", "Tower" };
		matches[3].respawnWait = 5f;
		
		matches[5].Name = "Team Fragmatch";
		matches[5].Descript = "Frag count is what counts, but don't hurt your mates!";
		matches[5].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", "TestLevelB", "Tower" };
		matches[5].respawnWait = 5f;
		matches[5].teamBased = true;
		matches[5].pickupSlot5 = 4;
		
		matches[4].Name = "BBall";
		matches[4].Descript = "Shooting hoops...and GUNS!  GANGSTA!";
		matches[4].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		matches[4].respawnWait = 5f;
		matches[4].deathsSubtractScore = false;
		matches[4].killsIncreaseScore = false;
		matches[4].teamBased = true;
		matches[4].basketball = true;
		matches[4].pickupSlot2 = 2;
		matches[4].pickupSlot3 = 3;
		matches[4].pickupSlot4 = 4;
		matches[4].pickupSlot5 = 5;
		
		matches[6].Name = "YOLT! (You Only Live Thrice)";
		matches[6].Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
		matches[6].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
		matches[6].Duration = 0f;
		matches[6].respawnWait = 5f;
		matches[6].killsIncreaseScore = false;
		matches[6].pickupSlot5 = 4;
		
		matches[7].Name = "Swap Meat";
		matches[7].Descript = "There is only the swapper gun, grenades and lava... have fun!";
		matches[7].allowedLevels = new string[] { "Furnace" , "Tower"};
		matches[7].respawnWait = 3f;
		matches[7].killsIncreaseScore = false;
		matches[7].spawnGunA = 5;
		matches[7].spawnGunB = 1;
		matches[7].pickupSlot1 = -1;
		matches[7].pickupSlot2 = -1;
		matches[7].pickupSlot3 = -1;
		matches[7].pickupSlot4 = -1;
		matches[7].pickupSlot5 = -1;
		
		matches[8].Name = "Weapon Lottery";
		matches[8].Descript = "Assigned weaponry is a crap shoot!  CRAP! SHOOT!";
		matches[8].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" , "Tower"};
		matches[8].winScore = 20;
		matches[8].respawnWait = 5f;
		matches[8].spawnGunA = -2;
		matches[8].spawnGunB = -2;
		matches[8].restockTime = 2f;
		matches[8].pickupSlot1 = -2;
		matches[8].pickupSlot2 = -2;
		matches[8].pickupSlot3 = -2;
		matches[8].pickupSlot4 = -2;
		matches[8].pickupSlot5 = -2;
	}

	void loadHudPics ()
	{
		UnityEngine.Object[] pics = Resources.LoadAll("Pic/Hud");
		
		// use this temp list to setup permanent vars
		for (int i = 0; i < pics.Length; i++) {
			var s = pics[i].name;
			
			switch (s) {
				case "Health": 
					lifeIcon = (Texture)pics[i]; 
					break;
				case "whiteTex": 
					backTex = (Texture)pics[i]; 
					break;
				case "blackTex": 
					blackTex = (Texture)pics[i]; 
					break;
				case "Crosshair": 
					crossHair = (Texture)pics[i]; 
					break;
				case "Logo - CazCore": 
					companyLogo = (Texture)pics[i]; 
					break;
				case "Logo - Paramancer": 
					gameLogo = (Texture)pics[i]; 
					break;
				case "FlagRed": 
					teamRedFlag = (Texture)pics[i]; 
					break;
				case "FlagBlue": 
					teamBlueFlag = (Texture)pics[i]; 
					break;
			}
		}
	}

	void avatarView() {
		if (GameObject.Find("CharaMesh") != null) {
			// colours
			Material[] mats = GameObject.Find("CharaMesh").renderer.materials;
			mats[0].color = net.localPlayer.colA;
			mats[1].color = net.localPlayer.colB;
			mats[2].color = net.localPlayer.colC;
			GameObject.Find("NormalHead").renderer.material.color = net.localPlayer.colA;
			
			// heads
			GameObject.Find("NormalHead").renderer.enabled = false;
			GameObject.Find("CardboardBoxHead").renderer.enabled = false;
			GameObject.Find("FishHead").renderer.enabled = false;
			GameObject.Find("BananaHead").renderer.enabled = false;
			GameObject.Find("CreeperHead").renderer.enabled = false;
			GameObject.Find("ElephantHeadMesh").renderer.enabled = false;
			GameObject.Find("MoonHead").renderer.enabled = false;
			GameObject.Find("PyramidHead").renderer.enabled = false;
			GameObject.Find("ChocoboHead").renderer.enabled = false;
			GameObject.Find("SpikeHead").renderer.enabled = false;
			GameObject.Find("TentacleRoot").renderer.enabled = false;
			GameObject.Find("RobotHead").renderer.enabled = false;
			GameObject.Find("head_spaceship").renderer.enabled = false;
			GameObject.Find("enforcer_face").renderer.enabled = false;
			GameObject.Find("SmileyHead").renderer.enabled = false;
			GameObject.Find("Helmet").renderer.enabled = false;
			GameObject.Find("PaperBag").renderer.enabled = false;
			GameObject.Find("Mahead").renderer.enabled = false;
			
			if (net.localPlayer.headType == 0) GameObject.Find("NormalHead").renderer.enabled = true;
			if (net.localPlayer.headType == 1) GameObject.Find("CardboardBoxHead").renderer.enabled = true;
			if (net.localPlayer.headType == 2) GameObject.Find("FishHead").renderer.enabled = true;
			if (net.localPlayer.headType == 3) GameObject.Find("BananaHead").renderer.enabled = true;
			if (net.localPlayer.headType == 4) GameObject.Find("CreeperHead").renderer.enabled = true;
			if (net.localPlayer.headType == 5) GameObject.Find("ElephantHeadMesh").renderer.enabled = true;
			if (net.localPlayer.headType == 6) GameObject.Find("MoonHead").renderer.enabled = true;
			if (net.localPlayer.headType == 7) GameObject.Find("PyramidHead").renderer.enabled = true;
			if (net.localPlayer.headType == 8) GameObject.Find("ChocoboHead").renderer.enabled = true;
			if (net.localPlayer.headType == 9) GameObject.Find("SpikeHead").renderer.enabled = true;
			if (net.localPlayer.headType == 10) GameObject.Find("TentacleRoot").renderer.enabled = true;
			if (net.localPlayer.headType == 11) GameObject.Find("RobotHead").renderer.enabled = true;
			if (net.localPlayer.headType == 12) GameObject.Find("head_spaceship").renderer.enabled = true;
			if (net.localPlayer.headType == 13) GameObject.Find("enforcer_face").renderer.enabled = true;
			if (net.localPlayer.headType == 14) GameObject.Find("SmileyHead").renderer.enabled = true;
			if (net.localPlayer.headType == 15) GameObject.Find("Helmet").renderer.enabled = true;
			if (net.localPlayer.headType == 16) GameObject.Find("PaperBag").renderer.enabled = true;
			if (net.localPlayer.headType == 17) GameObject.Find("Mahead").renderer.enabled = true;
			
			GameObject.Find("PlayerNameText").GetComponent<TextMesh>().text = net.localPlayer.name;
		}
	}

	void credits(Rect br) {
		if (GUI.Button(br, "Back..."))
			OfflineMenu = Menu.Main;
		
		DrawWindowBackground();
		
		GUI.BeginGroup(window);
		
		if (GUI.Button(new Rect(0,360,200,40), "Sophie Houlden"))
			Application.OpenURL("http://sophiehoulden.com");
		if (GUI.Button(new Rect(200,360,200,40), "7DFPS"))
			Application.OpenURL("http://7dfps.org");
		if (GUI.Button(new Rect(400,360,200,40), "SPLAT DEATH SALAD\nHomepage"))
			Application.OpenURL("http://sophiehoulden.com/games/splatdeathsalad");
		
		GUILayout.Label("Current team:");
		GUILayout.Label("");
		GUILayout.Label("Corpus Callosum - Coding, Logo, Controls & Match screens");
		GUILayout.Label("IceFlame            - Coding, Tower map, Other map additions");
		GUILayout.Label("CarnagePolicy     - Sounds");
		GUILayout.Label("");
		GUILayout.Label("This game is a fork of...");
		GUILayout.Label("");
		GUILayout.Label("~~~ SPLAT DEATH SALAD ~~~    (Version: 1.1)");
		GUILayout.Label("");
		GUILayout.Label("Made by Sophie Houlden.  Using Unity, for 7DFPS (June 2012)");
		GUILayout.Label("");
		GUILayout.Label("Don't be surprised if you have poor performance or get kicked with high ping");
		GUILayout.Label("Click a button to visit these sites:");
		
//		if (Application.isWebPlayer) 
//			GUILayout.Label("*** Visit the homepage for standalone client downloads (win/mac) ***");
		
		GUI.EndGroup();
	}
	
	
	
	
	
	
	
	
	
	
	
	void onlineMenus (int midX, int midY, bool settingUpMatch) {
		if (!Screen.lockCursor) {
			// buttons
			float h = button.height;
			button.x = window.xMax;
			button.y = controGui.BottomMost;
	
			if (settingUpMatch)
				button.x -= button.width;
	
			if (GUI.Button(button, "Resume")) {
				Screen.lockCursor = true;
				OnlineMenu = Menu.Controls;
			}
			
			button.y += h;
			if (GUI.Button(button, "Disconnect"))
				net.DisconnectNow();
			
			// server mode buttons
			if (net.isServer) {
				button.y += h;
				if (GUI.Button(button, Menu.Controls.ToString()))
					OnlineMenu = Menu.Controls;
				
				button.y += h;
				if (GUI.Button(button, Menu.Match.ToString()))
					OnlineMenu = Menu.Match;
				
				button.y += h;
				if (GUI.Button(button, Menu.Kick.ToString()))
					OnlineMenu = Menu.Kick;				
			}
			
			// show menus
			if (OnlineMenu == Menu.Kick) {
				if (net.isServer) {
					DrawWindowBackground();
					KickMenu();
				}else
					OnlineMenu = Menu.Controls;
			}else if (OnlineMenu == Menu.Match) {
				if (net.isServer) {
					MatchSetup(true);
				}else
					OnlineMenu = Menu.Controls;
			}else if (OnlineMenu == Menu.Controls) {
				MenuControls();
			}
		}else{ // connected & cursor locked
			if (viewingScores || net.gameOver) {
				DrawWindowBackground(true);
				scores.Draw(window, net, vSpan);
			}else{
				GUI.DrawTexture(new Rect(midX-8, midY-8, 16, 16), crossHair);
				
				if (gunA == 5) {
					//swapper
					int swapperFrame = Mathf.FloorToInt((Time.time*15f) % swapperCrosshair.Length);
					if (!swapperLocked) 
						swapperFrame = 0;
					
					GUI.DrawTexture(new Rect(swapperCrossX-32, (Screen.height-swapperCrossY)-32, 64, 64), swapperCrosshair[swapperFrame]);
				}
			}
			
			// health bar
			int healthWidth = (Screen.width/3);
			GUI.DrawTexture(new Rect(midX-(healthWidth/2)-2, Screen.height-15, healthWidth+4, 9), blackTex);
			int healthWidthB = (int)((((float)healthWidth)/100f)*net.localPlayer.health);
			GUI.DrawTexture(new Rect(midX-(healthWidth/2), Screen.height-13, healthWidthB, 5), backTex);
			
			//energy bar
			int energyWidth = (Screen.width/3);
			GUI.DrawTexture(new Rect(midX-(energyWidth/2)-2, Screen.height-30, healthWidth+4, 9), blackTex);
			int energyWidthB = (int)(((float)healthWidth)*EnergyLeft);
			GUI.DrawTexture(new Rect(midX-(energyWidth/2), Screen.height-28, energyWidthB, 5), backTex);
			
			// lives
			if (net.CurrMatch.playerLives>0) {
				int lifeCount = 0;
				for (int i=0; i<net.players.Count; i++){
					if (net.players[i].local) lifeCount = net.players[i].lives;
				}
				//Debug.Log(lifeCount);
				for (int i=0; i<lifeCount; i++){
					GUI.DrawTexture(new Rect(Screen.width-60, i*30, 64, 64), lifeIcon);
				}
			}
			
			// pickup stuff
			Color gcol = GUI.color;
			if (offeredPickup != "" && !net.autoPickup) {
				GUI.color = Color.black;
				string s = "Press '" + InputUser.GetKeyLabel(UserAction.GrabItem) + "' to grab " + offeredPickup.ToUpper();
				GUI.Label(new Rect(midX-51, midY+100, 100, 60), s);
				GUI.Label(new Rect(midX-49, midY+100, 100, 60), s);
				GUI.Label(new Rect(midX-50, midY+101, 100, 60), s);
				GUI.Label(new Rect(midX-50, midY+99, 100, 60), s);
				GUI.color = gcol;
				GUI.Label(new Rect(midX-50, midY+100, 100, 60), s);
			}
			
			if (Spectating) {
				GUI.color = Color.black;
				GUI.Label(new Rect(4, 5, 300, 60), "Spectating: " + net.players[Spectatee].name + "\n\nYou will be able to play once this round is over.");
				GUI.Label(new Rect(6, 5, 300, 60), "Spectating: " + net.players[Spectatee].name + "\n\nYou will be able to play once this round is over.");
				GUI.Label(new Rect(5, 4, 300, 60), "Spectating: " + net.players[Spectatee].name + "\n\nYou will be able to play once this round is over.");
				GUI.Label(new Rect(5, 6, 300, 60), "Spectating: " + net.players[Spectatee].name + "\n\nYou will be able to play once this round is over.");
				
				GUI.color = gcol;
				GUI.Label(new Rect(5, 5, 300, 60), "Spectating: " + net.players[Spectatee].name + "\n\nYou will be able to play once this round is over.");
				
			}
			
			// weapon
			GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
			if (gunB >= 0) 
				GUI.DrawTexture(new Rect(Screen.width-80,Screen.height-95,64,64), arse.Guns[gunB].Pic);
			
			GUI.color = gcol;
			if (gunA >= 0) 
				GUI.DrawTexture(new Rect(Screen.width-110,Screen.height-70,64,64), arse.Guns[gunA].Pic);
			
			if (gunA >= 0) {
				GUI.color = Color.black;
			
				GUI.Label(new Rect(Screen.width-99, Screen.height-20, 100, 30), arse.Guns[gunA].Name);
				GUI.Label(new Rect(Screen.width-101, Screen.height-20, 100, 30), arse.Guns[gunA].Name);
				GUI.Label(new Rect(Screen.width-100, Screen.height-21, 100, 30), arse.Guns[gunA].Name);
				GUI.Label(new Rect(Screen.width-100, Screen.height-19, 100, 30), arse.Guns[gunA].Name);
				
				GUI.color = gcol;
				GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 30), arse.Guns[gunA].Name);
			}
			
			// weapon cooldown
			if (gunA >= 0) {
				GUI.DrawTexture(new Rect(Screen.width-103, Screen.height-27, 56, 8), blackTex);
				float coolDownPercent = 50f;
				if (arse.Guns[gunA].Delay>0f) {
					coolDownPercent = (gunACooldown / arse.Guns[gunA].Delay) * 50f;
					coolDownPercent = 50f-coolDownPercent;
				}
				GUI.DrawTexture(new Rect(Screen.width-100, Screen.height-24, Mathf.FloorToInt(coolDownPercent), 2), backTex);
			}
			
			GUI.color = gcol;
		}
		
		Color gcolB = GUI.color;
		
		// team icons
		if (net.CurrMatch.teamBased && net.localPlayer.team != 0) {
			if /*``*/ (net.localPlayer.team == 1){
				GUI.DrawTexture(new Rect(Screen.width-68,4,64,64),teamRedFlag);
			} else if (net.localPlayer.team == 2){
				GUI.DrawTexture(new Rect(Screen.width-68,4,64,64),teamBlueFlag);
			}
		}
		
		// time
		if (!net.gameOver) {
			if (net.CurrMatch.Duration > 0f) {
				// show time left
				GUI.color = Color.black;
				GUI.Label(new Rect(midX-11, 5, 200, 30), TimeStringFromSecs(net.gameTimeLeft) );
				GUI.Label(new Rect(midX-9, 5, 200, 30), TimeStringFromSecs(net.gameTimeLeft) );
				GUI.Label(new Rect(midX-10, 4, 200, 30), TimeStringFromSecs(net.gameTimeLeft) );
				GUI.Label(new Rect(midX-10, 6, 200, 30), TimeStringFromSecs(net.gameTimeLeft) );
				
				GUI.color = gcolB;
				GUI.Label(new Rect(midX-10, 5, 200, 30), TimeStringFromSecs(net.gameTimeLeft) );
			}
		}else{
			string s = "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.";
			GUI.color = Color.black;
			GUI.Label(new Rect(midX-51, 5, 200, 30), s);
			GUI.Label(new Rect(midX-49, 5, 200, 30), s);
			GUI.Label(new Rect(midX-50, 4, 200, 30), s);
			GUI.Label(new Rect(midX-50, 6, 200, 30), s);
			
			GUI.color = gcolB;
			GUI.Label(new Rect(midX-50, 5, 200, 30), s);
		}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	bool buttonStarts(Menu menu, Rect rect) {
		if (GUI.Button(rect, menu.ToString())) {
			OfflineMenu = menu;
			return true;
		}
		
		return false;
	}

	void menuMain() {
		int cX = Screen.width/4; // center of item in x (aligns it to the center of the logo
		int hS = 64; // half the horizontal span of menu item
		int mIH = vSpan*2; // menu item height
		var r = new Rect(cX-hS, Screen.height-mIH, hS*2, mIH); // menu rect
		
		GUI.DrawTexture(new Rect(0, 0, Screen.width/2, Screen.width/2 /* dimensions are so close to perfect square */), gameLogo);
		
		// start drawing from the bottom
		if (!Application.isWebPlayer) {
			if (GUI.Button(r, "Quit")) {
				Application.Quit();
			} /*^*/ r.y -= mIH;
		}
		buttonStarts(Menu.Credits, r); /*^*/ r.y -= mIH;
		buttonStarts(Menu.Controls, r); /*^*/ r.y -= mIH;
		buttonStarts(Menu.Avatar, r); /*^*/ r.y -= mIH;
		if (buttonStarts(Menu.JoinGame, r)) {
			scrollPos = Vector2.zero;
			MasterServer.RequestHostList(net.uniqueGameName);
			hostPings = new Ping[0];
		} /*^*/ r.y -= mIH;
		
		if (buttonStarts(Menu.StartGame, r)) {
			net.gameName = net.localPlayer.name + "'s match...of the Damned!";
		} /*^*/ r.y -= mIH;
	}

	bool backButton(Rect br) {
		if (GUI.Button(br, "Back...")) {
			OfflineMenu = Menu.Main;
			return true;
		}
		
		return false;
	}
	
	void offlineMenus(Rect br) {
		Screen.lockCursor = false;
		
		switch (OfflineMenu) {
			case Menu.Main:
				menuMain();
				break;
			case Menu.StartGame:
				backButton(br);
				MatchSetup(false);
				break;
			case Menu.JoinGame:
				backButton(br);
				DrawWindowBackground();
				JoinMenu();
				break;
			case Menu.Avatar:
				if (backButton(br))
					net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
				
				DrawWindowBackground(true);
				DrawMenuConfigAvatar();
				break;
			case Menu.Controls:
				br.x = window.xMax;
				br.y = controGui.BottomMost;
				backButton(br);
				MenuControls();
				break;
			case Menu.Credits:
				credits(br);
				break;
			case Menu.ConnectionError:
				backButton(br);
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Failed to Connect:");
				GUILayout.Label(net.Error);
				GUI.EndGroup();
				break;
			case Menu.Connecting:
				if (backButton(br))
					Network.Disconnect();
				
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Connecting...");
				GUI.EndGroup();
				break;
			case Menu.InitializingServer:
				if (backButton(br))
					Network.Disconnect();
				
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Initialising Server...");
				GUI.EndGroup();
				break;
		}
	}
}
