using UnityEngine;
using System.Collections;

public class RoguelikeDungeon {

	public RoguelikeLevel[] lev;
	public int n_levs = 5; //the amount of levels a dungeon has

	public void Init() {
		lev = new RoguelikeLevel[n_levs];

		int cForms = 10; //current
		float cOverride = 0.3f;
		for (int i = 0; i < n_levs; i++) {
			lev[i].Forms = cForms;
		}
	}
}
