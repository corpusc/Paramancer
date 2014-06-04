using System;



    static partial class Dungeon
    {
        class rdes
        {
            public bool[] conn; /* possible to connect to room i? */
            public bool[] isconn; /* connection been made to room i? */
            public bool ingraph; /* this room in graph already? */

            public rdes(bool[] conn, bool[] isconn, bool ingraph)
            {
                this.conn = conn;
                this.isconn = isconn;
                this.ingraph = ingraph;
            }
        }

        // Draw all the passages on a level.
        public static void do_passages()
        {
            rdes r1, r2 = null;
            int r1num, r2num = 0;
            int i, j;
            int roomcount;
            rdes[] rdes = new[] {
                new rdes( new[] { false,  true, false,  true, false, false, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] {  true, false,  true, false,  true, false, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false,  true, false, false, false,  true, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] {  true, false, false, false,  true, false,  true, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false,  true, false,  true, false,  true, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false, false,  true, false,  true, false, false, false,  true }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false, false, false,  true, false, false, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false, false, false, false,  true, false,  true, false,  true }, new[] { false, false, false, false, false, false, false, false, false }, false ),
                new rdes( new[] { false, false, false, false, false,  true, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            };

            /*
             * starting with one room, connect it to a random adjacent room and
             * then pick a new room to start with.
             */
            roomcount = 1;
            r1num = R14.rnd(MAXROOMS);
            r1 = rdes[r1num];
            r1.ingraph = true;
            do
            {
                /*
                 * find a room to connect with
                 */
                j = 0;
                for (i = 0; i < MAXROOMS; i++)
                    if (r1.conn[i] && !rdes[i].ingraph && R14.rnd(++j) == 0)
                    {
                        r2num = i;
                        r2 = rdes[i];
                    }
                /*
                 * if no adjacent rooms are outside the graph, pick a new room
                 * to look from
                 */
                if (j == 0)
                {
                    do
                    {
                        r1num = R14.rnd(MAXROOMS);
                        r1 = rdes[r1num];
                    } while (!r1.ingraph);
                }
                /*
                 * otherwise, connect new room to the graph, and draw a tunnel
                 * to it
                 */
                else
                {
                    r2.ingraph = true;
                    i = r1num;
                    j = r2num;
                    conn(i, j);
                    r1.isconn[j] = true;
                    r2.isconn[i] = true;
                    roomcount++;
                }
            } while (roomcount < MAXROOMS);

            /*
             * attempt to add passages to the graph a random number of times so
             * that there isn't always just one unique passage through it.
             */
            for (roomcount = R14.rnd(5); roomcount > 0; roomcount--)
            {
                r1num = R14.rnd(MAXROOMS);
                r1 = rdes[r1num];	/* a random room to look from */
                /*
                 * find an adjacent room not already connected
                 */
                j = 0;
                for (i = 0; i < MAXROOMS; i++)
                    if (r1.conn[i] && !r1.isconn[i] && R14.rnd(++j) == 0)
                    {
                        r2num = i;
                        r2 = rdes[i];
                    }
                /*
                 * if there is one, connect it and look for the next added
                 * passage
                 */
                if (j != 0)
                {
                    i = r1num;
                    j = r2num;
                    conn(i, j);
                    r1.isconn[j] = true;
                    r2.isconn[i] = true;
                }
            }

            passnum();
        }

        static Coord del = new Coord();
        static Coord curr = new Coord();
        static Coord turn_delta = new Coord();
        static Coord spos = new Coord();
        static Coord epos = new Coord();

        // Draw a corridor from a room in a certain direction.
        static void conn(int r1, int r2)
        {
            room rpf, rpt = null;
            int rmt;
            int distance = 0, turn_spot, turn_distance = 0;
            int rm;
            char direc;

            if (r1 < r2)
            {
                rm = r1;
                if (r1 + 1 == r2)
                    direc = 'r';
                else
                    direc = 'd';
            }
            else
            {
                rm = r2;
                if (r2 + 1 == r1)
                    direc = 'r';
                else
                    direc = 'd';
            }
            rpf = rooms[rm];

            /*
             * Set up the movement variables, in two cases:
             * first drawing one down.
             */
            if (direc == 'd')
            {
                rmt = rm + 3;				/* room # of dest */
                rpt = rooms[rmt];			/* room pointer of dest */
                del.x = 0;				/* direction of move */
                del.y = 1;
                spos.x = rpf.PosUL.x;			/* start of move */
                spos.y = rpf.PosUL.y;
                epos.x = rpt.PosUL.x;			/* end of move */
                epos.y = rpt.PosUL.y;
                if (!((rpf.Flags & Room.ISGONE) == Room.ISGONE))		/* if not gone pick door pos */
                    do
                    {
                        spos.x = rpf.PosUL.x + R14.rnd(rpf.Size.x - 2) + 1;
                        spos.y = rpf.PosUL.y + rpf.Size.y - 1;
                    } while (((rpf.Flags & Room.ISMAZE) == Room.ISMAZE) && !((flat(spos.y, spos.x) & F_PASS) == F_PASS));

                if (!((rpt.Flags & Room.ISGONE) == Room.ISGONE))
                    do
                    {
                        epos.x = rpt.PosUL.x + R14.rnd(rpt.Size.x - 2) + 1;
                    } while (((rpt.Flags & Room.ISMAZE) == Room.ISMAZE) && !((flat(epos.y, epos.x) & F_PASS) == F_PASS));

                distance = Math.Abs(spos.y - epos.y) - 1;	/* distance to move */
                turn_delta.y = 0;			/* direction to turn */
                turn_delta.x = (spos.x < epos.x ? 1 : -1);
                turn_distance = Math.Abs(spos.x - epos.x);	/* how far to turn */
            }
            else if (direc == 'r')			/* setup for moving right */
            {
                rmt = rm + 1;
                rpt = rooms[rmt];
                del.x = 1;
                del.y = 0;
                spos.x = rpf.PosUL.x;
                spos.y = rpf.PosUL.y;
                epos.x = rpt.PosUL.x;
                epos.y = rpt.PosUL.y;
                if (!((rpf.Flags & Room.ISGONE) == Room.ISGONE))
                    do
                    {
                        spos.x = rpf.PosUL.x + rpf.Size.x - 1;
                        spos.y = rpf.PosUL.y + R14.rnd(rpf.Size.y - 2) + 1;
                    } while (((rpf.Flags & Room.ISMAZE) == Room.ISMAZE) && !((flat(spos.y, spos.x) & F_PASS) == F_PASS));
                if (!((rpt.Flags & Room.ISGONE) == Room.ISGONE))
                    do
                    {
                        epos.y = rpt.PosUL.y + R14.rnd(rpt.Size.y - 2) + 1;
                    } while (((rpt.Flags & Room.ISMAZE) == Room.ISMAZE) && !((flat(epos.y, epos.x) & F_PASS) == F_PASS));
                distance = Math.Abs(spos.x - epos.x) - 1;
                turn_delta.y = (spos.y < epos.y ? 1 : -1);
                turn_delta.x = 0;
                turn_distance = Math.Abs(spos.y - epos.y);
            }

            turn_spot = R14.rnd(distance - 1) + 1;		/* where turn starts */

            /*
             * Draw in the doors on either side of the passage or just put #'s
             * if the rooms are gone.
             */
            if (!((rpf.Flags & Room.ISGONE) == Room.ISGONE))
                door(rpf, spos);
            else
                putpass(spos);
            if (!((rpt.Flags & Room.ISGONE) == Room.ISGONE))
                door(rpt, epos);
            else
                putpass(epos);
            /*
             * Get ready to move...
             */
            curr.x = spos.x;
            curr.y = spos.y;
            while (distance > 0)
            {
                /*
                 * Move to new pos
                 */
                curr.x += del.x;
                curr.y += del.y;
                /*
                 * Check if we are at the turn place, if so do the turn
                 */
                if (distance == turn_spot)
                    while (turn_distance-- > 0)
                    {
                        putpass(curr);
                        curr.x += turn_delta.x;
                        curr.y += turn_delta.y;
                    }
                /*
                 * Continue digging along
                 */
                putpass(curr);
                distance--;
            }
            curr.x += del.x;
            curr.y += del.y;
            if (!TheseShareCoords(curr, epos))
                R14.AddToLog("warning, connectivity problem on this level");
        }

        //public static bool ce(Coord a, Coord b)
        public static bool TheseAreTheSame(Coord a, Coord b)
        {
            return ((a).x == (b).x && (a).y == (b).y);
        }

        public static bool TheseShareCoords(Coord a, Coord b)
        {
            return ((a).x == (b).x && (a).y == (b).y);
        }

        // add a passage character or secret passage here
        static void putpass(Coord c)
        {
            PLACE pp;

            pp = PlaceAt(c.y, c.x);
            pp.p_flags |= F_PASS;
            if (R14.rnd(10) + 1 < Agent.LevelOfMap && R14.rnd(40) == 0)
                pp.p_flags &= ~F_REAL;
            else
                pp.p_ch = Char.PASSAGE;
        }

        // Add a door or possibly a secret door.  Also enters the door in
        // the exits array of the room.
        static void door(room rm, Coord cp)
        {
            PLACE pp;

            rm.Exits[rm.NumExits++] = cp;

            if ((rm.Flags & Room.ISMAZE) == Room.ISMAZE)
                return;

            pp = PlaceAt(cp.y, cp.x);
            if (R14.rnd(10) + 1 < Agent.LevelOfMap && R14.rnd(5) == 0)
            {
                if (cp.y == rm.PosUL.y || cp.y == rm.PosUL.y + rm.Size.y - 1)
                    pp.p_ch = '-';
                else
                    pp.p_ch = '|';
                pp.p_flags &= ~Dungeon.F_REAL;
            }
            else
                pp.p_ch = Char.DOOR;
        }

        static int pnum;
        static bool newpnum;

        // Assign a number to each passageway
        static void passnum()
        {
            int i;

            pnum = 0;
            newpnum = false;
            foreach (var rp in passages)
                rp.NumExits = 0;
            foreach (var rp in rooms)
                for (i = 0; i < rp.NumExits; i++)
                {
                    newpnum = true;
                    numpass(rp.Exits[i].y, rp.Exits[i].x);
                }
        }

        // Number a passageway square and its brethren
        static void numpass(int y, int x)
        {
            int fp;
            room rp;
            char ch;

            if (x >= R14.NUMCOLS || x < 0 || y >= R14.NUMLINES || y <= 0)
                return;
            fp = flat(y, x);
            if ((fp & F_PNUM) != 0)
                return;
            if (newpnum)
            {
                pnum++;
                newpnum = false;
            }
            /*
             * check to see if it is a door or secret door, i.e., a new exit,
             * or a numerable type of place
             */
            if ((ch = GetCharAt(y, x)) == Char.DOOR ||
                (!((fp & F_REAL) == Dungeon.F_REAL) && (ch == '|' || ch == '-')))
            {
                rp = passages[pnum];
                rp.Exits[rp.NumExits].y = y;
                rp.Exits[rp.NumExits++].x = x;
            }
            else if (!((fp & F_PASS) == F_PASS))
                return;
            fp |= pnum;
            flat(y, x, fp);
            /*
             * recurse on the surrounding places
             */
            numpass(y + 1, x);
            numpass(y - 1, x);
            numpass(y, x + 1);
            numpass(y, x - 1);
        }
    }