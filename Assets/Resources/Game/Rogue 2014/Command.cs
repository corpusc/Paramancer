using System;

    static partial class R14
    {
        //static private char countch, direction;
        //static bool newcount = false;

        //// Process the user commands
        //void command(char ch) {
        //    //recent: char ch;
        //    int numMoves___ntimes = 1;			/* Number of player moves */
        //    int fp;
        //    THING mp;

        //    if (on(player, ISHASTE))
        //        numMoves___ntimes++;
        //    /*
        //     * Let the daemons start up
        //     */
        //    do_daemons(BEFORE);
        //    do_fuses(BEFORE);
        //    while (numMoves___ntimes-- != 0) {
        //        again = false;
        //        if (has_hit) {
        //            waitForSpaceLeavingPrevMsgVis___endmsg();
        //            has_hit = false;
        //        }
        //        /*
        //         * these are illegal things for the player to be, so if any are
        //         * set, someone's been poking in memory
        //         */
        //        if (on(player, ISSLOW | ISGREED | ISINVIS | ISREGEN))
        //            Environment.Exit(1);

        //        glanceAroundHero___look(true);
        //        door_stop = false;
        //        display___status();
        //        lastscore = purse;
        //        R12.move(hero.y, hero.x);
        //        if (
        //            !(
        //                (running || commandRepeat___count != 0) && ShowRunningAsSeriesOf___jump
        //            )
        //        )
        //            R12.Present__refresh();			/* Draw screen */
        //        take = '\0';
        //        after = true;
        //        /*
        //         * Read command or continue run
        //         */
        //        if (NumTurnsAsleep == 0) {
        //            if (commandRepeat___count != 0)
        //                ch = countch;
        //            else {
        //                //recent: ch = readchar();
        //                move_on = false;
        //                if (topLineCursorPos___mpos != 0)		/* Erase message if its there */
        //                    showAtTopOfScreen___msg("");
        //            }
        //        } else
        //            ch = '.';

        //        if (NumTurnsAsleep != 0) {
        //            if (--NumTurnsAsleep == 0) {
        //                player.t_flags |= ISRUN;
        //                showAtTopOfScreen___msg("you can move again");
        //            }
        //        } else {
        //            /*
        //             * check for prefixes
        //             */
        //            newcount = false;
        //            if (char.IsDigit(ch)) {
        //                commandRepeat___count = 0;
        //                newcount = true;
        //                while (char.IsDigit(ch)) {
        //                    commandRepeat___count = commandRepeat___count * 10 + (ch - '0');
        //                    if (commandRepeat___count > 255)
        //                        commandRepeat___count = 255;
        //                    //recent:     ch = readchar();
        //                }
        //                countch = ch;
        //                /*
        //                 * turn off count for commands which don't make sense
        //                 * to repeat
        //                 */
        //                switch (ch) {
        //                    case (char)2:
        //                    case (char)8:
        //                    case (char)10:
        //                    case (char)11:
        //                    case (char)12:
        //                    case (char)14:
        //                    case (char)21:
        //                    case (char)25:
        //                    case '.':
        //                    case 'a':
        //                    case 'b':
        //                    case 'h':
        //                    case 'j':
        //                    case 'k':
        //                    case 'l':
        //                    case 'm':
        //                    case 'n':
        //                    case 'q':
        //                    case 'r':
        //                    case 's':
        //                    case 't':
        //                    case 'u':
        //                    case 'y':
        //                    case 'z':
        //                    case 'B':
        //                    case 'C':
        //                    case 'H':
        //                    case 'I':
        //                    case 'J':
        //                    case 'K':
        //                    case 'L':
        //                    case 'N':
        //                    case 'U':
        //                    case 'Y':
        //                        break;
        //                    default:
        //                        commandRepeat___count = 0;
        //                        break;
        //                }
        //            }
        //            /*
        //             * execute a command
        //             */
        //            if (commandRepeat___count != 0 && !running)
        //                commandRepeat___count--;
        //            if (ch != 'a' && ch != ESCAPE && !(running || (commandRepeat___count != 0) || to_death)) {
        //                l_last_comm = last_comm;
        //                l_last_dir = last_dir;
        //                l_last_pick = last_pick;
        //                last_comm = ch;
        //                last_dir = '\0';
        //                last_pick = null;
        //            }


        
        //        over:
        //            switch (ch) {
        //                case ',': {
        //                        THING obj = null;
        //                        int found = 0;
        //                        for (obj = ItemList; obj != null; obj = next(obj)) {
        //                            if (obj.ItemPos.y == hero.y && obj.ItemPos.x == hero.x) {
        //                                found = 1;
        //                                break;
        //                            }
        //                        }
        //                        if (found != 0) {
        //                            if (!levit_check())
        //                                PickUp((char)obj.ItemType);
        //                        } else {
        //                            if (!Agent.Terse)
        //                                addmsg("there is ");
        //                            addmsg("nothing here");
        //                            if (!Agent.Terse)
        //                                addmsg(" to pick up");
        //                            waitForSpaceLeavingPrevMsgVis___endmsg();
        //                        }
        //                        break;
        //                    }
        //                case 'h': do_move(0, -1); break;
        //                case 'j': do_move(1, 0); break;
        //                case 'k': do_move(-1, 0); break;
        //                case 'l': do_move(0, 1); break;
        //                case 'y': do_move(-1, -1); break;
        //                case 'u': do_move(-1, 1); break;
        //                case 'b': do_move(1, -1); break;
        //                case 'n': do_move(1, 1); break;
        //                case (char)8:
        //                case (char)10:
        //                case (char)11:
        //                case (char)12:
        //                case (char)25:
        //                case (char)21:
        //                case (char)2:
        //                case (char)14: {
        //                        if (!on(player, ISBLIND)) {
        //                            door_stop = true;
        //                            firstmove = true;
        //                        }
        //                        if (commandRepeat___count != 0 && !newcount)
        //                            ch = direction;
        //                        else {
        //                            ch = (char)(ch + ('A' - CTRL('A')));
        //                            direction = ch;
        //                        }
        //                        goto over;
        //                    }
        //                case 'F':
        //                    kamikaze = true;
        //                    goto case 'f';
        //                case 't':
        //                    if (!get_dir())
        //                        after = false;
        //                    else
        //                        missile(delta.y, delta.x);
        //                    break;
        //                case 'a':
        //                    if (last_comm == '\0') {
        //                        showAtTopOfScreen___msg("you haven'tag typed a command yet");
        //                        after = false;
        //                    } else {
        //                        ch = last_comm;
        //                        again = true;
        //                        goto over;
        //                    }
        //                    break;
        //                case 'q': Quaff(); break;
        //                case 'Q':
        //                    after = false;
        //                    q_comm = true;
        //                    quit(0);
        //                    q_comm = false;
        //                    break;
        //                case 'i': after = false; listInven___inventory(pack, 0); break;
        //                case 'I': after = false; picky_inven(); break;
        //                case 'd': drop(); break;
        //                case 'r': read_scroll(); break;
        //                case 'e': eat(); break;
        //                case 'w': wield(); break;
        //                case 'W': wear(); break;
        //                case 'T': take_off(); break;
        //                case 'P': ring_on(); break;
        //                case 'R': ring_off(); break;
        //                //when 'o': option(); after = FALSE;
        //                case 'c': call(); after = false; break;
        //                case '>': after = false; d_level(); break;
        //                case '<': after = false; u_level(); break;
        //                case '?': after = false; help(); break;
        //                case '/': after = false; identify(); break;
        //                case 's': search(); break;
        //                case 'z':
        //                    if (get_dir())
        //                        do_zap();
        //                    else
        //                        after = false;
        //                    break;
        //                case 'D': after = false; discovered(); break;
        //                case (char)16: after = false; showAtTopOfScreen___msg(lastMsgShown___huh); break; // Ctrl+P
        //                //when CTRL('R'):
        //                //    after = FALSE;
        //                //    clearok(curscr,TRUE);
        //                //    wrefresh(curscr);
        //                //when 'v':
        //                //    after = FALSE;
        //                //    msg("version %s. (mctesq was here)", release);
        //                //when 'S': 
        //                //    after = FALSE;
        //                //    save_game();
        //                case '.': break;			/* Rest command */
        //                case ' ': after = false; break;	/* "Legal" illegal command */
        //                case '^':
        //                    after = false;
        //                    if (get_dir()) {
        //                        delta.y += hero.y;
        //                        delta.x += hero.x;
        //                        fp = flat(delta.y, delta.x);
        //                        if (!Agent.Terse)
        //                            addmsg("You have found ");
        //                        if (chat(delta.y, delta.x) != TRAP)
        //                            showAtTopOfScreen___msg("no trap there");
        //                        else if (on(player, IsTrippinBalls))
        //                            showAtTopOfScreen___msg(tr_name[rnd(NTRAPS)]);
        //                        else {
        //                            showAtTopOfScreen___msg(tr_name[fp & F_TMASK]);
        //                            fp |= F_SEEN;
        //                            flat(delta.y, delta.x, fp);
        //                        }
        //                    }
        //                    break;
        //                case (char)ESCAPE:	/* Escape */
        //                    door_stop = false;
        //                    commandRepeat___count = 0;
        //                    after = false;
        //                    again = false;
        //                    break;
        //                case 'm':
        //                    move_on = true;
        //                    if (!get_dir())
        //                        after = false;
        //                    else {
        //                        ch = dir_ch;
        //                        countch = dir_ch;
        //                        goto over;
        //                    }
        //                    break;
        //                case ')': current(CurrWeapon, "wielding", null); break;
        //                case ']': current(cur_armor, "wearing", null); break;
        //                case '=':
        //                    current(cur_ring[LEFT], "wearing",
        //                                Terse ? "(L)" : "on left hand");
        //                    current(cur_ring[RIGHT], "wearing",
        //                                Terse ? "(R)" : "on right hand");
        //                    break;
        //                case '@':
        //                    stat_msg = true;
        //                    display___status();
        //                    stat_msg = false;
        //                    after = false;
        //                    break;
        //                default:
        //                    after = false;
        //                    if (wizard)
        //                        switch (ch) {
        //                            case '|': showAtTopOfScreen___msg("our 'hero' is @ %d,%d", hero.y, hero.x); break;
        //                            //when '$': msg("inpack = %d", inpack);
        //                            //when CTRL('G'): inventory(ItemList, 0);
        //                            //when CTRL('W'): Identify(FALSE, 0);
        //                            case (char)4: level++; new_level(); break; // Ctrl+D
        //                            case (char)1: level--; new_level(); break; // Ctrl+A
        //                            case (char)6: show_map(); break; // Ctrl+F
        //                            //when CTRL('E'): msg("food left: %d", food_left);
        //                            //when CTRL('C'): add_pass();
        //                            case (char)24: turn_see(on(player, CanSeeInvisible) ? 1 : 0); break; // Ctrl+X
        //                            default: illcom(ch); break;
        //                        } 
        //                    else // not "wizard"
        //                        illcom(ch);
        //                    break;
        //            }
        //            /*
        //             * turn off flags if no longer needed
        //             */
        //            door_stop = false;
        //        }
        //        /*
        //         * If he ran into something to take, let him pick it up.
        //         */
        //        if (take != 0)
        //            PickUp(take);
        //        if (!running)
        //            door_stop = false;
        //        if (!after)
        //            numMoves___ntimes++;
        //    }
        //    do_daemons(AFTER);
        //    do_fuses(AFTER);
        //    if (ISRING(LEFT, R_SEARCH))
        //        search();
        //    else if (ISRING(LEFT, R_TELEPORT) && rnd(50) == 0)
        //        teleport();
        //    if (ISRING(RIGHT, R_SEARCH))
        //        search();
        //    else if (ISRING(RIGHT, R_TELEPORT) && rnd(50) == 0)
        //        teleport();
        //}

        //// What to do with an illegal command
        //void illcom(int ch) {
        //    saveLastMsg___save_msg = false;
        //    commandRepeat___count = 0;
        //    showAtTopOfScreen___msg("illegal command '{0}'", R12.GetSupposedlyPrintable___unctrl((char)ch));
        //    showAtTopOfScreen___msg("illEST  command '{0}'", R12.GetSupposedlyPrintable___unctrl((char)ch));
        //    saveLastMsg___save_msg = true;
        //}

        //// player gropes about him to find hidden things.
        //void search() {
        //    int y, x;
        //    int fp;
        //    int ey, ex;
        //    int probinc;
        //    bool found;

        //    ey = hero.y + 1;
        //    ex = hero.x + 1;
        //    probinc = (on(player, IsTrippinBalls) ? 3 : 0);
        //    probinc += (on(player, ISBLIND) ? 2 : 0);
        //    found = false;
        //    for (y = hero.y - 1; y <= ey; y++)
        //        for (x = hero.x - 1; x <= ex; x++) {
        //            if (y == hero.y && x == hero.x)
        //                continue;
        //            fp = flat(y, x);
        //            if (!((fp & F_REAL) != 0))
        //                switch (chat(y, x)) {
        //                    case '|':
        //                    case '-':
        //                        if (rnd(5 + probinc) != 0)
        //                            break;
        //                        chat(y, x, DOOR);
        //                        showAtTopOfScreen___msg("a secret door");
        //                    foundone:
        //                        found = true;
        //                        fp |= F_REAL;
        //                        flat(y, x, fp);
        //                        commandRepeat___count = 0;
        //                        running = false;
        //                        break;
        //                    case FLOOR:
        //                        if (rnd(2 + probinc) != 0)
        //                            break;
        //                        chat(y, x, TRAP);
        //                        if (!Agent.Terse)
        //                            addmsg("you found ");
        //                        if (on(player, IsTrippinBalls))
        //                            showAtTopOfScreen___msg(tr_name[rnd(NTRAPS)]);
        //                        else {
        //                            showAtTopOfScreen___msg(tr_name[fp & F_TMASK]);
        //                            fp |= F_SEEN;
        //                            flat(y, x, fp);
        //                        }
        //                        goto foundone;
        //                    case ' ':
        //                        if (rnd(3 + probinc) != 0)
        //                            break;
        //                        chat(y, x, PASSAGE);
        //                        goto foundone;
        //                }
        //        }
        //    if (found)
        //        glanceAroundHero___look(false);
        //}

        //// Give single character help, or the whole mess if he wants it
        //void help() {
        //    char helpch;
        //    int numprint, cnt;
        //    showAtTopOfScreen___msg("character you want help for (* for all): ");
        //    helpch = '*'; //recent:     readchar();
        //    topLineCursorPos___mpos = 0;
        //    /*
        //     * If its not a *, print the right help string
        //     * or an error if he typed a funny character.
        //     */
        //    if (helpch != '*') {
        //        R12.move(0, 0);
        //        foreach (var strp in helpstr)
        //            if (strp.h_ch == helpch) {
        //                lower_msg = true;
        //                showAtTopOfScreen___msg("{0}{1}", R12.GetSupposedlyPrintable___unctrl((char)strp.h_ch), strp.h_desc);
        //                lower_msg = false;
        //                return;
        //            }
        //        showAtTopOfScreen___msg("unknown character '{0}'", R12.GetSupposedlyPrintable___unctrl(helpch));
        //        return;
        //    }
        //    /*
        //     * Here we print help for everything.
        //     * Then wait before we return to command mode
        //     */
        //    numprint = 0;
        //    foreach (var strp in helpstr)
        //        if (strp.h_print)
        //            numprint++;
        //    if ((numprint & 01) != 0)		/* round odd numbers up */
        //        numprint++;
        //    numprint /= 2;
        //    if (numprint > LINES - 1)
        //        numprint = LINES - 1;

        //    R12.wclear(hw);
        //    cnt = 0;
        //    foreach (var strp in helpstr)
        //        if (strp.h_print) {
        //            R12.wmove(hw, cnt % numprint, cnt >= numprint ? COLS / 2 : 0);
        //            if (strp.h_ch != (char)0)
        //                R12.AddStr(hw, R12.GetSupposedlyPrintable___unctrl((char)strp.h_ch));
        //            R12.AddStr(hw, strp.h_desc);
        //            if (++cnt >= numprint * 2)
        //                break;
        //        }
        //    R12.wmove(hw, LINES - 1, 0);
        //    R12.AddStr(hw, "--Press space to continue--");
        //    R12.Present__refresh(hw);
        //    //recent:     waitForInputOfThis___wait_for(' ');
        //    R12.clearok(R12.stdscr, true);
        //    showAtTopOfScreen___msg("");
        //    R12.touchwin(R12.stdscr);
        //    R12.Present__refresh(R12.stdscr);
        //}

        //// Tell the player what a certain thing is.
        //void identify() {
        //    int ch;
        //    h_list hp;
        //    string str;
        //    h_list[] ident_list =
        //    {
        //        new h_list(WALL_VERT,     "wall of a room",       false),
        //        new h_list(WALL_HORI,     "wall of a room",       false),
        //        new h_list(GOLD,    "gold",                 false),
        //        new h_list(STAIRS,  "a staircase",          false),
        //        new h_list(DOOR,    "door",                 false),
        //        new h_list(FLOOR,   "room floor",           false),
        //        new h_list(PLAYER,  "you",                  false),
        //        new h_list(PASSAGE, "passage",              false),
        //        new h_list(TRAP,    "trap",                 false),
        //        new h_list(POTION,  "potion",               false),
        //        new h_list(SCROLL,  "scroll",               false),
        //        new h_list(FOOD,    "food",                 false),
        //        new h_list(WEAPON,  "weapon",               false),
        //        new h_list(' ',     "solid rock",           false),
        //        new h_list(ARMOUR,   "armor",                false),
        //        new h_list(AMULET,  "the Amulet of Yendor", false),
        //        new h_list(RING,    "ring",                 false),
        //        new h_list(STICK,   "wand or staff",        false)
        //    };

        //    showAtTopOfScreen___msg("what do you want identified? ");
        //    ch = readchar();
        //    topLineCursorPos___mpos = 0;
        //    if (ch == ESCAPE) {
        //        showAtTopOfScreen___msg("");
        //        return;
        //    }
        //    if (ItemType.IsUpper((char)ch))
        //        str = monsters[ch - 'A'].Name;
        //    else {
        //        str = "unknown character";
        //        for (int i = 0; i < ident_list.Length; i++) {
        //            hp = ident_list[i];
        //            if (hp.h_ch == ch) {
        //                str = hp.h_desc;
        //                break;
        //            }
        //        }
        //    }
        //    showAtTopOfScreen___msg("'{0}': {1}", R12.GetSupposedlyPrintable___unctrl((char)ch), str);
        //}

        //// He wants to go down a level
        //void d_level() {
        //    if (levit_check())
        //        return;
        //    if (chat(hero.y, hero.x) != STAIRS)
        //        showAtTopOfScreen___msg("I see no way down");
        //    else {
        //        level++;
        //        seenstairs = false;
        //        new_level();
        //    }
        //}

        //// He wants to go up a level
        //void u_level() {
        //    if (levit_check())
        //        return;
        //    if (chat(hero.y, hero.x) == STAIRS)
        //        if (amulet) {
        //            level--;
        //            if (level == 0)
        //                total_winner();
        //            new_level();
        //            showAtTopOfScreen___msg("you feel a wrenching sensation in your gut");
        //        } else
        //            showAtTopOfScreen___msg("your way is magically blocked");
        //    else
        //        showAtTopOfScreen___msg("I see no way up");
        //}

        //// Check to see if she's levitating, and if she is, print an
        //// appropriate message.
        //bool levit_check() {
        //    if (!on(player, ISLEVIT))
        //        return false;
        //    showAtTopOfScreen___msg("You can'tag.  You're floating off the ground!");
        //    return true;
        //}

        // Allow a user to call a potion, scroll, or ring something
        static public void call() {
            //THING obj;
            //ObjectData t = null;
            //string guess, elsewise = null;
            //bool know;
            //bool isitem = false;

            //obj = get_item("call", Int.CALLABLE);
            
            ///*
            // * Make certain that it is somethings that we want to wear
            // */
            //switch (obj.ItemType) {
            //    case RING:
            //        t = ring_info[obj.Which];
            //        elsewise = r_stones[obj.Which];
            //        goto norm;
            //    case POTION:
            //        t = pot_info[obj.Which];
            //        elsewise = ColorsUsed[obj.Which];
            //        goto norm;
            //    case SCROLL:
            //        t = scr_info[obj.Which];
            //        elsewise = s_names[obj.Which];
            //        goto norm;
            //    case STICK:
            //        t = ws_info[obj.Which];
            //        elsewise = ws_made[obj.Which];
            //    norm:
            //        isitem = true;
            //        know = t.oi_know;
            //        guess = t.oi_guess;
            //        if (!string.IsNullOrEmpty(guess))
            //            elsewise = guess;
            //        break;
            //    case FOOD:
            //        showAtTopOfScreen___msg("you can'tag call that anything");
            //        return;
            //    default:
            //        guess = obj.o_label;
            //        know = false;
            //        elsewise = obj.o_label;
            //        break;
            //}
            //if (know) {
            //    showAtTopOfScreen___msg("that has already been identified");
            //    return;
            //}
            //if (!string.IsNullOrEmpty(elsewise) && elsewise == guess) {
            //    if (!Agent.Terse)
            //        addmsg("Was ");
            //    showAtTopOfScreen___msg("called \"{0}\"", elsewise);
            //}
            //if (Agent.Terse)
            //    showAtTopOfScreen___msg("call it: ");
            //else
            //    showAtTopOfScreen___msg("what do you want to call it? ");

            //if (string.IsNullOrEmpty(elsewise))
            //    prbuf = "";
            //else
            //    prbuf = elsewise;
            //string buf;
            //if (get_str(out buf, R12.stdscr) == NORM) {
            //    if (isitem)
            //        t.oi_guess = buf;
            //    else
            //        obj.o_label = buf;
            //}
        }
    }