using System;
using System.Collections.Generic;
using UnityEngine;



// NOTICE!!!!!!!!!!!!!!!!!!!!!! 
// we will NOT try to replicate all Rogue features anytime soon! 
// we'll attempt only about %15-%20 of all the item/abilities for the 1st commercial release 

// ALL FILES IN Rogue2014 (or R14 for short) folder...
// have a different formatting style that Brandon/HaltingState dictated to me when 
// i was working for him.  
// but most of the current style comes from it being my refactoring of RogueSharp, and i was
// FAR from finished.  it WILL be eventually changed to my normal formatting style....
// ...at least everywhere that it gets touched/changed 


//i believe i've stripped the only needed data from these RogueSharp files:
//armor
//command
//extern
//daemon
//init
//io
//list
//main
//misc
//move
//new_level
//monsters
//potions
//pack???????????/
//RandomNumberGenerator
//rings
//rip
//rogue
//rooms
//scrolls
//sticks
//things
//weapons


//optimizeme 
//      * replace backpack list with modern list of PhysicsObjects.  they exist as such in order to detect picking them up, and thats
//                where tile # is stored at init 


/*
 HERE ARE ALL THE *RELEVANT* COMMANDS YOU CAN GIVE IN ROGUE
 * 
 * * // all these combine in "use"?   activate?
                                   new h_list('q', "	quaff potion", true),
                                   new h_list('r', "	read scroll", true),
                                   new h_list('e', "	eat food", true),
                                   new h_list('w', "	wield a weapon", true),
                                   new h_list('W', "	wear armor", true),
                                   new h_list('P', "	put-on(wear) ring", true),

 *                                 // taking off would only be useful so that you didn't have to carry a spare ring for when you can remove a cursed item....
 *                                 // ....or coming from an another angle.... putting something ELSE on in order to remove an item should invalidate need for 
 *                                 // takeoff/remove/unwear binding?  think of relating to dropping/throwing items?
                                   new h_list('T', "	take armor off", true),
                                   new h_list('R', "	remove ring", true),
 * 
                                   new h_list('d', "	drop object", true),          //like takeoff/remove?  //is there a throw command and how similar if so?
 * 
 * 
 * 
 *                                 new h_list('?', "	prints help", true),
                                   new h_list('/', "	identify object", true),
                                   new h_list('t', "<dir>	throw something", true),               // sticks can be either zapped/shot or thrown
                                   new h_list('z', "<dir>	zap a wand in a direction", true),     // others, just thrown?  or is that fire/shoot?
 *                                                                                                 // i'm just confusing using with dropping?
                                   new h_list('^', "<dir>	identify trap type", true),
                                   new h_list('s', "	search for trap/secret door", true),
                                   new h_list('>', "	go down a staircase", true),
                                   new h_list('<', "	go up a staircase", true),
                                   new h_list('i', "	inventory", true),
                                   new h_list('c', "	call object", true),
                                   new h_list('D', "	recall what's been discovered", true),
                                   new h_list('S', "	save game", true),
*/

// NONrogue Todo
//      * remove bullets after 1st hit (unless we have a reflecting "rune")
//      * melee "swings" and damage (maybe 2D DBTSword, where the harder you swing the more damage it does)
//      * shield system?
// skeletal animation for some tentacle trap?

static partial class R14 // Rogue 2014 or Rogue14 
{
	public class delayed_action
	{
		public int d_type;
		public Action<int> d_func;
		public int d_arg;
		public int d_time;
	}
	
	static RandomNumberGenerator rng = new RandomNumberGenerator();
	public static string fruit = "slime-mold";
	
	public static bool see_floor = true; /* Show the lamp illuminated floor */
	
	public static string prbuf;			/* buffer for sprintfs */
	public static char take;				/* Thing she is taking */
	
	public static int inpack = 0;           // # things in pack                      ^^^^^^^^^ what's the diff tween this and numItems?
	public static int inv_type = 0;			/* Chat of inventory to use */
	
	// max counts 
	public const int MAXDAEMONS = 20;
	public const int NUMLINES = 24;
	public const int NUMCOLS = 80;
	public const int STATLINE = (NUMLINES - 1);
	public const int BORE_LEVEL = 50;
	
