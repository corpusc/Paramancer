using System;



    static partial class R14
    {
        const int MAX_HERO_ATTACKS = 4;

        static string[] hitText = 
        {
            " hit ",
            " have injured ",
            " swing and hit ",
            " scored an excellent hit on ",

            " hit ",
            " has injured ",
            " swings and hits ",
            " scored an excellent hit on ",
        };

        static string[] missText = 
        {
            " miss",
            " swing and miss",
            " barely miss",
            " don't hit",

            " misses",
            " swings and misses",
            " barely misses",
            " doesn't hit",
        };

        // adjustments to hit probabilities due to strength
        static public int[] str_plus = 
        {
            -7, -6, -5, -4, -3, -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1,
            1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3,
        };

        // adjustments to damage done due to strength
        static public int[] add_dam = 
        {
            -7, -6, -5, -4, -3, -2, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 3,
            3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6
        };

        static public bool PlayerAttacks(Coord mp, THING weap, bool thrown)
        {
            THING tp;
            bool did_hit = true;
            string mname;
            char ch;

            /*
             * Find the monster we want to fight
             */
            tp = Dungeon.GetMonster(mp.y, mp.x);

            //since we are fighting, things are not Quiet... so no healing takes place
            Agent.Quiet = 0;
            Mob.runto(mp);
            
            //if it was really a xeroc, let him know
            ch = '\0';
            if (tp.MonsType == 'X' && tp.t_disguise != 'X' && !on(Agent.Plyr, Mob.ISBLIND))
            {
                tp.t_disguise = 'X';
                S.Log(choose_str(
                    "heavy!  That's a nasty critter!",
                    "wait!  That's a xeroc!"));

                if (!thrown)
                    return false;
            }

            mname = GetMonsterName(tp);
            did_hit = false;
            if (roll_em(Agent.Plyr, tp, weap, thrown))
            {
                did_hit = false;
                if (thrown)
                    hitRanged(weap, mname, Agent.Terse);
                else
                    hitMelee(null, mname, Agent.Terse);
                
                if (on(Agent.Plyr, Mob.CANHUH))
                {
                    did_hit = true;
                    tp.t_flags |= Mob.ISHUH;
                    Agent.Plyr.t_flags &= ~Mob.CANHUH;
                    R14.AddToLog("your hands stop glowing {0}", Potion.GetColor("red"));
                }
                if (tp.Stats.Hp <= 0)
                    killed(tp, true);
                else if (did_hit && !on(Agent.Plyr, Mob.ISBLIND))
                    R14.AddToLog("{0} appears confused", mname);
                did_hit = true;
            }
            else
                if (thrown)
                    missRanged(weap, mname, Agent.Terse);
                else
                    missMelee(null, mname, Agent.Terse);
            return did_hit;
        }

        // The monster attacks the player
        static public int AttackPlayerWith(THING monst)
        {
            string mname;
            int oldhp;

            //since this is an attack, stop running and any healing that was going on at the time
            Agent.running = false;
            Agent.Quiet = 0;

            if (monst.MonsType == 'X' && monst.t_disguise != 'X' && !on(Agent.Plyr, Mob.ISBLIND))
            {
                monst.t_disguise = 'X';
            }

            mname = GetMonsterName(monst);
            oldhp = Agent.Plyr.Stats.Hp;
            if (roll_em(monst, Agent.Plyr, null, false))
            {
                if (monst.MonsType != 'I')
                {
                    addmsg(".  ");
                    hitMelee(mname, null, false);
                }

                if (Agent.Plyr.Stats.Hp <= 0)
                    death(monst.MonsType);	/* Bye bye life ... */
                else
                {
                    oldhp -= Agent.Plyr.Stats.Hp;
                }

                if (!on(monst, Mob.ISCANC))
                    switch (monst.MonsType)
                    {
                        case 'A':
                            /*
                             * If an aquator hits, you can lose armor class.
                             */
                            RustMaybe(Agent.cur_armor);
                            break;
                        case 'I':
                            /*
                             * The ice monster freezes you
                             */
                            Agent.Plyr.t_flags &= ~Mob.ISRUN;
                            if (Agent.NumTurnsAsleep == 0)
                            {
                                addmsg("you are frozen");
                                if (!Agent.Terse)
                                    addmsg(" by the {0}", mname);
                            }
                            Agent.NumTurnsAsleep += rnd(2) + 2;
                            if (Agent.NumTurnsAsleep > BORE_LEVEL)
                                death('h');
                            break;
                        case 'R':
                            //rattlesnakes have poisonous bites
                            if (!save(VS_POISON))
                            {
                                if (!Ring.ISWEARING(Ring.R_SUSTSTR))
                                {
                                    ModifyStrength(-1);
                                    if (!Agent.Terse)
                                        S.Log("you feel a bite in your leg and now feel weaker");
                                    else
                                        S.Log("a bite has weakened you");
                                }
                                else
                                {
                                    if (!Agent.Terse)
                                        S.Log("a bite momentarily weakens you");
                                    else
                                        S.Log("bite has no effect");
                                }
                            }
                            break;
                        case 'W':
                        case 'V':
                            /*
                             * Wraiths might drain energy levels, and Vampires
                             * can steal max_hp
                             */
                            if (rnd(100) < (monst.MonsType == 'W' ? 15 : 30))
                            {
                                int fewer;

                                if (monst.MonsType == 'W')
                                {
                                    if (Agent.Plyr.Stats.Xp == 0)
                                        death('W');		/* All levels gone */
                                    if (--Agent.Plyr.Stats.Level == 0)
                                    {
                                        Agent.Plyr.Stats.Xp = 0;
                                        Agent.Plyr.Stats.Level = 1;
                                    }
                                    else
                                        Agent.Plyr.Stats.Xp = Agent.ExperienceGoals[Agent.Plyr.Stats.Level - 1] + 1;
                                    fewer = RollDice(1, 10);
                                }
                                else
                                    fewer = RollDice(1, 3);

                                Agent.Plyr.Stats.Hp -= fewer;
                                Agent.Plyr.Stats.HpMax -= fewer;

                                if (Agent.Plyr.Stats.Hp <= 0)
                                    Agent.Plyr.Stats.Hp = 1;

                                if (Agent.Plyr.Stats.HpMax <= 0)
                                    death(monst.MonsType);
                                S.Log("you suddenly feel weaker");
                            }
                            break;
                        case 'F':
                            //venus Flytrap stops the poor guy from moving
                            Agent.Plyr.t_flags |= Mob.ISHELD;
                            monsters['F' - 'A'].Stats.Dmg = string.Format("{0}x1", ++Agent.NumTimesFlytrapHasHit);
                            if (--Agent.Plyr.Stats.Hp <= 0)
                                death('F');
                            break;
                        case 'L':
                            {
                                //leprechaun steals some gold
                                int lastpurse;

                                lastpurse = Agent.purse;
                                Agent.purse -= GOLDCALC();
                                if (!save(VS_MAGIC))
                                    Agent.purse -= GOLDCALC() + GOLDCALC() + GOLDCALC() + GOLDCALC();
                                if (Agent.purse < 0)
                                    Agent.purse = 0;
                                remove_mon(monst.t_pos, monst, false);
                                monst = null;
                                if (Agent.purse != lastpurse)
                                    S.Log("your purse feels lighter");
                            }
                            break;
                        case 'N':
                            {
                                THING obj, steal;
                                int nobj;

                                //nymph's steal a magic item. look through the pack and pick out one she likes
                                steal = null;
                                for (nobj = 0, obj = Agent.Plyr.Pack; obj != null; obj = next(obj))
                                    if (obj != Agent.cur_armor && obj != Agent.CurrWeapon
                                        && obj != Agent.cur_ring[Ring.LEFT] && obj != Agent.cur_ring[Ring.RIGHT]
                                        && Item.IsMagic(obj) && rnd(++nobj) == 0)
                                        steal = obj;
                                if (steal != null)
                                {
                                    remove_mon(monst.t_pos, Dungeon.GetMonster(monst.t_pos.y, monst.t_pos.x), false);
                                    monst = null;
                                    leave_pack(steal, false, false);
                                    R14.AddToLog("she stole {0}!", Item.GetDescription(steal));
                                }
                            }
                            break;
                        default:
                            break;
                    }
            }
            else if (monst.MonsType != 'I')
            {
                addmsg(".  ");

                if (monst.MonsType == 'F')
                {
                    Agent.Plyr.Stats.Hp -= Agent.NumTimesFlytrapHasHit;
                    if (Agent.Plyr.Stats.Hp <= 0)
                        death(monst.MonsType);	/* Bye bye life ... */
                }

                missMelee(mname, null, false);
            }

            if (monst == null)
                return (-1);
            else
                return (0);
        }

        // Returns true if the swing hits
        static bool swing(int at_lvl, int op_arm, int wplus)
        {
            int res = rnd(20);
            int need = (20 - at_lvl) - op_arm;

            return (res + wplus >= need);
        }

        // Roll several attacks
        static public bool roll_em(THING att, THING def, THING weap, bool hurl)
        {
            string cp;
            int ndice, nsides, def_arm;
            bool did_hit = false;
            int hplus;
            int dplus;
            int damage;

            if (weap == null)
            {
                cp = att.Stats.Dmg;
                dplus = 0;
                hplus = 0;
            }
            else
            {
                hplus = (weap == null ? 0 : weap.PlusToHit);
                dplus = (weap == null ? 0 : weap.PlusToDmg);
                if (weap == Agent.CurrWeapon)
                {
                    if (Ring.ISRING(Ring.LEFT, Ring.R_ADDDAM))
                        dplus += Agent.cur_ring[Ring.LEFT].Ac;
                    else if (Ring.ISRING(Ring.LEFT, Ring.R_ADDHIT))
                        hplus += Agent.cur_ring[Ring.LEFT].Ac;
                    if (Ring.ISRING(Ring.RIGHT, Ring.R_ADDDAM))
                        dplus += Agent.cur_ring[Ring.RIGHT].Ac;
                    else if (Ring.ISRING(Ring.RIGHT, Ring.R_ADDHIT))
                        hplus += Agent.cur_ring[Ring.RIGHT].Ac;
                }
                cp = weap.swing___o_damage;
                if (hurl)
                {
                    if (((weap.o_flags & Item.ISMISL) != 0) && Agent.CurrWeapon != null &&
                        Agent.CurrWeapon.Which == weap.neededTo___o_launch)
                    {
                        cp = weap.o_hurldmg;
                        hplus += Agent.CurrWeapon.PlusToHit;
                        dplus += Agent.CurrWeapon.PlusToDmg;
                    }
                    else if (weap.neededTo___o_launch < 0)
                        cp = weap.o_hurldmg;
                }
            }
            
            //if the creature being attacked is 
            //not running (alseep or held)
            //then the attacker gets a plus four bonus to hit
            if (!on(def, Mob.ISRUN))
                hplus += 4;

            def_arm = def.Stats.Ac;
            if (def.Stats == Agent.Plyr.Stats)
            {
                if (Agent.cur_armor != null)
                    def_arm = Agent.cur_armor.Ac;
                if (Ring.ISRING(Ring.LEFT, Ring.R_PROTECT))
                    def_arm -= Agent.cur_ring[Ring.LEFT].Ac;
                if (Ring.ISRING(Ring.RIGHT, Ring.R_PROTECT))
                    def_arm -= Agent.cur_ring[Ring.RIGHT].Ac;
            }

            int i = 0;
            while (!string.IsNullOrEmpty(cp))
            {
                ndice = atoi(cp, i);
                if ((i = cp.IndexOf('x', i)) == -1)
                    break;
                nsides = atoi(cp, ++i);
                if (swing(att.Stats.Level, def_arm, hplus + str_plus[att.Stats.Str]))
                {
                    int proll;

                    proll = RollDice(ndice, nsides);
                    damage = dplus + proll + add_dam[att.Stats.Str];
                    def.Stats.Hp -= Math.Max(0, damage);
                    did_hit = true;
                }
                if ((i = cp.IndexOf('/', i)) == -1)
                    break;
                i++;
            }
            return did_hit;
        }

        static int atoi(string s, int i)
        {
            int n;
            for (n = i; n < s.Length; n++)
            {
                if (!char.IsDigit(s[n])) break;
            }
            int num = 0;
            if (n > i)
                num = int.Parse(s.Substring(i, n - i));
            return num;
        }

        // The print name of a combatant
        static public string prname(string mname, bool upper)
        {
            string tbuf;

            if (mname == null)
                tbuf = "you";
            else
                tbuf = mname;
            if (upper)
                tbuf = tbuf.Substring(0, 1).ToUpper() + tbuf.Substring(1);
            return tbuf;
        }

        static public void hitRanged(THING weap, string mname, bool noend)
        {
            if (weap.ItemType == Char.WEAPON)
                addmsg("the {0} hits ", Weapon.Data[weap.Which].Name);
            else
                addmsg("you hit ");

            addmsg("{0}", mname);
        }

        // Print a message to indicate a succesful hit
        static public void hitMelee(string er, string ee, bool noend)
        {
            int i;
            string s;

            addmsg(prname(er, true));
            if (Agent.Terse)
                s = " hit";
            else
            {
                i = rnd(MAX_HERO_ATTACKS);
                if (er != null)
                    i += 4;
                s = hitText[i];
            }
            addmsg(s);
            if (!Agent.Terse)
                addmsg(prname(ee, false));
        }

        // Print a message to indicate a poor swing
        static void missMelee(string er, string ee, bool noend)
        {
            int i;

            addmsg(prname(er, true));
            if (Agent.Terse)
                i = 0;
            else
                i = rnd(MAX_HERO_ATTACKS);
            if (er != null)
                i += 4;
            addmsg(missText[i]);
            if (!Agent.Terse)
                addmsg(" {0}", prname(ee, false));
        }

        static void missRanged(THING weap, string mname, bool noend)
        {
            if (weap.ItemType == Char.WEAPON)
                addmsg("the {0} misses ", Weapon.Data[weap.Which].Name);
            else
                addmsg("you missed ");

            addmsg(mname);
        }

        // Remove a monster from the screen
        static void remove_mon(Coord mp, THING tp, bool waskill)
        {
            THING obj, nexti;

            for (obj = tp.Pack; obj != null; obj = nexti)
            {
                nexti = next(obj);
                obj.ItemPos = tp.t_pos;
                detach(ref tp.Pack, obj);
                
                if (waskill)
                    Dungeon.fall(obj, false);
            }

            S.Log("remove_mon() needs fleshing out");

            //Dungeon.SetMonster(mp.y, mp.x, null);
            //detach(ref Dungeon.MobList, tp); //             wanna leave the original spawns static for the ASCII version of dungeon???
        }

        // Called to put a monster to death
        static public void killed(THING tp, bool pr)
        {
            string mname;

            Agent.Plyr.Stats.Xp += tp.Stats.Xp;

            // if the monster was a venus flytrap, un-hold him 
            switch (tp.MonsType)
            {
                case 'F':
                    Agent.Plyr.t_flags &= ~Mob.ISHELD;
                    Agent.NumTimesFlytrapHasHit = 0;
                    monsters['F' - 'A'].Stats.Dmg = "000x0";
                    break;
                case 'L':
                    {
                        THING gold;

                        if (Weapon.PickRndPosAroundYX(tp.t_pos, tp.CurrRoom.GoldPos) 
                            && Agent.LevelOfMap >= Agent.dyn___max_level)
                        {
                            gold = new THING();
                            gold.ItemType = Char.GOLD;
                            gold.o_goldval = GOLDCALC();

                            if (save(VS_MAGIC))
                                gold.o_goldval += GOLDCALC() + GOLDCALC() + GOLDCALC() + GOLDCALC();

                            attach(ref tp.Pack, gold);
                        }
                    }
                    break;
            }
            
            // get rid of the monster 
            mname = R14.GetMonsterName(tp);
            remove_mon(tp.t_pos, tp, true);

            if (pr)
            {
                if (!Agent.Terse)
                    addmsg("you have ");

                addmsg("defeated ");
                R14.AddToLog(mname);
            }

            Agent.LevelUpPlayerMaybe(Agent.Pos);
        }
    }