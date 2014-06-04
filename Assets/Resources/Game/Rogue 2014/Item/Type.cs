using System;
using System.Collections.Generic;



    public enum ItemType
    {
        Potion,
        Scroll,
        Food,
        Weapon,
        Armour,
        Ring,
        Stick,
        MAX,
    }

    public class ObjectData
    {
        public string Name;
        public int Probability;
        public int Worth;
        public string Guess;
        public bool Know;

        public ObjectData(string name, int probability, int worth, string guess, bool know)
        {
            Name = name;
            Probability = probability;
            Worth = worth;
            Guess = guess;
            Know = know;
        }

        public ObjectData(int probability)
        {
            Probability = probability;
        }
    }