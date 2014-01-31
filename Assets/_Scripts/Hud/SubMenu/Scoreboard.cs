using UnityEngine;
using System.Collections;

public class Scoreboard {
	public void Draw(Rect r, CcNet net, int vSpan) {
		GUI.BeginGroup(r);
		
		GUI.color = Color.cyan;
		GUI.Label(new Rect(250, 0, 100, vSpan), "Scores:");
		
		if (!net.CurrMatch.teamBased) {
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
			
			GUI.Label(new Rect(10, vSpan, 150, vSpan), "Name:");
			GUI.Label(new Rect(160, vSpan, 50, vSpan), "Frags:");
			GUI.Label(new Rect(210, vSpan, 50, vSpan), "Deaths:");
			GUI.Label(new Rect(270, vSpan, 50, vSpan), "Score:");
			
			if (net.CurrMatch.playerLives != 0) 
				GUI.Label(new Rect(400, vSpan,50,vSpan), "Lives:");
			
			for (int i=0; i<net.players.Count; i++) {
				GUI.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				
				if (net.players[i].local) 
					GUI.color = new Color(1, 1, 1, 1f);
				if (net.players[i].currentScore == highScore && mostLives == 0)
					GUI.color = new Color(
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 1f);
				if (net.players[i].lives == mostLives && mostLives > 0) 
					GUI.color = new Color(
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 
						Random.Range(0.5f, 1f), 1f);
				
				GUI.Label(new Rect(10, (i*vSpan) + 40, 150, vSpan), net.players[i].name);
				GUI.Label(new Rect(160, (i*vSpan) + 40, 50, vSpan), net.players[i].kills.ToString());
				GUI.Label(new Rect(210, (i*vSpan) + 40, 50, vSpan), net.players[i].deaths.ToString());
				GUI.Label(new Rect(270, (i*vSpan) + 40, 50, vSpan), net.players[i].currentScore.ToString());
				
				if (net.CurrMatch.playerLives != 0) 
					GUI.Label(new Rect(400, (i*vSpan) + 40, 50, vSpan), net.players[i].lives.ToString());
			}
			
		}
		
		if (net.CurrMatch.teamBased) {
			GUI.color = new Color(1f, 0f, 0f, 1f);
			
			if (net.gameOver && net.team1Score>net.team2Score) 
				GUI.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f,1f), Random.Range(0.5f, 1f), 1f);
			
			GUI.Label(new Rect(100, 20, 150, 20), "Team 1 Score: " + net.team1Score.ToString());
			GUI.color = new Color(0f, 1f, 1f, 1f);
			
			if (net.gameOver && net.team2Score>net.team1Score) 
				GUI.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f,1f), Random.Range(0.5f,1f), 1f);
			
			GUI.Label(new Rect(300, 20, 150, 20), "Team 2 Score: " + net.team2Score.ToString());
			GUI.Label(new Rect(10, 40, 150, 20), "Name:");
			GUI.Label(new Rect(160, 40, 50, 20), "Kills:");
			GUI.Label(new Rect(210, 40, 50, 20), "Deaths:");
			GUI.Label(new Rect(270, 40, 50, 20), "Score:");
			
			// team 1
			int yOffset = 0;
			GUI.Label(new Rect(10,(yOffset*20) + 60,150,20), "Team 1:");
			yOffset++;
			
			for (int i=0; i<net.players.Count; i++) {
				GUI.color = new Color(1f, 0f, 0f, 1f);
				if (net.players[i].team == 1) {
					
					if (net.players[i].local) 
						GUI.color = new Color(1, 0.3f, 0.3f, 1f);
					if (net.gameOver && net.team1Score>net.team2Score) 
						GUI.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
					
					GUI.Label(new Rect(10, (yOffset*20) + 60, 150, vSpan), net.players[i].name);
					GUI.Label(new Rect(160, (yOffset*20) + 60, 50, vSpan), net.players[i].kills.ToString());
					GUI.Label(new Rect(210, (yOffset*20) + 60, 50, vSpan), net.players[i].deaths.ToString());
					GUI.Label(new Rect(270, (yOffset*20) + 60, 50, vSpan), net.players[i].currentScore.ToString());
					
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
					if (net.players[i].local) 
						GUI.color = new Color(0.3f, 1, 1, 1f);
					if (net.gameOver && net.team2Score>net.team1Score) 
						GUI.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
				
					GUI.Label(new Rect(10, yOffset*20 + 60, 150, 20), net.players[i].name);
					GUI.Label(new Rect(160, yOffset*20 + 60, 50, 20), net.players[i].kills.ToString());
					GUI.Label(new Rect(210, yOffset*20 + 60, 50, 20), net.players[i].deaths.ToString());
					GUI.Label(new Rect(270, yOffset*20 + 60, 50, 20), net.players[i].currentScore.ToString());
					
					yOffset++;
				}
			}
			
			GUI.color = Color.black;
			yOffset++;
			GUI.Label(new Rect(10,(yOffset*20) + 60,300,20), 
				">> TO CHANGE TEAMS, PRESS '" + InputUser.GetKeyLabel(UserAction.SwapTeam) + "' <<");
		}
		
		GUI.EndGroup();
	}
}
