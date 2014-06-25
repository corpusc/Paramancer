using UnityEngine;
using System.Collections;

public class Scoreboard {
	float fragsX, deathsX, scoreX;



	public float Draw(CcNet net, Hud hud, float lvs) {
		GUI.color = Color.grey;
		float cY = lvs; // current Y 

		if (!net.CurrMatch.teamBased) {
			int highScore = -9999;
			if (net.gameOver) {
				for (int i=0; i<net.Entities.Count; i++) {
					if (highScore < net.Entities[i].currentScore) {
						highScore = net.Entities[i].currentScore;
					}
				}
			}
			
			int mostLives = 0;
			if (net.gameOver) {
				for (int i=0; i<net.Entities.Count; i++) {
					if (net.Entities[i].lives > mostLives) {
						mostLives = net.Entities[i].lives;
					}
				}
			}
			
			GUI.Label(new Rect(0,  0, 150, lvs), "Name:");
			GUI.Label(new Rect(150, 0, 50, lvs), "Frags:");
			GUI.Label(new Rect(200, 0, 50, lvs), "Deaths:");
			GUI.Label(new Rect(260, 0, 50, lvs), "Score:");
			
			if (net.CurrMatch.playerLives != 0) 
				GUI.Label(new Rect(400, 0, 50, lvs), "Lives:");
			
			// cycle thru players 
			for (int i=0; i<net.Entities.Count; i++) {
				GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				
				if (net.Entities[i].local) 
					GUI.color = new Color(1, 1, 1, 1f);
				if (net.Entities[i].currentScore == highScore && mostLives == 0)
					GUI.color = new Color(
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 1f);
				if (net.Entities[i].lives == mostLives && mostLives > 0) 
					GUI.color = new Color(
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 1f);
				
				GUI.Label(new Rect(0,   cY, 150, lvs), net.Entities[i].name);
				GUI.Label(new Rect(150, cY, 50, lvs), net.Entities[i].kills.ToString());
				GUI.Label(new Rect(200, cY, 50, lvs), net.Entities[i].deaths.ToString());
				GUI.Label(new Rect(260, cY, 50, lvs), net.Entities[i].currentScore.ToString());
				
				if (net.CurrMatch.playerLives != 0) 
					GUI.Label(new Rect(400, cY, 50, lvs), net.Entities[i].lives.ToString());

				cY += lvs;
			}			
		}



		// team mode scoreboard 
		if (net.CurrMatch.teamBased) {
			fragsX = hud.GetWidthLabel("Frags:   ");
			deathsX = hud.GetWidthLabel("Deaths:   ");
			scoreX = hud.GetWidthLabel("Score:   ");
			
			var left = hud.Window;
			var right = hud.Window;
			left.width = hud.Window.width/2;
			right.width = hud.Window.width/2;
			right.x = hud.Window.width/2 + hud.Window.x;

			// RED (left side) 
			// color 
			GUI.color = S.RedTRANS;
			if (net.gameOver && net.team1Score > net.team2Score)
				anyBrightColor();			
			
			GUI.DrawTexture(left, Pics.Get("BlankWhite"));
			showHotkey(UserAction.SwapTeam, left);
			GUI.color = Color.white;

			GUILayout.BeginArea(left);
			GUILayout.BeginVertical();

			// score 
			GUILayout.Box("RED: " + net.team1Score.ToString());
			teamPane();

			for (int i=0; i<net.Entities.Count; i++) {
				if (net.Entities[i].team == 1) {
					showNameAndStats(i, net);
				}
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();



			// BLUE (right side) 
			// color 
			GUI.color = S.BlueTRANS;
			if (net.gameOver && net.team2Score > net.team1Score) 
				anyBrightColor();	

			GUI.DrawTexture(right, Pics.Get("BlankWhite"));
			showHotkey(UserAction.Scores, right);
			GUI.color = Color.white;

			GUILayout.BeginArea(right);
			GUILayout.BeginVertical();

			// score 
			GUILayout.Box("BLUE: " + net.team2Score.ToString());
			teamPane();
			for (int i=0; i<net.Entities.Count; i++) {
				if (net.Entities[i].team == 2) {
					showNameAndStats(i, net);
				}
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		return cY;
	}

	void showNameAndStats(int i, CcNet net) {
		if (net.Entities[i].local) 
			S.SetShoutyColor();

		GUILayout.BeginHorizontal();
		GUILayout.Label(net.Entities[i].name);
		GUILayout.FlexibleSpace();
		GUILayout.Label(net.Entities[i].kills.ToString(), GUILayout.MinWidth(fragsX));
		GUILayout.Label(net.Entities[i].deaths.ToString(), GUILayout.MinWidth(deathsX));
		GUILayout.Label(net.Entities[i].currentScore.ToString(), GUILayout.MinWidth(scoreX));
		GUILayout.EndHorizontal();
	}

	void showHotkey(UserAction ua, Rect r) {
		float offs = 8;
		int span = 64;
		r.y = r.yMax - span;
		r.width = span;
		r.height = span;

		GUI.color = Color.white;
		GUI.Label(r, Pics.Get("KeyCap"));
		GUI.color = Color.black;
		r.x += offs;
		r.y += offs;
		GUI.Label(r, CcInput.GetKeyLabel(ua));
		GUI.color = Color.white;
		r.x += span;
		r.width = 200; // don't think we need a precise size 

		if (ua == UserAction.Scores)
			GUI.Label(r, "= Close");
		else
			GUI.Label(r, S.GetSpacedOut("= " + ua));
	}

	static void teamPane(){
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Frags:    ");
		GUILayout.Label("Deaths:   ");
		GUILayout.Label("Score:   ");
		GUILayout.EndHorizontal();
	}

	static void anyBrightColor() {
		GUI.color = new Color(
			Random.Range (0.5f, 1f), 
			Random.Range (0.5f, 1f), 
			Random.Range (0.5f, 1f), 1f);
	}
}
