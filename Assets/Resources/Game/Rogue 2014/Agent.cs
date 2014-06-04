using System;
using UnityEngine;



static public class Agent { // represents the local player in a Rogue2014 
	static public Vector2 Pos;

	static public Coord PosOld = new Coord();
	static public room OldRoom;
	static public bool Terse = false; /* True if we should be short */
	static public THING cur_armor;
	static public THING CurrWeapon;
	static public THING[] cur_ring = new THING[2];			/* Which rings are being worn */
	static public int hungry_state = 0;			/* How hungry is he */
	static public int LevelOfMap = 1;				/* What level she is on */
	static public int dyn___max_level;				/* Deepest player has gone */
	static public int no_food = 0;			/* Number of levels without food */
	static public int food_left;				/* Amount of food in hero's stomach */
	static public int NumTurnsAsleep = 0;
	static public int no_move = 0;			/* Number of turns held in place */
	static public int purse = 0;				/* How much gold he has */             //this is already in a THING....FIXME?
	static public int Quiet = 0;				/* Number of Quiet turns */
	static public int NumTimesFlytrapHasHit = 0;				/* Number of time flytrap has hit */
	static public THING Plyr = new THING();
	static public stats max_stats = new stats(16, 0, 1, 10, 12, "1x4", 12);
	static public DateTime LeveledUp = DateTime.Now.AddHours(-1);
	static public bool running = false;			/* True if player is running */
	static public bool seenstairs;			/* Have seen the stairs (for lsd) */
	static public bool amulet = false;			/* He found the amulet */

	static public int[] ExperienceGoals = 
	{
		10, 20, 40, 80, 160, 320, 640, 1300, 2600, 5200, 13000, 26000, 50000,
		100000, 200000, 400000, 800000, 2000000, 4000000, 8000000,    0
	};
	
	static public void LevelUpPlayerMaybe(Vector2 v)
	{
		int goal = Agent.ExperienceGoals[Agent.Plyr.Stats.Level - 1];
		if (Agent.Plyr.Stats.Xp >= goal)
		{
			Agent.Plyr.Stats.Xp -= goal; //keeping leftover xp
			
			int ol = Agent.Plyr.Stats.Level; /* old XpLevel */ Agent.Plyr.Stats.Level++;
			int nl = Agent.Plyr.Stats.Level; /* new XpLevel */
			
			Agent.LeveledUp = DateTime.Now;
			//S.Rend.Particles.Add(new renderer.Particle("Level " + Agent.Plyr.Stats.Level, v, Vector2.One * 0.75f, Vector2.UnitY/10, DateTime.Now.AddSeconds(3), S.Yellow) );
			
			if (nl > ol)
			{
				int add = R14.RollDice(nl - ol, 10);
				Agent.Plyr.Stats.HpMax += add;
				Agent.Plyr.Stats.Hp += add;
				S.Log("Welcome to level " + nl, Color.yellow);
				S.Log("Health +" + add, Color.green);
				//S.Audio.Play("Powerup2", 4);
			}
		}
	}
}