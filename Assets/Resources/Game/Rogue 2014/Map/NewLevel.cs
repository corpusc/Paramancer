using System;
using System.Collections.Generic;



    public static partial class Dungeon {
        const int TREAS_ROOM = 20;	/* one chance in TREAS_ROOM for a treasure room */
        const int MAXTREAS = 10;	/* maximum number of treasures in a treasure room */
        const int MINTREAS = 2;	/* minimum number of treasures in a treasure room */

        public static void new_level() {
			S.Log("Generating a dungeon map.... ( new_level() )");
            THING t;
            int i;

            /* unhold player when you go down.  just in case */
            Agent.Plyr.t_flags &= ~Mob.ISHELD;	
            if (Agent.dyn___max_level < Agent.LevelOfMap)
                Agent.dyn___max_level = Agent.LevelOfMap;

            // clean things off from last level 
            for (i = 0; i < places.Length; i++ )
            {
                var pp = new PLACE();
                places[i] = pp;
                pp.p_ch = ' ';
                pp.p_flags = F_REAL;
                pp.p_monst = null;
            }

            // clear lists 
            MobList = null;
            ItemList = null;

            do_rooms();
            do_passages();
            Agent.no_food++;
            put_things();
            
            // make traps 
            if (R14.rnd(10) < Agent.LevelOfMap)     {
                ntraps = R14.rnd(Agent.LevelOfMap / 4) + 1;

                if (ntraps > MAXTRAPS)
                    ntraps = MAXTRAPS;
                
                i = ntraps;
                while (i-- > 0) {
                    /* not only wouldn't it be NICE to have traps in mazes, since the trap
                     * number is stored where the passage number is, we can't actually do it.
                     */
                    do {
                        find_floor(null, out Stairs, 0, false);
                    } while (GetCharAt(Stairs.y, Stairs.x) != Char.FLOOR);

                    int sp = flat(Stairs.y, Stairs.x);
                    sp &= ~F_REAL;
                    sp |= R14.rnd(NTRAPS);
                    flat(Stairs.y, Stairs.x, sp);
                }
            }

            // make staircase down 
            find_floor(null, out Stairs, 0, false);
            SetCharAt(Stairs.y, Stairs.x, Char.STAIRS);
            Agent.seenstairs = false;
            StairsThing = new THING();
            StairsThing.ItemPos = Stairs;
            StairsThing.ItemType = Char.STAIRS;
            StairsThing.Name = Item.GetDescription(StairsThing);
			S.Log("makeOneSensor(StairsThing, Item.GetItemTile(StairsThing) );");

            
            S.Log("made traps and stairs");


            for (t = MobList; t != null; t = R14.next(t)) {
                t.CurrRoom = roomin(t.t_pos);
            }

            // place player in a room 
            Coord heroCoord;
            find_floor(null, out heroCoord, 0, true);
            R14.hero = heroCoord;
            enter_room(R14.hero);
            int offs = 2; // offset from char origin, since mobs origin is at their center 
			S.Log("wanted to make physical player here ************************");
			//S.Phys.MakePlayer(
                //offs + GetCubeY(R14.hero.y),
                //offs + GetCubeX(R14.hero.x) );


            if (R14.on(Agent.Plyr, Mob.CanSeeInvisible))
                Potion.turn_see(0);
            if (R14.on(Agent.Plyr, Mob.IsTrippinBalls))
         		R14.visuals(0);


            S.Log("Converting ASCII to cubes");
            setupCubeVisualsAndPhysicality();
        }

        static void setupCubeVisualsAndPhysicality() {
			S.Log("setupCubeVisualsAndPhysicality() ------- NEEDS EDITING!!!!!!!!!!!!");

//            for (int y = 0; y < MAXLINES; y++)
//            for (int x = 0; x < MAXCOLS; x++)
//            {
//                TileType tile = TileType.empty_block;
//                PLACE p = PlaceAt(y, x);
//                    
//                switch (p.p_ch)
//                {
//                    case Char.WALL_VERT:
//                    case Char.WALL_HORI:
//                        tile = TileType.rock;
//                        break;
//
//                    case Char.PASSAGE:
//                    case Char.DOOR:
//                        tile = TileType.regolith_surface;
//                        break;
//                        
//                    case Char.FLOOR:
//                        tile = TileType.regolith;
//                        break;
//                       
//                    case Char.TRAP:
//                    case Char.STAIRS:
//                    case Char.GOLD:
//                    case Char.POTION:
//                    case Char.SCROLL:
//                    case Char.MAGIC:
//                    case Char.FOOD:
//                    case Char.WEAPON:
//                    case Char.ARMOUR:
//                    case Char.AMULET:
//                    case Char.RING:
//                    case Char.STICK:
//                        tile = TileType.iridium_rock;
//                        break;
//                }
//
//                Coord offsetInCubeSpace = new Coord(GetCubeY(y), GetCubeX(x) );
//
//                switch (tile)
//                {
//                    case TileType.regolith_surface:
//                        //only make one side wall
//                        if (!isTraversable(y, x-1) )
//                        {
//                            Coord size = new Coord(CUBES_ACROSS_CHAR, 1);
//                            S.Phys.MakeRectInCubeSpace(offsetInCubeSpace.x, offsetInCubeSpace.y, size);
//                            S.map.set(offsetInCubeSpace.x, offsetInCubeSpace.y, size, (int)tile);
//                        }
//                        else       
//                        if (!isTraversable(y, x+1) )
//                        {
//                            Coord size = new Coord(CUBES_ACROSS_CHAR, 1);
//                            S.Phys.MakeRectInCubeSpace(offsetInCubeSpace.x+CUBES_ACROSS_CHAR-1, offsetInCubeSpace.y, size);
//                            /* */ S.map.set(offsetInCubeSpace.x+CUBES_ACROSS_CHAR-1, offsetInCubeSpace.y, size, (int)tile);
//                        }
//
//                        //make passage floor if no traversable space underneath us
//                        if (!isTraversable(y-1, x) )
//                        {
//                            Coord size = new Coord(1, CUBES_ACROSS_CHAR);
//                            S.Phys.MakeRectInCubeSpace(offsetInCubeSpace.x, offsetInCubeSpace.y, size);
//                            S.map.set(offsetInCubeSpace.x, offsetInCubeSpace.y, size, (int)tile);
//                        }
//                        break;
//                    default:
//                        //make all the cubes that occupy this char space
//                        for (int iy = 0; iy < CUBES_ACROSS_CHAR; iy++)
//                        for (int ix = 0; ix < CUBES_ACROSS_CHAR; ix++)
//                        {
//                            if (tile == TileType.rock && ix == 0 && iy == 0) //just make one physical object to cover whole char space
//                                S.Phys.MakeRectInCubeSpace(
//                                    offsetInCubeSpace.x + ix, 
//                                    offsetInCubeSpace.y + iy, 
//                                    new Coord(CUBES_ACROSS_CHAR,CUBES_ACROSS_CHAR) );
//
//                            S.map.set(offsetInCubeSpace.x + ix, offsetInCubeSpace.y + iy, (int)tile);
//                        }
//                        break;
//                }
//            }
//
//            // make items sensors (not obstructing mob movement) 
//            for (THING t = ItemList; t != null; t = R14.next(t) )
//            {
//                t.Name = Item.GetDescription(t);
//                makeOneSensor(t, Item.GetItemTile(t) );
//            }
        }

//        static void makeOneSensor(THING t, int frame = 0)
//        {
//            //offset by 1 cube, to place in center of ASCII char space
//            S.Phys.MakeRectInCubeSpace(
//                GetCubeX(t.ItemPos.x) + 1,
//                GetCubeY(t.ItemPos.y) + 1, new Coord(3, 3), t, frame, true);
//        }

//        static bool isTraversable(int y, int x)
//        {
//            if (y < 0 || y >= S.map.ydim ||
//                x < 0 || x >= S.map.xdim)
//                return false;
//
//            return 
//                PlaceAt(y, x).p_ch != Char.WALL_HORI &&
//                PlaceAt(y, x).p_ch != Char.WALL_VERT && 
//                PlaceAt(y, x).p_ch != ' ';
//        }

        static public int GetCubeY(int i)
        {
            return i * CUBES_ACROSS_CHAR + CUBES_UP_FROM_BOTTOM;
        }
        static public int GetCubeX(int i)
        {
            return i * CUBES_ACROSS_CHAR;
        }

        // Pick a room that is really there 
        public static int rnd_room() {
            int rm;

            do {
                rm = R14.rnd(MAXROOMS);
            } while ((rooms[rm].Flags & Room.ISGONE) == Room.ISGONE);

            return rm;
        }

        // populate map with items 
        public static void put_things() {
            int i;
            THING obj;

            /* Once you have found the amulet, the only way to get new stuff is go down into the dungeon */
            if (Agent.amulet && Agent.LevelOfMap < Agent.dyn___max_level)
                return;
            /* check for treasure rooms, and if so, put it in */
            if (R14.rnd(TREAS_ROOM) == 0)
                treas_room();
            
            // make items 
            for (i = 0; i < MAXOBJ; i++) {
                int roll = R14.rnd(100);
                //Console.WriteLine("rolled " + roll);
                if (roll < 36) {
                    obj = R14.GetNewRandomThing();
                    R14.attach(ref Dungeon.ItemList, obj);
                    find_floor(null, out obj.ItemPos, 0, false);
                    SetCharAt(obj.ItemPos.y, obj.ItemPos.x, (char)obj.ItemType);
                }
            }

            // place amulet... if far in enough & hasn't found 
            if (Agent.LevelOfMap >= AMULETLEVEL && !Agent.amulet) {
                obj = new THING();
                R14.attach(ref ItemList, obj);
                obj.PlusToHit = 0;
                obj.PlusToDmg = 0;
                obj.swing___o_damage = "0x0";
                obj.o_hurldmg = "0x0";
                obj.Ac = 11;
                obj.ItemType = Char.AMULET;
                /*
                 * Put it somewhere
                 */
                find_floor(null, out obj.ItemPos, 0, false);
                Dungeon.SetCharAt(obj.ItemPos.y, obj.ItemPos.x, Char.AMULET);
            }
        }

        public const int MAXTRIES = 10;	/* max number of tries to put down a monster */

        // Add a treasure room 
        public static void treas_room() {
            int nm;
            THING tp;
            room rp;
            int spots, num_monst;
            var mp = new Coord();

            rp = rooms[rnd_room()];
            spots = (rp.Size.y - 2) * (rp.Size.x - 2) - MINTREAS;
            if (spots > (MAXTREAS - MINTREAS))
                spots = (MAXTREAS - MINTREAS);
            num_monst = nm = R14.rnd(spots) + MINTREAS;
            
			while (nm-- != 0) {
                find_floor(rp, out mp, 2 * MAXTRIES, false);
                tp = R14.GetNewRandomThing();
                tp.ItemPos = mp;
                R14.attach(ref Dungeon.ItemList, tp);
                Dungeon.SetCharAt(mp.y, mp.x, (char)tp.ItemType);
            }

            //fill up room with monsters from the next level down
            if ((nm = R14.rnd(spots) + MINTREAS) < num_monst + 2)
                nm = num_monst + 2;
            spots = (rp.Size.y - 2) * (rp.Size.x - 2);
            if (nm > spots)
                nm = spots;

            Agent.LevelOfMap++;
            
			while (nm-- != 0) {
                spots = 0;

                if (find_floor(rp, out mp, MAXTRIES, true)) {
                    tp = new THING();
                    R14.new_monster(tp, R14.randmonster(false), mp);
                    tp.t_flags |= Mob.ISMEAN;	/* no sloughers in THIS room */
                    R14.give_pack(tp);
                }
            }

            Agent.LevelOfMap--;
        }
    }