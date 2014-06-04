using System;
using System.Collections.Generic;



    static partial class R14
    {
        public const int MAXPACK = 23;
        public static int NumItems;				/* # num items listed in inventory() call */
        static bool[] pack_used = new bool[26]; /* Is the character used in the pack? */
        
        public static class Int
        {
            // things that.....
            public const int CALLABLE = -1;
            public const int RingOrStick = -2;
        }


        // Return the next unused pack character.
        static char pack_char()
        {
            int i;

            for (i = 0; pack_used[i]; i++)
                continue;
            pack_used[i] = true;
            return (char)(i + 'a');
        }

        //pick up an object and add it to the pack
        static public void add_pack(THING obj, bool silent)
        {
            THING t, lp;
            bool from_floor = false;

            //get it off the ground
            if (obj == null) //should never go in here for realtime?
            {
                if (
                        (
                        obj = Dungeon.find_obj(hero.y, hero.x)
                        ) 
                        == null
                   )
                    return;

                from_floor = true;
            }

            //check for and deal with scare monster scrolls
            if (obj.ItemType == Char.SCROLL && obj.Which == Scroll.S_SCARE)
                if ((obj.o_flags & Mob.ISFOUND) == Mob.ISFOUND)
                {
                    //for realtime we just wanna leave ASCII map as generated?
                    //detach(ref Dungeon.ItemList, obj); 
                    //Dungeon.SetCharAt(hero.y, hero.x, (Agent.Plyr.CurrRoom.Flags & Room.ISGONE) == Room.ISGONE ? Char.PASSAGE : Char.FLOOR);

                    AddToLog("the scroll turns to dust as you pick it up");
                    return;
                }

            if (Agent.Plyr.Pack == null)
            {
                Agent.Plyr.Pack = obj;
                obj.CharInPack = pack_char();
                inpack++;
            }
            else
            {
                lp = null;
                for (t = Agent.Plyr.Pack; t != null; t = next(t))
                {
                    if (t.ItemType != obj.ItemType)
                        lp = t;
                    else
                    {
                        while (t.ItemType == obj.ItemType && t.Which != obj.Which)
                        {
                            lp = t;
                            if (next(t) == null)
                                break;
                            else
                                t = next(t);
                        }

                        if (t.ItemType == obj.ItemType && t.Which == obj.Which)
                        {
                            if (Dungeon.IsFoodScrollOrPotion(t.ItemType))
                            {
                                if (!pack_room(from_floor, obj))
                                    return;
                                t.Count++;
                                //dump_it:
                                //discard(obj);
                                obj = t;
                                lp = null;
                                goto break_out;
                            }
                            else if (obj.o_group != 0)
                            {
                                lp = t;
                                while (t.ItemType == obj.ItemType
                                    && t.Which == obj.Which
                                    && t.o_group != obj.o_group)
                                {
                                    lp = t;
                                    if (next(t) == null)
                                        break;
                                    else
                                        t = next(t);
                                }

                                if (t.ItemType == obj.ItemType
                                    && t.Which == obj.Which
                                    && t.o_group == obj.o_group)
                                {
                                    t.Count += obj.Count;
                                    inpack--;
                                    if (!pack_room(from_floor, obj))
                                        return;
                                    //goto dump_it;
                                    //discard(obj);
                                    obj = t;
                                    lp = null;
                                    goto break_out;
                                }
                            }
                            else
                                lp = t;
                        }
                    break_out:
                        break;
                    }
                }

                if (lp != null)
                {
                    if (!pack_room(from_floor, obj))
                        return;
                    else
                    {
                        obj.CharInPack = pack_char();
                        obj.l_next = next(lp);
                        obj.l_prev = lp;
                        if (next(lp) != null)
                            next(lp).l_prev = obj;
                        lp.l_next = obj;
                    }
                }
            }

            obj.o_flags |= Mob.ISFOUND;

            /*
             * If this was the object of something's desire, that monster will
             * get mad and run at the hero.
             */
            for (t = Dungeon.MobList; t != null; t = next(t))
                if (t.t_dest.Equals(obj.ItemPos))
                    t.t_dest = hero;

            if (obj.ItemType == Char.AMULET)
                Agent.amulet = true;

            //notify the user
            if (!silent)
            {
                if (!Agent.Terse)
                    addmsg("you now have ");
                AddToLog("{0} ({1})", Item.GetDescription(obj), obj.CharInPack);
            }

            obj.Name = Item.GetName(obj);
        }

        //see if there's room in pack
        static bool pack_room(bool from_floor, THING obj)
        {
            if (++inpack > MAXPACK)
            {
                if (!Agent.Terse)
                    addmsg("there's ");
                addmsg("no room");

                if (!Agent.Terse)
                    addmsg(" in your pack");

                if (from_floor)
                    move_msg(obj);

                inpack = MAXPACK;
                return false;
            }

            if (from_floor)
            {
                detach(ref Dungeon.ItemList, obj);
                Dungeon.SetCharAt(hero.y, hero.x, (Agent.Plyr.CurrRoom.Flags & Room.ISGONE) == Room.ISGONE ? Char.PASSAGE : Char.FLOOR);
            }

            return true;
        }

        //take an item out of the pack
        public static THING leave_pack(THING obj, bool newobj, bool all)
        {
            THING nobj;

            inpack--;
            nobj = obj;
            if (obj.Count > 1 && !all)
            {
                obj.Count--;
                if (obj.o_group != 0)
                    inpack++;

                if (newobj)
                {
                    nobj = obj.Copy();
                    nobj.l_next = null;
                    nobj.l_prev = null;
                    nobj.Count = 1;
                }
            }
            else
            {
                //FIXME      instead of marking a slot unused, just delete item from a dynamic pack list
                THING myPack = Agent.Plyr.Pack;
                detach(ref myPack, obj);
                Agent.Plyr.Pack = myPack;
            }
            return nobj;
        }

        //bool lookingForCallableAndCurrNotFoodAndNotAmulet(THING t, int type)
        //{
        //    return type == Int.CALLABLE &&
        //           t.ItemType != Char.FOOD && 
        //           t.ItemType != Char.AMULET;
        //}

        //bool lookingForAndFoundRingOrStick(THING t, int type)
        //{
        //    return type == Int.RingOrStick && 
        //           (t.ItemType == Char.RING || t.ItemType == Char.STICK);
        //}

        static string inv_temp;
        //list what is in the pack.  return TRUE if there is something of the given type
        static public bool ShowPackItems(THING t)
        {
            if (t == null)
            {
                AddToLog("You aren't carrying anything");
            }
            else
            {
                AddToLog("You are carrying:");
            }

            NumItems = 0;
            for (; t != null; t = next(t))
            {
                NumItems++;
                inv_temp = string.Format("{0}) {{0}}", t.CharInPack);
                S.Log(string.Format(inv_temp, Item.GetDescription(t) ) );
            }

            //if nothing in pack
            if (NumItems == 0)
            {
                if (Agent.Terse)
                    AddToLog("empty handed");
                else
                    AddToLog("you are empty handed");

                return false;
            }

            return true;
        }

        // Add something to characters pack.
        public static void PickUp(char ch)
        {
            THING obj = Dungeon.find_obj(R14.hero.y, R14.hero.x);
            PickUp(obj);
        }
        public static void PickUp(THING obj)
        {
            //this only makes sense in original Rogue.  with topdown view.  but not from a sideview (as a platformer)
            //if (on(Agent.Plyr, Mob.ISLEVIT))
            //    return;

            
            switch (obj.ItemType)
            {
                case Char.GOLD:
                    if (obj == null)
                        return;

                    money(obj.o_goldval);
                    //detach(ref Dungeon.ItemList, obj);   don't think i wanna mess with the ASCII form of the dungeon once its been
                    //      used to generate physics objects from?
                    //Agent.Plyr.CurrRoom.GoldVal = 0;      FIXME? do we care about updating a room's gold value in realtime
                    //      seems strange that its tied to a room.   

                    //      later i wanna distribute single coins (from generated gold piles) 
                    //      around the room to get players to run and jump along platforms to pick them all up.  like mario/sonic/etc.
                    break;
                default:
                case Char.ARMOUR:
                case Char.POTION:
                case Char.FOOD:
                case Char.WEAPON:
                case Char.SCROLL:
                case Char.AMULET:
                case Char.RING:
                case Char.STICK:
                    add_pack(obj, false);
                    break;
            }
        }

        // Print out the message if you are just moving onto an object (specified to not pick it up)
        static void move_msg(THING obj)
        {
            if (!Agent.Terse)
                addmsg("you ");
            AddToLog("moved onto {0}", Item.GetDescription(obj));
        }

        //get's an item to be "used".   --> what we REALLY want is to only come here knowing an item selected from inventory already.
        //ways this gets called:
        //("eat", FOOD / 
        //"quaff", POTION / 
        //"read", SCROLL / etc.) 
        //-------------------------------
        //{and both throw and wield weapons} / 
        //-------------------------------
        //-------------------------------
        //-------------------------------
        //"call", CALLABLE / 
        //"drop", 0 / 
        //"identify", type       (showing the exact parameters used)
        static public THING get_item(string specificVerbOfUse, int type)
        {
            S.Log("why do we even HAVE get_item() anymore?");
            return null;
        }

        // Add or subtract gold from the pack
        static void money(int value)
        {
            Agent.purse += value;
            //don't think below is relevant with our realtime with physics scheme?
            //      .....dunno that we'll ever wanna update 'hero' and what CurrRoom is for player???
            //Dungeon.SetCharAt(hero.y, hero.x, ((Agent.Plyr.CurrRoom.Flags & Room.ISGONE) != 0) ? Char.PASSAGE : Char.FLOOR);
            
            if (value > 0)
            {
                if (!Agent.Terse)
                    addmsg("you found ");

                AddToLog("{0} gold pieces", value);
            }
        }

        // Return the appropriate floor character for her room
        //static char floor_ch()
        //{
        //    if ((proom.Flags & ISGONE) == ISGONE)
        //        return PASSAGE;
        //    return (show_floor() ? FLOOR : ' ');
        //}

        // Return the character at hero's pos, taking see_floor
        // into account
        //static char floor_at()
        //{
        //    char ch;

        //    ch = chat(hero.y, hero.x);
        //    if (ch == FLOOR)
        //        ch = floor_ch();
        //    return ch;
        //}
    }