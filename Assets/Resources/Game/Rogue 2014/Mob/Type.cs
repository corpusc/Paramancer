    // a fighting being/entity (are there any that are not mobile?  flytrap maybe?) 
    public class stats
    {
        public int Level;			/* level of mastery */
        public int Xp;				/* Experience */
        public int Str;			/* Strength */
        public int Ac;				/* Armor class */
        public int Hp;			    /* Hit points */
        public int HpMax;			/* Max hit points */
        public string Dmg;			/* describing damage done */

        public stats()
        {
        }

        public stats(int sStr, int sExp, int sLvl, int sArm, int sHpt, string sDmg)
        {
            Str = sStr;
            Xp = sExp;
            Level = sLvl;
            Ac = sArm;
            Hp = sHpt;
            Dmg = sDmg;
        }

        public stats(int sStr, int sExp, int sLvl, int sArm, int sHpt, string sDmg, int sMaxhp)
        {
            Str = sStr;
            Xp = sExp;
            Level = sLvl;
            Ac = sArm;
            Hp = sHpt;
            Dmg = sDmg;
            HpMax = sMaxhp;
        }

        public stats Copy()
        {
            return (stats)MemberwiseClone();
        }
    }