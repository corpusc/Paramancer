using UnityEngine;
using System.Collections;

public class UserPlaying {
	public bool ShowingScores = true;

	// private 
	Scoreboard scores = new Scoreboard();
	// screenshot 
	float whenScreenshotShouldBeFinished;
	// meter bars 
	BarMeter health = new BarMeter();
	BarMeter energy = new BarMeter();
	BarMeter coolDown = new BarMeter();
	// crosshair 
	float prevTick;
	Color prevCrossHair;
	int crosshairRadius = 16;
	Crosshair status = Crosshair.Normal;



	public float Draw(CcNet net, Arsenal arse, int midX, int midY, float lvs, Hud hud) {
		// screenshot (might prevent Draw()ing) 
		if (hud.Invisible) {
			if (Time.time > whenScreenshotShouldBeFinished)
				hud.Invisible = false;
			return 0f;
		}else{ // visible 
			if (CcInput.Started(UserAction.TakePicture)) {
				var fn = "Paramancer " + (int)Random.Range(0, 99999) + ".png"; // file name 
			    Application.CaptureScreenshot(fn);
				hud.Log.AddEntry("+", "Took picture (" + fn + ")", S.ColToVec(Color.grey));
				hud.Invisible = true;
				whenScreenshotShouldBeFinished = Time.time + 1f;
				return 0f;
			}
		}

		var locEnt = net.LocEnt.Visuals;

		Gun gunHand = locEnt.GunInHand;
		Gun gunBack = locEnt.GunOnBack;
		
		// bars/meters 
		// the larger bars centered along bottom of screen 
		int barW = Screen.width/3; // width of entire possible meter space 
		int barHW = barW/2; // half width 
		int bm = 2; // black border margin 
		int hY = Screen.height-32; // health y pos 
		GUI.DrawTexture(new Rect(midX-barHW-bm, hY-bm*4, bm, 48), Pics.Black); // edge/extent indicator line 
		GUI.DrawTexture(new Rect(midX+barHW,    hY-bm*4, bm, 48), Pics.Black); // edge/extent indicator line 

		// damage bar 
		int hH = 11; // health height 
		int healthW = (int)((float)barW * (1f - net.LocEnt.Health/100f));
		health.SetBarColor(net.LocEnt.Health/100f);
		GUI.DrawTexture(new Rect(midX-healthW/2-bm, hY-bm, healthW+bm*2, hH+bm*2), Pics.Black); // background/outline 
		GUI.DrawTexture(new Rect(midX-healthW/2, hY, healthW, hH), Pics.White);
		S.DrawOutlinedTexture(new Rect(midX-8, hY-4, 16, 16), Pics.Health);

		// exhaustion bar 
		int eH = 7; // energy height 
		int eY = Screen.height-13; // energy y pos 
		int energyW = (int)((float)barW * (1f - locEnt.SprintEnergy));
		energy.SetBarColor(locEnt.SprintEnergy, false);
		GUI.DrawTexture(new Rect(midX-energyW/2-bm, eY-bm, energyW+bm*2, eH+bm*2), Pics.Black); // background/outline 
		GUI.DrawTexture(new Rect(midX-energyW/2, eY, energyW, eH), Pics.White);
		S.DrawOutlinedTexture(new Rect(midX-8, eY-4, 16, 16), Pics.Sprint);

		GUI.color = Color.white;
		
		// time remaining 
		if (!net.gameOver) {
			if (net.CurrMatch.Duration > 0f) {
				// show time left 
				string s = timeFromSecs(net.MatchTimeLeft);
				var w = hud.GetWidthLabel(s);
				GUI.color = Color.white;
				S.OutlinedLabel(new Rect(
					midX-w/2, 0, 
					w, hud.VSpanLabel), s);
			}
		}

		// show frames per second 
		int currFPS = (int)(1f / Time.deltaTime * Time.timeScale); // current 
		int avgFPS = (int)(Time.frameCount / Time.time); // average 
		var w1 = hud.GetWidthLabel("FPS: 888");
		var w2 = hud.GetWidthLabel("(average: 888)");
		var fps = "FPS: " + currFPS.ToString();
		var avg = "(average: " + avgFPS.ToString() + ")";
		GUI.color = Color.white;
		S.OutlinedLabel(new Rect(
			midX-w1/2, hud.VSpanLabel, 
			w1, hud.VSpanLabel), fps);
		S.OutlinedLabel(new Rect(
			midX-w2/2, hud.VSpanLabel*2, 
			w2, hud.VSpanLabel), avg);

		// lives 
		if (net.CurrMatch.playerLives > 0) {
			int lifeCount = 0;
			for (int i=0; i<net.Entities.Count; i++) {
				if (net.Entities[i].local) 
					lifeCount = net.Entities[i].lives;
			}
			
			//Debug.Log(lifeCount);
			for (int i=0; i<lifeCount; i++) {
				GUI.DrawTexture(new Rect(Screen.width-60, i*30, 64, 64), Pics.Health);
			}
		}
		
		// spectate maybe 
		Color gcol = GUI.color;
		if (locEnt.Spectating) {
			string s = "Spectating: " + net.Entities[locEnt.Spectatee].name + "\n\nYou will be able to play once this round is over.";
			S.OutlinedLabel(new Rect(5, 5, 300, 60), s);
		}
		
		// weapon cooldown (atm, only used for coloring equipped item) 
		if (gunHand >= Gun.Pistol) {
			float coolDownPercent = 50f; // cool down percent.  // more like: 0f -> 50f? 
			if (arse.Guns[(int)gunHand].Delay > 0f) {
				var cd = arse.Guns[(int)locEnt.GunInHand].Cooldown; // gun in hand cooldown 
				coolDownPercent = (cd / arse.Guns[(int)gunHand].Delay) * 50f;
				coolDownPercent = 50f-coolDownPercent;
			}
			
			coolDown.SetBarColor(coolDownPercent/50f);
			// width of old cooldown bar meter was: Mathf.FloorToInt(coolDownPercent)
		}

		// 2 types of crosshairs
		var old = GUI.matrix; // we need to store this, cuz we only want to spin the crosshairs
		GUIUtility.RotateAroundPivot(-Camera.main.transform.rotation.eulerAngles.y, new Vector2(midX, midY));

		if (gunHand != Gun.Gravulator) {
		GUI.DrawTexture(new Rect(
			midX-crosshairRadius, 
			midY-crosshairRadius, 
			crosshairRadius*2, 
			crosshairRadius*2), Pics.CrossHair);
		}

		if (gunHand == Gun.Swapper) {
			int swapperFrame = Mathf.FloorToInt((Time.time * 15f) % Pics.CrosshairSwapper.Length);
			if (!locEnt.swapperLocked) 
				swapperFrame = 0;
			
			GUI.DrawTexture(new Rect(
				locEnt.swapperCrossX-32, 
				(Screen.height-locEnt.swapperCrossY)-32, 64, 64), 
				Pics.CrosshairSwapper[swapperFrame]);
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
					if (crosshairRadius < 4)
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
			if /*'*/ ((Gun)i == gunHand) {
				GUI.color = prevCrossHair;
				GUI.DrawTexture(r, g.Pic);
				float w = hud.GetWidthLabel(g.Name) + 5;
				var forName = r;
				forName.x = Screen.width - w;
				forName.width = w;
				S.OutlinedLabel(forName, g.Name); // name
			}else if ((Gun)i == gunBack) {
				GUI.DrawTexture(r, g.Pic);
				S.OutlinedLabel(r, "on back"); // name
			}else{
				GUI.DrawTexture(r, g.Pic);
			}
			
			r.y -= maxH;
		}

		GUI.color = gcol;

		// team icons
		Color gcolB = GUI.color;
		if (net.CurrMatch.teamBased && net.LocEnt.team != 0) {
			if /*``*/ (net.LocEnt.team == 1) {
				GUI.DrawTexture(new Rect(Screen.width-68, 4, 64, 64), Pics.TeamRedFlag);
			} else if (net.LocEnt.team == 2) {
				GUI.DrawTexture(new Rect(Screen.width-68, 4, 64, 64), Pics.TeamBlueFlag);
			}
		}
		
		// scoreboard
		if (ShowingScores || net.gameOver) {
			return scores.Draw(net, hud, lvs);
		}else{
			return 0f;
		}
	}
	


	string timeFromSecs(float totalSecs) {
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