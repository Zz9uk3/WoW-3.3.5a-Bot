namespace AmeisenCore.Objects
{
    public abstract class WoWClass
    {
        public abstract class Warrior
        {
            public readonly int classID = 1;
            public enum ShapeshiftForms
            {
                BATTLE,
                DEFENSIVE,
                BERSERKER,
            }
        }

        public abstract class Paladin
        {
            public readonly int classID = 2;
        }

        public abstract class Hunter
        {
            public readonly int classID = 3;
        }

        public abstract class Rogue
        {
            public readonly int classID = 4;
            public enum ShapeshiftForms
            {
                STEALTH
            }
        }

        public abstract class Priest
        {
            public readonly int classID = 5;
        }

        public abstract class DeathKnight
        {
            public readonly int classID = 6;
        }

        public abstract class Shaman
        {
            public readonly int classID = 7;
        }

        public abstract class Mage
        {
            public readonly int classID = 8;
        }

        public abstract class Warlock
        {
            public readonly int classID = 9;
        }

        public abstract class Druid
        {
            public readonly int classID = 11;
            public enum ShapeshiftForms
            {
                BEAR,
                AQUA,
                CAT,
                TRAVEL,
                MOONKIN
            }
        }
    }
}
