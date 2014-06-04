using System;
using UnityEngine;



    public static partial class Item
    {
        public static string prbuf;			/* buffer for sprintfs */
        // flags for objects
        public static int IsCursed = Convert.ToInt32("000001", 8); /* object is cursed */
        public static int ISKNOW = Convert.ToInt32("0000002", 8); /* player knows details about the object */
        public static int ISMISL = Convert.ToInt32("0000004", 8); /* object is a missile type */
        public static int ISMANY = Convert.ToInt32("0000010", 8); /* object comes in groups */
        public static int ISPROT = Convert.ToInt32("0000040", 8); /* armor is permanently protected */

        public static ObjectData[] Things = new[]
        {
            //probabilities
            new ObjectData(26), /* potion */
            new ObjectData(36), /* scroll */
            new ObjectData(16), /* food */
            new ObjectData(7), /* weapon */
            new ObjectData(7), /* armor */
            new ObjectData(4), /* ring */
            new ObjectData(4), /* stick */
        };

        static Item()
        {
            init_probs();			/* Set up prob tables for objects */
        }

        // Initialize the probabilities for the various items
        static void init_probs()
        {
            sumprobs(Item.Things, (int)ItemType.MAX);
            sumprobs(Potion.Data, Potion.MAX);
            sumprobs(Scroll.Data, Scroll.MAX);
            sumprobs(Ring.Data, Ring.MAX);
            sumprobs(Stick.Data, Stick.MAX);
            sumprobs(Weapon.Data, Weapon.MAX);
            sumprobs(Armour.Data, (int)ArmourType.Max);
        }

        // Returns true if an object radiates magic
        static public bool IsMagic(THING obj)
        {
            switch (obj.ItemType)
            {
                case Char.ARMOUR:
                    return (bool)(((obj.o_flags & Item.ISPROT) != 0) || obj.Ac != Armour.Data[obj.Which].AC);
                case Char.WEAPON:
                    return (bool)(obj.PlusToHit != 0 || obj.PlusToDmg != 0);
                case Char.POTION:
                case Char.SCROLL:
                case Char.STICK:
                case Char.RING:
                case Char.AMULET:
                    return true;
            }
            return false;
        }

        // Set things up when we really know what a thing is
        static public void set_know(THING obj, ObjectData[] info)
        {
            string guess;

            info[obj.Which].Know = true;
            obj.o_flags |= ISKNOW;
            guess = info[obj.Which].Guess;
            if (!string.IsNullOrEmpty(guess))
            {
                info[obj.Which].Guess = null;
            }
        }

        // Sum up the probabilities for items appearing
        static void sumprobs(ObjectData[] info, int bound)
        {
            for (int i = 1; i < info.Length; ++i )
                info[i].Probability += info[i - 1].Probability;
        }

        // Figure out the plus number for armour/weapons
        static string numbuf;
        static public string num(int n1, int n2, char type)
        {
            numbuf = string.Format(n1 < 0 ? "{0}" : "+{0}", n1);
            if (type == Char.WEAPON)
                numbuf += string.Format(n2 < 0 ? ",{0}" : ",+{0}", n2);
            return numbuf;
        }

        // Return a pointer to a null-length string
        static string nullstr(THING ignored)
        {
            return "";
        }

        static public int GetItemTile(THING t)
        {
			Debug.LogError("GetItemTile() needs TO BE REWRITTEN!!!!!!!!!!!!");
			return 0;




//            int i = 0;
//            
//            switch (t.ItemType)
//            {
//                case Char.FOOD:
//                    i = S.Rend.Tiles["bread"];
//                    break;
//                case Char.GOLD:
//                    i = S.Rend.Tiles["gold_pile"];
//                    break;
//                case Char.SCROLL:
//                    i = S.Rend.Tiles["scroll"];
//                    break;
//                case Char.STAIRS:
//                    i = S.Rend.Tiles[t.Name];
//                    break;
//                case Char.ARMOUR:
//                    i = S.Rend.Tiles["armour/" + Armour.Data[t.Which].Name];
//                    break;
//                case Char.POTION:
//                    i = S.Rend.Tiles["potions/" + Potion.ColorsUsed[t.Which]];
//                    break;
//                case Char.RING:
//                    i = S.Rend.Tiles["rings/" + Ring.StonesUsed[t.Which]];
//                    break;
//                case Char.WEAPON:
//                    i = S.Rend.Tiles["weapons/" + Weapon.Data[t.Which].Name];
//                    break;
//            }
//
//            return i;
        }

        static public string GetName(THING t)
        { //just the essential root name that matches media filename/s
            string root = ""; //essential root name

            switch (t.ItemType)
            {
                case Char.STAIRS:
                    root = "portal down [RMB]";
                    break;
                case Char.POTION:
                case Char.RING:
                case Char.STICK:
                case Char.SCROLL:
                    break;
                case Char.FOOD:
                    if (t.Which == 1)
                        if (t.Count == 1)
                            root = string.Format("A{0} {1}", vowelstr(R14.fruit), R14.fruit);
                        else
                            root = string.Format("{0} {1}s", t.Count, R14.fruit);
                    else
                        if (t.Count == 1)
                            root = "Some food";
                        else
                            root = string.Format("{0} rations of food", t.Count);
                    break;
                case Char.WEAPON:
                    root = Weapon.Data[t.Which].Name;
                    break;
                case Char.ARMOUR:
                    root = Armour.Data[t.Which].Name;
                    break;
                case Char.AMULET:
                    root = "The Amulet of Yendor";
                    break;
                case Char.GOLD:
                    root = string.Format("{0} Gold pieces", t.o_goldval);
                    break;
            }

            return root;
        }

        //name potion, stick, or ring
        static void nameit(THING obj, string type, string which, ObjectData od, Func<THING, string> prfunc)
        {
            if (od.Know || !string.IsNullOrEmpty(od.Guess))
            {
                if (obj.Count == 1)
                    prbuf = string.Format("A {0} ", type);
                else
                    prbuf = string.Format("{0} {1}s ", obj.Count, type);

                if (od.Know)
                    prbuf += string.Format("of {0}{1}({2})", od.Name, prfunc(obj), which);
                else if (!string.IsNullOrEmpty(od.Guess))
                    prbuf += string.Format("called {0}{1}({2})", od.Guess, prfunc(obj), which);
            }
            else if (obj.Count == 1)
                prbuf = string.Format("A{0} {1} {2}", vowelstr(which), which, type);
            else
                prbuf = string.Format("{0} {1} {2}s", obj.Count, which, type);
        }

        static public string GetDescription(THING t)
        { //inventory text that shows count, pluses, and formerly if it was worn/equipped/wielded
            string n = "";
            ObjectData d;
            string root = GetName(t); //essential root name

            switch (t.ItemType)
            {
                case Char.POTION:
                    nameit(t, "potion", Potion.ColorsUsed[t.Which], Potion.Data[t.Which], nullstr);
                    n = prbuf;
                    break;
                case Char.RING:
                    nameit(t, "ring", Ring.StonesUsed[t.Which], Ring.Data[t.Which], Ring.ProtectionMaybe);
                    n = prbuf;
                    break;
                case Char.STICK:
                    nameit(t, Stick.ws_type[t.Which], Stick.ws_made[t.Which], Stick.Data[t.Which], Stick.charge_str);
                    n = prbuf;
                    break;
                case Char.SCROLL:
                    if (t.Count == 1)
                    {
                        n = "A scroll ";
                    }
                    else
                    {
                        n = string.Format("{0} scrolls ", t.Count);
                    }

                    d = Scroll.Data[t.Which];
                    if (d.Know)
                        n += string.Format("of {0}", d.Name);
                    else if (!string.IsNullOrEmpty(d.Guess))
                        n += string.Format("called {0}", d.Guess);
                    else
                        n += string.Format("titled \"{0}\"", Scroll.s_names[t.Which]);

                    break;
                case Char.FOOD:
                    if (t.Which == 1)
                        if (t.Count == 1)
                            n = string.Format("A{0} {1}", vowelstr(R14.fruit), R14.fruit);
                        else
                            n = string.Format("{0} {1}s", t.Count, R14.fruit);
                    else
                        if (t.Count == 1)
                            n = "Some food";
                        else
                            n = string.Format("{0} rations of food", t.Count);
                    break;
                case Char.WEAPON:
                    if (t.Count > 1)
                        n = string.Format("{0} ", t.Count);
                    else
                        n = string.Format("A{0} ", vowelstr(root));

                    if ((t.o_flags & ISKNOW) != 0)
                        n += string.Format("{0} {1} {2}", t.PlusToHit, t.PlusToDmg, root);
                    else
                        n += root;

                    if (t.Count > 1)
                        n += "s";

                    if (t.o_label != null)
                    {
                        n += string.Format(" called {0}", t.o_label);
                    }

                    break;
                case Char.ARMOUR:
                    if ((t.o_flags & ISKNOW) != 0)
                    {
                        n = string.Format("{0} {1} [", Armour.Data[t.Which].AC - t.Ac, root);
                        if (!Agent.Terse)
                            n += "protection ";
                        n += string.Format("{0}]", 10 - t.Ac);
                    }
                    else
                        n = root;

                    if (t.o_label != null)
                    {
                        n += string.Format(" called {0}", t.o_label);
                    }

                    break;
                default:
                    n = root;
                    break;
            }

            return n;
        }

        static void drop()
        {
            //char ch;
            //THING obj;

            //ch = chat(hero.y, hero.x);
            //if (ch != FLOOR && ch != PASSAGE)
            //{
            //    after = false;
            //    S.Hud.Log.Add("there is something there already");
            //    return;
            //}
            //if ((obj = get_item("drop", 0)) == null)
            //    return;
            //if (!detachable(obj))
            //    return;
            //obj = leave_pack(obj, true, (bool)!ISMULT(obj.ItemType));
            ///*
            // * Link it into the level object list
            // */
            //attach(ref ItemList, obj);
            //chat(hero.y, hero.x, (char)obj.ItemType);
            //flat(hero.y, hero.x, flat(hero.y, hero.x) | F_DROPPED);
            //obj.ItemPos = hero;
            //if (obj.ItemType == AMULET)
            //    amulet = false;
            //S.Hud.Log.Add("dropped {0}", GetDescription(obj, true));
        }

        // Do special checks for dropping or unweilding|unwearing|unringing
        //static bool dropcheck(THING t)
        static public bool IsDetachable(THING t)
        {
            if (t == null)
                return true; //don't think this is needed anymore, since we're reacting to something thats there already

            //if not wearable
            if (t != Agent.cur_armor && t != Agent.CurrWeapon
            && t != Agent.cur_ring[Ring.LEFT] && t != Agent.cur_ring[Ring.RIGHT])
                return true;

            if ((t.o_flags & IsCursed) != 0)
            {
                S.Log("You can't remove " + t.Name + ".  It appears to be cursed");
                return false;
            }

            if (t == Agent.CurrWeapon)
                Agent.CurrWeapon = null;
            else if (t == Agent.cur_armor)
            {
                //waste_time();
                Agent.cur_armor = null;
            }
            else
            {
                //remove ring's magic effects
                Agent.cur_ring[t == Agent.cur_ring[Ring.LEFT] ? Ring.LEFT : Ring.RIGHT] = null;
                switch (t.Which)
                {
                    case Ring.R_ADDSTR:
                        R14.ModifyStrength(-t.Ac);
                        break;
                    case Ring.R_SEEINVIS:
                        R14.unsee(0);
                        R14.extinguish(R14.unsee);
                        break;
                }
            }
            return true;
        }

        public static int pick_one(ObjectData[] info, int nitems)
        {
            int roll = R14.rnd(100);
            int j = 0;
            foreach (var item in info)
            {
                if (roll < item.Probability)
                    return j;

                j++;
            }
            return 0;
        }

        //what the player has discovered in this game of a certain type
        static string undiscovered;
        static public void ShowAllDiscovered()
        {
            undiscovered = "";
            ShowDiscoveriesForType(Char.POTION);
            ShowDiscoveriesForType(Char.SCROLL);
            ShowDiscoveriesForType(Char.RING);
            ShowDiscoveriesForType(Char.STICK);
            S.Log(undiscovered);
        }

        public static void ShowDiscoveriesForType(char type)
        {
            ObjectData[] od = null;
            int i, max = 0, numFound;
            THING obj = new THING();

            switch (type)
            {
                case Char.SCROLL:
                    max = Scroll.MAX;
                    od = Scroll.Data;
                    break;
                case Char.POTION:
                    max = Potion.MAX;
                    od = Potion.Data;
                    break;
                case Char.RING:
                    max = Ring.MAX;
                    od = Ring.Data;
                    break;
                case Char.STICK:
                    max = Stick.MAX;
                    od = Stick.Data;
                    break;
            }

            int[] order = new int[max];
            set_order(order, max);
            obj.Count = 1;
            obj.o_flags = 0;
            numFound = 0;
            
            for (i = 0; i < max; i++)
                if (od[order[i]].Know || !string.IsNullOrEmpty(od[order[i]].Guess))
                {
                    obj.ItemType = type;
                    obj.Which = order[i];
                    S.Log(string.Format("{0}", GetDescription(obj)));
                    numFound++;
                }
            
            if (numFound == 0)
                nothing(type);
        }

        // Set up order for list
        static void set_order(int[] order, int numthings)
        {
            int i, r, t;

            for (i = 0; i < numthings; i++)
                order[i] = i;

            for (i = numthings; i > 0; i--)
            {
                r = R14.rnd(i);
                t = order[i - 1];
                order[i - 1] = order[r];
                order[r] = t;
            }
        }

        // Set up prbuf so that message for "nothing found" is there
        static string nothing(char type)
        {
            if (undiscovered == "")
            {
                if (Agent.Terse)
                    undiscovered = "Nothing";
                else
                    undiscovered = "Haven't discovered anything";

                undiscovered += " about any ";
            }

            switch (type)
            {
                case Char.POTION: return undiscovered += "potions, ";
                case Char.SCROLL: return undiscovered += "scrolls, ";
                case Char.RING:   return undiscovered += "rings, ";
                case Char.STICK:  return undiscovered += "sticks";
            }

            return undiscovered;
        }

        // For printfs: if string starts with a vowel, return "n" for an "an".
        static string vowelstr(string str)
        {
            switch (str[0])
            {
            case 'a': case 'A':
            case 'e': case 'E':
            case 'i': case 'I':
            case 'o': case 'O':
            case 'u': case 'U':
                return "n";
            default:
                return "";
            }
        }

        static public void Use(THING t)
        {
            switch(t.ItemType)
            {
                case Char.POTION: Potion.Quaff(t); break;
                case Char.SCROLL: Scroll.Read(t); break;
                case Char.WEAPON: Weapon.Wield(t); break;
            }
        }

        static public void UseSlot(int slot)
        {
            int i = -1;
            for (THING t = Agent.Plyr.Pack; t != null; t = R14.next(t))
            {
                i++;

                if (slot == i && IsDetachable(t) )
                {
                    Use(t);
                }
            }
        }

        // Call an object something after use.
        //public static void call_it(ObjectData info)
        public static void UpdateGuess(ObjectData d)
        {
            if (d.Know)
            {
                if (!string.IsNullOrEmpty(d.Guess))
                {
                    d.Guess = null;
                }
            }
            else if (string.IsNullOrEmpty(d.Guess))
                if (d.Guess != null)
                {
                    S.Log("Here's where you'll (eventually) be able to type in an optional note about the item you just used");
                    d.Guess = "Auto-named to George.  Make sure he never lacks for love";
                }
        }

        //{identifying} what a certain object is
        public static void Identify(THING t)
        {
            if (Agent.Plyr.Pack == null)
            {
                S.Log("You don't have anything in your pack to identify", Color.red);
                return;
            }

            if (t == null)
                return;

            switch (t.ItemType)
            {
                case Char.SCROLL:
                    set_know(t, Scroll.Data);
                    break;
                case Char.POTION:
                    set_know(t, Potion.Data);
                    break;
                case Char.STICK:
                    set_know(t, Stick.Data);
                    break;
                case Char.WEAPON:
                case Char.ARMOUR:
                    t.o_flags |= ISKNOW;
                    break;
                case Char.RING:
                    set_know(t, Ring.Data);
                    break;
            }

            R14.AddToLog(Item.GetDescription(t));
        }
    }