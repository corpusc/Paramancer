using System;
using System.Collections.Generic;


// sticks == staves or wands
    static class Stick {
        public const int BOLT_LENGTH = 6;
        public static string[] ws_made = new string[MAX]; /* What sticks are made of */
        public static string[] ws_type = new string[MAX]; /* Is it a wand or a staff */

        // Rod/Wand/Staff types
        public const int WS_LIGHT = 0;
        public const int WS_INVIS = 1;
        public const int WS_ELECT = 2;
        public const int WS_FIRE = 3;
        public const int WS_COLD = 4;
        public const int WS_POLYMORPH = 5;
        public const int WS_MISSILE = 6;
        public const int WS_HASTE_M = 7;
        public const int WS_SLOW_M = 8;
        public const int WS_DRAIN = 9;
        public const int WS_NOP = 10;
        public const int WS_TELAWAY = 11;
        public const int WS_TELTO = 12;
        public const int WS_CANCEL = 13;
        public const int MAX = 14;

        public static ObjectData[] Data = new[] {
            new ObjectData("light", 12, 250, null, false),
            new ObjectData("invisibility", 6, 5, null, false),
            new ObjectData("lightning", 3, 330, null, false),
            new ObjectData("fire", 3, 330, null, false),
            new ObjectData("cold", 3, 330, null, false),
            new ObjectData("polymorph", 15, 310, null, false),
            new ObjectData("magic missile", 10, 170, null, false),
            new ObjectData("haste monster", 10, 5, null, false),
            new ObjectData("slow monster", 11, 350, null, false),
            new ObjectData("drain life", 9, 300, null, false),
            new ObjectData("nothing", 1, 5, null, false),
            new ObjectData("teleport away", 6, 340, null, false),
            new ObjectData("teleport to", 6, 50, null, false),
            new ObjectData("cancellation", 5, 280, null, false),
        };

        static string[] wood = new[] {
            "avocado wood",
            "balsa",
            "bamboo",
            "banyan",
            "birch",
            "cedar",
            "cherry",
            "cinnibar",
            "cypress",
            "dogwood",
            "driftwood",
            "ebony",
            "elm",
            "eucalyptus",
            "fall",
            "hemlock",
            "holly",
            "ironwood",
            "kukui wood",
            "mahogany",
            "manzanita",
            "maple",
            "oaken",
            "persimmon wood",
            "pecan",
            "pine",
            "poplar",
            "redwood",
            "rosewood",
            "spruce",
            "teak",
            "walnut",
            "zebrawood",
        };

        private static int NWOOD = wood.Length;
        static int cNWOOD = NWOOD;

        static string[] metal = new[] {
            "aluminum",
            "beryllium",
            "bone",
            "brass",
            "bronze",
            "copper",
			// coconut wood is awz said Justin 
            "electrum",
            "gold",
            "iron",
            "lead",
            "magnesium",
            "mercury",
            "nickel",
            "pewter",
            "platinum",
            "steel",
            "silver",
            "silicon",
            "tin",
            "titanium",
            "tungsten",
            "zinc",
        };

        private static int NMETAL = metal.Length;
        static int cNMETAL = NMETAL;

        static Stick()
        {
            init_materials(); /* Set up materials of wands */
        }

        static bool[] used = new bool[NWOOD];
        static public void init_materials() { // materials for wands/staves 
            int i, j;
            string str;
            bool[] metused = new bool[NMETAL];

            for (i = 0; i < NWOOD; i++)
                used[i] = false;
            for (i = 0; i < NMETAL; i++)
                metused[i] = false;
            for (i = 0; i < Stick.MAX; i++)
            {
                for (;;)
                    if (R14.rnd(2) == 0)
                    {
                        j = R14.rnd(NMETAL);
                        if (!metused[j])
                        {
                            ws_type[i] = "wand";
                            str = metal[j];
                            metused[j] = true;
                            break;
                        }
                    }
                    else
                    {
                        j = R14.rnd(NWOOD);
                        if (!used[j])
                        {
                            ws_type[i] = "staff";
                            str = wood[j];
                            used[j] = true;
                            break;
                        }
                    }
                ws_made[i] = str;
            }
        }

        //setup new stick
        public static void fix_stick(THING cur)
        {
            if (ws_type[cur.Which] == "staff")
                cur.swing___o_damage = "2x3";
            else
                cur.swing___o_damage = "1x1";

            cur.o_hurldmg = "1x1";

            switch (cur.Which)
            {
                case WS_LIGHT:
                    cur.o_charges = R14.rnd(10) + 10;
                    break;
                default:
                    cur.o_charges = R14.rnd(5) + 3;
                    break;
            }
        }

        //zap with a wand
        static void do_zap()
        {
            //THING obj, tp;
            //int y, x;
            //string name;
            //char monster, oldch;
            //THING bolt = new THING();

            //if ((obj = get_item("zap with", STICK)) == null)
            //    return;
            //if (obj.ItemType != STICK)
            //{
            //    after = false;
            //    AddToLog("you can'tag zap with that!");
            //    return;
            //}
            //if (obj.o_charges == 0)
            //{
            //    AddToLog("nothing happens");
            //    return;
            //}
            //switch (obj.Which)
            //{
            //    case WS_LIGHT:
            //        /*
            //         * Reddy Kilowat wand.  Light up the room
            //         */
            //        ws_info[WS_LIGHT].oi_know = true;
            //        if ((proom.Flags & ISGONE) != 0)
            //            AddToLog("the corridor glows and then fades");
            //        else
            //        {
            //            proom.Flags &= ~Room.ISDARK;
            //            /*
            //             * Light the room and put the player back up
            //             */
            //            enter_room(R14.hero);
            //            R14.addmsg("the room is lit");
            //            if (!Agent.Terse)
            //                R14.addmsg(" by a shimmering {0} light", GetColor("blue"));
            //            waitForSpaceLeavingPrevMsgVis___endmsg();
            //        }
            //        break;
            //    case WS_DRAIN:
            //        /*
            //         * take away 1/2 of hero's hit points, then take it away
            //         * evenly from the monsters in the room (or next to hero
            //         * if he is in a passage)
            //         */
            //        if (pstats.Hp < 2)
            //        {
            //            AddToLog("you are too weak to use it");
            //            return;
            //        }
            //        else
            //            drain();
            //        break;
            //    case WS_INVIS:
            //    case WS_POLYMORPH:
            //    case WS_TELAWAY:
            //    case WS_TELTO:
            //    case WS_CANCEL:
            //        y = R14.hero.y;
            //        x = R14.hero.x;
            //        while (walkable___step_ok(GetMonsterAppearance(y, x)))
            //        {
            //            y += delta.y;
            //            x += delta.x;
            //        }
            //        if ((tp = GetMonster(y, x)) != null)
            //        {
            //            monster = tp.MonsType;
            //            if (monster == 'F')
            //                player.t_flags &= ~ISHELD;
            //            switch (obj.Which)
            //            {
            //                case WS_INVIS:
            //                    tp.t_flags |= ISINVIS;
            //                    if (HeroCanSee(y, x))
            //                        R12.MvAddCh(y, x, tp.t_oldch);
            //                    break;
            //                case WS_POLYMORPH:
            //                    {
            //                        THING pp;
            //                        pp = tp.Pack;
            //                        detach(ref MobList, tp);
            //                        if (heroCanSeeMonster(tp))
            //                            R12.MvAddCh(y, x, chat(y, x));
            //                        oldch = tp.t_oldch;
            //                        delta.y = y;
            //                        delta.x = x;
            //                        new_monster(tp, monster = (char)(rnd(26) + 'A'), delta);
            //                        if (heroCanSeeMonster(tp))
            //                            R12.MvAddCh(y, x, monster);
            //                        tp.t_oldch = oldch;
            //                        tp.Pack = pp;
            //                        ws_info[WS_POLYMORPH].oi_know |= heroCanSeeMonster(tp);
            //                        break;
            //                    }
            //                case WS_CANCEL:
            //                    tp.t_flags |= ISCANC;
            //                    tp.t_flags &= ~(ISINVIS | CANHUH);
            //                    tp.t_disguise = tp.MonsType;
            //                    if (heroCanSeeMonster(tp))
            //                        R12.MvAddCh(y, x, tp.t_disguise);
            //                    break;
            //                case WS_TELAWAY:
            //                case WS_TELTO:
            //                    {
            //                        Coord new_pos = new Coord();

            //                        if (obj.Which == WS_TELAWAY)
            //                        {
            //                            do
            //                            {
            //                                find_floor(null, out new_pos, 0, true);
            //                            } while (TheseAreTheSame(new_pos, R14.hero));
            //                        }
            //                        else
            //                        {
            //                            new_pos.y = R14.hero.y + delta.y;
            //                            new_pos.x = R14.hero.x + delta.x;
            //                        }
            //                        tp.t_dest = R14.hero;
            //                        tp.t_flags |= ISRUN;
            //                        relocate(tp, new_pos);
            //                    }
            //                    break;
            //            }
            //        }
            //        break;
            //    case WS_MISSILE:
            //        ws_info[WS_MISSILE].oi_know = true;
            //        bolt.ItemType = '*';
            //        bolt.o_hurldmg = "1x4";
            //        bolt.PlusToHit = 100;
            //        bolt.PlusToDmg = 1;
            //        bolt.o_flags = ISMISL;
            //        if (CurrWeapon != null)
            //            bolt.neededTo___o_launch = CurrWeapon.Which;
            //        do_motion(bolt, delta.y, delta.x);
            //        if ((tp = GetMonster(bolt.ItemPos.y, bolt.ItemPos.x)) != null
            //            && !save_throw(VS_MAGIC, tp))
            //            hit_monster(bolt.ItemPos.y, bolt.ItemPos.x, bolt);
            //        else if (Agent.Terse)
            //            AddToLog("missle vanishes");
            //        else
            //            AddToLog("the missle vanishes with a puff of smoke");
            //        break;
            //    case WS_HASTE_M:
            //    case WS_SLOW_M:
            //        y = hero.y;
            //        x = hero.x;
            //        while (walkable___step_ok(GetMonsterAppearance(y, x)))
            //        {
            //            y += delta.y;
            //            x += delta.x;
            //        }
            //        if ((tp = GetMonster(y, x)) != null)
            //        {
            //            if (obj.Which == WS_HASTE_M)
            //            {
            //                if (on(tp, ISSLOW))
            //                    tp.t_flags &= ~ISSLOW;
            //                else
            //                    tp.t_flags |= ISHASTE;
            //            }
            //            else
            //            {
            //                if (on(tp, ISHASTE))
            //                    tp.t_flags &= ~ISHASTE;
            //                else
            //                    tp.t_flags |= ISSLOW;
            //                tp.t_turn = true;
            //            }
            //            delta.y = y;
            //            delta.x = x;
            //            runto(delta);
            //        }
            //        break;
            //    case WS_ELECT:
            //    case WS_FIRE:
            //    case WS_COLD:
            //        if (obj.Which == WS_ELECT)
            //            name = "bolt";
            //        else if (obj.Which == WS_FIRE)
            //            name = "flame";
            //        else
            //            name = "ice";
            //        fire_bolt(hero, delta, name);
            //        ws_info[obj.Which].oi_know = true;
            //        break;
            //    case WS_NOP:
            //        break;
            //}
            //obj.o_charges--;
        }

        //drain hit points from player
        static void drain()
        {
        //    THING mp;
        //    room corp;
        //    int cnt;
        //    bool inpass;
        //    List<THING> drainee = new List<THING>();

        //    /*
        //     * First cnt how many things we need to spread the hit points among
        //     */
        //    cnt = 0;
        //    if (Dungeon.chat(R14.hero.y, R14.hero.x) == Char.DOOR)
        //        corp = passages[flat(R14.hero.y, R14.hero.x) & F_PNUM];
        //    else
        //        corp = null;
        //    inpass = (bool)((proom.Flags & ISGONE) != 0);
        //    for (mp = MobList; mp != null; mp = next(mp))
        //        if (mp.CurrRoom == proom || mp.CurrRoom == corp ||
        //            (inpass && Dungeon.chat(mp.t_pos.y, mp.t_pos.x) == Char.DOOR &&
        //            passages[flat(mp.t_pos.y, mp.t_pos.x) & F_PNUM] == proom))
        //            drainee.Add(mp);
        //    if ((cnt = drainee.Count) == 0)
        //    {
        //        AddToLog("you have a tingling feeling");
        //        return;
        //    }
        //    pstats.Hp /= 2;
        //    cnt = pstats.Hp / cnt;
        //    /*
        //     * Now zot all of the monsters
        //     */
        //    foreach (var dp in drainee)
        //    {
        //        mp = dp;
        //        if ((mp.Stats.Hp -= cnt) <= 0)
        //            killed(mp, heroCanSeeMonster(mp));
        //        else
        //            runto(mp.t_pos);
        //    }
        //}

        //// Fire a bolt in a given direction from a specific starting place
        //static void fire_bolt(Coord start, Coord dir, string name)
        //{
        //    int c1, c2;
        //    THING tp;
        //    char dirch = (char)0, ch;
        //    bool hit_hero, used, changed;
        //    Coord pos = new Coord();
        //    Coord[] spotpos = new Coord[BOLT_LENGTH];
        //    THING bolt = new THING();

        //    bolt.ItemType = Char.WEAPON;
        //    bolt.Which = FLAME;
        //    bolt.o_hurldmg = "6x6";
        //    bolt.PlusToHit = 100;
        //    bolt.PlusToDmg = 0;
        //    bolt.neededTo___o_launch = -1;
        //    Weapon.Data[FLAME].oi_name = name;
        //    switch (dir.y + dir.x)
        //    {
        //        case 0: dirch = '/'; break;
        //        case 1:
        //        case -1: dirch = (dir.y == 0 ? '-' : '|'); break;
        //        case 2:
        //        case -2: dirch = '\\'; break;
        //    }
        //    pos.CopyFrom(start);
        //    hit_hero = (bool)(start != R14.hero);
        //    used = false;
        //    changed = false;
        //    for (c1 = 0; c1 <= BOLT_LENGTH - 1 && !used; c1++)
        //    {
        //        pos.y += dir.y;
        //        pos.x += dir.x;
        //        spotpos[c1] = new Coord(pos.y, pos.x);
        //        ch = GetMonsterAppearance(pos.y, pos.x);
        //        switch (ch)
        //        {
        //            case Char.DOOR:
        //                /*
        //                 * this code is necessary if the hero is on a door
        //                 * and he fires at the wall the door is in, it would
        //                 * otherwise loop infinitely
        //                 */
        //                if (Dungeon.TheseAreTheSame(R14.hero, pos))
        //                    goto def;
        //                /* FALLTHROUGH */
        //                goto case '|';
        //            case '|':
        //            case '-':
        //            case ' ':
        //                if (!changed)
        //                    hit_hero = !hit_hero;
        //                changed = false;
        //                dir.y = -dir.y;
        //                dir.x = -dir.x;
        //                c1--;
        //                AddToLog("the {0} bounces", name);
        //                break;
        //            default:
        //            def:
        //                if (!hit_hero && (tp = GetMonster(pos.y, pos.x)) != null)
        //                {
        //                    hit_hero = true;
        //                    changed = !changed;
        //                    tp.t_oldch = Dungeon.chat(pos.y, pos.x);
        //                    if (!save_throw(VS_MAGIC, tp))
        //                    {
        //                        bolt.ItemPos = pos;
        //                        used = true;
        //                        if (tp.MonsType == 'D' && name == "flame")
        //                        {
        //                            R14.addmsg("the flame bounces");
        //                            if (!Agent.Terse)
        //                                R14.addmsg(" off the dragon");
        //                            waitForSpaceLeavingPrevMsgVis___endmsg();
        //                        }
        //                        else
        //                            hit_monster(pos.y, pos.x, bolt);
        //                    }
        //                    else if (ch != 'M' || tp.t_disguise == 'M')
        //                    {
        //                        if (start == R14.hero)
        //                            runto(pos);
        //                        if (Agent.Terse)
        //                            AddToLog("{0} misses", name);
        //                        else
        //                            AddToLog("the {0} whizzes past {1}", name, GetMonsterName(tp));
        //                    }
        //                }
        //                else if (hit_hero && Dungeon.TheseAreTheSame(pos, R14.hero))
        //                {
        //                    hit_hero = false;
        //                    changed = !changed;
        //                    if (!save(VS_MAGIC))
        //                    {
        //                        if ((pstats.Hp -= RollDice(6, 6)) <= 0)
        //                        {
        //                            if (start == R14.hero)
        //                                death('b');
        //                            else
        //                                death(GetMonster(start.y, start.x).MonsType);
        //                        }
        //                        used = true;
        //                        if (Agent.Terse)
        //                            AddToLog("the {0} hits", name);
        //                        else
        //                            AddToLog("you are hit by the {0}", name);
        //                    }
        //                    else
        //                        AddToLog("the {0} whizzes by you", name);
        //                }
        //            R12.MvAddCh(pos.y, pos.x, dirch);
        //            R12.Present__refresh();
        //            break;
        //        }
        //    }
        //    for (c2 = 0; c2 < c1; c2++)
        //        R12.MvAddCh(spotpos[c2].y, spotpos[c2].x, Dungeon.chat(spotpos[c2].y, spotpos[c2].x));
        }

        // Return an appropriate string for a wand charge
        static public string charge_str(THING obj)
        {
            if (!((obj.o_flags & Item.ISKNOW) != 0))
                return "";
            else if (Agent.Terse)
                return string.Format(" [{0}]", obj.o_charges);
            else
                return string.Format(" [{0} charges]", obj.o_charges);
        }
    }