	// constants
	static public int BEARTIME { get { return spread(3); } }
	static public int SLEEPTIME { get { return spread(5); } }
	static public int WANDERTIME { get { return spread(70); } }
	static public int BEFORE { get { return spread(1); } }
	static public int AFTER { get { return spread(2); } }
	public const int HEALTIME = 30;
	public const int HUHDURATION = 20;
	public const int SEEDURATION = 850;
	public const int HUNGERTIME = 1300;
	public const int MORETIME = 150;
	public const int STOMACHSIZE = 2000;
	public const int STARVETIME = 850;
	public const int ESCAPE = 27;
	public const int LAMPDIST = 3;
	
	// save versus... 
	public static int VS_POISON = 00;
	public static int VS_PARALYZATION = 00;
	public static int VS_DEATH = 00;
	public static int VS_BREATH = 02;
	public static int VS_MAGIC = 03;
	
	static R14()
	{
		//start_daemon(runners, 0, AFTER);
		//start_daemon(doctor, 0, AFTER);
		//LightFuse(swander, 0, WANDERTIME, AFTER);
		//start_daemon(stomach, 0, AFTER);
	}
	
	public static int rnd(int range)
	{
		return rnd2(range, null);
	}
	
	public static int rnd2(int range, string key)
	{
		return rng.Next(key, range);
	}
	
	public static int RollDice(int number, int sides)
	{
		int dtotal = 0;
		
		while (number-- > 0)
			dtotal += rnd(sides) + 1;
		return dtotal;
	}
	
	//spread around a given number (+/- 20%)
	static int spread(int nm)
	{
		return nm - nm / 20 + rnd(nm / 10);
	}
	
	// Pick a random thing appropriate for this level
	public static char rnd_thing()
	{
		int i;
		char[] thing_list = {
			Char.POTION, Char.SCROLL, Char.RING, Char.STICK, Char.FOOD, Char.WEAPON, Char.ARMOUR, Char.STAIRS, Char.GOLD, Char.AMULET
		};
		
		if (Agent.LevelOfMap >= Dungeon.AMULETLEVEL)
			i = rnd(thing_list.Length);
		else
			i = rnd(thing_list.Length - 1);
		
		return thing_list[i];
	}
	
	public static bool on(THING thing, int flag)
	{
		return (thing.t_flags & flag) != 0;
	}
	
	//depending on whether it the player is tripping
	//public static string choose_str(string ts, string ns)
	public static string choose_str(string ts, string ns)
	{
		return (on(Agent.Plyr, Mob.IsTrippinBalls) ? ts : ns);
	}
	
	//list stuff ****************************************************
	//detach from linked list
	public static THING next(THING ptr)
	{
		return ptr.l_next;
	}
	
	public static THING prev(THING ptr)
	{
		return ptr.l_prev;
	}
	
	static void detach(ref THING list, THING item)
	{
		if (list == item)
			list = next(item);
		
		if (prev(item) != null)
			item.l_prev.l_next = next(item);
		
		if (next(item) != null)
			item.l_next.l_prev = prev(item);
		
		item.l_next = null;
		item.l_prev = null;
	}
	
	// attach to the head of a list 
	public static void attach(ref THING list, THING item)
	{
		if (list != null)
		{
			item.l_next = list;
			list.l_prev = item;
			item.l_prev = null;
		}
		else
		{
			item.l_next = null;
			item.l_prev = null;
		}
		list = item;
	}
	
	public static Coord hero
	{
		get { return Agent.Plyr.t_pos; }
		set { Agent.Plyr.t_pos = value; }
	}
	
	public static int GOLDCALC()
	{
		return rnd(50 + 10 * Agent.LevelOfMap) + 2;
	}
	
	public static void eat() {
		//THING obj;
		
		//if ((obj = get_item("eat", FOOD)) == null)
		//    return;
		//if (obj.ItemType != FOOD)
		//{
		//    if (!Agent.Terse)
		//        AddToLog("ugh, you would get ill if you ate that");
		//    else
		//        AddToLog("that's Inedible!");
		//    return;
		//}
		//if (food_left < 0)
		//    food_left = 0;
		//if ((food_left += HUNGERTIME - 200 + rnd(400)) > STOMACHSIZE)
		//    food_left = STOMACHSIZE;
		Agent.hungry_state = 0;
		//if (obj == CurrWeapon)
		//    CurrWeapon = null;
		//if (obj.Which == 1)
		//    AddToLog("my, that was a yummy %s", fruit);
		//else
		//    if (rnd(100) > 70)
		//    {
		//        Agent.Plyr.Stats.Xp++;
		//        AddToLog("{0}, this food tastes awful", choose_str("bummer", "yuk"));
		//        LevelUpPlayerMaybe();
		//    }
		//    else
		//        AddToLog("{0}, that tasted good", choose_str("oh, wow", "yum"));
		//leave_pack(obj, false, false);
	}
	
