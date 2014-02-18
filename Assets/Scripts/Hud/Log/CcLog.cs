using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class CcLog : MonoBehaviour {
	public List<LogEntry> Entries = new List<LogEntry>();
	public float FadeTime = 10f;
	public float DisplayTime = -100;
	public bool Typing = false;

	// private 
	string newEntry = "";
	CcNet net;
	
	
	
	void Start() {
		net = GetComponent<CcNet>();
	}
	
	void Update() {
		//if (Input.GetKeyDown("t") && 
		//	!Input.GetKey("tab") && !chatTextEntry){
	}
	
	void OnGUI() {
		// display entries
		if (Time.time < DisplayTime) {
			for (int i=0; i<30; i++) {
				if (i < Entries.Count) {
					Color guiColA = GUI.color;
					
					// highlight
					GUI.color = /* white */ new Color(1, 1, 1, 0.5f);
					GUI.Label(new Rect(10,Screen.height - 55 - (i*15) -1, 700, 20), Entries[Entries.Count-i-1].Maker + " " + Entries[Entries.Count-i-1].Text);
					
					// drop shadow
					GUI.color = /* black */ new Color(0, 0, 0, 0.5f);
					//GUI.color = new Color((messages[messages.Count-i-1].senderColor.r * -1) + 0.5f, (messages[messages.Count-i-1].senderColor.g * -1) + 0.5f, (messages[messages.Count-i-1].senderColor.b * -1) + 0.5f, 1);
					GUI.Label(new Rect(11,Screen.height - 55 - (i*15) +1, 700, 20), Entries[Entries.Count-i-1].Maker + " " + Entries[Entries.Count-i-1].Text);
					//GUI.Label(new Rect(9,Screen.height - 55 - (i*15) -1, 500, 20), messages[messages.Count-i-1].sender + " " + messages[messages.Count-i-1].message);
					
					// normal
					GUI.color = Entries[Entries.Count-i-1].Color;
					//GUI.color = new Color(messages[messages.Count-i-1].senderColor.r + 0.25f, messages[messages.Count-i-1].senderColor.g + 0.25f, messages[messages.Count-i-1].senderColor.b + 0.25f, 1);
					GUI.Label(new Rect(10,Screen.height - 55 - (i*15), 700, 20), Entries[Entries.Count-i-1].Maker + " " + Entries[Entries.Count-i-1].Text);
					
					GUI.color = guiColA;
				}
			}
		}
		
		
		Event e = Event.current;
		if (Typing) {
			DisplayTime = Time.time + FadeTime;
			
			switch (e.keyCode) {
				case KeyCode.Escape:
					Typing = false;
					newEntry = "";
					//AddToLog(net.localPlayer.name + ":", "e.keyCode == KeyCode.Escape", net.ColToVec(net.localPlayer.colA) );
					break;
				case KeyCode.Return:
					if (e.type != EventType.KeyDown) {
						//AddToLog(net.localPlayer.name + ":", "goto default;", net.ColToVec(net.localPlayer.colA) );
						goto default; // without this (was originally a break;) 
						// the webplayer version kept coming in here MANY times in a row, 
						// (so....seeing tons of consecutive KeyUp events?) making the chat entry
						// box invisible until you started typing
					}
				
					Typing = false;
					//AddToLog(net.localPlayer.name + ":", "e.keyCode == KeyCode.Return", net.ColToVec(net.localPlayer.colA) );
						
					if (newEntry != "") {
						if (net.connected) {
							networkView.RPC("AddToLog", RPCMode.All, net.localPlayer.name + ":", newEntry, net.ColToVec(net.localPlayer.colA) );
						}else{
							AddToLog(net.localPlayer.name + ":", newEntry, net.ColToVec(net.localPlayer.colA) );
						}
					}
					
					newEntry = "";
					break;
				default:
					GUI.SetNextControlName("TextField");
					newEntry = GUI.TextField(new Rect(10, Screen.height - 35, 500, 20), newEntry);
					GUI.FocusControl("TextField");
					break;
			}
		}else if (e.keyCode == KeyCode.Return) {
			if (e.type == EventType.KeyDown)
				Typing = true;
		}
	}
	
	[RPC]
	void AddToLog(string name, string s, Vector3 col) {
		var en = new LogEntry(); // new entry
		en.Maker = name;
		en.Color = net.VecToCol(col);
		en.Text = s;
		Entries.Add(en);
		DisplayTime = Time.time + FadeTime;
	}
}
