using UnityEngine;



    static class Scroll
    {
        public static string[] s_names = new string[MAX];		/* Names of the scrolls */
        
        static int MAXNAME = 40;

        //types
        public const int S_CONFUSE = 0;
        public const int S_MAP = 1;
        public const int S_HOLD = 2;
        public const int S_SLEEP = 3;
        public const int S_ARMOR = 4;
        public const int S_ID_POTION = 5;
        public const int S_ID_SCROLL = 6;
        public const int S_ID_WEAPON = 7;
        public const int IdentifyArmour = 8;
        public const int IdentifyRingOrStick = 9;
        public const int S_SCARE = 10;
        public const int S_FDET = 11;
        public const int S_TELEP = 12;
        public const int S_ENCH = 13;
        public const int S_CREATE = 14;
        public const int S_REMOVE = 15;
        public const int S_AGGR = 16;
        public const int S_PROTECT = 17;
        public const int MAX = 18;

        public static ObjectData[] Data = new[]
        {
            new ObjectData("Monster Confusion", 7, 140, null, false),
            new ObjectData("Magic Mapping", 4, 150, null, false),
            new ObjectData("Hold Monster", 2, 180, null, false),
            new ObjectData("Sleep", 3, 5, null, false),
            new ObjectData("Enchant Armour", 7, 160, null, false),
            new ObjectData("Identify Potion", 10, 80, null, false),
            new ObjectData("Identify Scroll", 10, 80, null, false),
            new ObjectData("Identify Weapon", 6, 80, null, false),
            new ObjectData("Identify Armour", 7, 100, null, false),
            new ObjectData("Identify Ring, Wand or Staff", 10, 115, null, false),
            new ObjectData("Scare Monster", 3, 200, null, false),
            new ObjectData("Food Detection", 2, 60, null, false),
            new ObjectData("Teleportation", 5, 165, null, false),
            new ObjectData("Enchant Weapon", 8, 150, null, false),
            new ObjectData("Create Monster", 4, 75, null, false),
            new ObjectData("Remove Curse", 7, 105, null, false),
            new ObjectData("Aggravate Monsters", 3, 20, null, false),
            new ObjectData("Protect Armour", 2, 250, null, false),
        };

        static string[] sylls = new[] {
            "a", "ab", "ag", "aks", "ala", "an", "app", "arg", "arze", "ash",
            "bek", "bie", "bit", "bjor", "blu", "bot", "bu", "byt", "comp",
            "con", "cos", "cre", "dalf", "dan", "den", "do", "e", "eep", "el",
            "eng", "er", "ere", "erk", "esh", "evs", "fa", "fid", "fri", "fu",
            "gan", "gar", "glen", "gop", "gre", "ha", "hyd", "amount", "ing", "ip",
            "ish", "it", "ite", "iv", "jo", "kho", "kli", "klis", "la", "lech",
            "mar", "me", "mi", "mic", "mik", "mon", "mung", "mur", "nej",
            "nelg", "nep", "ner", "nes", "nes", "nih", "nin", "o", "od", "ood",
            "org", "orn", "ox", "oxy", "pay", "ple", "plu", "po", "pot",
            "prok", "re", "rea", "rhov", "ri", "ro", "rog", "rok", "rol", "sa",
            "san", "sat", "sef", "seh", "shu", "ski", "sna", "sne", "snik",
            "sno", "so", "sol", "sri", "sta", "sun", "ta", "tab", "tem",
            "ther", "ti", "tox", "trol", "tue", "turs", "u", "ulk", "um", "un",
            "uni", "ur", "val", "viv", "vly", "vom", "wah", "wed", "werg",
            "wex", "whon", "wun", "xo", "y", "yot", "yu", "zant", "zeb", "zim",
            "zok", "zon", "zum",
        };

        static Scroll() {
            S.Log("Scroll names");

            //init names of scrolls            
            string title, word;
            int i, numSyl, numWords;

            for (i = 0; i < Scroll.MAX; i++)
            {
                title = "";
                numWords = (int)Random.Range(0, 3) + 2; //R14.rnd(3) + 2;
                while (numWords-- > 0)
                {
                    word = "";
                    numSyl = (int)Random.Range(0, 3) + 1; //R14.rnd(3) + 1;
                    while (numSyl-- > 0)
                    {
					word += sylls[(int)Random.Range(0, sylls.Length)]; //sp = sylls[R14.rnd(sylls.Length)];
                        word += "'";

                        if (title.Length > MAXNAME)
                            break;
                    }

                    word = word.Substring(0, word.Length - 1); // trim last char 
                    title += word; 
                    title += " ";
                }

                s_names[i] = title.Substring(0, title.Length - 1); // trim last char 
                S.Log(s_names[i]);
            }
        }

        // Uncurse an item
        static void uncurse(THING obj)
        {
            if (obj != null)
                obj.o_flags &= ~Item.IsCursed;
        }

        static public void Read(THING t)
        {
            S.Log("Reading scroll: " + t.Name);

            bool discardit = false;
            PLACE pp;
            int y, x;
            int ch;
            int i;
            room cur_room;
            THING orig_obj;
            Coord mp = new Coord();

            if (t == null)
                return;

            //calculate the effect it has on the poor guy
            if (t == Agent.CurrWeapon)
                Agent.CurrWeapon = null;

            discardit = (bool)(t.Count == 1);
            R14.leave_pack(t, false, false);
            orig_obj = t;

            switch (t.Which)
            {
                case S_CONFUSE:
                    /*
                     * Scroll of monster confusion.  Give him that power.
                     */
                    Agent.Plyr.t_flags |= Mob.CANHUH;
                    S.Log("Your hands begin to glow " + Potion.GetColor("red"));
                    break;
                case S_ARMOR:
                    if (Agent.cur_armor != null)
                    {
                        Agent.cur_armor.Ac--;
                        Agent.cur_armor.o_flags &= ~Item.IsCursed;
                        R14.AddToLog("Your armor glows {0} for a moment", Potion.GetColor("silver"));
                    }
                    break;
                case S_HOLD:
                    /*
                     * Hold monster scroll.  Stop all monsters within two spaces
                     * from chasing after the hero.
                     */
                    ch = '\0';
                    for (x = R14.hero.x - 2; x <= R14.hero.x + 2; x++)
                        if (x >= 0 && x < R14.NUMCOLS)
                            for (y = R14.hero.y - 2; y <= R14.hero.y + 2; y++)
                                if (y >= 0 && y <= R14.NUMLINES - 1)
                                    if ((t = Dungeon.GetMonster(y, x)) != null && R14.on(t, Mob.ISRUN))
                                    {
                                        t.t_flags &= ~Mob.ISRUN;
                                        t.t_flags |= Mob.ISHELD;
                                        ch++;
                                    }
                    if (ch != '\0')
                    {
                        R14.addmsg("the monster");
                        if (ch > 1)
                            R14.addmsg("s around you");
                        R14.addmsg(" freeze");
                        if (ch == 1)
                            R14.addmsg("s");

                        Data[S_HOLD].Know = true;
                    }
                    else
                        S.Log("You feel a strange sense of loss");
                    break;
                case S_SLEEP:
                    //makes you fall asleep
                    Data[S_SLEEP].Know = true;
                    Agent.NumTurnsAsleep += R14.rnd(R14.SLEEPTIME) + 4;
                    Agent.Plyr.t_flags &= ~Mob.ISRUN;
                    S.Log("You fall asleep");
                    break;
                case S_CREATE:
                    /*
                     * Create a monster:
                     * First look in a circle around him, next try his room
                     * otherwise give up
                     */
                    i = 0;
                    for (y = R14.hero.y - 1; y <= R14.hero.y + 1; y++)
                        for (x = R14.hero.x - 1; x <= R14.hero.x + 1; x++)
                            //don't put on top of player
                            if (y == R14.hero.y && x == R14.hero.x)
                                continue;
                            /*
                             * Or anything else nasty
                             */
                            else if (Dungeon.walkable___step_ok((char)(ch = Dungeon.GetMonsterAppearance(y, x))))
                            {
                                if (ch == Char.SCROLL && Dungeon.find_obj(y, x).Which == S_SCARE)
                                    continue;
                                else if (R14.rnd(++i) == 0)
                                {
                                    mp.y = y;
                                    mp.x = x;
                                }
                            }

                    if (i == 0)
                    {
                        S.Log("You hear a faint cry of anguish in the distance");
                        //S.Audio.Play("cry_of_anguish_:)");
                    }
                    else
                    {
                        t = new THING();
                        R14.new_monster(t, R14.randmonster(false), mp);
                    }
                    break;
                case S_ID_POTION:
                case S_ID_SCROLL:
                case S_ID_WEAPON:
                case IdentifyArmour:
                case IdentifyRingOrStick:
                    {
                        Data[t.Which].Know = true;
                        R14.AddToLog("this scroll is an {0} scroll", Data[t.Which].Name);
                        Item.Identify(t);
                    }
                    break;
                case S_MAP:
                    /*
                     * Scroll of magic mapping.
                     */
                    Data[S_MAP].Know = true;
                    S.Log("oh, now this scroll has a map on it");
                    
                    //take all the things we want to keep hidden out of the window
                    for (y = 1; y < R14.NUMLINES - 1; y++)
                        for (x = 0; x < R14.NUMCOLS; x++)
                        {
                            pp = Dungeon.PlaceAt(y, x);
                            switch (ch = pp.p_ch)
                            {
                                case Char.DOOR:
                                case Char.STAIRS:
                                    break;
                                case Char.WALL_HORI:
                                case Char.WALL_VERT:
                                    if (!((pp.p_flags & Dungeon.F_REAL) != 0))
                                    {
                                        ch = pp.p_ch = Char.DOOR;
                                        pp.p_flags |= Dungeon.F_REAL;
                                    }
                                    break;
                                case ' ':
                                    if ((pp.p_flags & Dungeon.F_REAL) != 0)
                                        goto def;
                                    pp.p_flags |= Dungeon.F_REAL;
                                    ch = pp.p_ch = Char.PASSAGE;
                                    goto case Char.PASSAGE;
                                case Char.PASSAGE:
                                    pass:
                                        if (!((pp.p_flags & Dungeon.F_REAL) != 0))
                                            pp.p_ch = Char.PASSAGE;
                                    pp.p_flags |= (Dungeon.F_SEEN | Dungeon.F_REAL);
                                    ch = Char.PASSAGE;
                                    break;
                                case Char.FLOOR:
                                    if ((pp.p_flags & Dungeon.F_REAL) != 0)
                                        ch = ' ';
                                    else
                                    {
                                        ch = Char.TRAP;
                                        pp.p_ch = Char.TRAP;
                                        pp.p_flags |= (Dungeon.F_SEEN | Dungeon.F_REAL);
                                    }
                                    break;

                                default:
                                    def:
                                    
                                    if ((pp.p_flags & Dungeon.F_PASS) != 0)
                                        goto pass;

                                    ch = ' ';
                                    break;
                            }

                            if (ch != ' ')
                            {
                                if ((t = pp.p_monst) != null)
                                    t.t_oldch = (char)ch;

                                //if no monster or player can't see invisible, draw floor
                                if (t == null || !R14.on(Agent.Plyr, Mob.CanSeeInvisible))
                                    ;//R12.MvAddCh(y, x, ch);
                            }
                        }
                    break;
                case S_FDET:
                    ch = 0;

                    S.Log("Should be showing food indicators, but..... ALPHA testing and all!");
                    //arrows at the edges of screen for offscreen food, and some highlighted effect for onscreen");
                    //could maybe send out lots of particles from foods towards player.
                    //they'd startout stupid fast & slow down as they get farther away from spawn, or to the player.
                    //giving a visual sense of distance
                    for (t = Dungeon.ItemList; t != null; t = R14.next(t))
                        if (t.ItemType == Char.FOOD)
                        {
                            ch = 1;
                            //move(ItemPos);
                            //addch(Char.FOOD);
                        }

                    if (ch != 0)
                    {
                        Data[S_FDET].Know = true;
                        S.Log("Your nose tingles and you smell food.");
                    }
                    else
                        S.Log("Your nose tingles");
                    break;
                case S_TELEP:
                    /*
                     * Scroll of teleportation:
                     * Make him dissapear and reappear
                     */
                    {
                        cur_room = Agent.Plyr.CurrRoom;
                        Teleport();

                        if (cur_room != Agent.Plyr.CurrRoom) //if teleported to another room, we obviously know its a Teleport scroll
                            Scroll.Data[S_TELEP].Know = true;
                    }
                    break;
                case S_ENCH:
                    if (Agent.CurrWeapon == null || Agent.CurrWeapon.ItemType != Char.WEAPON)
                        S.Log("You feel a strange sense of loss");
                    else
                    {
                        Agent.CurrWeapon.o_flags &= ~Item.IsCursed;
                        if (R14.rnd(2) == 0)
                            Agent.CurrWeapon.PlusToHit++;
                        else
                            Agent.CurrWeapon.PlusToDmg++;
                        S.Log(string.Format("Your {0} glows {0} for a moment", 
                            Weapon.Data[Agent.CurrWeapon.Which].Name, Potion.GetColor("blue") ) );
                    }
                    break;
                case S_SCARE:
                    //reading it is a mistake and produces laughter at her poor boo boo
                    S.Log("You hear maniacal laughter in the distance");
                    //S.Audio.Play("laugh");   //FIXME add laughter
                    break;
                case S_REMOVE:
                    uncurse(Agent.cur_armor);
                    uncurse(Agent.CurrWeapon);
                    uncurse(Agent.cur_ring[Ring.LEFT]);
                    uncurse(Agent.cur_ring[Ring.RIGHT]);
                    S.Log(R14.choose_str("You feel in touch with the Universal Onenes",
                           "You feel as if somebody is watching over you"));
                    break;
                case S_AGGR:
                    /*
                     * This scroll aggravates all the monsters on the current
                     * level and sets them running towards the hero
                     */
                    Dungeon.aggravate();
                    S.Log("You hear a high pitched humming noise");
                    //fixme: play hum sound
                    break;
                case S_PROTECT:
                    if (Agent.cur_armor != null)
                    {
                        Agent.cur_armor.o_flags |= Item.ISPROT;
                        S.Log(string.Format("Your armor is covered by a shimmering {0} shield", Potion.GetColor("gold")));
                    }
                    else
                        S.Log("You feel a strange sense of loss");
                    break;
            }

            t = orig_obj;
            R14.glanceAroundHero___look(true);	/* put the result of the scroll on the screen */
            Item.UpdateGuess(Data[t.Which]);
        }

        static public void Teleport()
        {
            Coord c = new Coord();

            Dungeon.find_floor(null, out c, 0, true);
            if (Dungeon.roomin(c) != Agent.Plyr.CurrRoom)
            {
                Dungeon.leave_room(R14.hero);
                R14.hero = c;
                Dungeon.enter_room(R14.hero);
            }
            else
            {
                R14.hero = c;
                R14.glanceAroundHero___look(true);
            }

            //let teleportation break being "held"/rooted      apparently only a Flytrap can root you?
            if (R14.on(Agent.Plyr, Mob.ISHELD))
            {
                Agent.Plyr.t_flags &= ~Mob.ISHELD;
                Agent.NumTimesFlytrapHasHit = 0;
                R14.monsters['F' - 'A'].Stats.Dmg = "000x0";
            }
            Agent.no_move = 0;
            Agent.running = false;
        }
    }