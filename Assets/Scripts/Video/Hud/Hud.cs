using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Hud : MonoBehaviour {
	public string GoToPrevMenu = "<< Back <<";
	public float TopOfMaxedLog = 0f; // the maximum height it can be, without overlapping anything else 

	// gui 
	public CcLog Log;
	public Font Font;
	public GUIStyle GS;
	public GUIContent GC;
	public float VSpanBox; // box vertical span
	public float VSpanButton; // button vertical span
	public float VSpanLabel; // label vertical span
	public Rect Window = new Rect(0, 0, 600, 400); // background for most menus 
	private HudMode mode = HudMode.SplashLogos;
	public HudMode Mode {
		get { return mode; }
		set {
			scrollPos = Vector2.zero;

			// tasks to do when LEAVING this mode
			switch (mode) {
				case HudMode.Controls:
					CcInput.SaveKeyConfig();
					controls.enabled = false;
					break;
			case HudMode.Settings:
				PlayerPrefs.Save(); //potential FIXME(I didn't see saving anywhere in the settings code, so added it here)
				break;
			}
			
			mode = value;
			
			// tasks to do when ENTERING this mode
			switch (mode) {
				case HudMode.MainMenu:
					lookForServer();
					break;
				case HudMode.Controls: 
					controls.enabled = true;
					break;
				case HudMode.Settings:
					net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
					headSliderPos = (float)net.localPlayer.headType / ((float)Head.Count - 1f);
					break;
			}
		}
	}


	// private
	string defaultName = "Lazy Noob";
	MessageOfTheMoment motm = new MessageOfTheMoment();
	PlayingHud playHud = new PlayingHud();
	MatchSetup matchSetup = new MatchSetup();
	AboutMenu aboutMenu = new AboutMenu();
	float tFOV = 90f;

	// UI element sizes
	int midX, midY; // middle of the screen
	int vSpan = 20; // FIXME: hardwired vertical span of the text.  doubled in many places for button height
	Vector2 scrollPos = Vector2.zero;

	// scripts
	CcNet net;
	Arsenal arse;
	Controls controls;
	LocalUser locUser;

	
	
	void Start() {
		matchSetup.Init();
		//GUI.skin = ScriptableObject.CreateInstance<GUISkin>();

		// scripts
		net = GetComponent<CcNet>();
		Log = GetComponent<CcLog>();
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
		Log.FadeTime = PlayerPrefs.GetFloat("textFadeTime", 10f);
		net.gunBobbing = PlayerPrefs.GetInt("GunBobbing", 1) == 1;
		net.JumpAuto = PlayerPrefs.GetInt("JumpAuto", 1) == 1;
		Sfx.VolumeMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
		tFOV = PlayerPrefs.GetFloat("FOV", 90f);
		//print ("Loaded FOV, value = " + tFOV.ToString());
	}

	float serverSearch;
	void Update() {
		// periodically check for servers
		if (!net.Connected) {
			if (serverSearch+5f < Time.time) {
				serverSearch = Time.time;
				lookForServer();
			}
		}

		if (CcInput.Started(UserAction.Menu)) 
			if (Mode != HudMode.MainMenu) {
				Mode = HudMode.MainMenu;
				Screen.lockCursor = false;
			}else{
				// the only people who should see my fullscreen'ish button
				// now (about how you gotta click in the window to grab cursor)
				// should be people running the game in the Unity IDE
				// & those who hit ESC key
				if (net.Connected && !Application.isWebPlayer)
					Mode = HudMode.Playing;
			}
			
		if (CcInput.Started(UserAction.Scores))
			playHud.viewingScores = !playHud.viewingScores;
	}

	bool firstTime = true;
	int oldW, oldH;
	void OnGUI() {
		// init stuff that HAS to be inside OnGUI() 
		if (firstTime) {
			firstTime = false;

			// setup vertical span sizes
			GC = new GUIContent("Qypjg");
			GS = "Box";
			GS.font = Font;
			GS.fontSize = 24;
			VSpanBox = GS.CalcSize(GC).y;
			GS = "Button";
			GS.font = Font;
			GS.fontSize = 16;
			VSpanButton = GS.CalcSize(GC).y;
			GS = "Label";
			GS.font = Font;
			GS.fontSize = 16;
			VSpanLabel = GS.CalcSize(GC).y;
			
		}

		//setupSkin();
		GUI.skin.button.hover.textColor = S.ShoutyBlue;
		GUI.skin.button.active.textColor = S.Purple;

		// if screen dimensions changed 
		if (oldW != Screen.width ||
		    oldH != Screen.height) 
		{
			oldW = Screen.width;
			oldH = Screen.height;
			// sizes of UI elements 
			midX = oldW/2;
			midY = oldH/2;
			Window.width = oldW * (S.GoldenRatio / (1f + S.GoldenRatio) );
			Window.height = oldH * (S.GoldenRatio / (1f + S.GoldenRatio) );
			Window.x = (Screen.width - Window.width) / 2;
			Window.y = (Screen.height - Window.height) / 2;
		}



		// handle all the modes! 
		switch (Mode) {
			case HudMode.Playing:
				net.localPlayer.Entity.FOV = tFOV;
				TopOfMaxedLog = playHud.Draw(net, arse, midX, midY, VSpanLabel, this);
				maybePromptClickIn();
				break;
				
			case HudMode.NewGame:
			case HudMode.MatchSetup:
				matchSetup.Draw(net.isServer, net, this, vSpan);
				break;
				
//			case HudMode.JoinGame:
//				joinMatchInProgress();
//				break;

			case HudMode.SplashLogos:
				showSplashLogos();
				break;

			case HudMode.MainMenu:
				if (!net.Connected) {
					avatarView();
				}

				menuMain();
				break;

			case HudMode.About:
				// maybe this should be titled "ABOUT"?  would be more accurate?... and more standard 
				menuBegin();
				if /* future goals scope changed */ (aboutMenu.Draw(this))
					scrollPos = Vector2.zero;
				menuEnd();
				break;
			case HudMode.Controls:
				break;
				
			case HudMode.Settings:
				avatarView();
				drawSettings();
				break;
			
			case HudMode.Credits:
				credits();
				break;

			case HudMode.ConnectionError:
				drawSimpleWindow(net.Error, Color.red);
				break;

			case HudMode.Connecting:
				drawSimpleWindow("", Color.yellow);
				break;

			case HudMode.InitializingServer:
				drawSimpleWindow("", S.WhiteTRANS);
				break;

			// server
			case HudMode.KickAPlayer:
				kickWindow();
				break;
		}
			
		// intermission countdown til next match
		if (net.Connected && 
		    net.gameOver) 
		{
			string s = "Next Game in: " +  Mathf.FloorToInt(net.NextMatchTime).ToString() + " seconds.";
			S.SetShoutyColor();
			S.OutlinedLabel(new Rect(midX-50, 5, 200, 30), s);
		}
	}









	void drawSimpleWindow(string s, Color col) {
		menuBegin(col);
		GUILayout.Label(s);
		menuEnd();
	}




	
	
	
	
	
	
	
	void maybePromptClickIn() {
		if (!Screen.lockCursor) {
			Screen.lockCursor = true;
			
			int mar = 32; // margin to push inwards from screen dimensions
			var r = new Rect(0, 0, Screen.width, Screen.height);
			r.x += mar;    r.width -= mar*2;
			r.y += mar;   r.height -= mar*2;				
			GUI.Button(r, "To grab mouse cursor,\nUnity REQUIRES clicking on the game screen");
		}
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
	
	
	void menuBegin(bool scrolling = true, bool startAtTop = false) {
		menuBegin(S.WhiteTRANS, scrolling, startAtTop);
	}
	void menuBegin(Color col, bool scrolling = true, bool startAtTop = false) {
		var r = Window;

		// special exceptions 
		if (startAtTop) {
			r.y = 0;
			r.height = Screen.height - controls.HeightOfKeyboard;
		}

		switch (Mode) {
			case HudMode.Settings:
				r.x = r.y = 0;
				r.width = Screen.width/3;
				r.height = Screen.height;
				break;
		}

		// universal 
		GUI.color = col;
		GUI.DrawTexture(r, Pics.White);
		
		GUILayout.BeginArea(r, GS);
		
		GUI.color = Color.white;
		GUILayout.Box(S.GetSpacedOut(Mode + ""));

		if (scrolling)
			scrollPos = GUILayout.BeginScrollView(scrollPos);
	}

	void menuEnd(bool scrolling = true) {
		GUI.color = Color.white;

		if (scrolling)
			GUILayout.EndScrollView();

		if (GUILayout.Button(GoToPrevMenu)) {
			switch (Mode) {
				case HudMode.ConnectionError:
					Network.Disconnect();
					break;
				case HudMode.Settings:
					net.localPlayer.name = PlayerPrefs.GetString("PlayerName", defaultName);
					break;
			}

			Mode = HudMode.MainMenu;
		}
		
		GUILayout.EndArea();
	}
	






	
	
	
	public void CategoryHeader(string s, bool wantCentering = true, bool spacing = true) {
		if (spacing)
			GUILayout.Label("");
		
		GUILayout.BeginHorizontal();
		
		if (wantCentering) 
			GUILayout.FlexibleSpace();
		
		GUILayout.Box(s, GUILayout.MaxWidth(GetWidthBox(s)));
		
		if (wantCentering) 
			GUILayout.FlexibleSpace();
		
		GUILayout.EndHorizontal();
	}













	public void SizedLabel(string s) {
		GUILayout.Label(s, GUILayout.MaxWidth(GetWidthLabel(s)));
	}









	int fsWidth = 1280;
	int fsHeight = 720;
	Vector2 innerScrollPos;
	float headSliderPos = 0f;
	void drawSettings() {
		// warn people changes can be ignored 
		if (net.Connected) {
			S.SetShoutyColor();
			GUI.Box(new Rect(Screen.width/3, 0, Screen.width/3*2, 80), "Currently, you have to change AVATAR settings while disconnected \n for changes to be networked");
		}

		menuBegin();

		// allow sidestepping to control config screen 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Configure Mouse & Keys, or Controller")) {
			Mode = HudMode.Controls;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// misc settings 
		// gun bob 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		net.gunBobbing = TickBox.Display(net.gunBobbing, "Gun bobbing");
		if (net.gunBobbing)
			PlayerPrefs.SetInt("GunBobbing", 1);
		else
			PlayerPrefs.SetInt("GunBobbing", 0);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// auto jump on jump pad
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		net.JumpAuto = TickBox.Display(net.JumpAuto, "Automatically jump on jump pads");
		if (net.JumpAuto)
			PlayerPrefs.SetInt("JumpAuto", 1);
		else
			PlayerPrefs.SetInt("JumpAuto", 0);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// chat fade
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		SizedLabel("Chat/Log fade time:   ");
		Log.FadeTime = (float)S.GetInt(GUILayout.TextField(Log.FadeTime.ToString()));
		PlayerPrefs.SetFloat("textFadeTime", Log.FadeTime);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// audio
		//categoryHeader("Audio");
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Master Volume:   ");
		Sfx.VolumeMaster = GUILayout.HorizontalSlider(Sfx.VolumeMaster, 0.0f, 1f, GUILayout.MinWidth(128));
		PlayerPrefs.SetFloat("MasterVolume", Sfx.VolumeMaster);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		// FOV slider 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		var s = "FOV (field of view): ";
		float wid = GetWidthLabel(s + "188.8");
		GUILayout.Label(s + tFOV.ToString("#.#"), GUILayout.MinWidth(wid));
		tFOV = GUILayout.HorizontalSlider(tFOV, 50f, 160f, GUILayout.MinWidth(128));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// graphics
		//categoryHeader("Graphics");
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Full screen"))
			Screen.SetResolution(fsWidth, fsHeight, true);
		GUILayout.Label("(");
		fsWidth = S.GetInt(GUILayout.TextField(fsWidth.ToString()));
		GUILayout.Label("X");
		fsHeight = S.GetInt(GUILayout.TextField(fsHeight.ToString()));
		GUILayout.Label(")");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// avatar settings
		CategoryHeader("Avatar");
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		SizedLabel("Name:   ");
		net.localPlayer.name = GUILayout.TextField(net.localPlayer.name);
		if (net.localPlayer.name.Length > 20) 
			net.localPlayer.name = net.localPlayer.name.Substring(0, 20);
		net.localPlayer.name = FormatName(net.localPlayer.name);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		innerScrollPos = GUILayout.BeginScrollView(innerScrollPos);
		
		// head slider 
		float hss = 1f / (int)Head.Count; // head slider span
		net.localPlayer.headType = (int)(headSliderPos / hss);
		GUILayout.BeginHorizontal();
		float w = GetWidthLabel(S.GetSpacedOut("" + Head.ElephantHeadMesh));
		GUILayout.Label(S.GetSpacedOut("" + (Head)net.localPlayer.headType), GUILayout.MaxWidth(w));
		headSliderPos = GUILayout.HorizontalSlider(headSliderPos, 0f, 1f);
		GUILayout.EndHorizontal();

		// colour sliders
		GUILayout.BeginHorizontal();
		GUI.color = net.localPlayer.colA;
		GUILayout.Box(Pics.White);
		GUI.color = net.localPlayer.colB;
		GUILayout.Box(Pics.White);
		GUI.color = net.localPlayer.colC; 
		GUILayout.Box(Pics.White);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUI.color = net.localPlayer.colA;
		GUILayout.Box("Colour A");
		GUI.color = net.localPlayer.colB;
		GUILayout.Box("Colour B");
		GUI.color = net.localPlayer.colC; 
		GUILayout.Box("Colour C");
		GUILayout.EndHorizontal();
		
		colorSliders();

		GUI.color = Color.white;
		GUILayout.EndScrollView();
		menuEnd();

		// now just save player 
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
		PlayerPrefs.SetFloat("FOV", tFOV);
	}

	void colorSliders() {
		GUILayout.BeginHorizontal();

		GUI.color = Color.red;
		net.localPlayer.colA.r = GUILayout.HorizontalSlider(net.localPlayer.colA.r, 0f, 1f);
		net.localPlayer.colB.r = GUILayout.HorizontalSlider(net.localPlayer.colB.r, 0f, 1f);
		net.localPlayer.colC.r = GUILayout.HorizontalSlider(net.localPlayer.colC.r, 0f, 1f);
		
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();
		
		GUI.color = Color.green;
		net.localPlayer.colA.g = GUILayout.HorizontalSlider(net.localPlayer.colA.g, 0f, 1f);
		net.localPlayer.colB.g = GUILayout.HorizontalSlider(net.localPlayer.colB.g, 0f, 1f);
		net.localPlayer.colC.g = GUILayout.HorizontalSlider(net.localPlayer.colC.g, 0f, 1f);
		
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();
		
		GUI.color = Color.blue;
		net.localPlayer.colA.b = GUILayout.HorizontalSlider(net.localPlayer.colA.b, 0f, 1f);
		net.localPlayer.colB.b = GUILayout.HorizontalSlider(net.localPlayer.colB.b, 0f, 1f);
		net.localPlayer.colC.b = GUILayout.HorizontalSlider(net.localPlayer.colC.b, 0f, 1f);

		GUILayout.EndHorizontal();
	}	
	
	
	
	
	
	
	
	
	void kickWindow() {
		if (net.isServer) {
			menuBegin(Color.red);

			for (int i=0; i<net.players.Count; i++) {
				if (net.players[i].viewID != net.localPlayer.viewID) {
					GUILayout.BeginHorizontal();
					
					if (GUILayout.Button(HudMode.KickAPlayer.ToString())) {
						net.Kick(i, false);
						break;
					}

					string pingString = "?";
					if (net.players[i].ping.isDone) 
						pingString = net.players[i].ping.time.ToString();
					
					GUILayout.Label("- " + net.players[i].name + " - [Ping: " + pingString + "]");
					
					GUILayout.EndHorizontal();
				}
			}

			menuEnd();
		}
	}
	
	
	
	
	
	
	void setupSkin() {
		// normal button 
		// hover button 
		// active button

//		var nt = (Texture2D)Pics.Get("Button"); // normal texture 
//		//var nt = (Texture2D)Pics.Get("ButtonIce"); // normal texture 
//		var ht = (Texture2D)Pics.Get("ButtonIceHover"); // hover texture 
//		var at = (Texture2D)Pics.Get("ButtonIceClicked"); // active texture 
//		nt.filterMode = FilterMode.Trilinear;
//		ht.filterMode = FilterMode.Trilinear;
//		at.filterMode = FilterMode.Trilinear;



//		// handle button modes 
//		var hmm = 10;
		//var pixel = 4; // pixel margin 
		//var uvMar = 32; // UV margin (for unstretchable texture coord border) 
		//GUI.skin.button.border = new RectOffset(uvMar, uvMar, uvMar, uvMar);
//		GUI.skin.button.margin = new RectOffset(hmm, hmm, hmm, hmm); // spacing tween UI elements 
//		GUI.skin.button.padding = new RectOffset(pixel, pixel, pixel, pixel);     // padding i was able to get working predictably (by itself) 
//		GUI.skin.button.stretchWidth = true;
//		GUI.skin.button.stretchHeight = true;




//		GUI.skin.button.normal.background = nt;
//		GUI.skin.button.hover.background = ht;
//		GUI.skin.button.active.background = at;



		GUI.skin.box.font = Font;
		GUI.skin.button.font = Font;
		GUI.skin.label.font = Font;
		GUI.skin.textArea.font = Font;
		GUI.skin.textField.font = Font;

		//GUI.skin.button.normal.textColor = S.Orange;
		//.skin.button.hover gets set to ShoutyColor per frame 
		//GUI.skin.button.active.textColor = Color.cyan; 
		
		GUI.skin.box.fontSize = 24; // these are the semi-transparent dark gray boxes with a border around the text 
		GUI.skin.button.fontSize = 16;
		GUI.skin.label.fontSize = 16;
		GUI.skin.textArea.fontSize = 16;
		GUI.skin.textField.fontSize = 16;

		GUI.skin = GUI.skin;
	}












	void centeredLabel(string s) {
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(s);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}














	public void DrawControlsAdjunct() {
		menuBegin(S.WhiteTRANS, true, true);

		// inform user of remapping abilities 
		centeredLabel("Press keys to LIGHT THEM UP");
		S.SetShoutyColor();
		centeredLabel("Left-Click to change actions");
		GUI.color = Color.white;
		centeredLabel("Right-Click to move keys");
		GUILayout.Label("");

		// look inversion 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		/**/locUser.LookInvert = TickBox.Display(locUser.LookInvert, "Look up/down reversed");
		if (locUser.LookInvert) PlayerPrefs.SetInt("InvertY", 1);
		else /*``````````````*/ PlayerPrefs.SetInt("InvertY", 0);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// sensitivity 
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Look sensitivity:", GUILayout.ExpandWidth(false));
		locUser.LookSensitivity = GUILayout.HorizontalSlider(locUser.LookSensitivity, 0.1f, 10f, GUILayout.MinWidth(196));
		PlayerPrefs.SetFloat("LookSensitivity", locUser.LookSensitivity);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// allow resetting to defaults 
		//if (GUILayout.Button("Default Keys")) CcInput.SetDefaultBinds();

		// the pics 
		GUILayout.BeginHorizontal(); 
		GUILayout.FlexibleSpace();
		for (int i = 0; i < (int)ControlDevice.Count; i++) {
			Texture p;

			if ((ControlDevice)i == ControlDevice.GamePad) {
				p = Pics.Get("PerspectiveMin");
				
				if (GUILayout.Button(p, GUILayout.MaxWidth(256), GUILayout.MaxHeight(128))) {
					controls.SetCurrDevice((ControlDevice)i);
				}
			}else{
				p = Pics.Get(((ControlDevice)i) + "");
				
				if (GUILayout.Button(p, GUILayout.MaxWidth(64), GUILayout.MaxHeight(128))) {
					controls.SetCurrDevice((ControlDevice)i);
				}
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

//		// the names 
//		GUILayout.BeginHorizontal(); 
//		for (int i = 0; i < (int)ControlDevice.Count; i++)
//			GUILayout.Box(((ControlDevice)i).ToString());
//		GUILayout.EndHorizontal();
		
		menuEnd(); 
	}

	
	
	
	
	
	
	
	
	
	
	
	
	
	







	

	Ping[] hostPings;
	void lookForServer() {
		MasterServer.RequestHostList(net.uniqueGameName);
		hostPings = new Ping[0];
	}








	int prevNumServers = 0;
	void listMatchesInProgress(float halfWidth, float topOfJoinButtons) {
		HostData[] hostData = MasterServer.PollHostList();

		// setup ping info 
		if (hostData.Length == 0) {
			//GUILayout.Label("No hosts found!");
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
		
		Rect r = new Rect(0, topOfJoinButtons, 0, 0);
		// iterate thru all matches that are being served 
		for (int i=0; i<hostData.Length; i++) {
			// build all the text for the JOIN MATCH button 
			var s = " JOIN ";
			if (net.Connected)
				s += "DIFFERENT ";
			s += "MATCH \n\n";
			s += "\"" /*'''*/ + hostData[i].gameName + "\" \n";

			if (net.Connected && 
			    net.gameName == hostData[i].gameName) { // don't show this button if it has the same game name 
				continue;
			}else{
				// play sound if number of servers goes up 
				if (prevNumServers < hostData.Length)
					Sfx.PlayOmni("NewGame");
			}

			// comment 
			var com = hostData[i].comment;
			if (com.Contains("\n")) {
				com = com.Insert(com.IndexOf('\n')+1, "Map:   ");
				com = "Mode:   " + com + '\n';
			}
			s += com;

			// ping 
			if (hostPings[i] != null && hostPings[i].isDone)
				s += "Ping:   " + hostPings[i].time;
			else
				s += "Ping:   ???";

			// user # 
			s += "    Users:   " + hostData[i].connectedPlayers; // "/" + hostData[i].playerLimit 
			


			// rectangle of join button 
			float w = GetWidthButton(s);
			float h = GetHeightButton(s);
			r.x = midX-w/2;
			r.y -= h;
			r.width = w;
			r.height = h;
			if (GUI.Button(r, s)) {
				if (net.Connected)
					net.DisconnectNow();

				Network.Connect(hostData[i],net.password);
				Mode = HudMode.Connecting;
			}

			// password box if needed 
			var rect = r;
			if (hostData[i].passwordProtected) {
				S.SetShoutyColor();
				s = "Password:";
				w = GetWidthBox(s);
				rect.x = rect.xMax;
				rect.width = w;
				rect.height = h/2;
				GUI.Box(rect, s);
				rect.y += h/2;
				net.password = GUI.TextField(rect, net.password/*, GUILayout.MinWidth(16)*/);
				GUI.color = Color.white;
			}

			prevNumServers = hostData.Length;
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
			
			// visibility of heads 
			for (int i = 0; i < (int)Head.Count; i++) {
				if (i == net.localPlayer.headType)
					GameObject.Find("" + (Head)i).renderer.enabled = true;
				else
					GameObject.Find("" + (Head)i).renderer.enabled = false;
			}

			GameObject.Find("PlayerNameText").GetComponent<TextMesh>().text = net.localPlayer.name;
		}
	}














	void credits() {
		menuBegin();

		CategoryHeader("Current team", false, false);
		GUILayout.Label("Corpus Callosum - Coding, Various media & effects, Game design, Logos, Control config");
		GUILayout.Label("IceFlame       - Coding, Various media & effects, Announcer, ProcGen maps");

		CategoryHeader("Engine", false);
		GUILayout.Label("This is an extensively remodeled fork of a game made in a seven day game jam.  By Sophie Houlden");
		GUILayout.Label("(sophiehoulden.com)");
		// CLEANME: IS THERE A WAY TO OPEN URL IN A BACKGROUND TAB/WINDOW?! 
		// ... because otherwise, the player is thrown out of the game to the website 
		//if (GUILayout.Button("Sophie Houlden (web page)"))
			//Application.OpenURL("http://sophiehoulden.com");  

		CategoryHeader("Media authors", false);
		GUILayout.Label("CarnagePolicy          - Sounds");
		GUILayout.Label("Nobiax/yughues        - Textures");
		GUILayout.Label("Wayne Brown         - A few icons");
		//if (Application.isWebPlayer) 
//			GUILayout.Label("some nebulous message to you Mr. Webplayer Guy");

		menuEnd();
	}

	
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	void showSplashLogos() {
		Rect r = new Rect(0, 0, Screen.width, Screen.height);
		GUI.DrawTexture(r, Pics.Get("blackTex"));
		GUI.DrawTexture(r, Pics.Get("Logo - CazCore"));

		if (Time.time > 5f || Debug.isDebugBuild)
			Mode = HudMode.MainMenu;
	}
	


	
	
	
	
	
	
	
	
	
	
	bool buttonStarts(HudMode hMode, Rect rect) {
		if (GUI.Button(rect, S.GetSpacedOut(hMode.ToString()))) {
			Mode = hMode;
			return true;
		}
		
		return false;
	}

	void menuMain() {
		float hS = 112; // half the horizontal span of menu items 

		// draw logos 
		float paraWid = midX-hS;
		// 'Paramancer' dimensions are close to perfect square, so midX can be both wid & hei 
		GUI.DrawTexture(new Rect(0, Screen.height-paraWid, paraWid, paraWid), Pics.Get("Logo - Paramancer"));

		// 		company logo 
		float cazWid = paraWid/2;
		var r = new Rect(cazWid/2, 0, cazWid, cazWid/2);
		//GUI.DrawTexture(r, Pics.Get("Logo - CazCore"));
		string s = "brings you:";
		r.width = GetWidthLabel(s);
		r.height = 100; // doesn't really matter with a label 
		r.x = cazWid-r.width/2;
		r.y = cazWid/2;
		//GUI.Label(r, s);

		// draw menus 
		int mIH = vSpan + vSpan/2; // menu item height 
		r = new Rect(midX-hS, Screen.height, hS*2, mIH); // menu rect 
		r.y -= mIH;

		// ...from the bottom 
		if (!Application.isWebPlayer) {
			if (GUI.Button(r, "Quit")) {
				if (net.Connected)
					net.DisconnectNow();
				Application.Quit();
			}
			r.y -= mIH;
		}

		if (net.Connected) {
			if (GUI.Button(r, "Disconnect"))
				net.DisconnectNow();
			r.y -= mIH;
		}

		buttonStarts(HudMode.Credits, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.Settings, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.Controls, r); /*^*/ r.y -= mIH;
		buttonStarts(HudMode.About, r); /*^*/ r.y -= mIH;

//		// JoinGame button 
//		if (!net.Connected) {
//			if (buttonStarts(HudMode.JoinGame, r)) {
//				lookForServer();
//			}
//			r.y -= mIH;
//		}

		// server mode buttons
		if (net.isServer) {
			buttonStarts(HudMode.KickAPlayer, r);
			r.y -= mIH;

			buttonStarts(HudMode.MatchSetup, r);
			r.y -= mIH;
		}

		// last/topmost static button 
		if (net.Connected) {
			if (GUI.Button(r, "RESUME"))
				Mode = HudMode.Playing;
		}else{				
			if (buttonStarts(HudMode.NewGame, r)) {
				net.gameName = net.localPlayer.name + "'s match...of the Damned";
			}
		}

		// dynamic buttons for all the servers/matches/instances currently in progress 
		listMatchesInProgress(hS, r.y);

		motm.Draw(this);
	}














	public float GetWidthBox(string s) {
		GS = "Box";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).x;
	}
	
	public float GetHeightBox(string s) {
		GS = "Box";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).y;
	}
	
	public float GetWidthButton(string s) {
		GS = "Button";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).x;
	}
	
	public float GetHeightButton(string s) {
		GS = "Button";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).y;
	}
	
	public float GetWidthLabel(string s) {
		GS = "Label";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).x;
	}
	
	public float GetHeightLabel(string s) {
		GS = "Label";
		GC = new GUIContent(s);
		return GS.CalcSize(GC).y;
	}
}
