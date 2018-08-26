namespace AmeisenUtilities
{
    public abstract class WoWClass
    {
        #region Public Classes

        public abstract class DeathKnight
        {
            #region Public Fields

            public readonly int classID = 6;

            #endregion Public Fields
        }

        public abstract class Druid
        {
            #region Public Fields

            public readonly int classID = 11;

            #endregion Public Fields

            #region Public Enums

            public enum ShapeshiftForms
            {
                BEAR,
                AQUA,
                CAT,
                TRAVEL,
                MOONKIN
            }

            #endregion Public Enums
        }

        public abstract class Hunter
        {
            #region Public Fields

            public readonly int classID = 3;

            #endregion Public Fields
        }

        public abstract class Mage
        {
            #region Public Fields

            public readonly int classID = 8;

            #endregion Public Fields
        }

        public abstract class Paladin
        {
            #region Public Fields

            public readonly int classID = 2;

            #endregion Public Fields
        }

        public abstract class Priest
        {
            #region Public Fields

            public readonly int classID = 5;

            #endregion Public Fields
        }

        public abstract class Rogue
        {
            #region Public Fields

            public readonly int classID = 4;

            #endregion Public Fields

            #region Public Enums

            public enum ShapeshiftForms
            {
                STEALTH
            }

            #endregion Public Enums
        }

        public abstract class Shaman
        {
            #region Public Fields

            public readonly int classID = 7;

            #endregion Public Fields
        }

        public abstract class Warlock
        {
            #region Public Fields

            public readonly int classID = 9;

            #endregion Public Fields
        }

        public abstract class Warrior
        {
            #region Public Fields

            public readonly int classID = 1;

            #endregion Public Fields

            #region Public Enums

            public enum ShapeshiftForms
            {
                BATTLE,
                DEFENSIVE,
                BERSERKER,
            }

            #endregion Public Enums
        }

        #endregion Public Classes
    }
}