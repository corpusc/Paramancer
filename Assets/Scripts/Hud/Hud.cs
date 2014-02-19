using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Hud : MonoBehaviour {
	private HudMode mode = HudMode.MenuMain;
	public HudMode Mode {
		get { return mode; }
		set {
			// tasks to do when LEAVING this mode
			switch (mode) {
				case HudMode.Controls:
					InputUser.SaveKeyConfig();
					controls.enabled = false;
					break;
			}
			
			mode = value;
			
			// tasks to do when ENTERING this mode
			switch (mode) {
				case HudMode.Controls: 
					controls.enabled = true;
					break;
			}
		}
	}
	
	// private
	bool viewingScores = false;
	string defaultName = "Lazy Noob";
	Scoreboard scores = new Scoreboard();
	Playing playHud = new Playing();
	MatchSetup matchSetup = new MatchSetup();
	
	// UI element sizes
	int midX, midY; // middle of the screen
	Rect window = new Rect(0, 0, 600, 400); // background for most menus
	Rect br = new Rect(0, 0, 100, 0); // back button rectangle
	int vSpan = 20; // fixme: hardwired vertical span of the text.  doubled in many places for button height
	Vector2 scrollPos = Vector2.zero;

	// scripts
	CcNet net;
	CcLog log;
	Arsenal arse;
	Controls controls;
	LocalUser locUser;

	
	
	void Start() {
		matchSetup.Init();

		// scripts
		net = GetComponent<CcNet>();
		log = GetComponent<CcLog>();
		arse = GetComponent<Arsenal>();
		controls = GetComponent<Controls>();
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
		if (InputUser.Started(UserAction.Menu)) 
			if (Mode != HudMode.MenuMain) {
				Mode = HudMode.MenuMain;
				Screen.lockCursor = false;
			}else{
				// the only people who will see my informative fullscreen box
				// now (about how you gotta click in the window to grab cursor)
				// should be people running the game in the Unity IDE
				if (net.connected && !Application.isWebPlayer)
					Mode = HudMode.Playing;
			}
			
		if (InputUser.Started(UserAction.Scores))
			viewingScores = !viewingScores;
	}

	void OnGUI() {
		// sizes of UI elements
		midX = Screen.width/2;
		midY = Screen.height/2;
		window.y = Screen.height - window.height;
		br.height = vSpan * 2;
		br.y = window.y - br.height;


		// handle all the modes!!!
		switch (Mode) {
			case HudMode.Playing:
				playHud.Draw(net, arse, midX, midY);
				maybePromptClickIn();
				break;
				
			case HudMode.StartGame:
			case HudMode.MatchSetup:
				matchSetup.Draw(net.isServer, net, this, vSpan);
				break;
				
			case HudMode.JoinGame:
				JoinMenu();
				break;

			case HudMode.Avatar:
				avatarView();
				if (backButton(br))
					net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
				avatarSetup();
				break;

			case HudMode.MenuMain:
				if (!net.connected) {
					avatarView();
				}

				menuMain();
				break;

			case HudMode.Controls:
				drawControlsAdjunct();
				break;
				
			case HudMode.Settings:
				drawSettings();
				break;
			
			case HudMode.Credits:
				credits(br);
				break;

			case HudMode.ConnectionError:
				drawSimpleWindow("Failed to Connect ----> " + net.Error, false);
				break;

			case HudMode.Connecting:
				drawSimpleWindow("Connecting...");
				break;

			case HudMode.InitializingServer:
				drawSimpleWindow("Initialising Server...");
				break;

			// server
			case HudMode.Kick:
				KickMenu();
				break;
		}
			


		if (net.connected)
			moveThisToPlayingHud(midX, midY);
	}









	void drawSimpleWindow(string s, bool disconnect = true) {
		if (disconnect) {
			if (backButton(br))
				Network.Disconnect();
		}else{
			backButton(br);
		}

		DrawWindowBackground();
		GUI.BeginGroup(window);
		GUILayout.Label(s);
		GUI.EndGroup();
	}




	
	
	
	
	
	
	
	void maybePromptClickIn() {
		if (!Screen.lockCursor) {
			Screen.lockCursor = true;
			
			int mar = 32; // margin to push inwards from screen dimensions
			var r = new Rect(0, 0, Screen.width, Screen.height);
			r.x += mar;    r.width -= mar*2;
			r.y += mar;   r.height -= mar*2;				
			GUI.Button(r, "To grab mouse cursor,\n" +
			           "Unity REQUIRES clicking on the game\n" +
			           "screen; after ESC has been pushed.\n" +
			           "(You can remap  MENU action to another key)");
		}
	}
	
	
	
	
	
	
	
	
	
	
	void DrawWindowBackground(bool halfWidth = false) {
		DrawWindowBackground(window, halfWidth);
	}
	void DrawWindowBackground(Rect r, bool halfWidth = false, bool halfHeight = false) {
		GUI.color = new Color(0.3f, 0f, 0.4f, 0.7f);
		
		if (halfWidth)
			r.width /= 2;
		
		if (halfHeight)
			r.height /= 2;
		
		GUI.DrawTexture(r, Pics.White);
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
	
	
	
	
	
	
	
	
	void avatarSetup() {
		DrawWindowBackground(true);

		if (net.connected)
			GUI.Box(new Rect(0, 0, Screen.width, 80), "Currently, you have to change this while disconnected, for changes to be networked");

		GUI.BeginGroup(window);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: ");
		net.localPlayer.name = GUILayout.TextField(net.localPlayer.name);
		if (net.localPlayer.name.Length > 20) 
			net.localPlayer.name = net.localPlayer.name.Substring(0, 20);
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
			if (net.localPlayer.headType > 17) 
				net.localPlayer.headType = 0;
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
		if (net.isServer) { // do we need to check?   can't get here if not server?
			backButton(br);
			DrawWindowBackground();

			GUI.BeginGroup(window);
			
			GUI.Label(new Rect(250,0,100,20), "Kick a player:");
			GUILayout.Label("\n\n\n");
			
			for (int i=0; i<net.players.Count; i++) {
				if (net.players[i].viewID != net.localPlayer.viewID) {
					GUILayout.BeginHorizontal();
					
					if (GUILayout.Button(HudMode.Kick.ToString()))
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
	}
	
	
	
	
	
	
	
	
	
	int fsWidth = 1280;
	int fsHeight = 720;
	void drawSettings() {
		br.x = window.xMax / 2;
		br.y = controls.BottomOfKeyboard;
		backButton(br);

		Rect r = window;
		r.y = controls.BottomOfKeyboard;
		DrawWindowBackground(r, true);
		r.width /= 2;
		r.height = vSpan + vSpan / 2;
		GUI.Box(r, Mode + "");
		
		r.height = window.height;
		GUI.BeginGroup(r);
		
		GUILayout.BeginArea(new Rect(5,vSpan*2,280,380));
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Chat messages fade time: ");
		log.FadeTime = (float)S.GetInt(GUILayout.TextField(log.FadeTime.ToString()));
		GUILayout.EndHorizontal();
		
		net.gunBobbing = GUILayout.Toggle(net.gunBobbing, "Gun Bobbing");
		net.autoPickup = GUILayout.Toggle(net.autoPickup, "Auto-Pickup");
		
		GUILayout.BeginHorizontal();
		fsWidth = S.GetInt(GUILayout.TextField(fsWidth.ToString()));
		fsHeight = S.GetInt(GUILayout.TextField(fsHeight.ToString()));
		if (GUILayout.Button("Fullscreen")) {
			Screen.SetResolution(fsWidth, fsHeight, true);
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Audio Volume:");
		net.gameVolume = GUILayout.HorizontalSlider(net.gameVolume, 0.0f, 1f);

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
	
	void drawControlsAdjunct() {
		br.x = window.xMax / 2;
		br.y = controls.BottomOfKeyboard;
		backButton(br);
		
		Rect r = window;
		r.y = controls.BottomOfKeyboard;
		DrawWindowBackground(r, true, true);
		r.width /= 2;
		r.height = vSpan + vSpan / 2;
		GUI.Box(r, Mode + "");

		r.height = window.height;
		GUI.BeginGroup(r);
		
		GUILayout.BeginArea(new Rect(5,vSpan*2,280,380));
		
		locUser.LookInvert = GUILayout.Toggle(locUser.LookInvert, "Mouselook inversion");
		GUILayout.Label("Mouse Sensitivity:");
		locUser.mouseSensitivity = GUILayout.HorizontalSlider(locUser.mouseSensitivity, 0.1f, 10f);
		if (locUser.LookInvert) PlayerPrefs.SetInt("InvertY", 1);
		else /*``````````````*/ PlayerPrefs.SetInt("InvertY", 0);
		PlayerPrefs.SetFloat("MouseSensitivity", locUser.mouseSensitivity);

		GUILayout.EndArea();
		
		GUI.EndGroup();
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	







	

	Ping[] hostPings;
	void JoinMenu() {
		backButton(br);
		DrawWindowBackground();

		GUI.BeginGroup(window);
				
		GUI.Label(new Rect(250, 0, 100, 20), "Join a game:");
		
		GUILayout.BeginArea(new Rect(5, 20, 290, 400));
		if (GUILayout.Button("Refresh Host List")){
			MasterServer.RequestHostList(net.uniqueGameName);
			hostPings = new Ping[0];
		}
		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(305, 20, 290, 400));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Game Password: ");
			net.password = GUILayout.TextField(net.password);
			GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		
		HostData[] hostData = MasterServer.PollHostList();
		
		int scrollHeight = hostData.Length * 40;
		if (scrollHeight < 350) 
			scrollHeight = 350;
		
		scrollPos = GUI.BeginScrollView(new Rect(0, 50, 600, 350), 
							scrollPos, new Rect(0, 0, 550, scrollHeight+40));
		
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
			GUI.DrawTexture(new Rect(5, (i*40), 550, 1), Pics.White);
			
			if (GUI.Button(new Rect(5, (i*40)+2, 80, 36), "Connect")) {
				Network.Connect(hostData[i],net.password);
				Mode = HudMode.Connecting;
			}
	
			GUI.Label(new Rect(100, (i*40)+2, 150, 30), hostData[i].gameName);
			GUI.Label(new Rect(100, (i*40)+17, 150, 30), "[" + hostData[i].connectedPlayers.ToString() + "/" + hostData[i].playerLimit.ToString() + "]");
			GUI.Label(new Rect(300, (i*40)+2, 150, 60), hostData[i].comment);
			
			if (hostData[i].passwordProtected)
				GUI.Label(new Rect(160, (i*40)+17, 150, 30), "[PASSWORDED]");
			
			if (hostPings[i].isDone)
				GUI.Label(new Rect(450, (i*40)+17, 150, 30), "Ping: " + hostPings[i].time.ToString());
			else
				GUI.Label(new Rect(450, (i*40)+17 ,150, 30), "Ping: ?");
			
			if (i==hostData.Length-1)
				GUI.DrawTexture(new Rect(5, (i*40)+40, 550, 1), Pics.White);
		}
		
		GUI.EndScrollView();
		
		GUI.EndGroup();	
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
			Mode = HudMode.MenuMain;
		
		DrawWindowBackground();
		
		GUI.BeginGroup(window);
		
		if (GUI.Button(new Rect(0,360,200,40), "Sophie Houlden"))
			Application.OpenURL("http://sophiehoulden.com");
		if (GUI.Button(new Rect(200,360,200,40), "7DFPS"))
			Application.OpenURL("http://7dfps.org");
		if (GUI.Button(new Rect(400,360,200,40), "SPLAT DEATH SALAD\nHomepage"))
			Application.OpenURL("http://sophiehoulden.com/games/splatdeathsalad");
		
		GUILayout.Label("_____Current team_____");
		GUILayout.Label("Corpus Callosum - Coding, Logo, Controls, GUI/HUD, Weapon effects");
		GUILayout.Label("IceFlame            - Coding, Tower map, Other map additions");
		GUILayout.Label("");
		GUILayout.Label("_____Media authors_____");
		GUILayout.Label("CarnagePolicy     - Sounds");
		GUILayout.Label("Nobiax/yughues   - Textures");
		GUILayout.Label("");
		GUILayout.Label("This game is a fork of...");
		GUILayout.Label("");
		GUILayout.Label("~~~ SPLAT DEATH SALAD ~~~    (Version: 1.1)");
		GUILayout.Label("");
		GUILayout.Label("Made by Sophie Houlden.  Using Unity, for 7DFPS (June 2012)");
		GUILayout.Label("Click a button to visit these sites:");
		
//		if (Application.isWebPlayer) 
//			GUILayout.Label("*** Visit the homepage for standalone client downloads (win/mac) ***");
		
		GUI.EndGroup();
	}

	
	
	
	
	
	
	
	
	
	

	void moveThisToPlayingHud (int midX, int midY) {
		// team icons
		Color gcolB = GUI.color;
		if (net.CurrMatch.teamBased && net.localPlayer.team != 0) {
			if /*``*/ (net.localPlayer.team == 1) {
				GUI.DrawTexture(new Rect(Screen.width-68,4,64,64), Pics.teamRedFlag);
			} else if (net.localPlayer.team == 2) {
				GUI.DrawTexture(new Rect(Screen.width-68,4,64,64), Pics.teamBlueFlag);
			}
		}
		
		// scoreboard
		if (viewingScores || net.gameOver) {
			DrawWindowBackground(true);
			scores.Draw(window, net, vSpan);
		}
		
		// intermission countdown til next match
		if (net.gameOver) {
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
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	
	
	bool buttonStarts(HudMode hMode, Rect rect) {
		if (GUI.Button(rect, S.GetSpacedOut(hMode.ToString()))) {
			Mode = hMode;
			return true;
		}
		
		return false;
	}

	void menuMain() {
		// draw logo (dimensions are close to perfect square, so midX can be both wid & hei)
		GUI.DrawTexture(new Rect(midX/2, 0, midX, midX), Pics.gameLogo);
		
		int hS = 64; // half the horizontal span of menu item
		int mIH = vSpan + vSpan/2; // menu item height
		var r = new Rect(midX-hS, Screen.height-mIH, hS*2, mIH); // menu rect
		
		// start drawing menu items from the bottom
		if (!Application.isWebPlayer) {
			if (GUI.Button(r, "Quit")) {
				if (net.connected)
					net.DisconnectNow();
				Application.Quit();
			}
			r.y -= mIH;
		}

		if (net.connected) {
			if (GUI.Button(r, "Disconnect"))
				net.DisconnectNow();
			r.y -= mIH;
		}

		buttonStarts(HudMode.Credits, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.Settings, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.Controls, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.Avatar, r); /*^*/ r.y -= mIH;

		if (!net.connected) {
			if (buttonStarts(HudMode.JoinGame, r)) {
				scrollPos = Vector2.zero;
				MasterServer.RequestHostList(net.uniqueGameName);
				hostPings = new Ping[0];
			}
			r.y -= mIH;
		}

		// server mode buttons
		if (net.isServer) {
			buttonStarts(HudMode.Kick, r);
			r.y -= mIH;

			buttonStarts(HudMode.MatchSetup, r);
			r.y -= mIH;
		}

		if (net.connected) {
			if (GUI.Button(r, "RESUME"))
				Mode = HudMode.Playing;
			r.y -= mIH;
		}else{				
			if (buttonStarts(HudMode.StartGame, r)) {
				net.gameName = net.localPlayer.name + "'s match...of the Damned";
			}
			r.y -= mIH;
		}
	}

	bool backButton(Rect r) {
		if (GUI.Button(r, "Back...")) {
			Mode = HudMode.MenuMain;
			return true;
		}
		
		return false;
	}
}
