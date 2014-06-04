using System;



    static partial class R14
    {
        // A healing daemon that restors hit points after rest
        static void doctor(int ignored)
        {
            //int lv, ohp;
            //var pstats = Agent.Plyr.Stats;

            //lv = pstats.Level;
            //ohp = pstats.Hp;
            //Quiet++;
            //if (lv < 8)
            //{
            // if (Quiet + (lv << 1) > 20)
            //        pstats.Hp++;
            //}
            //else if (Quiet >= 3)
            //    pstats.Hp += rnd(lv - 7) + 1;
            //if (Ring.ISRING(Ring.LEFT, Ring.R_REGEN))
            //    pstats.Hp++;
            //if (Ring.ISRING(Ring.RIGHT, Ring.R_REGEN))
            //    pstats.Hp++;
            //if (ohp != pstats.Hp)
            //{
            //    if (pstats.Hp > max_hp)
            //        pstats.Hp = max_hp;
            //    Quiet = 0;
            //}
        }

        // Called when it is time to start rolling for wandering monsters
        static void swander(int ignored)
        {
            start_daemon(rollwand, 0, BEFORE);
        }

        static int between = 0;

        // Called to roll to see if a wandering monster starts up
        static void rollwand(int ignored)
        {
            if (++between >= 4)
            {
                if (RollDice(1, 6) == 4)
                {
                    wanderer();
                    kill_daemon(rollwand);
                    LightFuse(swander, 0, WANDERTIME, BEFORE);
                }
                between = 0;
            }
        }

        // Release the poor player from his confusion
        public static void unconfuse(int ignored)
        {
            Agent.Plyr.t_flags &= ~Mob.ISHUH;
            AddToLog("you feel less {0} now", choose_str("trippy", "confused"));
        }

        // Turn off the ability to see invisible
        public static void unsee(int ignored)
        {
            THING t;

            for (t = Dungeon.MobList; t != null; t = next(t))
                if (on(t, Mob.ISINVIS) && heroCanSeeMonster(t))
                    S.Log("this is where we'd: R12.MvAddCh(th.t_pos.y, th.t_pos.x, th.t_oldch)");
            Agent.Plyr.t_flags &= ~Mob.CANSEE;
        }

        // He gets his sight back
        public static void sight(int ignored)
        {
            //if (on(player, ISBLIND))
            //{
            //    extinguish(sight);
            //    player.t_flags &= ~ISBLIND;
            //    if (!((proom.Flags & ISGONE) != 0))
            //        enter_room(hero);
            //    AddToLog(choose_str("far out!  Everything is all cosmic again",
            //               "the veil of darkness lifts"));
            //}
        }

        // End the hasting
        static public void StopHaste(int ignored)
        {
            Agent.Plyr.t_flags &= ~Mob.ISHASTE;
            AddToLog("you feel yourself slowing down");
        }

        // Digest the hero's food
        static void stomach(int ignored)
        {
            //int oldfood;
            int orig_hungry = Agent.hungry_state;

            //if (food_left <= 0)
            //{
            //    if (food_left-- < -STARVETIME)
            //        death('s');
            //    /*
            //     * the hero is fainting
            //     */
            //    if ((NumTurnsAsleep != 0) || (rnd(5) != 0))
            //        return;
            //    NumTurnsAsleep += rnd(8) + 4;
            //    hungry_state = 3;
            //    if (!Agent.Terse)
            //        addmsg(choose_str("the munchies overpower your motor capabilities.  ",
            //              "you feel too weak from lack of food.  "));
            //    AddToLog(choose_str("You freak out", "You faint"));
            //}
            //else
            //{
            //    oldfood = food_left;
            //    food_left -= ring_eat(LEFT) + ring_eat(RIGHT) + 1 - (amulet ? 1 : 0);

            //    if (food_left < MORETIME && oldfood >= MORETIME)
            //    {
            //        hungry_state = 2;
            //        AddToLog(choose_str("the munchies are interfering with your motor capabilites",
            //               "you are starting to feel weak"));
            //    }
            //    else if (food_left < 2 * MORETIME && oldfood >= 2 * MORETIME)
            //    {
            //        hungry_state = 1;
            //        if (Agent.Terse)
            //            AddToLog(choose_str("getting the munchies", "getting hungry"));
            //        else
            //            AddToLog(choose_str("you are getting the munchies",
            //                   "you are starting to get hungry"));
            //    }
            //}

            if (Agent.hungry_state != orig_hungry)
            {
                Agent.Plyr.t_flags &= ~Mob.ISRUN; 
                Agent.running = false; //FIXME: both .ISRUN & also a seperate bool outside of mob flags for player?
            }
        }

        // change the characters for the player
        static public void visuals(int ignored)
        {
            //THING tp;
            //bool seemonst;

            //if (!after || (running && ShowRunningAsSeriesOf___jump))
            //    return;
            ///*
            // * change the things
            // */
            //for (tp = ItemList; tp != null; tp = next(tp))
            //    if (HeroCanSee(tp.ItemPos.y, tp.ItemPos.x))
            //        R12.MvAddCh(tp.ItemPos.y, tp.ItemPos.x, rnd_thing());

            ///*
            // * change the stairs
            // */
            //if (!seenstairs && HeroCanSee(stairs.y, stairs.x))
            //    R12.MvAddCh(stairs.y, stairs.x, rnd_thing());

            ///*
            // * change the monsters
            // */
            //seemonst = on(player, CanSeeInvisible);
            //for (tp = MobList; tp != null; tp = next(tp))
            //{
            //    R12.move(tp.t_pos.y, tp.t_pos.x);
            //    if (heroCanSeeMonster(tp))
            //    {
            //        if (tp.MonsType == 'X' && tp.t_disguise != 'X')
            //            R12.addch(rnd_thing());
            //        else
            //            R12.addch(rnd(26) + 'A');
            //    }
            //    else if (seemonst)
            //    {
            //        R12.standout();
            //        R12.addch(rnd(26) + 'A');
            //        R12.standend();
            //    }
            //}
        }

        // Land from a levitation potion
        public static void land(int ignored)
        {
            Agent.Plyr.t_flags &= ~Mob.ISLEVIT;
            AddToLog(choose_str("bummer!  You've hit the ground",
                   "you float gently to the ground"));
        }
    }