	public static int AddToLog(string fmt, params object[] args) 
	{
		S.Log(string.Format(fmt, args), Color.white);
		return 0;
	}
	
	// Add things to the current message
	static string msgbuf = "";
	public static void addmsg(string fmt, params object[] args) 
	{ //the inner func this USED to have..... didn't have 'params' keyword
		string buf = string.Format(fmt, args);
		msgbuf += buf;
	}
	
	
	
	
	
	
	//// A quick glance all around the player
	public static void glanceAroundHero___look(bool wakeup)
	{
		int x, y;
		int ch;
		THING tp;
		PLACE pp;
		int ey, ex;
		char pch;
		int pfl;
		int fp;
		int sy, sx;
		
		if (!Dungeon.TheseAreTheSame(Agent.PosOld, hero))
		{
			//erase_lamp(oldpos, oldrp);    DON'T THINK WE EVER WANNA USE THE OLD LIGHTING PARADIGM IN ANY WAY
			Agent.PosOld = hero;
			Agent.OldRoom = Agent.Plyr.CurrRoom;;
		}
		
		ey = hero.y + 1;
		ex = hero.x + 1;
		sx = hero.x - 1;
		sy = hero.y - 1;
		pp = Dungeon.PlaceAt(hero.y, hero.x);
		pch = pp.p_ch;
		pfl = pp.p_flags;
		
		for (y = sy; y <= ey; y++)
			if (y > 0 && y < NUMLINES - 1) for (x = sx; x <= ex; x++)
		{
			if (x < 0 || x >= NUMCOLS)
				continue;
			if (!on(Agent.Plyr, Mob.ISBLIND))
			{
				if (y == hero.y && x == hero.x)
					continue;
			}
			
			pp = Dungeon.PlaceAt(y, x);
			ch = pp.p_ch;
			if (ch == ' ')		/* nothing need be done with a ' ' */
				continue;
			
			fp = pp.p_flags;
			if (pch != Char.DOOR && ch != Char.DOOR)
				if ((pfl & Dungeon.F_PASS) != (fp & Dungeon.F_PASS))
					continue;
			
			if ((((fp & Dungeon.F_PASS) != 0) || ch == Char.DOOR) && 
			    (((pfl & Dungeon.F_PASS) != 0) || pch == Char.DOOR))
			{
				if (hero.x != x && hero.y != y &&
				    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(y, hero.x)) && 
				    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(hero.y, x)))
					continue;
			}
			
			if ((tp = pp.p_monst) == null)
				;//ch = Mob.trip_ch(y, x, ch);        don't think we wanna use random (but distinctly clear) chars as a hallucinating effect,
			//                                   but rather have some effects applied over the whole screen to melt/blur everything as a whole
			else {
				if (on(Agent.Plyr, Mob.CanSeeInvisible) && on(tp, Mob.ISINVIS)) {
					continue;
				} else {
					if (wakeup)
						R14.wake_monster(y, x);
					if (Mob.see_monst(tp)) {
						if (on(Agent.Plyr, Mob.IsTrippinBalls))
							ch = rnd(26) + 'A';
						else
							ch = tp.t_disguise;
					}
				}
			}
			
			if (on(Agent.Plyr, Mob.ISBLIND) && (y != hero.y || x != hero.x))
				continue;
			
