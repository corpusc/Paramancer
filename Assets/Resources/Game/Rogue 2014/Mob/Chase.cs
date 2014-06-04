// mob chases another mob 
using System;



    public static class Mob
    {
        static int DRAGONSHOT = 5; // one chance in "DRAGONSHOT" that a dragon will flame 
        static Coord ch_ret = new Coord(); // Where chasing takes you 

        // flags for creatures/mobs
        public static int CANHUH = Convert.ToInt32("0000001", 8); /* creature can confuse */
        public static int CANSEE = Convert.ToInt32("0000002", 8); /* creature can see invisible creatures */
        public static int ISBLIND = Convert.ToInt32("0000004", 8); /* creature is blind */
        public static int ISCANC = Convert.ToInt32("0000010", 8); /* creature has special qualities cancelled */
        public static int ISLEVIT = Convert.ToInt32("0000010", 8); /* hero is levitating */
        public static int ISFOUND = Convert.ToInt32("0000020", 8); /* creature has been seen (used for objects) */
        public static int ISGREED = Convert.ToInt32("0000040", 8); /* creature runs to protect gold */
        public static int ISHASTE = Convert.ToInt32("0000100", 8); /* creature has been hastened */
        public static int ISHELD = Convert.ToInt32("0000400", 8); /* creature has been held */
        public static int ISHUH = Convert.ToInt32("0001000", 8); /* creature is confused */
        public static int ISINVIS = Convert.ToInt32("0002000", 8); /* creature is invisible */
        public static int ISMEAN = Convert.ToInt32("0004000", 8); /* creature can wake when player enters room */
        public static int IsTrippinBalls = Convert.ToInt32("0004000", 8); /* hero is on acid trip */            //FIXME?  hallucination only ever works on player?   monsters can't hallucinate?  if they could, wouldn't that make them act the same as "confused"?  should prob be ONLY a player property?
        public static int ISREGEN = Convert.ToInt32("0010000", 8); /* creature can regenerate */
        public static int ISRUN = Convert.ToInt32("0020000", 8); /* creature is running at the player */
        public static int CanSeeInvisible = Convert.ToInt32("040000", 8); /* hero can detect unseen monsters */
        public static int ISFLY = Convert.ToInt32("0040000", 8); /* creature can fly */
        public static int ISSLOW = Convert.ToInt32("0100000", 8); /* creature has been slowed */

        // Make all the running monsters move.
        public static void runners(int ignored)
        {
            THING nt; //next thing
            Coord orig_pos;

            for (THING t = Dungeon.MobList; t != null; t = nt)
            {
                /* remember this in case the monster's "next" is changed */
                nt = R14.next(t);
                if (!R14.on(t, ISHELD) && R14.on(t, ISRUN))
                {
                    orig_pos = t.t_pos;
                    if (move_monst(t) == -1)
                        continue;
                    if (R14.on(t, ISFLY) && dist_cp(R14.hero, t.t_pos) >= 3)
                        move_monst(t);
                }
            }
        }

        // Execute a single turn of running for a monster
        public static int move_monst(THING tp)
        {
            if (!R14.on(tp, Mob.ISSLOW) || tp.t_turn)
                if (do_chase(tp) == -1)
                    return (-1);
            if (R14.on(tp, Mob.ISHASTE))
                if (do_chase(tp) == -1)
                    return (-1);

            tp.t_turn ^= true;
            return (0);
        }

        //// Make the monster's new location be the specified one, updating
        //// all the relevant state.
        //public static void relocate(THING th, Coord new_loc)
        //{
        //    room oroom;

        //    char symbol = th.t_disguise;

        //    if (!TheseAreTheSame(new_loc, th.t_pos))
        //    {
        //        Debug.WriteLine(string.Format("{0} moved to {1}.", symbol, new_loc));

        //        R12.MvAddCh(th.t_pos.y, th.t_pos.x, th.t_oldch);
        //        th.t_room = roomin(new_loc);
        //        set_oldch(th, new_loc);
        //        oroom = th.t_room;
        //        moat(th.t_pos.y, th.t_pos.x, null);

        //        if (oroom != th.t_room)
        //        {
        //            Debug.WriteLine(string.Format("{0} moved into room {1}.", symbol, oroom));
        //            th.t_dest = find_dest(th);
        //        }

        //        th.t_pos.CopyFrom(new_loc);
        //        moat(new_loc.y, new_loc.x, th);
        //    }

        //    R12.move(new_loc.y, new_loc.x);

        //    if (see_monst(th))
        //        R12.addch(th.t_disguise);
        //    else if (on(player, CanSeeInvisible))
        //    {
        //        R12.standout();
        //        R12.addch(th.t_type);
        //        R12.standend();
        //    }
        //}

        public static Coord thisCoord = new Coord();			/* Temporary destination for chaser */

        // Make one thing chase another.
        public static int do_chase(THING th)
        {
            S.Log("********** UNCOMMENT do_chase() **********");
        //    coord cp;
        //    room rer, ree;	/* room of chaser, room of chasee */
        //    int mindist = 32767, curdist;
        //    bool stoprun = false;	/* TRUE means we are there */
        //    bool door;
        //    THING obj;

        //    rer = th.t_room;		/* Find room of chaser */
        //    if (on(th, ISGREED) && rer.r_goldval == 0)
        //        th.t_dest = hero;	/* If gold has been taken, run after hero */
        //    if (th.t_dest == hero)	/* Find room of chasee */
        //        ree = proom;
        //    else
        //        ree = roomin(th.t_dest);
        //    /*
        //     * We don't count doors as inside rooms for this routine
        //     */
        //    door = (Dungeon.chat(th.t_pos.y, th.t_pos.x) == DOOR);
        ///*
        // * If the object of our desire is in a different room,
        // * and we are not in a corridor, run to the door nearest to
        // * our goal.
        // */
        //over:
        //    if (rer != ree)
        //    {
        //        for (int i = 0; i < rer.r_nexits; i++)
        //        {
        //            cp = rer.r_exit[i];
        //            curdist = dist_cp(th.t_dest, cp);
        //            if (curdist < mindist)
        //            {
        //                thisCoord = cp;
        //                mindist = curdist;
        //            }
        //        }
        //        if (door)
        //        {
        //            rer = passages[flat(th.t_pos.y, th.t_pos.x) & F_PNUM];
        //            door = false;
        //            goto over;
        //        }
        //    }
        //    else
        //    {
        //        thisCoord = th.t_dest;
        //        /*
        //         * For dragons check and see if 
        //         * (a) the hero is on a straight
        //         * line from it, and 
        //         * (child) that it's within shooting distance, but outside of striking range.
        //         */
        //        if (th.t_type == 'D' && (th.t_pos.y == R14.hero.y || th.t_pos.x == R14.hero.x
        //            || Math.Abs(th.t_pos.y - R14.hero.y) == Math.Abs(th.t_pos.x - R14.hero.x))
        //            && dist_cp(th.t_pos, hero) <= BOLT_LENGTH * BOLT_LENGTH
        //            && !on(th, ISCANC) && R14.rnd(DRAGONSHOT) == 0)
        //        {
        //            delta.y = Math.Sign(R14.hero.y - th.t_pos.y);
        //            delta.x = Math.Sign(R14.hero.x - th.t_pos.x);
        //            fire_bolt(th.t_pos, delta, "flame");
        //            Agent.running = false;
        //            Agent.Quiet = 0;
        //            return (0);
        //        }
        //    }
        //    /*
        //     * This now contains what we want to run to this time
        //     * so we run to it.  If we hit it we either want to fight it
        //     * or stop running
        //     */
        //    if (!chase(th, thisCoord))
        //    {
        //        if (Dungeon.TheseAreTheSame(thisCoord, R14.hero))
        //        {
        //            return (attack(th));
        //        }
        //        else if (TheseAreTheSame(thisCoord, th.t_dest))
        //        {
        //            for (obj = lvl_obj; obj != null; obj = R14.next(obj))
        //                if (th.t_dest == obj.ItemPos)
        //                {
        //                    detach(ref lvl_obj, obj);
        //                    attach(ref th.t_pack, obj);
        //                    chat(obj.o_pos.y, obj.o_pos.x, (th.t_room.r_flags & ISGONE) != 0 ? PASSAGE : FLOOR);
        //                    th.t_dest = find_dest(th);
        //                    break;
        //                }
        //            if (th.t_type != 'F')
        //                stoprun = true;
        //        }
        //    }
        //    else
        //    {
        //        if (th.t_type == 'F')
        //            return (0);
        //    }
        //    relocate(th, ch_ret);
        //    /*
        //     * And stop running if need be
        //     */
        //    if (stoprun && TheseAreTheSame(th.t_pos, th.t_dest))
        //        th.t_flags &= ~ISRUN;
            return (0);
        }

        // Set the oldch character for the monster
        public static void set_oldch(THING tp, Coord cp)
        {
            S.Log(" ************ set_oldch() largely commented out ***********");
            char sch;

            if (Dungeon.TheseAreTheSame(tp.t_pos, cp))
                return;

            sch = tp.t_oldch;
            //tp.t_oldch = CCHAR(R12.mvinch(cp.y, cp.x));
            //if (!R14.on(Agent.Plyr, Mob.ISBLIND))
            //{
            //    if ((sch == Char.FLOOR || tp.t_oldch == Char.FLOOR) && ((tp.CurrRoom.Flags & Room.ISDARK) != 0))
            //        tp.t_oldch = ' ';
            //    else if (dist_cp(cp, R14.hero) <= R14.LAMPDIST && R14.see_floor)
            //        tp.t_oldch = Dungeon.chat(cp.y, cp.x);
            //}
        }

        // Return TRUE if the hero can see the monster
        public static bool see_monst(THING mp)
        {
            int y, x;

            if (R14.on(Agent.Plyr, ISBLIND))
                return false;
            if (R14.on(mp, ISINVIS) && !R14.on(Agent.Plyr, CANSEE))
                return false;

            y = mp.t_pos.y;
            x = mp.t_pos.x;
            if (dist(y, x, R14.hero.y, R14.hero.x) < R14.LAMPDIST)
            {
                if (y != R14.hero.y && x != R14.hero.x && 
                    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(y, R14.hero.x)) && 
                    !Dungeon.walkable___step_ok(Dungeon.GetCharAt(R14.hero.y, x)))
                    return false;
                return true;
            }

            if (mp.CurrRoom != Agent.Plyr.CurrRoom)
                return false;

            return ((bool)!((mp.CurrRoom.Flags & Room.ISDARK) == Room.ISDARK));
        }

        // Set a monster running directly at player
        public static void runto(Coord runner)
        {
            THING tp;

            /*
             * If we couldn't find him, something is funny
             */
            tp = Dungeon.GetMonster(runner.y, runner.x);
            /*
             * Start the beastie running
             */
            tp.t_flags |= ISRUN;
            tp.t_flags &= ~ISHELD;
            tp.t_dest = find_dest(tp);
        }

        public static Coord tryp = new Coord();

        //// Find the spot for the chaser(er) to move closer to the
        //// chasee(ee).  Returns TRUE if we want to keep on chasing later
        //// FALSE if we reach the goal.
        //public static bool chase(THING tp, Coord ee)
        //{
        //    THING obj;
        //    int x, y;
        //    int curdist, thisdist;
        //    coord er = tp.t_pos;
        //    char ch;
        //    int plcnt = 1;

        //    /*
        //     * If the thing is confused, let it move randomly. Invisible
        //     * Stalkers are slightly confused all of the time, and bats are
        //     * quite confused all the time
        //     */
        //    if ((on(tp, ISHUH) && rnd(5) != 0) || (tp.t_type == 'P' && rnd(5) == 0) || (tp.t_type == 'B' && rnd(2) == 0))
        //    {
        //        /*
        //         * get a valid random move
        //         */
        //        ch_ret.CopyFrom(rndmove(tp));
        //        curdist = dist_cp(ch_ret, ee);
        //        /*
        //         * Small chance that it will become un-confused 
        //         */
        //        if (rnd(20) == 0)
        //            tp.t_flags &= ~ISHUH;
        //    }
        //    /*
        //     * Otherwise, find the empty spot next to the chaser that is
        //     * closest to the chasee.
        //     */
        //    else
        //    {
        //        int ey, ex;
        //        /*
        //         * This will eventually hold where we move to get closer
        //         * If we can't find an empty spot, we stay where we are.
        //         */
        //        curdist = dist_cp(er, ee);
        //        ch_ret.CopyFrom(er);

        //        ey = er.y + 1;
        //        if (ey >= NUMLINES - 1)
        //            ey = NUMLINES - 2;
        //        ex = er.x + 1;
        //        if (ex >= NUMCOLS)
        //            ex = NUMCOLS - 1;

        //        for (x = er.x - 1; x <= ex; x++)
        //        {
        //            if (x < 0)
        //                continue;
        //            tryp.x = x;
        //            for (y = er.y - 1; y <= ey; y++)
        //            {
        //                tryp.y = y;
        //                if (!diag_ok(er, tryp))
        //                    continue;
        //                ch = GetMonsterAppearance(y, x);
        //                if (walkable___step_ok(ch))
        //                {
        //                    /*
        //                     * If it is a scroll, it might be a scare monster scroll
        //                     * so we need to look it up to see what type it is.
        //                     */
        //                    if (ch == SCROLL)
        //                    {
        //                        for (obj = lvl_obj; obj != null; obj = next(obj))
        //                        {
        //                            if (y == obj.o_pos.y && x == obj.o_pos.x)
        //                                break;
        //                        }
        //                        if (obj != null && obj.o_which == S_SCARE)
        //                            continue;
        //                    }
        //                    /*
        //                     * It can also be a Xeroc, which we shouldn't step on
        //                     */
        //                    if ((obj = moat(y, x)) != null && obj.t_type == 'X')
        //                        continue;
        //                    /*
        //                     * If we didn't find any scrolls at this place or it
        //                     * wasn't a scare scroll, then this place counts
        //                     */
        //                    thisdist = dist(y, x, ee.y, ee.x);
        //                    if (thisdist < curdist)
        //                    {
        //                        plcnt = 1;
        //                        ch_ret.CopyFrom(tryp);
        //                        curdist = thisdist;
        //                    }
        //                    else if (thisdist == curdist && rnd(++plcnt) == 0)
        //                    {
        //                        ch_ret.CopyFrom(tryp);
        //                        curdist = thisdist;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return (bool)(curdist != 0 && !TheseAreTheSame(ch_ret, hero));
        //}

        // Check to see if the move is legal if it is diagonal
        public static bool diag_ok(Coord sp, Coord ep)
        {
            if (ep.x < 0 || ep.x >= R14.NUMCOLS || ep.y <= 0 || ep.y >= R14.NUMLINES - 1)
                return false;
            if (ep.x == sp.x || ep.y == sp.y)
                return true;

            return (bool)(
                Dungeon.walkable___step_ok(Dungeon.GetCharAt(ep.y, sp.x)) && 
                Dungeon.walkable___step_ok(Dungeon.GetCharAt(sp.y, ep.x)));
        }

        // find the proper destination for the monster
        public static Coord find_dest(THING tp)
        {
            THING obj;
            int prob = R14.monsters[tp.MonsType - 'A'].CarryProb;

            if ((prob) <= 0 || tp.CurrRoom == Agent.Plyr.CurrRoom || see_monst(tp))
                return R14.hero;
            for (obj = Dungeon.ItemList; obj != null; obj = R14.next(obj))
            {
                if (obj.ItemType == Char.SCROLL && obj.Which == Scroll.S_SCARE)
                    continue;
                if (Dungeon.roomin(obj.ItemPos) == tp.CurrRoom && R14.rnd(100) < prob)
                {
                    for (tp = Dungeon.MobList; tp != null; tp = R14.next(tp))
                        if (tp.t_dest.Equals(obj.ItemPos))
                            break;
                    if (tp == null)
                        return obj.ItemPos;
                }
            }
            return R14.hero;
        }

        // Calculate the "distance" between to points.  Actually,
        // this calculates d^2, not d, but that's good enough for
        // our purposes, since it's only used comparitively.
        public static int dist(int y1, int x1, int y2, int x2)
        {
            return ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        // Call dist() with appropriate arguments for coord pointers
        public static int dist_cp(Coord c1, Coord c2)
        {
            return dist(c1.y, c1.x, c2.y, c2.x);
        }
    }