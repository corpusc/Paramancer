using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class CcLog : MonoBehaviour {
	public List<LogEntry> Entries = new List<LogEntry>();
	public float FadeTime = 20f; // how many seconds a new LogEntry stays visible 
	public float TimeToHideEntireLog = -100;
	public bool Typing = false;

	// private 
	string newEntry = "";
	float leftMargin = 10;
	float vs;
	CcNet net;
	Hud hud;
	
	
	
	void Start() {
		net = GetComponent<CcNet>();
		hud = GetComponent<Hud>();
	}
	
	void Update() {
		//if (Input.GetKeyDown("t") && 
		//	!Input.GetKey("tab") && !chatTextEntry){
	}
	
	void OnGUI() {
		vs = hud.VSpanLabel; // vertical span 

		// display entries
		if (Time.time < TimeToHideEntireLog) {
			var prev = GUI.color;
			float cY = Screen.height - 55; // current Y 

			for (int i=Entries.Count-1;
			     i >= 0 && 
			     (Typing || Entries[i].TimeAdded > Time.time-FadeTime) && 
			     cY>hud.TopOfMaxedLog; 
			     i--
			) {
				string s = Entries[i].Maker + " " + Entries[i].Text;

				// highlight
				GUI.color = Color.white; // /* white */ new Color(1, 1, 1, 0.5f);
				GUI.Label(new Rect(leftMargin-1, cY-1, Screen.width, vs), s);
				
				// drop shadow
				GUI.color = Color.black; // /* black */ new Color(0, 0, 0, 0.5f);
				GUI.Label(new Rect(leftMargin+1, cY+1, Screen.width, vs), s);

				// normal
				GUI.color = Entries[i].Color;
				GUI.Label(new Rect(leftMargin, cY, Screen.width, vs), s);

				cY -= vs;
			}
			
			GUI.color = prev;
		}
		
		
		// FIXME!!! so chat action can be rebound!!!! 
		// handle typing 
		Event e = Event.current;
		if (Typing) {
			TimeToHideEntireLog = Time.time + FadeTime;
			
			switch (e.keyCode) {
				case KeyCode.Escape:
					Typing = false;
					newEntry = "";
					//AddToLog(net.localPlayer.name + ":", "e.keyCode == KeyCode.Escape", net.ColToVec(net.localPlayer.colA) );
					break;
				case KeyCode.Return:
					//if (hud.Mode != HudMode.Playing)
						//break;

					if (e.type != EventType.KeyDown) {
						//AddToLog(net.localPlayer.name + ":", "goto default;", net.ColToVec(net.localPlayer.colA) );
						goto default; // without this (was originally a break;) 
						// the webplayer version kept coming in here MANY times in a row, 
						// (so....seeing tons of consecutive KeyUp events?) making the chat entry
						// box invisible until you started typing
					}
				
					Typing = false;
						
					// if input box not empty, add entry 
					if (newEntry != "") {
						if (net.Connected) {
							networkView.RPC("AddToLog", RPCMode.All, 
			                	net.LocUs.name + ":", newEntry, S.ColToVec(net.LocUs.colA) );
						}else{
							AddToLog(
								net.LocUs.name + ":", newEntry, S.ColToVec(net.LocUs.colA) );
						}
					}
					
					newEntry = "";
					break;
				default:
					GUI.SetNextControlName("TextField");
					newEntry = GUI.TextField(new Rect(leftMargin, Screen.height - 35, Screen.width-leftMargin, 20), newEntry);
					GUI.FocusControl("TextField");
					break;
			}
		}else{ // not typing 
			// if not appropriate to enter chat/commands 
			if (hud.Mode != HudMode.Playing)
				return;

			if (e.keyCode == KeyCode.Return) {
				if (e.type == EventType.KeyDown)
					Typing = true;
			}
		}
	}
	
	[RPC]
	public void AddToLog(string name, string s, Vector3 col) {
		var en = new LogEntry(); // new entry
		en.Maker = name;
		en.Color = S.VecToCol(col);
		en.Text = s;
		Entries.Add(en);
		TimeToHideEntireLog = Time.time + FadeTime;

		if (name != "+")
			Sfx.PlayOmni("Chat - drip");
	}

	public void BroadcastSystemMessage(string msg, Color col) {
		networkView.RPC("AddSystemMessage", RPCMode.All, msg, S.ColToVec(col));
	}

	[RPC]
	void AddSystemMessage(string msg, Vector3 col) {
		var en = new LogEntry(); // new entry
		en.Maker = "";
		en.Color = S.VecToCol(col);
		en.Text = msg;
		Entries.Add(en);
		TimeToHideEntireLog = Time.time + FadeTime;
		Sfx.PlayOmni("Chat - drip");
	}
}
