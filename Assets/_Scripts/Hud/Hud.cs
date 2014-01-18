using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public class Hud : MonoBehaviour {
	public GameModeScript[] modes;
	public LevelInfo[] levels;

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
	public Texture gamelogo;
	
	// misc
	public string offeredPickup = "";
	public string menuPoint = "top";
	
	public bool Spectating = false;
	public int Spectatee = 0;
	
	public float gunACooldown = 0f;
	public int gunA = 0;
	public int gunB = 0;
	
	// private
	bool viewingScores = false;
	string gameMenuPoint = "config";
	string defaultName = "Lazy Noob";
	int mode = 1;
	int mapId = 0;
	
	// windows
	Vector2 scrollPos = Vector2.zero;
	Rect window = new Rect(0, 0, 600, 400);
	Rect button = new Rect(0, 0, 100, 40); // fixme: are buttons always 40 in height
	int vSpan = 20; // fixme: hardwired vertical span of the font.  doubled in places for buttons...are they always 40?
	
	// scripts
	CcNet net;
	CcLog log;
	Weapon artillery;
	ControlsGui controGui;
	LocalUser locUser;

	
	
	void Start() {
		// setup match configs
		modes = new GameModeScript[9];
		for (int i = 0; i < modes.Length; i++)
			modes[i] = new GameModeScript();
		
		modes[0].Name = "Custom";
		modes[0].Descript = "Have it your way!  All the exact settings you prefer.";
		modes[0].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome", "TestLevel", "TestLevelB" };
		modes[0].respawnWait = 5f;
		
		modes[1].Name = "Grav-O-Rama"; // Gravity Of The Matter/Situation?  Your Own Gravity? A Gravity Of Your Own?
		modes[1].Descript = "Each player has their own, independent, changeable gravity";
		modes[1].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[1].respawnWait = 5f;
		
		modes[2].Name = "Grue Food";
		modes[2].Descript = "It is pitch black.  You are likely to be eaten by a grue.";
		modes[2].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[2].respawnWait = 5f;
		modes[2].pitchBlack = true;
		modes[2].pickupSlot1 = 1;
		modes[2].pickupSlot2 = 2;
		modes[2].pickupSlot3 = 3;
		modes[2].pickupSlot4 = 4;
		modes[2].pickupSlot5 = 7;
		
		modes[3].Name = "FFA Fragmatch";
		modes[3].Descript = "Frag count is ALL that counts in this freestyle Free For All!";
		modes[3].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[3].respawnWait = 5f;
		
		modes[5].Name = "Team Fragmatch";
		modes[5].Descript = "Frag count is what counts, but don't hurt your mates!";
		modes[5].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[5].respawnWait = 5f;
		modes[5].teamBased = true;
		modes[5].pickupSlot5 = 4;

		modes[4].Name = "BBall";
		modes[4].Descript = "Shooting hoops...and GUNS!  GANGSTA!";
		modes[4].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[4].respawnWait = 5f;
		modes[4].deathsSubtractScore = false;
		modes[4].killsIncreaseScore = false;
		modes[4].teamBased = true;
		modes[4].basketball = true;
		modes[4].pickupSlot2 = 2;
		modes[4].pickupSlot3 = 3;
		modes[4].pickupSlot4 = 4;
		modes[4].pickupSlot5 = 5;
		
		modes[6].Name = "YOLT! (You Only Live Thrice)";
		modes[6].Descript = "Last Person Standing, but you have 3 lives... like Pac-Man";
		modes[6].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[6].Duration = 0f;
		modes[6].respawnWait = 5f;
		modes[6].killsIncreaseScore = false;
		modes[6].pickupSlot5 = 4;
		
		modes[7].Name = "Swap Meat";
		modes[7].Descript = "There is only the swapper gun, grenades and lava... have fun!";
		modes[7].allowedLevels = new string[] { "Furnace" };
		modes[7].respawnWait = 3f;
		modes[7].killsIncreaseScore = false;
		modes[7].spawnGunA = 5;
		modes[7].spawnGunB = 1;
		modes[7].pickupSlot1 = -1;
		modes[7].pickupSlot2 = -1;
		modes[7].pickupSlot3 = -1;
		modes[7].pickupSlot4 = -1;
		modes[7].pickupSlot5 = -1;
		
		modes[8].Name = "Weapon Lottery";
		modes[8].Descript = "Assigned weaponry is a crap shoot!  CRAP! SHOOT!";
		modes[8].allowedLevels = new string[] { "Furnace", "Overpass", "Conflict Room", "The OctaDrome" };
		modes[8].winScore = 20;
		modes[8].respawnWait = 5f;
		modes[8].spawnGunA = -2;
		modes[8].spawnGunB = -2;
		modes[8].restockTime = 2f;
		modes[8].pickupSlot1 = -2;
		modes[8].pickupSlot2 = -2;
		modes[8].pickupSlot3 = -2;
		modes[8].pickupSlot4 = -2;
		modes[8].pickupSlot5 = -2;

		// load textures
		UnityEngine.Object[] pics = Resources.LoadAll("Hud");
		
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
				case "Logo": 
					gamelogo = (Texture)pics[i]; 
					break;
				case "FlagRed": 
					teamRedFlag = (Texture)pics[i]; 
					break;
				case "FlagBlue": 
					teamBlueFlag = (Texture)pics[i]; 
					break;
			}
		}
		
		// scripts
		net = GetComponent<CcNet>();
		log = GetComponent<CcLog>();
		artillery = GetComponent<Weapon>();
		controGui = GetComponent<ControlsGui>();
		locUser = GetComponent<LocalUser>();
		
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
		if (Screen.lockCursor == true && Input.GetKeyDown(KeyCode.Escape)) 
			Screen.lockCursor = false;
		
		if (Input.GetKeyDown(KeyCode.Tab))
			viewingScores = !viewingScores;
	}
	
	void OnGUI() {
		controGui.enabled = false;
		
		int midX = Screen.width/2;
		int midY = Screen.height/2;
		
		// sizes of windows and button
		window.y = Screen.height - window.height;
		var br = window; // back button rectangle
		br.y -= vSpan * 2;
		br.width = 100; // fixme for actual string width?
		br.height = vSpan * 2;
		
		if (!net.connected){
			Screen.lockCursor = false;
			
			if (menuPoint == "top"){
				GUI.BeginGroup(window);
				
				if (GUILayout.Button("Host a game")){
					menuPoint = "host";
					net.gameName = net.localPlayer.name + "'s Game";
				}
				if (GUILayout.Button("Join a game")){
					menuPoint = "join";
					scrollPos = Vector2.zero;
					MasterServer.RequestHostList(net.uniqueGameName);
					hostPings = new Ping[0];
				}
				if (GUILayout.Button("Avatar")){
					menuPoint = "personalise";
				}
				if (GUILayout.Button("Config")){
					menuPoint = "config";
				}
				if (GUILayout.Button("About")){
					menuPoint = "about";
				}
				
				GUILayout.Label("");
				if (!Application.isWebPlayer){
					if (GUILayout.Button("Quit")){
						Application.Quit();
					}
				}
				
				GUI.DrawTexture(new Rect(0,150,256,256), gamelogo);
				
				GUI.EndGroup();
				
			} else if (menuPoint == "host") {
				if (GUI.Button(br, "Back..."))
					menuPoint = "top";
				
				DrawWindowBackground();
				GameSetup(false);
			} else if (menuPoint == "join") {
				if (GUI.Button(br, "Back..."))
					menuPoint = "top";
				
				DrawWindowBackground();
				JoinMenu();
			} else if (menuPoint == "personalise") {
				if (GUI.Button(br, "Back...")) {
					menuPoint = "top";
					net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
				}
				
				DrawWindowBackground(true);
				DrawMenuConfigAvatar();
			} else if (menuPoint == "config") {
				br.x = window.xMax;
				br.y = controGui.BottomMost;
				if (GUI.Button(br, "Back...")) {
					menuPoint = "top";
				}
				
				DrawMenuConfig();
			} else if(menuPoint=="about"){
				if (GUI.Button(br, "Back..."))
					menuPoint = "top";
				
				DrawWindowBackground();
				
				GUI.BeginGroup(window);
				
				if (GUI.Button(new Rect(0,360,200,40), "Sophie Houlden"))
					Application.OpenURL("http://sophiehoulden.com");
				if (GUI.Button(new Rect(200,360,200,40), "7DFPS"))
					Application.OpenURL("http://7dfps.org");
				if (GUI.Button(new Rect(400,360,200,40), "SPLAT DEATH SALAD\nHomepage"))
					Application.OpenURL("http://sophiehoulden.com/games/splatdeathsalad");
				
				GUILayout.Label("This game is a fork of...");
				GUILayout.Label("");
				GUILayout.Label("");
				GUILayout.Label("~~^~~~ SPLAT DEATH SALAD ~~~^~~");
				GUILayout.Label("(Version: 1.1)");
				GUILayout.Label("");
				GUILayout.Label("Made by Sophie Houlden.  Using Unity; for 7DFPS (June 2012)");
				GUILayout.Label("");
				GUILayout.Label("Don't be surprised if you have poor performance or get kicked with high ping");
				GUILayout.Label("Click a button to visit these sites:");
				
				if (Application.isWebPlayer) 
					GUILayout.Label("*** Visit the SDS homepage for standalone client downloads (win/mac) ***");
				
				GUI.EndGroup();
			} else if (menuPoint == "connectionError"){
				if (GUI.Button(br, "Back..."))
					menuPoint = "top";
				
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Failed to Connect:");
				GUILayout.Label(net.Error);
				GUI.EndGroup();
			}else if (menuPoint == "connecting"){
				if (GUI.Button(br, "Back...")){
					Network.Disconnect();
					menuPoint = "top";
				}
				
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Connecting...");
				GUI.EndGroup();
			}else if (menuPoint == "initializingServer"){
				if (GUI.Button(br, "Back...")){
					Network.Disconnect();
					menuPoint = "top";
				}
				
				DrawWindowBackground();
				GUI.BeginGroup(window);
				GUILayout.Label("Initialising Server...");
				GUI.EndGroup();
			}
		}
		
		if (menuPoint == "top" || menuPoint == "personalise") {
			if (GameObject.Find("CharaMesh") != null){
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
			
		if (net.connected) {
			if (!Screen.lockCursor) {
				// buttons
				float h = button.height;
				button.x = window.xMax;
				button.y = controGui.BottomMost;
				if (GUI.Button(button, "Resume"))
					Screen.lockCursor = true;
				
				button.y += h;
				if (GUI.Button(button, "Disconnect"))
					net.DisconnectNow();
				
				if (net.isServer) {
					button.y += h;
					if (GUI.Button(button, "Config"))
						gameMenuPoint = "config";
					
					button.y += h;
					if (GUI.Button(button, "Change Game"))
						gameMenuPoint = "gameType";
					
					button.y += h;
					if (GUI.Button(button, "Kick"))
						gameMenuPoint = "kick";
				}
				
				// show menus
				if (gameMenuPoint == "kick") {
					if (net.isServer) {
						DrawWindowBackground();
						KickMenu();
					}else
						gameMenuPoint = "config";
				}else if (gameMenuPoint == "gameType") {
					if (net.isServer) {
						DrawWindowBackground();
						GameSetup(true);
					}else
						gameMenuPoint = "config";
				}else if (gameMenuPoint == "config") {
					DrawMenuConfig();
				}
			}else{ // connected & cursor locked
				if (viewingScores || net.gameOver) {
					DrawWindowBackground();
					Scoreboard();
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
				
				// lives
				if (net.ModeCfg.playerLives>0) {
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
					GUI.Label(new Rect(midX-51,midY+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
					GUI.Label(new Rect(midX-49,midY+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
					GUI.Label(new Rect(midX-50,midY+101,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
					GUI.Label(new Rect(midX-50,midY+99,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
					GUI.color = gcol;
					GUI.Label(new Rect(midX-50,midY+100,100,60),"PRESS 'E' TO PICK UP " + offeredPickup.ToUpper());
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
				GUI.color = new Color(0.1f,0.1f,0.1f,0.7f);
				if (gunB >= 0) GUI.DrawTexture(new Rect(Screen.width-80,Screen.height-95,64,64),artillery.gunTypes[gunB].iconTex);
				GUI.color = gcol;
				if (gunA >= 0) GUI.DrawTexture(new Rect(Screen.width-110,Screen.height-70,64,64),artillery.gunTypes[gunA].iconTex);
				
				if (gunA >= 0){
					GUI.color = Color.black;
				
					GUI.Label(new Rect(Screen.width-99, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-101, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-100, Screen.height-21, 100, 30), artillery.gunTypes[gunA].gunName );
					GUI.Label(new Rect(Screen.width-100, Screen.height-19, 100, 30), artillery.gunTypes[gunA].gunName );
					
					GUI.color = gcol;
					GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 30), artillery.gunTypes[gunA].gunName );
				}
				
				// weapon cooldown
				if (gunA >= 0) {
					GUI.DrawTexture(new Rect(Screen.width-103, Screen.height-27, 56, 8), blackTex);
					float coolDownPercent = 50f;
					if (artillery.gunTypes[gunA].fireCooldown>0f) {
						coolDownPercent = (gunACooldown / artillery.gunTypes[gunA].fireCooldown) * 50f;
						coolDownPercent = 50f-coolDownPercent;
					}
					GUI.DrawTexture(new Rect(Screen.width-100, Screen.height-24, Mathf.FloorToInt(coolDownPercent), 2), backTex);
				}
				
				GUI.color = gcol;
			}
			
			Color gcolB = GUI.color;
			
			// team icons
			if (net.ModeCfg.teamBased && net.localPlayer.team != 0) {
				if /*``*/ (net.localPlayer.team == 1){
					GUI.DrawTexture(new Rect(Screen.width-68,4,64,64),teamRedFlag);
				} else if (net.localPlayer.team == 2){
					GUI.DrawTexture(new Rect(Screen.width-68,4,64,64),teamBlueFlag);
				}
			}
			
			// time
			if (!net.gameOver) {
				if (net.ModeCfg.Duration > 0f) {
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
				GUI.color = Color.black;
				GUI.Label(new Rect(midX-51, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect(midX-49, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect(midX-50, 4, 200, 30), "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.");
				GUI.Label(new Rect(midX-50, 6, 200, 30), "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.");
				
				GUI.color = gcolB;
				GUI.Label(new Rect(midX-50, 5, 200, 30), "Next Game in: " +  Mathf.FloorToInt(net.nextMatchTime).ToString() + " seconds.");
			}
		}
	}
	
	string TimeStringFromSecs(float totalSecs){
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
	
	void Scoreboard() {
		GUI.BeginGroup(window);
		
		GUI.Label(new Rect(250,0,100,vSpan), "Scores:");
		
		if (!net.ModeCfg.teamBased){
			int highScore = -9999;
			if (net.gameOver) {
				for (int i=0; i<net.players.Count; i++) {
					if (highScore < net.players[i].currentScore) {
						highScore = net.players[i].currentScore;
					}
				}
			}
			
			int mostLives = 0;
			if (net.gameOver) {
				for (int i=0; i<net.players.Count; i++) {
					if (net.players[i].lives > mostLives) {
						mostLives = net.players[i].lives;
					}
				}
			}
			
			GUI.Label(new Rect(10, vSpan,150,vSpan), "Name:");
			GUI.Label(new Rect(160, vSpan,50,vSpan), "Frags:");
			GUI.Label(new Rect(210, vSpan,50,vSpan), "Deaths:");
			GUI.Label(new Rect(270, vSpan,50,vSpan), "Score:");
			
			if (net.ModeCfg.playerLives != 0) 
				GUI.Label(new Rect(400, vSpan,50,vSpan), "Lives:");
			
			for (int i=0; i<net.players.Count; i++) {
				GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				if (net.players[i].local) GUI.color = new Color(1, 1, 1, 1f);
				if (net.players[i].currentScore == highScore && mostLives == 0) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				if (net.players[i].lives == mostLives && mostLives>0) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				GUI.Label(new Rect(10,(i*vSpan) + 40,150,vSpan), net.players[i].name);
				GUI.Label(new Rect(160,(i*vSpan) + 40,50,vSpan), net.players[i].kills.ToString());
				GUI.Label(new Rect(210,(i*vSpan) + 40,50,vSpan), net.players[i].deaths.ToString());
				GUI.Label(new Rect(270,(i*vSpan) + 40,50,vSpan), net.players[i].currentScore.ToString());
				if (net.ModeCfg.playerLives!= 0) GUI.Label(new Rect(400, (i*vSpan) + 40,50,vSpan), net.players[i].lives.ToString());
			}
			
		}
		
		if (net.ModeCfg.teamBased) {
			GUI.color = new Color(1f, 0f, 0f, 1f);
			if (net.gameOver && net.team1Score>net.team2Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
			GUI.Label(new Rect(100, 20,150,20), "Team 1 Score: " + net.team1Score.ToString());
			GUI.color = new Color(0f, 1f, 1f, 1f);
			if (net.gameOver && net.team2Score>net.team1Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
			GUI.Label(new Rect(300, 20,150,20), "Team 2 Score: " + net.team2Score.ToString());
			
			GUI.Label(new Rect(10, 40,150,20), "Name:");
			GUI.Label(new Rect(160, 40,50,20), "Kills:");
			GUI.Label(new Rect(210, 40,50,20), "Deaths:");
			GUI.Label(new Rect(270, 40,50,20), "Score:");
			
			// team 1
			int yOffset = 0;
			GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), "Team 1:");
			yOffset++;
			for (int i=0; i<net.players.Count; i++) {
				GUI.color = new Color(1f, 0f, 0f, 1f);
				if (net.players[i].team == 1) {
					
					if (net.players[i].local) GUI.color = new Color(1, 0.3f, 0.3f, 1f);
					if (net.gameOver && net.team1Score>net.team2Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
					
					GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), net.players[i].name);
					GUI.Label(new Rect(160,(yOffset*20) + 60,50,20), net.players[i].kills.ToString());
					GUI.Label(new Rect(210,(yOffset*20) + 60,50,20), net.players[i].deaths.ToString());
					GUI.Label(new Rect(270,(yOffset*20) + 60,50,20), net.players[i].currentScore.ToString());
					
					yOffset++;
				}
			}
			
			// team 2
			yOffset++;
			GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), "Team 2:");
			yOffset++;
			for (int i=0; i<net.players.Count; i++) {
				GUI.color = new Color(0f, 1f, 1f, 1f);
				if (net.players[i].team == 2) {
					
					if (net.players[i].local) GUI.color = new Color(0.3f, 1, 1, 1f);
					if (net.gameOver && net.team2Score>net.team1Score) GUI.color = new Color(UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), UnityEngine.Random.Range(0.5f,1f), 1f);
				
					GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), net.players[i].name);
					GUI.Label(new Rect(160,(yOffset*20) + 60,50,20), net.players[i].kills.ToString());
					GUI.Label(new Rect(210,(yOffset*20) + 60,50,20), net.players[i].deaths.ToString());
					GUI.Label(new Rect(270,(yOffset*20) + 60,50,20), net.players[i].currentScore.ToString());
					
					yOffset++;
				}
			}
			
			GUI.color = Color.black;
			yOffset++;
			GUI.Label(new Rect(10,(yOffset*20) + 60,300,20), ">> TO CHANGE TEAMS, PRESS 'T' <<");
		}
		
		GUI.EndGroup();
	}
	
	string FormatName(string s) {
		string ret = "";
		
		for (int i=0; i<s.Length; i++) {
			bool pass = true;
			if (s.Substring(i,1) == " ")
				if (i<s.Length-1)
					if (s.Substring(i+1,1) == " ")
						pass = false;
			
			if (s.Substring(i,1) == "\n") pass = false;
			if (s.Substring(i,1) == "	") pass = false;
			if (pass) ret += s.Substring(i,1);
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
				
				if (GUILayout.Button("Kick"))
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
	void DrawMenuConfig() {
		controGui.enabled = true;
		Rect r = window;
		r.y = controGui.BottomMost;
		DrawWindowBackground(r);
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
		GUILayout.BeginArea(new Rect(305,vSpan*2,280,380));
		
		
		
		GUILayout.Label("---------- Controls: ----------");
		
		GUILayout.Label("Left Shift/Ctrl - Crouch");
		GUILayout.Label("Q/Right Click - Swap Weapon");
		GUILayout.Label("E - Pickup Weapon (replaces weapon in hand)");
		GUILayout.Label("K - Kill yourself");
		GUILayout.Label("R - Sprint");
		GUILayout.Label("ENTER - Chat");
		GUILayout.Label("Tab - Scoreboard");
		GUILayout.Label("Tab+T - Swap Teams");
		
		GUILayout.EndArea();
		
		GUI.EndGroup();
	}
	
	void GameSetup(bool serving) {
		GUI.BeginGroup(window);
		
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
			int lastInt = mode;
			
			mode--;
			if (mode < 0) 
				mode += modes.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<modes[mode].allowedLevels.Length; i++) {
				if (modes[mode].allowedLevels[i] == 
					modes[lastInt].allowedLevels[mapId]
				) 
					levelChangeIndex = i;
			}
			
			mapId = levelChangeIndex;
		}
		
		if (GUI.Button(new Rect(255,100,30,30), ">") ) {
			int lastInt = mode;
			
			mode++;
			if (mode>=modes.Length) mode-=modes.Length;
			
			int levelChangeIndex = 0;
			for (int i=0; i<modes[mode].allowedLevels.Length; i++) {
				if (modes[mode].allowedLevels[i] == modes[lastInt].allowedLevels[mapId]) 
					levelChangeIndex = i;
			}
			
			mapId = levelChangeIndex;
		}
		
		GUI.Label(new Rect(60,100,200,30), "Mode: " + modes[mode].Name);
				
		// game level
		if (GUI.Button(new Rect(305,100,30,30), "<") ) {
			mapId--;
			if (mapId < 0) 
				mapId += modes[mode].allowedLevels.Length;
		}
		if (GUI.Button(new Rect(555,100,30,30), ">") ) {
			mapId++;
			if (mapId >= modes[mode].allowedLevels.Length) 
				mapId -= modes[mode].allowedLevels.Length;
		}
		
		GUI.Label(new Rect(360,100,200,30), "Level: " + modes[mode].allowedLevels[mapId]);
				
				
				
		if (mode != 0) { // not custom
			// show icon
			for (int i=0; i<levels.Length; i++) {
				if (levels[i].Name == modes[mode].allowedLevels[mapId]) {
					GUI.DrawTexture(new Rect(5,135,590,100), levels[i].icon);
				}
			}
			
			//description:
			GUI.Label(new Rect(5,240,590,200), modes[mode].Name + ":\n" + modes[mode].Descript);
			
			if (GUI.Button(new Rect(495,240,100,25), "Customise...") ) {
				
				int levelChangeIndex = 0;
				for (int i=0; i<modes[0].allowedLevels.Length; i++) {
					if (modes[0].allowedLevels[i] == modes[mode].allowedLevels[mapId]) 
						levelChangeIndex = i;
				}
				
				mapId = levelChangeIndex;
				
				modes[0].killsIncreaseScore = modes[mode].killsIncreaseScore;
				modes[0].deathsSubtractScore = modes[mode].deathsSubtractScore;
				modes[0].respawnWait = modes[mode].respawnWait;
				modes[0].teamBased = modes[mode].teamBased;
				modes[0].allowFriendlyFire = modes[mode].allowFriendlyFire;
				modes[0].pitchBlack = modes[mode].pitchBlack;
				modes[0].Duration = modes[mode].Duration;
				modes[0].winScore = modes[mode].winScore;
				modes[0].spawnGunA = modes[mode].spawnGunA;
				modes[0].spawnGunB = modes[mode].spawnGunB;
				modes[0].pickupSlot1 = modes[mode].pickupSlot1;
				modes[0].pickupSlot2 = modes[mode].pickupSlot2;
				modes[0].pickupSlot3 = modes[mode].pickupSlot3;
				modes[0].pickupSlot4 = modes[mode].pickupSlot4;
				modes[0].pickupSlot5 = modes[mode].pickupSlot5;
				modes[0].restockTime = modes[mode].restockTime;
				modes[0].playerLives = modes[mode].playerLives;
				modes[0].basketball = modes[mode].basketball;
				
				mode = 0;
			}
		}else{ // custom, show options here
			scrollPos = GUI.BeginScrollView(new Rect(5,135,590,160), scrollPos, new Rect(0,0,570,700));
			
			modes[mode].killsIncreaseScore = GUILayout.Toggle(modes[mode].killsIncreaseScore, "Kills Increase score");
			modes[mode].deathsSubtractScore = GUILayout.Toggle(modes[mode].deathsSubtractScore, "Deaths Reduce score");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Respawn Time: ");
			modes[mode].respawnWait = MakeInt( GUILayout.TextField(modes[mode].respawnWait.ToString()) );
			GUILayout.EndHorizontal();
			
			modes[mode].teamBased = GUILayout.Toggle(modes[mode].teamBased, "Team Based");
			
			if (modes[mode].teamBased)
				modes[mode].basketball = GUILayout.Toggle(modes[mode].basketball, "Basketball");
			else
				modes[mode].basketball = false;
			
			
			modes[mode].allowFriendlyFire = GUILayout.Toggle(modes[mode].allowFriendlyFire, "Allow Friendly Fire");
			
			modes[mode].pitchBlack = GUILayout.Toggle(modes[mode].pitchBlack, "Pitch Black");
			
			GUILayout.Label(" --- Round end conditions (set to 0 to ignore) --- ");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Round Time (minutes): ");
			modes[mode].Duration = MakeInt( GUILayout.TextField(modes[mode].Duration.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Winning score: ");
			modes[mode].winScore = MakeInt( GUILayout.TextField(modes[mode].winScore.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Player Lives: ");
			modes[mode].playerLives = MakeInt( GUILayout.TextField(modes[mode].playerLives.ToString()) );
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- Weapon Settings --- ");
			
			//spawn gun A
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].spawnGunA--;
			if (modes[mode].spawnGunA<-2) modes[mode].spawnGunA = artillery.gunTypes.Length-1;
			string gunName = "none";
			if (modes[mode].spawnGunA==-2) gunName = "random";
			if (modes[mode].spawnGunA>=0) gunName = artillery.gunTypes[modes[mode].spawnGunA].gunName;
			GUILayout.Label("Spawn Gun A: " + gunName);
			if (GUILayout.Button(">")) modes[mode].spawnGunA++;
			if (modes[mode].spawnGunA>=artillery.gunTypes.Length) modes[mode].spawnGunA = -2;
			GUILayout.EndHorizontal();
			//spawn gun B
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].spawnGunB--;
			if (modes[mode].spawnGunB<-2) modes[mode].spawnGunB = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].spawnGunB==-2) gunName = "random";
			if (modes[mode].spawnGunB>=0) gunName = artillery.gunTypes[modes[mode].spawnGunB].gunName;
			GUILayout.Label("Spawn Gun B: " + gunName);
			if (GUILayout.Button(">")) modes[mode].spawnGunB++;
			if (modes[mode].spawnGunB>=artillery.gunTypes.Length) modes[mode].spawnGunB = -2;
			GUILayout.EndHorizontal();
			
			GUILayout.Label(" --- ");
			//gun slot 1
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].pickupSlot1--;
			if (modes[mode].pickupSlot1<-3) modes[mode].pickupSlot1 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].pickupSlot1==-2) gunName = "random";
			if (modes[mode].pickupSlot1==-3) gunName = "health";
			if (modes[mode].pickupSlot1>=0) gunName = artillery.gunTypes[modes[mode].pickupSlot1].gunName;
			GUILayout.Label("Pickup Slot 1: " + gunName);
			if (GUILayout.Button(">")) modes[mode].pickupSlot1++;
			if (modes[mode].pickupSlot1>=artillery.gunTypes.Length) modes[mode].pickupSlot1 = -3;
			GUILayout.EndHorizontal();
			//gun slot 2
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].pickupSlot2--;
			if (modes[mode].pickupSlot2<-3) modes[mode].pickupSlot2 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].pickupSlot2==-2) gunName = "random";
			if (modes[mode].pickupSlot2==-3) gunName = "health";
			if (modes[mode].pickupSlot2>=0) gunName = artillery.gunTypes[modes[mode].pickupSlot2].gunName;
			GUILayout.Label("Pickup Slot 2: " + gunName);
			if (GUILayout.Button(">")) modes[mode].pickupSlot2++;
			if (modes[mode].pickupSlot2>=artillery.gunTypes.Length) modes[mode].pickupSlot2 = -3;
			GUILayout.EndHorizontal();
			//gun slot 3
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].pickupSlot3--;
			if (modes[mode].pickupSlot3<-3) modes[mode].pickupSlot3 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].pickupSlot3==-2) gunName = "random";
			if (modes[mode].pickupSlot3==-3) gunName = "health";
			if (modes[mode].pickupSlot3>=0) gunName = artillery.gunTypes[modes[mode].pickupSlot3].gunName;
			GUILayout.Label("Pickup Slot 3: " + gunName);
			if (GUILayout.Button(">")) modes[mode].pickupSlot3++;
			if (modes[mode].pickupSlot3>=artillery.gunTypes.Length) modes[mode].pickupSlot3 = -3;
			GUILayout.EndHorizontal();
			//gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].pickupSlot4--;
			if (modes[mode].pickupSlot4<-3) modes[mode].pickupSlot4 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].pickupSlot4==-2) gunName = "random";
			if (modes[mode].pickupSlot4==-3) gunName = "health";
			if (modes[mode].pickupSlot4>=0) gunName = artillery.gunTypes[modes[mode].pickupSlot4].gunName;
			GUILayout.Label("Pickup Slot 4: " + gunName);
			if (GUILayout.Button(">")) modes[mode].pickupSlot4++;
			if (modes[mode].pickupSlot4>=artillery.gunTypes.Length) modes[mode].pickupSlot4 = -3;
			GUILayout.EndHorizontal();
			//gun slot 4
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<")) modes[mode].pickupSlot5--;
			if (modes[mode].pickupSlot5<-3) modes[mode].pickupSlot5 = artillery.gunTypes.Length-1;
			gunName = "none";
			if (modes[mode].pickupSlot5==-2) gunName = "random";
			if (modes[mode].pickupSlot5==-3) gunName = "health";
			if (modes[mode].pickupSlot5>=0) gunName = artillery.gunTypes[modes[mode].pickupSlot5].gunName;
			GUILayout.Label("Pickup Slot 5: " + gunName);
			if (GUILayout.Button(">")) modes[mode].pickupSlot5++;
			if (modes[mode].pickupSlot5>=artillery.gunTypes.Length) modes[mode].pickupSlot5 = -3;
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Restock time (seconds): ");
			modes[mode].restockTime = MakeInt( GUILayout.TextField(modes[mode].restockTime.ToString()) );
			GUILayout.EndHorizontal();
			
			GUI.EndScrollView();
		}
				
		// init button
		if (!serving){
			if (GUI.Button(new Rect(10,310,580,80), "Init Server!")){
				// init a server with the current game mode settings
				net.serverGameChange = true;
				Network.incomingPassword = net.password;
				net.lastGameWasTeamBased = false;
				net.AssignGameModeConfig(modes[mode], modes[mode].allowedLevels[mapId]);
				net.comment = net.ModeCfg.Name + "\n" + net.ModeCfg.levelName;
				bool useNat = !Network.HavePublicAddress();
				Debug.Log("Initialising server, has public address: " + Network.HavePublicAddress().ToString());
				Network.InitializeServer(net.connections,net.listenPort, useNat);
				menuPoint = "initializingServer";
			}
		}else{
			if (GUI.Button(new Rect(10,310,580,80), "Change Game")){
				net.serverGameChange = true;
				net.lastGameWasTeamBased = net.ModeCfg.teamBased;
				net.AssignGameModeConfig(modes[mode], modes[mode].allowedLevels[mapId]);
				net.NetVI = Network.AllocateViewID();
				net.RequestGameData();
			}
		}
				
		GUI.EndGroup();
	}
	
	Ping[] hostPings;// = new Ping[0];
	void JoinMenu(){
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
						menuPoint = "connecting";
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
}
