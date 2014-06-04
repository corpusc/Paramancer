    static partial class Dungeon
    {
        // trap types 
        private const int T_DOOR = 00;
        private const int T_ARROW = 01;
        private const int T_SLEEP = 02;
        private const int T_BEAR = 03;
        private const int T_TELEP = 04;
        private const int T_DART = 05;
        private const int T_RUST = 06;
        private const int T_MYST = 07;
        private const int NTRAPS = 8;

        public static string[] tr_name = 
        {			/* Names of the traps */
            "a trapdoor",
            "an arrow trap",
            "a sleeping gas trap",
            "a beartrap",
            "a teleport trap",
            "a poison dart trap",
            "a rust trap",
            "a mysterious trap"
        };
    }