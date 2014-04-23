using UnityEngine;
using System.Collections;

public class RoguelikeMap : ScriptableObject {

	public RoguelikeLevel[] lev;
	public int n_levs = 5; // the amount of levels a dungeon has
	public int size_x = 1024;
	public int size_y = 1024;

	public int FormsStep = 2;
	public float OverrideStep = -0.05f;
	public int MinWidthStep = -1;
	public int MaxWidthStep = 0;
	public int MaxAreaStep = -2 * S.K;

	public int FormsInitial = 10;
	public float OverrideInitial = 0.3f;
	public int MinWidthInitial = 16;
	public int MaxWidthInitial = 500;
	public int MaxAreaInitial = 20 * S.K;

	public void Init() {
		lev = new RoguelikeLevel[n_levs];
		Vec2i Size;
		Size.x = size_x;
		Size.y = size_y;

		int cForms = FormsInitial; // current
		float cOverride = OverrideInitial;
		int cMinWidth = MinWidthInitial;
		int cMaxWidth = MaxWidthInitial;
		int cMaxArea = MaxAreaInitial;

		for (int i = 0; i < n_levs; i++) {
			lev[i].Forms = cForms;
			lev[i].MaxOverride = cOverride;
			lev[i].MinFormWidth = cMinWidth;
			lev[i].MaxFormWidth = cMaxWidth;
			lev[i].MaxArea = cMaxArea;

			cForms += FormsStep;
			cOverride += OverrideStep;
			cMinWidth += MinWidthStep;
			cMaxWidth += MaxWidthStep;
			cMaxArea += MaxAreaStep;

			lev[i].MapSize = Size;
			lev[i].Init();
		}
	}
}
