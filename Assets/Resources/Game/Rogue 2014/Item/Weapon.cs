using System;



    static class Weapon
    {
        // Weapon types
        public const int MACE = 0;
        public const int SWORD = 1;
        public const int BOW = 2;
        public const int ARROW = 3;
        public const int DAGGER = 4;
        public const int TWOSWORD = 5;
        public const int DART = 6;
        public const int SHURIKEN = 7;
        public const int SPEAR = 8;
        public const int FLAME = 9; /* fake entry for dragon breath (ick) */
        public const int MAX = 9; /* this should equal FLAME */

        public static ObjectData[] Data = new[]
        {
            new ObjectData("mace", 11, 8, null, false),
            new ObjectData("long sword", 11, 15, null, false),
            new ObjectData("short bow", 12, 15, null, false),
            new ObjectData("arrow", 12, 1, null, false),
            new ObjectData("dagger", 8, 3, null, false),
            new ObjectData("two handed sword", 10, 75, null, false),
            new ObjectData("dart", 12, 2, null, false),
            new ObjectData("shuriken", 12, 5, null, false),
            new ObjectData("spear", 12, 5, null, false),
            new ObjectData(0), /* DO NOT REMOVE: fake entry for dragon's breath */
        };










        const int NO_WEAPON = -1;
        static int group = 2;

        class init_weaps
        {
            public string iw_dam;	/* Damage when wielded */
            public string iw_hrl;	/* Damage when thrown */
            public int iw_launch;	/* Launching weapon */
            public int iw_flags;	/* Miscellaneous flags */

            public init_weaps(string dam, string hrl, int launch, int flags)
            {
                iw_dam = dam;
                iw_hrl = hrl;
                iw_launch = launch;
                iw_flags = flags;
            }
        }

        static init_weaps[] init_dam = new[]
        {
            new init_weaps("2x4",	"1x3",	NO_WEAPON,	0		),	/* Mace */
            new init_weaps("3x4",	"1x2",	NO_WEAPON,	0		),	/* Long sword */
            new init_weaps("1x1",	"1x1",	NO_WEAPON,	0		),	/* Bow */
            new init_weaps("1x1",	"2x3",	BOW,		Item.ISMANY|Item.ISMISL	),	/* Arrow */
            new init_weaps("1x6",	"1x4",	NO_WEAPON,	Item.ISMISL|Item.ISMISL	),	/* Dagger */
            new init_weaps("4x4",	"1x2",	NO_WEAPON,	0		),	/* 2h sword */
            new init_weaps("1x1",	"1x3",	NO_WEAPON,	Item.ISMANY|Item.ISMISL	),	/* Dart */
            new init_weaps("1x2",	"2x4",	NO_WEAPON,	Item.ISMANY|Item.ISMISL	),	/* Shuriken */
            new init_weaps("2x3",	"1x6",	NO_WEAPON,	Item.ISMISL		),	/* Spear */
        };

        // Fire a missile in a given direction
        static void missile(int ydelta, int xdelta)
        {
            //THING obj;

            ///*
            // * Get which thing we are hurling
            // */
            //if ((obj = get_item("throw", WEAPON)) == null)
            //    return;
            //if (!detachable(obj) || CurrentlyUsing(obj))
            //    return;
            //obj = leave_pack(obj, true, false);
            //do_motion(obj, ydelta, xdelta);
            ///*
            // * AHA! Here it has hit something.  If it is a wall or a door,
            // * or if it misses (combat) the monster, put it on the floor
            // */
            //if (GetMonster(obj.ItemPos.y, obj.ItemPos.x) == null ||
            //!hit_monster(obj.ItemPos.y, obj.ItemPos.x, obj))
            //    fall(obj, true);
        }

        // Do the actual motion on the screen done by an object traveling
        // across the room
        static void do_motion(THING obj, int ydelta, int xdelta)
        {
            //char ch;

            ///*
            // * Come fly with us ...
            // */
            //obj.ItemPos.CopyFrom(hero);
            //for (; ; )
            //{
            //    /*
            //     * Erase the adjac one
            //     */
            //    if (!TheseAreTheSame(obj.ItemPos, hero) && HeroCanSee(obj.ItemPos.y, obj.ItemPos.x) && !Agent.Terse)
            //    {
            //        ch = chat(obj.ItemPos.y, obj.ItemPos.x);
            //        if (ch == FLOOR && !show_floor())
            //            ch = ' ';
            //        R12.MvAddCh(obj.ItemPos.y, obj.ItemPos.x, ch);
            //    }
            //    /*
            //     * Get the new pos
            //     */
            //    obj.ItemPos.y += ydelta;
            //    obj.ItemPos.x += xdelta;
            //    if (walkable___step_ok(ch = GetMonsterAppearance(obj.ItemPos.y, obj.ItemPos.x)) && ch != DOOR)
            //    {
            //        /*
            //         * It hasn't hit anything yet, so display it
            //         * If it alright.
            //         */
            //        if (HeroCanSee(obj.ItemPos.y, obj.ItemPos.x) && !Agent.Terse)
            //        {
            //            R12.MvAddCh(obj.ItemPos.y, obj.ItemPos.x, obj.ItemType);
            //            R12.Present__refresh();
            //        }
            //        continue;
            //    }
            //    break;
            //}
        }

        // Set up the initial goodies for a weapon
        public static void Init(THING weap, int which)
        {
            init_weaps iwp;

            weap.ItemType = Char.WEAPON;
            weap.Which = which;
            iwp = init_dam[which];
            weap.swing___o_damage = iwp.iw_dam;
            weap.o_hurldmg = iwp.iw_hrl;
            weap.neededTo___o_launch = iwp.iw_launch;
            weap.o_flags = iwp.iw_flags;
            weap.PlusToHit = 0;
            weap.PlusToDmg = 0;

            if (which == DAGGER)
            {
                weap.Count = R14.rnd(4) + 2;
                weap.o_group = group++;
            }
            else if ((weap.o_flags & Item.ISMANY) == Item.ISMANY)
            {
                weap.Count = R14.rnd(8) + 8;
                weap.o_group = group++;
            }
            else
            {
                weap.Count = 1;
                weap.o_group = 0;
            }
        }

        // Does the missile hit the monster?
        static bool hit_monster(int y, int x, THING obj)
        {
            Coord mp = new Coord();

            mp.y = y;
            mp.x = x;
            return R14.PlayerAttacks(mp, obj, true);
        }

        //was 'fallpos'
        static public bool PickRndPosAroundYX(Coord o, Coord n)
        {
            int y, x, cnt, ch;

            cnt = 0;
            for (y = o.y - 1; y <= o.y + 1; y++)
                for (x = o.x - 1; x <= o.x + 1; x++)
                {
                    //make certain the spot is empty. 
                    //if it is, put the object there, set it in the level list, & and re-draw the room if he can see it
                    if (y == R14.hero.y && x == R14.hero.x)
                        continue;
                    if (((ch = Dungeon.GetCharAt(y, x)) == Char.FLOOR || ch == Char.PASSAGE)
                                && R14.rnd(++cnt) == 0)
                    {
                        n.y = y;
                        n.x = x;
                    }
                }
            return (bool)(cnt != 0);
        }

        static public void Wield(THING t)
        {
            THING oW; //old weapon
            string sp;

            oW = Agent.CurrWeapon;

            if (!Item.IsDetachable(Agent.CurrWeapon))
            {
                Agent.CurrWeapon = oW;
                return;
            }

            Agent.CurrWeapon = oW;


            sp = Item.GetDescription(t);
            Agent.CurrWeapon = t;

            if (!Agent.Terse)
                R14.addmsg("You are now ");
            R14.AddToLog("wielding {0} ({1})", sp, t.CharInPack);
        }
    }