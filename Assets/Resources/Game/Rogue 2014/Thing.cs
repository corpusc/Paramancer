using System;
//using System.Collections.Generic;



	static partial class R14
    {
        public static THING GetNewRandomThing()
        {
            THING cur;
            int r;

            cur = new THING();
            cur.PlusToHit = 0;
            cur.PlusToDmg = 0;
            cur.swing___o_damage = "0x0";
            cur.o_hurldmg = "0x0";
            cur.Ac = 11;
            cur.Count = 1;
            cur.o_group = 0;
            cur.o_flags = 0;

            //pick thing type.  make it food if its been scarce
            ItemType tt = 
                (Agent.no_food > 3)
                ? ItemType.Food 
				: (ItemType)Item.pick_one(Item.Things, (int)ItemType.MAX);
            switch (tt)
            {
                case ItemType.Potion:
                    cur.ItemType = Char.POTION;
					cur.Which = Item.pick_one(Potion.Data, Potion.MAX);
                    break;
                case ItemType.Scroll:
                    cur.ItemType = Char.SCROLL;
					cur.Which = Item.pick_one(Scroll.Data, Scroll.MAX);
                    break;
                case ItemType.Food:
                    cur.ItemType = Char.FOOD;
                    Agent.no_food = 0;
                    if (rnd(10) != 0)
                        cur.Which = 0;
                    else
                        cur.Which = 1;
                    break;
                case ItemType.Weapon:
					Weapon.Init(cur, Item.pick_one(Weapon.Data, Weapon.MAX));
                    if ((r = rnd(100)) < 10)
                    {
						cur.o_flags |= Item.IsCursed;
                        cur.PlusToHit -= rnd(3) + 1;
                    }
                    else if (r < 15)
                        cur.PlusToHit += rnd(3) + 1;
                    break;
                case ItemType.Armour:
                    cur.ItemType = Char.ARMOUR;
					cur.Which = Item.pick_one(Armour.Data, (int)ArmourType.Max);
                    cur.Ac = Armour.Data[cur.Which].AC;
                    if ((r = rnd(100)) < 20)
                    {
						cur.o_flags |= Item.IsCursed;
                        cur.Ac += rnd(3) + 1;
                    }
                    else if (r < 28)
                        cur.Ac -= rnd(3) + 1;
                    break;
                case ItemType.Ring:
                    cur.ItemType = Char.RING;
					cur.Which = Item.pick_one(Ring.Data, Ring.MAX);
                    switch (cur.Which)
                    {
                        case Ring.R_ADDSTR:
                        case Ring.R_PROTECT:
                        case Ring.R_ADDHIT:
                        case Ring.R_ADDDAM:
                            if ((cur.Ac = rnd(3)) == 0)
                            {
                                cur.Ac = -1;
                                cur.o_flags |= Item.IsCursed;
                            }
                            break;
                        case Ring.R_AGGR:
                        case Ring.R_TELEPORT:
                            cur.o_flags |= Item.IsCursed;
                            break;
                    }
                    break;
                case ItemType.Stick:
                    cur.ItemType = Char.STICK;
                    cur.Which = Item.pick_one(Stick.Data, Stick.MAX);
                    Stick.fix_stick(cur);
                    break;
            }

            cur.Name = Item.GetDescription(cur);
            Console.WriteLine("made (" + (char)cur.ItemType + ") " + cur.Name);
            return cur;
        }
    }

    public class THING //monsters are derived from this
    {
        public string Name = "InitName";

        public THING l_next; /* Next pointer in link */
        public THING l_prev;

        public bool t_turn; /* If slowed, is it a turn to move */
        public int Which; /* Which object of that thing type */
        public char t_disguise; /* What mimic looks like */
        public char t_oldch; /* Character that was where it was */
        public Coord t_dest = new Coord(); /* Where it is running to */
        public stats Stats; /* Physical description */
        public room CurrRoom; /* Current room for thing */
        public THING Pack; /* What the thing is carrying */

        public char MonsType; /* What type of thing (originally t_type*/
        public int ItemType; /* What char for that kind of thing (originally o_type) */

        public Coord ItemPos = new Coord(); /* Where it lives on the screen (o_pos) */
        public Coord t_pos = new Coord(); /* Position */

        public int t_flags;
        public int o_flags; /* information about objects */

        public string o_text; /* What it says if you read it */
        public int neededTo___o_launch; /* What you need to launch it */
        public char CharInPack; /* What character it is in the pack */
        public string swing___o_damage; /* Damage if used like sword */
        public string o_hurldmg; /* Damage if thrown */
        public int Count; /* count for plural objects */
        public int o_charges; // FIXME? to combine charges, goldval & Count?  think those are mutually exclusive numbers?  as in, only one of those 3 is relevant/used depending on object type?  'count' makes sense for all?  not so intuitive with wands/staves, but those obviously don't need to stack, and don't think they do
        public int o_goldval;
        public int PlusToHit;
        public int PlusToDmg;
        public int Ac; /* Armor protection */
        public int o_group; /* group number for this object */
        public string o_label; /* Label for object */


        public THING Copy()
        {
            var newThing = (THING)MemberwiseClone();
            newThing.t_pos = new Coord();
            newThing.t_pos = t_pos;
            newThing.ItemPos = new Coord();
            newThing.ItemPos = ItemPos;
            return newThing;
        }
    }