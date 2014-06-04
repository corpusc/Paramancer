


    public class room
    {
        public Coord PosUL = new Coord();      /* Upper left corner */
        public Coord Size = new Coord();
        public Coord GoldPos = new Coord();
        public int GoldVal;                  /* How much the gold is worth */
        public int Flags;                    /* info about the room */
        public int NumExits;                   /* Number of exits */
        public Coord[] Exits = new Coord[12]; /* Where the exits are */

        public room()
        {
            for (int i = 0; i < Exits.Length; i++)
            {
                Exits[i] = new Coord();
            }
        }

        public room(int flags) : this()
        {
            Flags = flags;
        }

        public bool IsGone { get { return (this.Flags & Room.ISGONE) != 0; } }
        public bool IsDark { get { return (this.Flags & Room.ISDARK) != 0; } }
        public bool IsMaze { get { return (this.Flags & Room.ISMAZE) != 0; } }
    }

    static partial class Dungeon
    {
        class SPOT
        {		/* pos matrix for maze positions */
            public int nexits;
            public Coord[] exits = new Coord[4];
            public int used;
        }

        const int GOLDGRP = 1;

        // Create rooms and corridors with a connectivity graph
        static public void do_rooms()
        {
            int i;
            int left_out;
            THING t;
            Coord top = new Coord();
            Coord maxSize = new Coord();
            Coord mp;

            maxSize.x = R14.NUMCOLS / 3;
            maxSize.y = R14.NUMLINES / 3;
            
            //clear things for a new level
            for (i = 0; i < rooms.Length; i++)
            {
                var r = new room();
                r.GoldVal = 0;
                r.NumExits = 0;
                r.Flags = 0;
                rooms[i] = r;
            }
            
            //put the gone rooms, if any, on the level
            left_out = R14.rnd2(4, "gone rooms");
            for (i = 0; i < left_out; i++)
                rooms[rnd_room()].Flags |= Room.ISGONE;
            
            //make and populate all the rooms on the level
            for (i = 0; i < MAXROOMS; i++)
            {
                var r = rooms[i];
                //find upper left corner of box that this room goes in
                top.x = (i % 3) * maxSize.x + 1;
                top.y = (i / 3) * maxSize.y;
                if ((r.Flags & Room.ISGONE) == Room.ISGONE)
                {
                    //make gone room.  Make certain that there is a blank line for passage drawing
                    do
                    {
                        r.PosUL.x = top.x + R14.rnd(maxSize.x - 2) + 1;
                        r.PosUL.y = top.y + R14.rnd(maxSize.y - 2) + 1;
                        r.Size.x = -R14.NUMCOLS;
                        r.Size.y = -R14.NUMLINES;
                    } while (!(r.PosUL.y > 0 && r.PosUL.y < R14.NUMLINES - 1));

                    continue;
                }

                /*
                 * set room type
                 */
                if (R14.rnd2(10, "is dark room") < Agent.LevelOfMap - 1)
                {
                    r.Flags |= Room.ISDARK;		/* dark room */
                    if (R14.rnd2(15, "is maze room") == 0)
                        r.Flags = Room.ISMAZE;		/* maze room */
                }

                /*
                 * Find a place and size for a random room
                 */
                if ((r.Flags & Room.ISMAZE) == Room.ISMAZE)
                {
                    r.Size.x = maxSize.x - 1;
                    r.Size.y = maxSize.y - 1;
                    if ((r.PosUL.x = top.x) == 1)
                        r.PosUL.x = 0;
                    if ((r.PosUL.y = top.y) == 0)
                    {
                        r.PosUL.y++;
                        r.Size.y--;
                    }
                }
                else
                    do
                    {
                        r.Size.x = R14.rnd(maxSize.x - 4) + 4;
                        r.Size.y = R14.rnd(maxSize.y - 4) + 4;
                        r.PosUL.x = top.x + R14.rnd(maxSize.x - r.Size.x);
                        r.PosUL.y = top.y + R14.rnd(maxSize.y - r.Size.y);
                    } while (!(r.PosUL.y != 0));
                draw_room(r);
                
                /*
                 * Put the gold in
                 */
                if (R14.rnd2(2, "has gold") == 0 && (!Agent.amulet || Agent.LevelOfMap >= Agent.dyn___max_level))
                {
                    THING gold;

                    gold = new THING();
                    gold.o_goldval = r.GoldVal = R14.GOLDCALC();
                    find_floor(r, out r.GoldPos, 0, false);
                    gold.ItemPos = r.GoldPos;
                    SetCharAt(r.GoldPos.y, r.GoldPos.x, Char.GOLD);
                    gold.o_flags = Item.ISMANY;
                    gold.o_group = GOLDGRP;
                    gold.ItemType = Char.GOLD;
                    R14.attach(ref ItemList, gold);
                }
                
                /*
                 * Put the monster in
                 */
                if (R14.rnd2(100, "has monster") < (r.GoldVal > 0 ? 80 : 25))
                {
                    t = new THING();
                    find_floor(r, out mp, 0, true);
                    R14.new_monster(t, R14.randmonster(false), mp);
                    R14.give_pack(t);
                }
            }
        }

        // Draw a box around a room and lay down the floor for normal
        // rooms; for maze rooms, draw maze.
        static void draw_room(room r)
        {
            int y, x;

            if ((r.Flags & Room.ISMAZE) == Room.ISMAZE)
                do_maze(r);
            else
            {
                vert(r, r.PosUL.x);				/* Draw left side */
                vert(r, r.PosUL.x + r.Size.x - 1);	/* Draw right side */
                horiz(r, r.PosUL.y);				/* Draw top */
                horiz(r, r.PosUL.y + r.Size.y - 1);	/* Draw bottom */

                /*
                 * Put the floor down
                 */
                for (y = r.PosUL.y + 1; y < r.PosUL.y + r.Size.y - 1; y++)
                    for (x = r.PosUL.x + 1; x < r.PosUL.x + r.Size.x - 1; x++)
                        SetCharAt(y, x, Char.FLOOR);
            }
        }

        // Draw a vertical line
        static void vert(room rp, int startx)
        {
            int y;

            for (y = rp.PosUL.y + 1; y <= rp.Size.y + rp.PosUL.y - 1; y++)
                SetCharAt(y, startx, '|');
        }

        // Draw a horizontal line
        static void horiz(room rp, int starty)
        {
            int x;

            for (x = rp.PosUL.x; x <= rp.PosUL.x + rp.Size.x - 1; x++)
                SetCharAt(starty, x, '-');
        }

        static int Maxy, Maxx, Starty, Startx;
        static SPOT[,] maze = new SPOT[R14.NUMLINES / 3 + 1, R14.NUMCOLS / 3 + 1];

        // Dig a maze
        static void do_maze(room rp)
        {
            S.Log("do_maze()");
            //SPOT sp;
            int starty, startx;
            Coord pos = new Coord();

            for (int i = 0; i < R14.NUMLINES / 3; i++)
                for (int j = 0; j < R14.NUMCOLS / 3; j++)
                {
                    maze[i, j] = new SPOT();

                }

            Maxy = rp.Size.y;
            Maxx = rp.Size.x;
            Starty = rp.PosUL.y;
            Startx = rp.PosUL.x;
            starty = (R14.rnd(rp.Size.y) / 2) * 2;
            startx = (R14.rnd(rp.Size.x) / 2) * 2;
            pos.y = starty + Starty;
            pos.x = startx + Startx;
            putpass(pos);
            dig(starty, startx);
        }

        // Dig out from around where we are now, if possible
        static void dig(int y, int x)
        {
            Coord cp;
            int cnt, newy, newx, nexty = 0, nextx = 0;
            Coord pos = new Coord();
            Coord[] del = new Coord[4] {
                new Coord(2, 0),
                new Coord(-2, 0),
                new Coord(0, 2),
                new Coord(0, -2)
            };

            for (; ; )
            {
                cnt = 0;
                for (int i = 0; i < del.Length; i++)
                {
                    cp = del[i];
                    newy = y + cp.y;
                    newx = x + cp.x;
                    if (newy < 0 || newy > Maxy || newx < 0 || newx > Maxx)
                        continue;
                    if ((flat(newy + Starty, newx + Startx) & F_PASS) != 0)
                        continue;
                    if (R14.rnd(++cnt) == 0)
                    {
                        nexty = newy;
                        nextx = newx;
                    }
                }
                if (cnt == 0)
                    return;
                accnt_maze(y, x, nexty, nextx);
                accnt_maze(nexty, nextx, y, x);
                if (nexty == y)
                {
                    pos.y = y + Starty;
                    if (nextx - x < 0)
                        pos.x = nextx + Startx + 1;
                    else
                        pos.x = nextx + Startx - 1;
                }
                else
                {
                    pos.x = x + Startx;
                    if (nexty - y < 0)
                        pos.y = nexty + Starty + 1;
                    else
                        pos.y = nexty + Starty - 1;
                }
                putpass(pos);
                pos.y = nexty + Starty;
                pos.x = nextx + Startx;
                putpass(pos);
                dig(nexty, nextx);
            }
        }

        // Account for maze exits
        static void accnt_maze(int y, int x, int ny, int nx)
        {
            SPOT sp;
            Coord cp = new Coord();

            sp = maze[y, x];

            for (int i = 0; i < sp.nexits; i++)
            {
                cp = sp.exits[i];
                if (cp.y == ny && cp.x == nx)
                    return;
            }

            cp.y = ny;
            cp.x = nx;
        }

        // Pick a random spot in a room
        static void rnd_pos(room rp, out Coord cp)
        {
            cp = new Coord();
            cp.x = rp.PosUL.x + R14.rnd(rp.Size.x - 2) + 1;
            cp.y = rp.PosUL.y + R14.rnd(rp.Size.y - 2) + 1;
        }

        // Find a valid floor spot in this room.  If rp is NULL, then
        // pick a new room each time around the loop.
        public static bool find_floor(room rp, out Coord cp, int limit, bool monst)
        {
            cp = new Coord();

            PLACE pp;
            int cnt;
            char compchar = '\0';
            bool pickroom;

            pickroom = (bool)(rp == null);

            if (!pickroom)
                compchar = (((rp.Flags & Room.ISMAZE) == Room.ISMAZE) ? Char.PASSAGE : Char.FLOOR);
            cnt = limit;
            for (; ; )
            {
                if (limit != 0 && cnt-- == 0)
                    return false;
                if (pickroom)
                {
                    rp = rooms[rnd_room()];
                    compchar = (((rp.Flags & Room.ISMAZE) == Room.ISMAZE) ? Char.PASSAGE : Char.FLOOR);
                }
                rnd_pos(rp, out cp);
                pp = PlaceAt(cp.y, cp.x);
                if (monst)
                {
                    if (pp.p_monst == null && walkable___step_ok(pp.p_ch))
                        return true;
                }
                else if (pp.p_ch == compchar)
                    return true;
            }
        }

        //executed whenever you appear in a room
        public static void enter_room(Coord cp)
        {
            S.Log("enter_room() needs fleshing out");
            //room rp;
            //THING tp;
            //int y, x;
            //char ch;

            //rp = proom = roomin(cp);
            //door_open(rp);
            //if (!((rp.Flags & ISDARK) == ISDARK) && !on(player, ISBLIND))
            //    for (y = rp.PosUL.y; y < rp.Size.y + rp.PosUL.y; y++)
            //    {
            //        R12.move(y, rp.PosUL.x);
            //        for (x = rp.PosUL.x; x < rp.Size.x + rp.PosUL.x; x++)
            //        {
            //            tp = GetMonster(y, x);
            //            ch = chat(y, x);
            //            if (tp == null)
            //                if (CCHAR(R12.inch()) != ch)
            //                    R12.addch(ch);
            //                else
            //                    R12.move(y, x + 1);
            //            else
            //            {
            //                tp.t_oldch = ch;
            //                if (!heroCanSeeMonster(tp))
            //                    if (on(player, CanSeeInvisible))
            //                    {
            //                        R12.standout();
            //                        R12.addch(tp.t_disguise);
            //                        R12.standend();
            //                    }
            //                    else
            //                        R12.addch(ch);
            //                else
            //                    R12.addch(tp.t_disguise);
            //            }
            //        }
            //    }
        }

        // Code for when we exit a room
        public static void leave_room(Coord cp)
        {
            S.Log("leave_room() needs fleshing out");
        //    PLACE pp;
        //    room rp;
        //    int y, x;
        //    char floor;
        //    char ch;

        //    rp = proom;

        //    if ((rp.Flags & ISMAZE) != 0)
        //        return;

        //    if ((rp.Flags & ISGONE) != 0)
        //        floor = PASSAGE;
        //    else if (!((rp.Flags & ISDARK) != 0) || on(player, ISBLIND))
        //        floor = FLOOR;
        //    else
        //        floor = ' ';

        //    proom = passages[flat(cp.y, cp.x) & F_PNUM];
        //    for (y = rp.PosUL.y; y < rp.Size.y + rp.PosUL.y; y++)
        //        for (x = rp.PosUL.x; x < rp.Size.x + rp.PosUL.x; x++)
        //        {
        //            R12.move(y, x);
        //            switch (ch = CCHAR(R12.inch()))
        //            {
        //                case FLOOR:
        //                    if (floor == ' ' && ch != ' ')
        //                        R12.addch(' ');
        //                    break;
        //                default:
        //                    /*
        //                     * to check for monster, we have to strip out
        //                     * standout bit
        //                     */
        //                    if (char.IsUpper(ch))
        //                    {
        //                        if (on(player, CanSeeInvisible))
        //                        {
        //                            R12.standout();
        //                            R12.addch(ch);
        //                            R12.standend();
        //                            break;
        //                        }
        //                        pp = PlaceAt(y, x);
        //                        R12.addch(pp.p_ch == DOOR ? DOOR : floor);
        //                    }
        //                    break;
        //            }
        //        }
        //    door_open(rp);
        }
    }