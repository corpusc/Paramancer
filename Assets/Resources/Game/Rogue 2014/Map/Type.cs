using System;



	// describe a place on the level map 
    public class PLACE
    {
        public char p_ch;
        public int p_flags;
        public THING p_monst;
    }

    public static class Room
    {
        public static int ISDARK = Convert.ToInt32("0000001", 8); /* room is dark */
        public static int ISGONE = Convert.ToInt32("0000002", 8); /* room is gone (a corridor) */
        public static int ISMAZE = Convert.ToInt32("0000004", 8); /* room is gone (a corridor) */
    }

    public static class Char
    {
        // things that appear on the screen (except ESCAPE_CHAR) 
        public const char WALL_HORI = '-';
        public const char WALL_VERT = '|';
        public const char PASSAGE = '#';
        public const char DOOR = '+';
        public const char FLOOR = '.';
        public const char PLAYER = '@';
        public const char TRAP = '^';
        public const char STAIRS = '%';
        public const char GOLD = '*';
        public const char POTION = '!';
        public const char SCROLL = '?';
        public const char MAGIC = '$';
        public const char FOOD = ':';
        public const char WEAPON = ')';
        public const char ARMOUR = ']';
        public const char AMULET = ',';
        public const char RING = '=';
        public const char STICK = '/';
        public const char ESCAPE_CHAR = (char)27;
    }