﻿using UnityEngine;
using System.Collections;

public class PlayingHud {
	public void Draw(CcNet net, Arsenal arse, int midX, int midY) {
		var locEnt = net.localPlayer.Entity;
		//var locEnt = net.localPlayer.GetComponent<EntityClass>();
		
		if (locEnt == null)
			return;
		
		var gunACooldown = locEnt.handGunCooldown;
		Item gunA = locEnt.handGun;
		Item gunB = locEnt.handGun;
		
		// 2 types of crosshairs
		GUI.DrawTexture(new Rect(
			midX-Pics.crossHair.width/2, 
			midY-Pics.crossHair.height/2, 
			Pics.crossHair.width, 
			Pics.crossHair.height), Pics.crossHair);
		
		if (gunA == Item.Swapper) {
			int swapperFrame = Mathf.FloorToInt((Time.time*15f) % Pics.swapperCrosshair.Length);
			if (!locEnt.swapperLocked) 
				swapperFrame = 0;
			
			GUI.DrawTexture(new Rect(
				locEnt.swapperCrossX-32, 
				(Screen.height-locEnt.swapperCrossY)-32, 64, 64), Pics.swapperCrosshair[swapperFrame]);
		}
		
		// health bar
		int healthWidth = (Screen.width/3);
		GUI.DrawTexture(new Rect(midX-(healthWidth/2)-2, Screen.height-15, healthWidth+4, 9), Pics.blackTex);
		int healthWidthB = (int)((((float)healthWidth)/100f)*net.localPlayer.health);
		GUI.DrawTexture(new Rect(midX-(healthWidth/2), Screen.height-13, healthWidthB, 5), Pics.backTex);
		
		// energy bar
		int energyWidth = (Screen.width/3);
		GUI.DrawTexture(new Rect(midX-(energyWidth/2)-2, Screen.height-30, healthWidth+4, 9), Pics.blackTex);
		int energyWidthB = (int)(((float)healthWidth) * locEnt.EnergyLeft);
		GUI.DrawTexture(new Rect(midX-(energyWidth/2), Screen.height-28, energyWidthB, 5), Pics.backTex);
		
		// lives
		if (net.CurrMatch.playerLives > 0) {
			int lifeCount = 0;
			for (int i=0; i<net.players.Count; i++) {
				if (net.players[i].local) lifeCount = net.players[i].lives;
			}
			
			//Debug.Log(lifeCount);
			for (int i=0; i<lifeCount; i++) {
				GUI.DrawTexture(new Rect(Screen.width-60, i*30, 64, 64), Pics.lifeIcon);
			}
		}
		
		// pickup stuff
		Color gcol = GUI.color;
		if (locEnt.offeredPickup != "" && !net.autoPickup) {
			GUI.color = Color.black;
			string s = "Press '" + InputUser.GetKeyLabel(UserAction.GrabItem) + "' to grab " + locEnt.offeredPickup.ToUpper();
			GUI.Label(new Rect(midX-51, midY+100, 200, 60), s);
			GUI.Label(new Rect(midX-49, midY+100, 200, 60), s);
			GUI.Label(new Rect(midX-50, midY+101, 200, 60), s);
			GUI.Label(new Rect(midX-50, midY+99, 200, 60), s);
			GUI.color = gcol;
			GUI.Label(new Rect(midX-50, midY+100, 200, 60), s);
		}
		
		if (locEnt.Spectating) {
			string s = "Spectating: " + net.players[locEnt.Spectatee].name + "\n\nYou will be able to play once this round is over.";
			GUI.color = Color.black;
			GUI.Label(new Rect(4, 5, 300, 60), s);
			GUI.Label(new Rect(6, 5, 300, 60), s);
			GUI.Label(new Rect(5, 4, 300, 60), s);
			GUI.Label(new Rect(5, 6, 300, 60), s);
			
			GUI.color = gcol;
			GUI.Label(new Rect(5, 5, 300, 60), s);
		}
		
		// weapon
		GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
		if (gunB >= 0) 
			GUI.DrawTexture(new Rect(Screen.width-80,Screen.height-95,64,64), arse.Guns[(int)gunB].Pic);
		
		GUI.color = gcol;
		if (gunA >= 0) 
			GUI.DrawTexture(new Rect(Screen.width-110,Screen.height-70,64,64), arse.Guns[(int)gunA].Pic);
		
		if (gunA >= 0) {
			GUI.color = Color.black;
		
			GUI.Label(new Rect(Screen.width-99, Screen.height-20, 100, 30), arse.Guns[(int)gunA].Name);
			GUI.Label(new Rect(Screen.width-101, Screen.height-20, 100, 30), arse.Guns[(int)gunA].Name);
			GUI.Label(new Rect(Screen.width-100, Screen.height-21, 100, 30), arse.Guns[(int)gunA].Name);
			GUI.Label(new Rect(Screen.width-100, Screen.height-19, 100, 30), arse.Guns[(int)gunA].Name);
			
			GUI.color = gcol;
			GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 30), arse.Guns[(int)gunA].Name);
		}
		
		// weapon cooldown
		if (gunA >= Item.Pistol) {
			GUI.DrawTexture(new Rect(Screen.width-103, Screen.height-27, 56, 8), Pics.blackTex);
			float coolDownPercent = 50f;
			if (arse.Guns[(int)gunA].Delay>0f) {
				coolDownPercent = (gunACooldown / arse.Guns[(int)gunA].Delay) * 50f;
				coolDownPercent = 50f-coolDownPercent;
			}
			GUI.DrawTexture(new Rect(Screen.width-100, Screen.height-24, Mathf.FloorToInt(coolDownPercent), 2), Pics.backTex);
		}
		
		GUI.color = gcol;
		
		// time remaining
		if (!net.gameOver) {
			if (net.CurrMatch.Duration > 0f) {
				// show time left
				string s = TimeStringFromSecs(net.gameTimeLeft);
				GUI.color = Color.black;
				GUI.Label(new Rect(midX-11, 5, 200, 30), s);
				GUI.Label(new Rect(midX-9, 5, 200, 30), s);
				GUI.Label(new Rect(midX-10, 4, 200, 30), s);
				GUI.Label(new Rect(midX-10, 6, 200, 30), s);
				
				GUI.color = Color.white;
				GUI.Label(new Rect(midX-10, 5, 200, 30), s);
			}
		}
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
			if (minutes < 10) 
				timeString += "0";
		}
		
		timeString += minutes.ToString() + ":";
		
		if (seconds < 10) 
			timeString += "0";
		timeString += seconds.ToString();
		
		return timeString;
	}
}