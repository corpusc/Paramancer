using System;



    public interface IRandomNumberGenerator
    {
        int Next(string key, int range);
    }

    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        public static IRandomNumberGenerator rng;

        public RandomNumberGenerator() : this((int)DateTime.Now.Ticks)
        {
        }
        public RandomNumberGenerator(int seed)
        {
            this.seed = seed;
        }

        private int seed;

        public virtual int Next(string key, int range)
        {
            return rnd(range);
        }

        private int rnd(int range)
        {
            return range == 0 ? 0 : Math.Abs(RN()) % range;
        }

        private int RN()
        {
            return (((seed = seed * 11109 + 13849) >> 16) & 0xffff);
        }
    }