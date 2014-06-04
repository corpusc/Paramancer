using System;



    public static partial class Dungeon
    {
        public const int CUBES_ACROSS_CHAR = 5;
        public const int CUBES_UP_FROM_BOTTOM = 32;
        const int MAXLINES = 32; /* maximum number of screen lines used */
        const int MAXCOLS = 80; /* maximum number of screen columns used */

        // Flags for level map
        public const int F_PASS = 0x80; /* is a passageway */
        public const int F_SEEN = 0x40; /* have seen this spot before */
        public const int F_DROPPED = 0x20; /* object was dropped here */
        public const int F_LOCKED = 0x20; /* door is locked */
        public const int F_REAL = 0x10; /* what you see is what you get */
        public const int F_PNUM = 0x0f; /* passage number mask */
        public const int F_TMASK = 0x07; /* trap number mask */

        public static int ntraps;				/* Number of traps on this level */
        public static Coord Stairs;				/* Location of staircase */
        public static THING StairsThing;        //fixme: replacae of 'Stairs' Coord with this
        public static PLACE[] places = new PLACE[MAXLINES * MAXCOLS];		/* level map */
        public static THING ItemList = null;			/* List of objects on this level */
        public static THING MobList = null;			/* List of monsters on the level */

        public const int MAXROOMS = 9;
        public const int MAXTHINGS = 9; //what's the diff tween <<< things
        public const int MAXOBJ = 9;    //and <<< objects?
        public const int MAXTRAPS = 10;
        public const int AMULETLEVEL = 26;
        public const int MAXPASS = 13; /* upper limit on number of passages */

        public static room oldrp;			/* Roomin(&oldpos) */
        public static room[] rooms = new room[MAXROOMS];
        public static room[] passages = new room[MAXPASS]
        {
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
            new room(Room.ISGONE|Room.ISDARK),
        };

        static Dungeon()
        {
            new_level();
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

        // Find the unclaimed object at y, x
        public static THING find_obj(int y, int x)
        {
            THING obj;

            for (obj = ItemList; obj != null; obj = R14.next(obj))
            {
                if (obj.ItemPos.y == y && obj.ItemPos.x == x)
                    return obj;
            }

            /* NOTREACHED */
            return null;
        }

        // Returns true if it is ok to step on ch
        public static bool walkable___step_ok(char ch) {
            switch (ch) {
                case ' ':
                case '|':
                case '-':
                    return false;
                default:
                    return (!char.IsLetter(ch));
            }
        }

        //find what room coords are in. NULL means they aren't in any
        public static room roomin(Coord cp)
        {
            int fp;

            fp = flat(cp.y, cp.x);
            if ((fp & F_PASS) != 0)
                return passages[fp & F_PNUM];

            foreach (var rp in rooms)
                if (cp.x <= rp.PosUL.x + rp.Size.x && rp.PosUL.x <= cp.x
                    && cp.y <= rp.PosUL.y + rp.Size.y && rp.PosUL.y <= cp.y)
                    return rp;

            R14.AddToLog("in some bizarre place ({0}, {1})", cp.y, cp.x);
            return null;
        }        
        
        public static bool IsFoodScrollOrPotion(int type)
        {
            return type == Char.POTION || type == Char.SCROLL || type == Char.FOOD;
        }

        public static PLACE PlaceAt(int y, int x)
        {
            return (places[(x << 5) + y]);
        }

        public static char GetCharAt(int y, int x)
        {
            return places[(x << 5) + y].p_ch;
        }

        public static void SetCharAt(int y, int x, char ch)
        {
            places[(x << 5) + y].p_ch = ch;
        }

        //GetFloorAt
        static int flat(int y, int x)
        {
            return places[(x << 5) + y].p_flags;
        }

         //SetFloorAt
       static void flat(int y, int x, int flags)
        {
            places[(x << 5) + y].p_flags = flags;
        }

        public static THING GetMonster(int y, int x)
        {
            return places[(x << 5) + y].p_monst;
        }

        public static void SetMonster(int y, int x, THING monst) {
			S.Log("SetMonster() NEEDS TO BE REWRITTEN!!!!!!!!!!!!!!!!!!");
			return;




//            places[(x << 5) + y].p_monst = monst;
//
//            /*-----make physics version----- */ 
//            //var diagOffs = S.CubeSpan * Dungeon.CUBES_ACROSS_CHAR / 2; //diagonal offset to middle of a char's space
//            S.Phys.MakeMob(PhysicsType.Jumper, 
//                GetCubeY(y),
//                GetCubeX(x),
//                S.CubeSpan*2, 
//                S.Rend.Tiles["monsters/" + monst.MonsType],
//                monst);
//            S.Phys.MakeMob(PhysicsType.Jumper, 
//                GetCubeY(y),
//                GetCubeX(x),
//                S.CubeSpan*2, 
//                S.Rend.Tiles["monsters/" + monst.MonsType],
//                monst);
//            S.Phys.MakeMob(PhysicsType.Jumper, 
//                GetCubeY(y),
//                GetCubeX(x),
//                S.CubeSpan*2, 
//                S.Rend.Tiles["monsters/" + monst.MonsType],
//                monst);
//            S.Phys.MakeMob(PhysicsType.Jumper, 
//                GetCubeY(y),
//                GetCubeX(x),
//                S.CubeSpan*2, 
//                S.Rend.Tiles["monsters/" + monst.MonsType],
//                monst);
        }

        static public bool HeroCanSee(int y, int x)
        {
            room rer;
            var tp = new Coord();

            if (R14.on(Agent.Plyr, Mob.ISBLIND))
            return false;
            if (dist(y, x, R14.hero.y, R14.hero.x) < R14.LAMPDIST)
            {
                if ((flat(y, x) & F_PASS) != 0)
                    if 
                    (   y != R14.hero.y && 
                        x != R14.hero.x && 
                        !walkable___step_ok(GetCharAt(y, R14.hero.x)) && 
                        !walkable___step_ok(GetCharAt(R14.hero.y, x))
                    )
                        return false;

                return true;
            }

            //we can only see if the hero in the same room as the coordinate and the room is lit or if it is close.
            tp.y = y;
            tp.x = x;
            return (bool)((rer = roomin(tp)) == Agent.Plyr.CurrRoom && !((rer.Flags & Room.ISDARK) != 0));
        }

        // Drop an item someplace around here.
        static public void fall(THING obj, bool pr)
        {
            S.Log("fall() called, but commented out");
            //PLACE pp;
            //var fpos = new Coord();

            //if (PickRndPosAroundYX(obj.ItemPos, fpos))
            //{
            //    pp = PlaceAt(fpos.y, fpos.x);
            //    pp.p_ch = (char)obj.ItemType;
            //    obj.ItemPos = fpos;
            //    if (HeroCanSee(fpos.y, fpos.x))
            //    {
            //        if (pp.p_monst != null)
            //            pp.p_monst.t_oldch = (char)obj.ItemType;
            //        else
            //            R12.MvAddCh(fpos.y, fpos.x, obj.ItemType);
            //    }
            //    R14.attach(ref ItemList, obj);
            //    return;
            //}
            //if (pr)
            //{
            //    if (has_hit)
            //    {
            //        waitForSpaceLeavingPrevMsgVis___endmsg();
            //        has_hit = false;
            //    }
            //    R14.AddToLog("the {0} vanishes as it hits the ground",
            //    Weapon.Data[obj.Which].Name);
            //}
        }

        // Should we show the floor in her room at this time?
        static public bool show_floor()
        {
            if ((Agent.Plyr.CurrRoom.Flags & (Room.ISGONE | Room.ISDARK)) == Room.ISDARK && !R14.on(Agent.Plyr, Mob.ISBLIND))
                return R14.see_floor;
            else
                return true;
        }

        //aggravate all the monsters on this level
        public static void aggravate()
        {
            THING mp;

            for (mp = MobList; mp != null; mp = R14.next(mp))
                S.Log("*** NEED TO DO: 'runto(mp.t_pos)' in aggravate()");
        }

        //public static char winat(int y, int x)
        public static char GetMonsterAppearance(int y, int x)
        {
            THING m = GetMonster(y, x);
            return (m != null ? m.t_disguise : GetCharAt(y, x));
        }
    }