			//old lighting paradigm
			//if (((Agent.Plyr.CurrRoom.Flags & Room.ISDARK) != 0) && !see_floor && ch == Char.FLOOR)
			//ch = ' ';
		}
	}
	
	//// Return the character appropriate for this space, taking into
	//// account whether or not the player is tripping.
	//int trip_ch(int y, int x, int ch)
	//{
	//    if (on(player, IsTrippinBalls) && after)
	//        switch (ch)
	//        {
	//            case FLOOR:
	//            case ' ':
	//            case PASSAGE:
	//            case '-':
	//            case '|':
	//            case DOOR:
	//            case TRAP:
	//                break;
	//            default:
	//                if (y != stairs.y || x != stairs.x || !seenstairs)
	//                    ch = rnd_thing();
	//                break;
	//        }
	//    return ch;
	//}
	
	//// Erase the area shown by a lamp in a dark room.
	//void erase_lamp(coord pos, room rp)
	//{
	//    int y, x, ey, sy, ex;
	
	//    if (!(see_floor && (rp.r_flags & (ISGONE|ISDARK)) == ISDARK
	//            && !on(player,ISBLIND)))
	//        return;
	
	//    ey = pos.y + 1;
	//    ex = pos.x + 1;
	//    sy = pos.y - 1;
	//    for (x = pos.x - 1; x <= ex; x++)
	//        for (y = sy; y <= ey; y++)
	//        {
	//            if (y == hero.y && x == hero.x)
	//                continue;
	//            R12.move(y, x);
	//            if (R12.inch() == FLOOR)
	//                R12.addch(' ');
	//        }
	//}
	
	//keeps track of the highest strength has been, just in case
	static public void ModifyStrength(int amt)
	{
		int comp;
		
		if (amt == 0)
			return;
		int myStr = Agent.Plyr.Stats.Str;
		add_str(ref myStr, amt);
		Agent.Plyr.Stats.Str = myStr;
		comp = Agent.Plyr.Stats.Str;
		if (Ring.ISRING(Ring.LEFT, Ring.R_ADDSTR))
			add_str(ref comp, -Agent.cur_ring[Ring.LEFT].Ac);
		if (Ring.ISRING(Ring.RIGHT, Ring.R_ADDSTR))
			add_str(ref comp, -Agent.cur_ring[Ring.RIGHT].Ac);
		if (comp > Agent.max_stats.Str)
			Agent.max_stats.Str = comp;
	}
	
	// Perform the actual add, checking upper and lower bound limits
	static public void add_str(ref int sp, int amt)
	{
		if ((sp += amt) < 3)
			sp = 3;
		else if (sp > 31)
			sp = 31;
	}
	
	//coord last_delt = new coord();
	
	//init direction coord for use in varios "prefix" commands
	//bool get_dir()
	//{
	//    string prompt;
	//    bool gotit;
	
	//    if (again && last_dir != '\0')
	//    {
	//        delta.y = last_delt.y;
	//        delta.x = last_delt.x;
	//        dir_ch = last_dir;
	//    }
	//    else
	//    {
	//        if (!Agent.Terse)
	//            showAtTopOfScreen___msg(prompt = "which direction? ");
	//        else
	//            prompt = "direction: ";
	//        do
	//        {
	//            gotit = true;
	//            switch (dir_ch = readchar())
	//            {
	//                case 'h': case'H': delta.y =  0; delta.x = -1; break;
	//                case 'j': case'J': delta.y =  1; delta.x =  0; break;
	//                case 'k': case'K': delta.y = -1; delta.x =  0; break;
	//                case 'l': case'L': delta.y =  0; delta.x =  1; break;
	//                case 'y': case'Y': delta.y = -1; delta.x = -1; break;
	//                case 'u': case'U': delta.y = -1; delta.x =  1; break;
	//                case 'b': case'B': delta.y =  1; delta.x = -1; break;
	//                case 'n': case'N': delta.y =  1; delta.x =  1; break;
	//                case (char)ESCAPE: last_dir = '\0'; reset_last(); return false;
	//                default:
	//                    topLineCursorPos___mpos = 0;
	//                    showAtTopOfScreen___msg(prompt);
	//                    gotit = false;
	//                    break;
	//            }
	//        }
	//        while (!gotit);
	//        if (char.IsUpper(dir_ch))
	//            dir_ch = char.ToLower(dir_ch);
	//        last_dir = dir_ch;
	//        last_delt.y = delta.y;
	//        last_delt.x = delta.x;
	//    }
	//    if (on(player, ISHUH) && rnd(5) == 0)
	//    do
	//    {
	//        delta.y = rnd(3) - 1;
	//        delta.x = rnd(3) - 1;
	//    }
	//    while (delta.y == 0 && delta.x == 0);
	//        topLineCursorPos___mpos = 0;
	//    return true;
	//}
	
	//todo       add the gravestone showing at what level and with what gold, and what monster you were slain by.
	//           and highscores
	static public void death(char monst)
	{
		S.Log("You have been killed!");
		//Agent.PlayMode = PlayMode.Dead;
	}
	
	// Code for a winner
	static public void total_winner()
	{
		S.Log("A winnar is you!");
		//Agent.PlayMode = PlayMode.Dead;
	}
}
