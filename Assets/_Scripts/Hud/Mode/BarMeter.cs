﻿using UnityEngine;
using System.Collections;

public class BarMeter {
	float next; // next visibility toggle
	bool visible = true; // visibility status of blink



	public void SetBarColor(float f) { // f should be 0f - 1f
		if (f < 0.5f) { // anything above halfway doesn't blink
			if (next - Time.time >= f) // should handle drastic changes better
				next = Time.time + f;

			if (Time.time >= next) {
				visible = !visible;
				next = Time.time + ((visible) ? f : 0.075f);
			}
			
			if (!visible)
				if (f < 0.001f) // make sure we're not dead with an all black bar & icon
					GUI.color = Color.red;
				else
					GUI.color = Color.black;
			else{
				f -= 0.1f; // fudge factor to make it redder quicker.  otherwise  you'd almost never see pure red
				if (f < 0)
					f = 0;
				GUI.color = Color.Lerp(Color.red, Color.yellow, f*2);
			}
		}else{
			GUI.color = Color.Lerp(Color.yellow, Color.green, (f-0.5f)*2);
		}
	}
}
