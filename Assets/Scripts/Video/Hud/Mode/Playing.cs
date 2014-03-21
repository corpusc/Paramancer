using UnityEngine;
using System.Collections;

public class Playing {
	public bool viewingScores = true;

	Scoreboard scores = new Scoreboard();
	BarMeter health = new BarMeter();
	BarMeter energy = new BarMeter();
	BarMeter coolDown = new BarMeter();
	Texture sprint = Pics.Sprint;
	//Texture sprint = Pics.GetFirstWith("Sprint");


	Crosshair status = Crosshair.Normal;
	int crosshairRadius = 16;
	Color prevCrossHair;
	float prevTick;
	public void Draw(CcNet net, Arsenal arse, int midX, int midY, float lvs, Hud hud) {
		var locEnt = net.localPlayer.Entity;
//		if (locEnt == null)
//			return;
		
		var gunACooldown = arse.Guns[(int)locEnt.GunInHand].Cooldown;
		Item gunA = locEnt.GunInHand;
		Item gunB = locEnt.GunOnBack;
		
		// bars/meters
		// the larger bars centered along bottom of screen
		int barW = Screen.width/3; // width of entire possible meter space
		int barHW = barW/2; // half width
		int bm = 2; // black border margin
		int hY = Screen.height-32; // health y pos
		GUI.DrawTexture(new Rect(midX-barHW-bm, hY-bm*4, bm, 48), Pics.Black); // edge/extent indicator line
		GUI.DrawTexture(new Rect(midX+barHW,    hY-bm*4, bm, 48), Pics.Black); // edge/extent indicator line

		// health bar
		int hH = 11; // health height
		int healthW = (int)((float)barW * (1f - net.localPlayer.health/100f));
		health.SetBarColor(net.localPlayer.health/100f);
		GUI.DrawTexture(new Rect(midX-healthW/2-bm, hY-bm, healthW+bm*2, hH+bm*2), Pics.Black); // background/outline
		GUI.DrawTexture(new Rect(midX-healthW/2, hY, healthW, hH), Pics.White);
		GUI.DrawTexture(new Rect(midX-8, hY-4, 16, 16), Pics.Health);

		// energy bar
		int eH = 7; // energy height
		int eY = Screen.height-13; // energy y pos
		int energyW = (int)((float)barW * (1f - locEnt.EnergyLeft));
		energy.SetBarColor(locEnt.EnergyLeft);
		GUI.DrawTexture(new Rect(midX-energyW/2-bm, eY-bm, energyW+bm*2, eH+bm*2), Pics.Black); // background/outline
		GUI.DrawTexture(new Rect(midX-energyW/2, eY, energyW, eH), Pics.White);
		S.GUIDrawOutlinedTexture(new Rect(midX-8, eY-4, 16, 16), sprint);

		GUI.color = Color.white;
		
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

		// lives
		if (net.CurrMatch.playerLives > 0) {
			int lifeCount = 0;
			for (int i=0; i<net.players.Count; i++) {
				if (net.players[i].local) 
					lifeCount = net.players[i].lives;
			}
			
			//Debug.Log(lifeCount);
			for (int i=0; i<lifeCount; i++) {
				GUI.DrawTexture(new Rect(Screen.width-60, i*30, 64, 64), Pics.Health);
			}
		}
		
		// spectate maybe 
		Color gcol = GUI.color;
		if (locEnt.Spectating) {
			string s = "Spectating: " + net.players[locEnt.Spectatee].name + "\n\nYou will be able to play once this round is over.";
			S.GUIOutlinedLabel(new Rect(5, 5, 300, 60), s);
		}
		
		// weapon cooldown (atm, only used for coloring equipped item) 
		if (gunA >= Item.Pistol) {
			float coolDownPercent = 50f; // more like: 0f to 50f
			if (arse.Guns[(int)gunA].Delay > 0f) {
				coolDownPercent = (gunACooldown / arse.Guns[(int)gunA].Delay) * 50f;
				coolDownPercent = 50f-coolDownPercent;
			}
			
			coolDown.SetBarColor(coolDownPercent/50f);
			// width of old cooldown bar meter was: Mathf.FloorToInt(coolDownPercent)
		}

		// 2 types of crosshairs
		var old = GUI.matrix; // we need to store this, cuz we only want to spin the crosshairs
		GUIUtility.RotateAroundPivot(-Camera.main.transform.rotation.eulerAngles.y, new Vector2(midX, midY));

		if (gunA != Item.GravGun) {
		GUI.DrawTexture(new Rect(
			midX-crosshairRadius, 
			midY-crosshairRadius, 
			crosshairRadius*2, 
			crosshairRadius*2), Pics.crossHair);
		}

		if (gunA == Item.Swapper) {
			int swapperFrame = Mathf.FloorToInt((Time.time * 15f) % Pics.swapperCrosshair.Length);
			if (!locEnt.swapperLocked) 
				swapperFrame = 0;
			
			GUI.DrawTexture(new Rect(
				locEnt.swapperCrossX-32, 
				(Screen.height-locEnt.swapperCrossY)-32, 64, 64), 
				Pics.swapperCrosshair[swapperFrame]);
		}

		// setup for end of cooldown crosshair animation effect
		if (Color.green == GUI.color && // if cooldown just ended
		    Color.green != prevCrossHair) 
		{
			status = Crosshair.Shrinking;
		}

		while (Time.time - prevTick > 0.008f) {
			prevTick += 0.008f;

			switch (status) {
				case Crosshair.Shrinking:
					crosshairRadius--;
					if (crosshairRadius < 8)
						status = Crosshair.Growing;
					break;

				case Crosshair.Growing:
					crosshairRadius++;
					if (crosshairRadius >= 16)
						status = Crosshair.Normal;
					break;
			}
		}

		prevCrossHair = GUI.color;
		GUI.matrix = old; // done drawing spinny stuff
		
		// draw carried item icons 
		int scaleUp = 2;
		int maxW = arse.WidestIcon * scaleUp;
		int maxH = arse.TallestIcon * scaleUp;

		Rect r = new Rect(Screen.width-maxW, Screen.height-maxH, maxW, maxH);
		for (int i = 0; i < arse.Guns.Length; i++) {
			if (!arse.Guns[i].Carrying)
				continue;
			
			var g = arse.Guns[i];
			GUI.color = new Color(1f, 1f, 1f, 0.2f); // trans white
			if /*'*/ ((Item)i == gunA) {
				GUI.color = prevCrossHair;
				GUI.DrawTexture(r, g.Pic);
				float w = hud.GetWidthLabel(g.Name) + 5;
				var forName = r;
				forName.x = Screen.width - w;
				forName.width = w;
				S.GUIOutlinedLabel(forName, g.Name); // name
			}else if ((Item)i == gunB) {
				GUI.DrawTexture(r, g.Pic);
				S.GUIOutlinedLabel(r, "on back"); // name
			}else{
				GUI.DrawTexture(r, g.Pic);
			}
			
			r.y -= maxH;
		}

		GUI.color = gcol;

		// team icons
		Color gcolB = GUI.color;
		if (net.CurrMatch.teamBased && net.localPlayer.team != 0) {
			if /*``*/ (net.localPlayer.team == 1) {
				GUI.DrawTexture(new Rect(Screen.width-68, 4, 64, 64), Pics.TeamRedFlag);
			} else if (net.localPlayer.team == 2) {
				GUI.DrawTexture(new Rect(Screen.width-68, 4, 64, 64), Pics.TeamBlueFlag);
			}
		}
		
		// scoreboard
		if (viewingScores || net.gameOver) {
			scores.Draw(net, hud, lvs);
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