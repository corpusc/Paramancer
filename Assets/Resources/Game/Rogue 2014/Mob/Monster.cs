using System;


	// array containing information on all the various types of monsters 
    public class monster
    {
        public string Name;
        public int CarryProb; /* Probability of carrying */
        public int Flags;
        public stats Stats; /* Initial stats */

        public monster(string name, int carry, int flags, stats stats)
        {
            Name = name;
            CarryProb = carry;
            Flags = flags;
            Stats = stats.Copy();
        }
    };

    public static partial class R14
    {
        static char[] lvl_mons = 
        {
            'K', 'E', 'B', 'S', 'H', 'I', 'R', 'O', 'Z', 'L', 'C', 'Q', 'A',
            'N', 'Y', 'F', 'T', 'W', 'P', 'X', 'U', 'M', 'V', 'G', 'J', 'D'
        };

        static char[] wand_mons =
        {
            'K', 'E', 'B', 'S', 'H', '\0', 'R', 'O', 'Z', '\0', 'C', 'Q', 'A',
            '\0', 'Y', '\0', 'T', 'W', 'P', '\0', 'U', 'M', 'V', 'G', 'J', '\0'
        };

        public static monster[] monsters = new[]
        {
            /* Name, CARRY, FLAG, str, exp, lvl, amr, hpt, dmg */
            new monster("aquator", 0, Mob.ISMEAN, new stats(10, 20, 5, 2, 1, "0x0/0x0")),
            new monster("bat", 0, Mob.ISFLY, new stats(10, 1, 1, 3, 1, "1x2")),
            new monster("centaur", 15, 0, new stats(10, 17, 4, 4, 1, "1x2/1x5/1x5")),
            new monster("dragon", 100, Mob.ISMEAN, new stats(10, 5000, 10, -1, 1, "1x8/1x8/3x10")),
            new monster("emu", 0, Mob.ISMEAN, new stats(10, 2, 1, 7, 1, "1x2")),
            new monster("venus flytrap", 0, Mob.ISMEAN, new stats(10, 80, 8, 3, 1, "%%%x0")),
            /* NOTE: the damage is %%% so that xstr won't merge this */
            /* string with others, since it is written on in the program */
            new monster("griffin", 20, Mob.ISMEAN | Mob.ISFLY | Mob.ISREGEN, new stats(10, 2000, 13, 2, 1, "4x3/3x5")),
            new monster("hobgoblin", 0, Mob.ISMEAN, new stats(10, 3, 1, 5, 1, "1x8")),
            new monster("ice monster", 0, 0, new stats(10, 5, 1, 9, 1, "0x0")),
            new monster("jabberwock", 70, 0, new stats(10, 3000, 15, 6, 1, "2x12/2x4")),
            new monster("kestrel", 0, Mob.ISMEAN | Mob.ISFLY, new stats(10, 1, 1, 7, 1, "1x4")),
            new monster("leprechaun", 0, 0, new stats(10, 10, 3, 8, 1, "1x1")),
            new monster("medusa", 40, Mob.ISMEAN, new stats(10, 200, 8, 2, 1, "3x4/3x4/2x5")),
            new monster("nymph", 100, 0, new stats(10, 37, 3, 9, 1, "0x0")),
            new monster("orc", 15, Mob.ISGREED, new stats(10, 5, 1, 6, 1, "1x8")),
            new monster("phantom", 0, Mob.ISINVIS, new stats(10, 120, 8, 3, 1, "4x4")),
            new monster("quagga", 0, Mob.ISMEAN, new stats(10, 15, 3, 3, 1, "1x5/1x5")),
            new monster("rattlesnake", 0, Mob.ISMEAN, new stats(10, 9, 2, 3, 1, "1x6")),
            new monster("snake", 0, Mob.ISMEAN, new stats(10, 2, 1, 5, 1, "1x3")),
            new monster("troll", 50, Mob.ISREGEN | Mob.ISMEAN, new stats(10, 120, 6, 4, 1, "1x8/1x8/2x6")),
            new monster("black unicorn", 0, Mob.ISMEAN, new stats(10, 190, 7, -2, 1, "1x9/1x9/2x9")),
            new monster("vampire", 20, Mob.ISREGEN | Mob.ISMEAN, new stats(10, 350, 8, 1, 1, "1x10")),
            new monster("wraith", 0, 0, new stats(10, 55, 5, 4, 1, "1x6")),
            new monster("xeroc", 30, 0, new stats(10, 100, 7, 7, 1, "4x4")),
            new monster("yeti", 30, 0, new stats(10, 50, 4, 6, 1, "1x6/1x6")),
            new monster("zombie", 0, Mob.ISMEAN, new stats(10, 6, 2, 8, 1, "1x8"))
        };

        

        static bool heroCanSeeMonster(THING mp)
        {
            int y, x;

            if (on(Agent.Plyr, Mob.ISBLIND))
                return false;
            if (on(mp, Mob.ISINVIS) && !on(Agent.Plyr, Mob.CANSEE))
                return false;
            y = mp.t_pos.y;
            x = mp.t_pos.x;
            if (Dungeon.dist(y, x, hero.y, hero.x) < LAMPDIST)
            {
                if (y != hero.y && 
                    x != hero.x && 
                    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(y, hero.x)) && 
                    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(hero.y, x)))
                    return false;
                return true;
            }

            if (mp.CurrRoom != Agent.Plyr.CurrRoom)
                return false;

            return ((bool)!((mp.CurrRoom.Flags & Room.ISDARK) == Room.ISDARK));
        }

        // return the monster name for the given monster
        static public string GetMonsterName(THING tp)
        {
            string mname = "";

            //if (!heroCanSeeMonster(tp) && !on(Agent.Plyr, Mob.CanSeeInvisible))
            //    return (Agent.Terse ? "it" : "something");
            //else if (on(Agent.Plyr, Mob.IsTrippinBalls))
            //{
            //    //FIXME for proper hallucinating

            //    //R12.move(tp.t_pos.y, tp.t_pos.x);
            //    //ch = R12.inch();
            //    //if (!char.IsUpper((char)ch))
            //    //    ch = R14.rnd(26);
            //    //else
            //    //    ch -= 'A';
            //    //mname = monsters[ch].Name;
            //}
            //else
                mname = monsters[tp.MonsType - 'A'].Name;

            tp.Name = mname;
            return char.ToUpper(mname[0]) + mname.Substring(1);
        }

        // Pick a monster to show up.  The lower the Agent.LevelOfMap,
        // the meaner the monster.
        public static char randmonster(bool wander)
        {
            int d;
            char[] mons;

            mons = (wander ? wand_mons : lvl_mons);
            do
            {
                d = Agent.LevelOfMap + (R14.rnd(10) - 6);
                if (d < 0)
                    d = R14.rnd(5);
                if (d > 25)
                    d = R14.rnd(5) + 21;
            }
            while (mons[d] == 0);
            return mons[d];
        }

        // Pick a new monster and add it to the list
        public static void new_monster(THING tp, char type, Coord cp)
        {
            monster mp;
            int lev_add;

            if ((lev_add = Agent.LevelOfMap - Dungeon.AMULETLEVEL) < 0)
                lev_add = 0;

            attach(ref Dungeon.MobList, tp);
            tp.MonsType = type;
            tp.t_disguise = type;
            tp.t_pos = cp;
            //here we used to move (cursor?) to hero Coord
            tp.CurrRoom = Dungeon.roomin(cp);
            Dungeon.SetMonster(cp.y, cp.x, tp);
            mp = monsters[tp.MonsType-'A'];
            tp.Stats = new stats();
            tp.Stats.Level = mp.Stats.Level + lev_add;
            tp.Stats.HpMax = tp.Stats.Hp = RollDice(tp.Stats.Level, 8);
            tp.Stats.Ac = mp.Stats.Ac - lev_add;
            tp.Stats.Dmg = mp.Stats.Dmg;
            tp.Stats.Str = mp.Stats.Str;
            tp.Stats.Xp = mp.Stats.Xp + lev_add * 10 + getFactorConsideringLevelAndHpMax(tp);
            tp.t_flags = mp.Flags;
            if (Agent.LevelOfMap > 29)
                tp.t_flags |= Mob.ISHASTE;
            tp.t_turn = true;
            tp.Pack = null;

            if (Ring.ISWEARING(Ring.R_AGGR))
                Mob.runto(cp);
            if (type == 'X')
                tp.t_disguise = R14.rnd_thing();

            Console.Write("new_monster() = " + GetMonsterName(tp) + "     ");
            Console.Write("Level: " + tp.Stats.Level);
            Console.WriteLine("     HpMax: " + tp.Stats.HpMax);
        }

        static public int getFactorConsideringLevelAndHpMax(THING t)
        {
            int mod;

            if (t.Stats.Level == 1)
                mod = t.Stats.HpMax / 8;
            else
                mod = t.Stats.HpMax / 6;

            if (t.Stats.Level > 9)
                mod *= 20;
            else if (t.Stats.Level > 6)
                mod *= 4;

            return mod;
        }

        //make wandering monster and aim it at the player
        static void wanderer()
        {
            //THING tp;
            //Coord cp = new Coord();

            //tp = new THING();
            //do
            //{
            //    Dungeon.find_floor(null, out cp, 0, true);
            //}
            //while (roomin(cp) == Agent.Plyr.CurrRoom);
            //new_monster(tp, randmonster(true), cp);
            //if (on(Agent.Plyr, CanSeeInvisible))
            //{
            //    R12.standout();
            //    if (!on(Agent.Plyr, Mob.IsTrippinBalls))
            //        R12.addch(tp.MonsType);
            //    else
            //        R12.addch(R14.rnd(26) + 'A');
            //    R12.standend();
            //}
            //runto(tp.t_pos);
            //if (wizard)
            //    AddToLog("started a wandering {0}", monsters[tp.MonsType-'A'].Name);
        }

        // when the hero steps next to a monster 
        static THING wake_monster(int y, int x)
        {
            THING tp;
            room rp;
            char ch;
            string mname;

            tp = Dungeon.GetMonster(y, x);
            if (tp == null)
                throw new Exception("Couldn't find monster!");
            ch = tp.MonsType;
            
            // every time he sees mean monster, it might start chasing him 
            if (!on(tp, Mob.ISRUN) && R14.rnd(3) != 0 && on(tp, Mob.ISMEAN) && !on(tp, Mob.ISHELD)
                && !Ring.ISWEARING(Ring.R_STEALTH) && !on(Agent.Plyr, Mob.ISLEVIT))
            {
                tp.t_dest = hero;
                tp.t_flags |= Mob.ISRUN;
            }
            if (ch == 'M' && !on(Agent.Plyr, Mob.ISBLIND) && !on(Agent.Plyr, Mob.IsTrippinBalls)
                && !on(tp, Mob.ISFOUND) && !on(tp, Mob.ISCANC) && on(tp, Mob.ISRUN))
            {
                rp = Agent.Plyr.CurrRoom;
                if ((rp != null && !((rp.Flags & Room.ISDARK) == Room.ISDARK))
                    || Dungeon.dist(y, x, hero.y, hero.x) < LAMPDIST)
                {
                    tp.t_flags |= Mob.ISFOUND;
                    if (!save(VS_MAGIC))
                    {
                        if (on(Agent.Plyr, Mob.ISHUH))
                            lengthen(unconfuse, spread(HUHDURATION));
                        else
                            LightFuse(unconfuse, 0, spread(HUHDURATION), AFTER);

                        Agent.Plyr.t_flags |= Mob.ISHUH;
                        mname = GetMonsterName(tp);
                        addmsg("{0}", mname);
                        if (mname != "it")
                            addmsg("'");
                        AddToLog("s gaze has confused you");
                    }
                }
            }
            
            //greedy ones guard gold
            if (on(tp, Mob.ISGREED) && !on(tp, Mob.ISRUN))
            {
                tp.t_flags |= Mob.ISRUN;
                if (Agent.Plyr.CurrRoom.GoldVal > 0)
                    tp.t_dest = Agent.Plyr.CurrRoom.GoldPos;
                else
                    tp.t_dest = hero;
            }
            return tp;
        }

        // Give a pack to a monster if it deserves one
        public static void give_pack(THING tp)
        {
            if (Agent.LevelOfMap >= Agent.dyn___max_level && R14.rnd(100) < monsters[tp.MonsType - 'A'].CarryProb)
                attach(ref tp.Pack, GetNewRandomThing());
        }

        // See if a creature save against something
        static bool save_throw(int which, THING tp)
        {
            int need;

            need = 14 + which - tp.Stats.Level / 2;
            return (RollDice(1, 20) >= need);
        }

        // See if he saves against various nasty things
        static bool save(int which)
        {
            if (which == VS_MAGIC)
            {
                if (Ring.ISRING(Ring.LEFT, Ring.R_PROTECT))
                    which -= Agent.cur_ring[Ring.LEFT].Ac;
                if (Ring.ISRING(Ring.RIGHT, Ring.R_PROTECT))
                    which -= Agent.cur_ring[Ring.RIGHT].Ac;
            }
            return save_throw(which, Agent.Plyr);
        }
    }