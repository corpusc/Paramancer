using System;



	public class ArmourData : ObjectData
    {
        public int AC; //armour class

        public ArmourData(int ac, string name, int probability, int worth, string guess, bool know)
                                   : base (name, probability, worth, guess, know)
        {
            AC = ac;
        }
    }

    //not sure this enum will ever be needed.  SPLINT_MAIL was never used
    enum ArmourType 
    {
        Leather,
        RingMail,
        StuddedLeather,
        ScaleMail,
        ChainMail,
        SplintMail,
        BandedMail,
        PlateMail,
        Max
    };

    static class Armour
    {
        public static ArmourData[] Data = new[]
        {
            new ArmourData(8, "leather armour", 20, 20, null, false),
            new ArmourData(7, "ring mail", 15, 25, null, false),
            new ArmourData(7, "studded leather armour", 15, 20, null, false),
            new ArmourData(6, "scale mail", 13, 30, null, false),
            new ArmourData(5, "chain mail", 12, 75, null, false),
            new ArmourData(4, "splint mail", 10, 80, null, false),
            new ArmourData(4, "banded mail", 10, 90, null, false),
            new ArmourData(3, "plate mail", 5, 150, null, false),
        };

        static void wear()
        {
            //THING obj;
            //string sp;

            //if ((obj = get_item("wear", ARMOUR)) == null)
            //    return;

            //if (cur_armor != null)
            //{
            //    S.Hud.Log.Add("you are already wearing some");
                
            //    if (!Agent.Terse)
            //        S.Hud.Log.Add(".  You'll have to take it off first");

            //    after = false;
            //    return;
            //}
            //if (obj.ItemType != ARMOUR)
            //{
            //    S.Hud.Log.Add("you can'tag wear that");
            //    return;
            //}
            //waste_time();
            //obj.o_flags |= ISKNOW;
            //sp = GetDescription(obj, true);
            //cur_armor = obj;

            //if (!Agent.Terse)
            //    S.Hud.Log.Add("you are now ");

            //S.Hud.Log.Add("wearing {0}", sp);
        }

        static void take_off()
        {
            //THING obj;

            //if ((obj = cur_armor) == null)
            //{
            //    after = false;

            //    if (Agent.Terse)
            //        AddToLog("not wearing armor");
            //    else
            //        AddToLog("you aren'tag wearing any armor");

            //    return;
            //}
            //if (!detachable(cur_armor))
            //    return;

            //cur_armor = null;
            
            //if (Agent.Terse)
            //    S.Hud.Log.Add("was");
            //else
            //    S.Hud.Log.Add("you used to be");

            //S.Hud.Log.Add(" wearing {0}) {1}", obj.CharInPack, GetDescription(obj, true));
        }

        //static void waste_time()
        //{
        //    do_daemons(BEFORE);
        //    do_fuses(BEFORE);
        //    do_daemons(AFTER);
        //    do_fuses(AFTER);
        //}
    }