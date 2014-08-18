using System;



    class PACT
    {
        public int pa_flags;
        public Action<int> pa_daemon;
        public int pa_time;
        public string pa_high, pa_straight;

        public PACT(int paFlags, Action<int> paDaemon, int paTime, string paHigh, string paStraight)
        {
            pa_flags = paFlags;
            pa_daemon = paDaemon;
            pa_time = paTime;
            pa_high = paHigh;
            pa_straight = paStraight;
        }
    }

    static class Potion
    {
        static PACT[] p_actions;

        // Potion types
        public const int P_CONFUSE = 0;
        public const int P_LSD = 1;
        public const int P_POISON = 2;
        public const int P_STRENGTH = 3;
        public const int P_SEEINVIS = 4;
        public const int P_HEALING = 5;
        public const int P_MFIND = 6;
        public const int P_TFIND = 7;
        public const int P_RAISE = 8;
        public const int P_XHEAL = 9;
        public const int P_HASTE = 10;
        public const int P_RESTORE = 11;
        public const int P_BLIND = 12;
        public const int P_LEVIT = 13;
        public const int MAX = 14; //all consecutive numbers like an enum

        public static ObjectData[] Data = new[]
        {
            new ObjectData("confusion", 7, 5, null, false),
            new ObjectData("hallucination", 8, 5, null, false),
            new ObjectData("poison", 8, 5, null, false),
            new ObjectData("gain strength", 13, 150, null, false),
            new ObjectData("see invisible", 3, 100, null, false),
            new ObjectData("healing", 13, 130, null, false),
            new ObjectData("monster detection", 6, 130, null, false),
            new ObjectData("magic detection", 6, 105, null, false),
            new ObjectData("raise level", 2, 250, null, false),
            new ObjectData("extra healing", 5, 200, null, false),
            new ObjectData("haste self", 5, 190, null, false),
            new ObjectData("restore strength", 13, 130, null, false),
            new ObjectData("blindness", 5, 5, null, false),
            new ObjectData("levitation", 6, 75, null, false),
        };

        public static string[] ColorsUsed = new string[MAX];
        static string[] colorsPossible = new[] {
            "amber",
            "aquamarine",
            "black",
            "blue",
            "brown",
            "chartreuse", //"grey", //replacing this cuz its dull and too close to silver
            "clear",
            "crimson",
            "cyan",
            "ecru",
            "glowing green", //"red",   replacing this cuz its almost exactly the same as crimson
            "gold",
            "green",
            "magenta", //like VIOLET but more red
            "maroon", //"topaz",   replacing cuz......same stuff as amber?nah,morelikelightbrownordarktan pink/lavender? aquamarine? cyan?  this material's colors can vary widely, but they seem to be all colors similar to other things in this list!
            "orange",
            "pink",
            "plaid",
            "purple",
            "silver",
            "tan",
            "tangerine",
            "turquoise",
            "vermilion",//
            "violet", //MAGENTA but more purple, can even be bluish purple?  some violets are mostly that
            "white",
            "yellow",
            //my own replacements?  
            //puce - less saturated, greyish      //mauve - about the same as puce      //rose - more pinky/purpley
            //fuschia?
            //atomic blue, and 
            //grey/blue, maybe
            //baby/powder blue seperate from the cyan'ish colors?
        };
        public static int NCOLORS = colorsPossible.Length;
        static int cNCOLORS = NCOLORS;
        static bool[] used = new bool[NCOLORS];

        static Potion()
        {
            InitColors(); /* Set up colors of potions */
            InitActions();
        }
        
        static public void InitColors()
        {
            S.Log("Potion colors");
            int i, j;

            for (i = 0; i < NCOLORS; i++)
                used[i] = false;

            for (i = 0; i < Potion.MAX; i++)
            {
                do
                    j = R14.rnd(NCOLORS);
                while (used[j]);

                used[j] = true;
                ColorsUsed[i] = colorsPossible[j];
                S.Log(colorsPossible[j]);
            }
        }

        // If he is hallucinating, pick a random color name and return it, else return the given color.
        //public static string pick_color(string col)
        public static string GetColor(string col)
        {
            return (R14.on(Agent.Plyr, Mob.IsTrippinBalls) ? colorsPossible[R14.rnd(NCOLORS)] : col);
        }

        public static void InitActions()
        {
            p_actions = new[]
            {
                new PACT (Mob.ISHUH,	R14.unconfuse,	R14.HUHDURATION,	/* P_CONFUSE */
                    "what a tripy feeling!",
                    "wait, what's going on here. Huh? What? Who?" ),
                new PACT(Mob.IsTrippinBalls,	come_down,	R14.SEEDURATION,	/* P_LSD */
                    "Oh, wow!  Everything seems so cosmic!",
                    "Oh, wow!  Everything seems so cosmic!" ),
                new PACT( 0,		null,	0, null, null ),			/* P_POISON */
                new PACT( 0,		null,	0, null, null ),			/* P_STRENGTH */
                new PACT(Mob.CANSEE,	R14.unsee,	R14.SEEDURATION,		/* P_SEEINVIS */
                    string.Format("this potion tastes like {0} juice", R14.fruit),
                    string.Format("this potion tastes like {0} juice", R14.fruit)),
                new PACT( 0,		null,	0, null, null ),			/* P_HEALING */
                new PACT( 0,		null,	0, null, null ),			/* P_MFIND */
                new PACT( 0,		null,	0, null, null ),			/* P_TFIND  */
                new PACT( 0,		null,	0, null, null ),			/* P_RAISE */
                new PACT( 0,		null,	0, null, null ),			/* P_XHEAL */
                new PACT( 0,		null,	0, null, null ),			/* P_HASTE */
                new PACT( 0,		null,	0, null, null ),			/* P_RESTORE */
                new PACT(Mob.ISBLIND,	R14.sight,	R14.SEEDURATION,		/* P_BLIND */
                    "oh, bummer!  Everything is dark!  Help!",
                    "a cloak of darkness falls around you" ),
                new PACT(Mob.ISLEVIT,	R14.land,	R14.HEALTIME,		/* P_LEVIT */
                    "oh, wow!  You're floating in the air!",
                    "you start to float in the air" )
            };
        }

        // Take the hero down off her acid trip.
        static void come_down(int ignored)
        {
            //THING tp;
            //bool seemonst;

            //if (!on(player, IsTrippinBalls))
            //    return;

            //kill_daemon(visuals);
            //player.t_flags &= ~IsTrippinBalls;

            //if (on(player, ISBLIND))
            //    return;

            ///*
            //    * undo the things
            //    */
            //for (tp = ItemList; tp != null; tp = next(tp))
            //    if (HeroCanSee(tp.ItemPos.y, tp.ItemPos.x))
            //        R12.MvAddCh(tp.ItemPos.y, tp.ItemPos.x, tp.ItemType);

            ///*
            //    * undo the monsters
            //    */
            //seemonst = on(player, CanSeeInvisible);
            //for (tp = MobList; tp != null; tp = next(tp))
            //{
            //    R12.move(tp.t_pos.y, tp.t_pos.x);
            //    if (HeroCanSee(tp.t_pos.y, tp.t_pos.x))
            //        if (!on(tp, ISINVIS) || on(player, Mob.CANSEE))
            //            R12.addch(tp.t_disguise);
            //        else
            //            R12.addch(chat(tp.t_pos.y, tp.t_pos.x));
            //    else if (seemonst)
            //    {
            //        R12.standout();
            //        R12.addch(tp.MonsType);
            //        R12.standend();
            //    }
            //}
            //AddToLog("Everything looks SO boring now.");
        }

        // Quaff a potion from the pack
        public static void Quaff(THING t)
        {
            S.Log("Quaffing " + Item.GetDescription(t) + "        (unfinished method)");

            //THING obj, tp, mp;
            //bool show, trip;

            //obj = get_item("quaff", POTION);
            ///*
            // * Make certain that it is somethings that we want to drink
            // */
            //if (obj == null)
            //    return;
            //if (obj.ItemType != POTION)
            //{
            //    if (!Agent.Terse)
            //        AddToLog("yuk! Why would you want to drink that?");
            //    else
            //        AddToLog("that's undrinkable");
            //    return;
            //}
            //if (obj == CurrWeapon)
            //    CurrWeapon = null;

            ///*
            // * Calculate the effect it has on the poor guy.
            // */
            //trip = on(Agent.Plyr, Mob.IsTrippinBalls);
            //discardit = (bool)(obj.Count == 1);
            //leave_pack(obj, false, false);
            //switch (obj.Which)
            //{
            //    case P_CONFUSE:
            //        do_pot(P_CONFUSE, !trip);
            //        break;
            //    case P_POISON:
            //        pot_info[P_POISON].oi_know = true;
            //        if (ISWEARING(R_SUSTSTR))
            //            AddToLog("you feel momentarily sick");
            //        else
            //        {
            //            ModifyStrength(-(rnd(3) + 1));
            //            AddToLog("you feel very sick now");
            //            come_down(0);
            //        }
            //        break;
            //    case P_HEALING:
            //        pot_info[P_HEALING].oi_know = true;
            //        if ((pstats.Hp += RollDice(pstats.Level, 4)) > max_hp)
            //            pstats.Hp = ++max_hp;
            //        sight(0);
            //        AddToLog("you begin to feel better");
            //        break;
            //    case P_STRENGTH:
            //        pot_info[P_STRENGTH].oi_know = true;
            //        ModifyStrength(1);
            //        AddToLog("you feel stronger, now.  What bulging muscles!");
            //        break;
            //    case P_MFIND:
            //        player.t_flags |= CanSeeInvisible;
            //        LightFuse(turn_see_fuse, 1, HUHDURATION, AFTER);
            //        if (!turn_see(0))
            //            AddToLog("you have a {0} feeling for a moment, then it passes",
            //        choose_str("normal", "strange"));
            //        break;
            //    case P_TFIND:
            //        /*
            //         * Potion of magic detection.  Show the potions and scrolls
            //         */
            //        show = false;
            //        if (ItemList != null)
            //        {
            //            R12.wclear(hw);
            //            for (tp = ItemList; tp != null; tp = next(tp))
            //            {
            //                if (Item.IsMagic(tp))
            //                {
            //                    show = true;
            //                    R12.wmove(hw, tp.ItemPos.y, tp.ItemPos.x);
            //                    R12.waddch(hw, MAGIC);
            //                    pot_info[P_TFIND].oi_know = true;
            //                }
            //            }
            //            for (mp = MobList; mp != null; mp = next(mp))
            //            {
            //                for (tp = mp.Pack; tp != null; tp = next(tp))
            //                {
            //                    if (Item.IsMagic(tp))
            //                    {
            //                        show = true;
            //                        R12.wmove(hw, mp.t_pos.y, mp.t_pos.x);
            //                        R12.waddch(hw, MAGIC);
            //                    }
            //                }
            //            }
            //        }
            //        if (show)
            //        {
            //            pot_info[P_TFIND].oi_know = true;
            //            show_win("You sense the presence of magic on this level.--More--");
            //        }
            //        else
            //            AddToLog("you have a {0} feeling for a moment, then it passes",
            //                choose_str("normal", "strange"));
            //        break;
            //    case P_LSD:
            //        if (!trip)
            //        {
            //            if (on(player, CanSeeInvisible))
            //                turn_see(0);
            //            start_daemon(visuals, 0, BEFORE);
            //            seenstairs = seen_stairs();
            //        }
            //        do_pot(P_LSD, true);
            //        break;
            //    case P_SEEINVIS:
            //        show = on(player, CANSEE);
            //        do_pot(P_SEEINVIS, false);
            //        if (!show)
            //            invis_on();
            //        sight(0);
            //        break;
            //    case P_RAISE:
            //        pot_info[P_RAISE].oi_know = true;
            //        AddToLog("you suddenly feel much more skillful");
            //        raise_level();
            //        break;
            //    case P_XHEAL:
            //        pot_info[P_XHEAL].oi_know = true;
            //        if ((pstats.Hp += RollDice(pstats.Level, 8)) > max_hp)
            //        {
            //            if (pstats.Hp > max_hp + pstats.Level + 1)
            //                ++max_hp;
            //            pstats.Hp = ++max_hp;
            //        }
            //        sight(0);
            //        come_down(0);
            //        AddToLog("you begin to feel much better");
            //        break;
            //    case P_HASTE:
            //        pot_info[P_HASTE].oi_know = true;
            //        after = false;
            //        if (add_haste(true))
            //            AddToLog("you feel yourself moving much faster");
            //        break;
            //    case P_RESTORE:
            //        if (ISRING(LEFT, R_ADDSTR))
            //            add_str(ref pstats.Str, -cur_ring[LEFT].Ac);
            //        if (ISRING(RIGHT, R_ADDSTR))
            //            add_str(ref pstats.Str, -cur_ring[RIGHT].Ac);
            //        if (pstats.Str < max_stats.Str)
            //            pstats.Str = max_stats.Str;
            //        if (ISRING(LEFT, R_ADDSTR))
            //            add_str(ref pstats.Str, cur_ring[LEFT].Ac);
            //        if (ISRING(RIGHT, R_ADDSTR))
            //            add_str(ref pstats.Str, cur_ring[RIGHT].Ac);
            //        AddToLog("hey, this tastes great.  It make you feel warm all over");
            //        break;
            //    case P_BLIND:
            //        do_pot(P_BLIND, true);
            //        break;
            //    case P_LEVIT:
            //        do_pot(P_LEVIT, true);
            //        break;
            //}
            //display___status();
            ///*
            // * Throw the item away
            // */

            //UpdateGuess(pot_info[obj.Which]);
            return;
        }

        // Turn on the ability to see invisible
        static void invis_on()
        {
            //THING mp;

            //Agent.Plyr.t_flags |= Mob.CANSEE;
            //for (mp = Dungeon.MobList; mp != null; mp = R14.next(mp))
            //    if (R14.on(mp, Mob.ISINVIS) && heroCanSeeMonster(mp) && !R14.on(Agent.Plyr, Mob.IsTrippinBalls))
            //        R12.MvAddCh(mp.t_pos.y, mp.t_pos.x, mp.t_disguise);
        }

        static void turn_see_fuse(int turn_off)
        {
            turn_see(turn_off);
        }

        // set whether we're seeing monsters on this level 
        static public bool turn_see(int turn_off)
        {
            //THING mp;
            //bool can_see, add_new;

            //add_new = false;
            //for (mp = Dungeon.MobList; mp != null; mp = R14.next(mp))
            //{
            //    R12.move(mp.t_pos.y, mp.t_pos.x);
            //    can_see = heroCanSeeMonster(mp);
            //    if (turn_off != 0)
            //    {
            //        if (!can_see)
            //            R12.addch(mp.t_oldch);
            //    }
            //    else
            //    {
            //        if (!can_see)
            //            R12.standout();
            //        if (!R14.on(Agent.Plyr, Mob.IsTrippinBalls))
            //            R12.addch(mp.MonsType);
            //        else
            //            R12.addch(R14.rnd(26) + 'A');
            //        if (!can_see)
            //        {
            //            R12.standend();
            //            add_new = true;
            //        }
            //    }
            //}
            //if (turn_off != 0)
            //    Agent.Plyr.t_flags &= ~Mob.CanSeeInvisible;
            //else
            //    Agent.Plyr.t_flags |= Mob.CanSeeInvisible;
            //return add_new;
            return false;
        }

        // Return TRUE if the player has seen the stairs
        static bool seen_stairs()
        {
            //THING tp;

            //R12.move(Dungeon.stairs.y, Dungeon.stairs.x);
            //if (R12.inch() == Char.STAIRS)			/* it's on the map */
            //    return true;
            //if (Dungeon.TheseShareCoords(R14.hero, Dungeon.stairs))			/* It's under him */
            //    return true;

            ///*
            // * if a monster is on the stairs, this gets hairy
            // */
            //if ((tp = Dungeon.GetMonster(Dungeon.stairs.y, Dungeon.stairs.x)) != null)
            //{
            //    if (heroCanSeeMonster(tp) && R14.on(tp, Mob.ISRUN))	/* if it's visible and awake */
            //        return true;			/* it must have moved there */

            //    if (R14.on(Agent.Plyr, Mob.CanSeeInvisible)		/* if she can detect monster */
            //        && tp.t_oldch == Char.STAIRS)		/* and there once were stairs */
            //        return true;			/* it must have moved there */
            //}
            return false;
        }

        // The guy just magically went up a level 
        static void raise_level() {
            Agent.Plyr.Stats.Xp = 
                Agent.ExperienceGoals[
                Agent.Plyr.Stats.Level - 1] + 1;
            Agent.LevelUpPlayerMaybe(Agent.Pos);
        }

        // do a potion with standard setup.  This means it uses a LightFuse and turns on a flag 
        static void do_pot(int type, bool knowit) {
            S.Log("************** do_pot() (POTION) commented out");
        //    PACT pp;
        //    int t;

        //    pp = p_actions[type];
        //    if (!pot_info[type].oi_know)
        //        pot_info[type].oi_know = knowit;
        //    t = spread(pp.pa_time);
        //    if (!on(Agent.Plyr, pp.pa_flags))
        //    {
        //        Agent.Plyr.t_flags |= pp.pa_flags;
        //        LightFuse(pp.pa_daemon, 0, t, AFTER);
        //        glanceAroundHero___look(false);
        //    }
        //    else
        //        lengthen(pp.pa_daemon, t);
        //    AddToLog(choose_str(pp.pa_high, pp.pa_straight));
        }

        // Add a haste to the player
        static bool add_haste(bool potion)
        {
            if (R14.on(Agent.Plyr, Mob.ISHASTE))
            {
                Agent.NumTurnsAsleep += R14.rnd(8);
                Agent.Plyr.t_flags &= ~(Mob.ISRUN | Mob.ISHASTE);
                S.Log("*** NEED TO DO: extinguish(nohaste) ***");
                R14.extinguish(R14.StopHaste);
                S.Log("you faint from exhaustion");
                return false;
            }
            else
            {
                Agent.Plyr.t_flags |= Mob.ISHASTE;
                if (potion)
                    R14.LightFuse(R14.StopHaste, 0, R14.rnd(4) + 4, R14.AFTER);
                return true;
            }
        }
    }