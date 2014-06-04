using System;



	// was this from tgame or Rogue?  i foolishly started to mix the code together 
    // IntPtr arr = Marshal.AllocHGlobal (sizeof (MyStruct) * 256);
    // use unsafe memory for performance, see:
    // http://stackoverflow.com/questions/7973998/fixed-size-array-of-structure-type

    public class Coord
    {
        public int x;
        public int y;

        public Coord()
        {
        }

        public Coord(int y, int x)
        {
            this.y = y;
            this.x = x;
        }

        public override string ToString()
        {
            return string.Format("(y:{0},x:{1})", y, x);
        }
    }