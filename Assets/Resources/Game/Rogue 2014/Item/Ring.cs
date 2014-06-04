using System;



    static class Ring //related
    {
        public const int LEFT = 0;
        public const int RIGHT = 1;
        public static string[] StonesUsed = new string[MAX];		/* Stone settings of the rings */

        public static STONE[] StonesPossible = new [] {
            new STONE("agate",	     25),      //looks like colors can be pretty much anything and with any saturation level.  but seems to always be a pattern of different distinct layers of colors, when cut for show usuall has an eye'ish shape (the layers form complete concentric "circles").  the layers appear to go from the center (formation point?) of the rock and outwards to the surface.  as if the rock kept growing with differently colored layers of material
            new STONE("alexandrite", 40),      //kinda ranges from less violety amethyst (but more greyish) to dull dark cyan/aqua/green         "The alexandrite variety displays a color change (alexandrite effect) dependent upon the nature of ambient lighting. Alexandrite effect is the phenomenon of an observed color change from greenish to reddish with a change in source illumination due physiological response of the human eye in a particular part of the visible spectrum. This color change is independent of any change of hue with viewing direction through the crystal that would arise from pleochroism."
            new STONE("amethyst",    50),      //more towards violet than i imagined
            new STONE("carnelian",   40),      //dull milky orange (sometimes salmonish tinged) that occasionally has grey streaks in it
            new STONE("diamond",	300),
            new STONE("emerald",	300),      //let's say pure green?  but more towards aqua/turquoise than i'd imagined
            new STONE("garnet",     50),       //almost same as ruby, but slightly more towards vermilion
            new STONE("germanium",	225),      //dark silver....also looks different in some other way....more like a shiny foil look than silvers more....diffuse?/blurry? shading
            new STONE("granite",	  5),      //mottled (sometimes wavy patterns) very light and very dark grey....sometimes with a rosy/salmon tint
            new STONE("jade",		150),      //smokey, milky, grayish? green
            new STONE("kryptonite", 300),      //milky regular green (how i imagined emerald)....sometimes glowing
            new STONE("lapis lazuli", 50),     //mostly regular somewhat dark blue.  mixed with some grey streaks/spots often
            new STONE("moonstone",	 50),      //kinda like pearls, but stones.  maybe light blue streaking dominants more than other colors.  can be transparent almost like clear glass, but shaped like stones.  and usually fairly opaque
            new STONE("obsidian",	 15),      //black, but a bit greyish, milky and subsurface scattering
            new STONE("onyx",		 60),      //shiny pure, fully opaque black
            new STONE("opal",		200),      //looks like different patterns of random saturated colors, pretty much all main slices of the rainbow?  dominant overall color is probably a greyish blue? but other colors all bright flecks     "Colorless, white, yellow, red, orange, green, brown, black, blue"
            new STONE("pearl",		220),      //iridescent? cream/off-white
            new STONE("peridot",	 63),      //on the chartreuse side, but ranges into what i imagined emerald was;  a regular, darker (not kawasaki) green
            new STONE("ruby",		350),   
            new STONE("sapphire",	285),      //darkish, but pure blue
            new STONE("stibiotantalite", 200), //was "stibotantalite" originally.   black, brown, yellow,....sometimes in distinct layers in that order?  greenish says one site
            new STONE("tiger eye",	 50),      //shades of yellows and/or oranges, ranging from fairly bright sometimes down to dark browns or even black in some examples
            new STONE("topaz",		 60),      //orangey and/or yellowish tinged brown.  light, maybe even tan?  fairly solid color.  
            new STONE("turquoise",	 70),
            new STONE("taaffeite",	300),      //dark, dull purple typically but can be radically diff colors.   "Colorless, greyish violet, violet red, red, greenish, light green, pink violet, mauve"
            new STONE("zircon",	 80),      //typicall dark cyan?....not too saturated but very close to "plain regular blue"         "Reddish brown, yellow, green, blue, gray, colorless; in thin section, colorless to pale brown"
        };
        static int NSTONES = StonesPossible.Length;
        static int cNSTONES = NSTONES;
        static bool[] used = new bool[NSTONES];



        public class STONE 
        {
            public string st_name;
            public int st_value;

            public STONE(string name, int value)
            {
                st_name = name;
                st_value = value;
            }
        }

        // Ring types
        public const int R_PROTECT = 0;
        public const int R_ADDSTR = 1;
        public const int R_SUSTSTR = 2;
        public const int R_SEARCH = 3;
        public const int R_SEEINVIS = 4;
        public const int R_NOP = 5;
        public const int R_AGGR = 6;
        public const int R_ADDHIT = 7;
        public const int R_ADDDAM = 8;
        public const int R_REGEN = 9;
        public const int R_DIGEST = 10;
        public const int R_TELEPORT = 11;
        public const int R_STEALTH = 12;
        public const int R_SUSTARM = 13;
        public const int MAX = 14;

        public static ObjectData[] Data = new[]
        {
            new ObjectData("Protection", 9, 400, null, false),
            new ObjectData("Add Strength", 9, 400, null, false),
            new ObjectData("Sustain Strength", 5, 280, null, false),
            new ObjectData("Searching", 10, 420, null, false),
            new ObjectData("See Invisible", 10, 310, null, false),
            new ObjectData("Adornment", 1, 10, null, false),
            new ObjectData("Aggravate Monster", 10, 10, null, false),
            new ObjectData("Dexterity", 8, 440, null, false),
            new ObjectData("Increase Damage", 8, 400, null, false),
            new ObjectData("Regeneration", 4, 460, null, false),
            new ObjectData("Slow Digestion", 9, 240, null, false),
            new ObjectData("Teleportation", 5, 30, null, false),
            new ObjectData("Stealth", 7, 470, null, false),
            new ObjectData("Maintain Armour", 5, 380, null, false)
        };

        //setup stone settings
        static Ring()
        {
            S.Log("Ring stones");
            int i, j;

            for (i = 0; i < NSTONES; i++)
                used[i] = false;

            for (i = 0; i < Ring.MAX; i++)
            {
                do
                    j = R14.rnd(NSTONES);
                while (used[j]);

                used[j] = true;
                StonesUsed[i] = StonesPossible[j].st_name;
                Data[i].Worth += StonesPossible[j].st_value;
                
                string s = StonesUsed[i][0].ToString().ToUpper();
				s /***/ += StonesUsed[i].Substring(1) + " (" + Data[i].Worth + ")";
				S.Log(s);
            }
        }

        public static bool ISRING(int h, int r)
        {
            return (Agent.cur_ring[h] != null && Agent.cur_ring[h].Which == r);
        }

        public static bool ISWEARING(int r)
        {
            return (ISRING(LEFT, r) || ISRING(RIGHT, r));
        }

        //// Put a ring on a hand
        //void ring_on()
        //{
        //    THING obj;
        //    int ring;

        //    obj = get_item("put on", RING);
        //    /*
        //     * Make certain that it is somethings that we want to wear
        //     */
        //    if (obj == null)
        //        return;
        //    if (obj.ItemType != RING)
        //    {
        //        if (!Agent.Terse)
        //            AddToLog("it would be difficult to wrap that around a finger");
        //        else
        //            AddToLog("not a ring");
        //        return;
        //    }

        //    /*
        //     * find out which hand to put it on
        //     */
        //    if (CurrentlyUsing(obj))
        //        return;

        //    if (cur_ring[LEFT] == null && cur_ring[RIGHT] == null)
        //    {
        //        if ((ring = gethand()) < 0)
        //            return;
        //    }
        //    else if (cur_ring[LEFT] == null)
        //        ring = LEFT;
        //    else if (cur_ring[RIGHT] == null)
        //        ring = RIGHT;
        //    else
        //    {
        //        if (!Agent.Terse)
        //            AddToLog("you already have a ring on each hand");
        //        else
        //            AddToLog("wearing two");
        //        return;
        //    }
        //    cur_ring[ring] = obj;

        //    /*
        //     * Calculate the effect it has on the poor guy.
        //     */
        //    switch (obj.Which)
        //    {
        //        case R_ADDSTR:
        //            ModifyStrength(obj.Ac);
        //            break;
        //        case R_SEEINVIS:
        //            invis_on();
        //            break;
        //        case R_AGGR:
        //            aggravate();
        //            break;
        //    }

        //    if (!Agent.Terse)
        //        addmsg("you are now wearing ");
        //    AddToLog("{0} ({1})", GetDescription(obj, true), obj.CharInPack);
        //}

        //// take off a ring
        //void ring_off()
        //{
        //    int ring;
        //    THING obj;

        //    if (cur_ring[LEFT] == null && cur_ring[RIGHT] == null)
        //    {
        //        if (Agent.Terse)
        //            AddToLog("no rings");
        //        else
        //            AddToLog("you aren'tag wearing any rings");
        //        return;
        //    }
        //    else if (cur_ring[LEFT] == null)
        //        ring = RIGHT;
        //    else if (cur_ring[RIGHT] == null)
        //        ring = LEFT;
        //    else
        //        if ((ring = gethand()) < 0)
        //            return;
        //    topLineCursorPos___mpos = 0;
        //    obj = cur_ring[ring];
        //    if (obj == null)
        //    {
        //        AddToLog("not wearing such a ring");
        //        return;
        //    }
        //    if (detachable(obj))
        //        AddToLog("was wearing {0}({1})", GetDescription(obj, true), obj.CharInPack);
        //}

        //// Which hand is the hero interested in?
        //int gethand()
        //{
        //    int c;

        //    for (; ; )
        //    {
        //        if (Agent.Terse)
        //            AddToLog("left or right ring? ");
        //        else
        //            AddToLog("left hand or right hand? ");
        //        if ((c = readchar()) == ESCAPE)
        //            return -1;
        //        topLineCursorPos___mpos = 0;
        //        if (c == 'l' || c == 'L')
        //            return LEFT;
        //        else if (c == 'r' || c == 'R')
        //            return RIGHT;
        //        if (Agent.Terse)
        //            AddToLog("L or R");
        //        else
        //            AddToLog("please type L or R");
        //    }
        //}

        //// How much food does this ring use up?
        //int ring_eat(int hand)
        //{
        //    THING ring;
        //    int eat;
        //    int[] uses = {
        //     1,	/* R_PROTECT */		 1,	/* R_ADDSTR */
        //     1,	/* R_SUSTSTR */		-3,	/* R_SEARCH */
        //    -5,	/* R_SEEINVIS */	 0,	/* R_NOP */
        //     0,	/* R_AGGR */		-3,	/* R_ADDHIT */
        //    -3,	/* R_ADDDAM */		 2,	/* R_REGEN */
        //    -2,	/* R_DIGEST */		 0,	/* R_TELEPORT */
        //     1,	/* R_STEALTH */		 1	/* R_SUSTARM */
        //    };

        //    if ((ring = cur_ring[hand]) == null)
        //        return 0;
        //    if ((eat = uses[ring.Which]) < 0)
        //        eat = (rnd(-eat) == 0) ? 1 : 0;
        //    if (ring.Which == R_DIGEST)
        //        eat = -eat;
        //    return eat;
        //}

        // Print ring bonuses
        static public string ProtectionMaybe(THING obj)
        {
            if (!((obj.o_flags & Item.ISKNOW) != 0))
                return "";

            //there are 4 rings which want to show protection #
            switch (obj.Which)
            {
                case R_PROTECT:
                case R_ADDSTR:
                case R_ADDDAM:
                case R_ADDHIT:
                    return string.Format(" [{0}]", obj.Ac);
            }

            return "";
        }
    }