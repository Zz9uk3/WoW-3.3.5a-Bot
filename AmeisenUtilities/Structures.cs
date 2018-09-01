namespace AmeisenUtilities
{
    /// <summary>
    /// Action types, most of them should be pretty self-explaining
    /// </summary>
    public enum AmeisenActionType
    {
        USE_SPELL,
        USE_SPELL_ON_ME,
        INTERACT_TARGET,
        MOVE_TO_POSITION,
        MOVE_NEAR_POSITION,
        FORCE_MOVE_TO_POSITION,
        FORCE_MOVE_NEAR_TARGET,
        TARGET_ENTITY,
        FACETARGET,
        GO_TO_CORPSE_AND_REVIVE
    }

    /// <summary>
    /// DynamicUnitFlags
    /// </summary>
    public enum DynamicUnitFlags
    {
        NONE = 0,
        LOOTABLE = 0x1,
        TRACKUNIT = 0x2,
        TAGGEDBYOTHER = 0x4,
        TAGGEDBYME = 0x8,
        SPECIALINFO = 0x10,
        DEAD = 0x20,
        REFERAFRIENDLINKED = 0x40,
        TAPPEDBYTHREAT = 0x80,
    }

    /// <summary>
    /// CTM Interaction types
    /// </summary>
    public enum Interaction
    {
        FACETARGET = 0x1,
        FACEDESTINATION = 0x2,
        STOP = 0x3,
        MOVE = 0x4,
        INTERACT = 0x5,
        LOOT = 0x6,
        INTERACTOBJECT = 0x7,
        FACEOTHER = 0x8,
        SKIN = 0x9,
        ATTACK = 0x10,
        ATTACKPOS = 0xA,
        ATTACKGUID = 0xB,
        WALKANDROTATE = 0x13
    }

    /// <summary>
    /// </summary>
    public enum LUAUnit
    {
        player,
        target,
        party1,
        party2,
        party3,
        party4,
    }

    /// <summary>
    /// UnitFlags
    /// </summary>
    public enum UnitFlags : int
    {
        NONE = 0,
        SITTING = 0x1,
        TOTEM = 0x10,
        NOT_ATTACKABLE = 0x100,
        LOOTING = 0x400,
        PET_IN_COMBAT = 0x800,
        PVP_FLAGGED = 0x1000,
        SILENCED = 0x4000,
        COMBAT = 0x80000,
        FLIGHTMASTER_FLIGHT = 0x100000,
        DISARMED = 0x200000,
        CONFUSED = 0x400000,
        FLEEING = 0x800000,
        SKINNABLE = 0x8000000,
        MOUNTED = 0x4000000,
        DAZED = 0x20000000
    }

    /// <summary>
    /// Weird values, need to investigate this crap but hey its working...
    ///
    /// Looks like some sort of flags to be honest...
    /// </summary>
    public enum UnitState
    {
        STANDING = 0,
        MOVING = 5,
        ROTATINGLEFT = 11,
        ROTATINGRIGHT = 12,
        AUTOHIT = 18,
        ATTACKING = 27,
        JUMPING = 37,
        LANDED = 39,
        CASTING = 54,
        SITTING = 96,
        LONGSITTING = 97,
        LOOTING = 188
    }

    /// <summary>
    /// WoW Object types
    /// </summary>
    public enum WoWObjectType
    {
        NONE = 0,
        ITEM = 1,
        CONTAINER = 2,
        UNIT = 3,
        PLAYER = 4,
        GAMEOBJECT = 5,
        DYNOBJECT = 6,
        CORPSE = 7
    }

    public struct Credentials
    {
        public string charname;
        public int charSlot;
        public string password;
        public string username;
    }

    public struct NetworkBot
    {
        public string base64Image;
        public int id;
        public string ip;
        public long lastUpdate;
        public MeSendable me;
        public string name;
    }

    /// <summary> Simple X,Y & Z struct </summary>
    public struct Vector3
    {
        public Vector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    /// WoW Buff/Debuff info
    /// </summary>
    public struct WoWAuraInfo
    {
        public int duration;
        public string expirationTime;
        public string name;
        public int stacks;
    }

    /// <summary>
    /// WoW Spell info
    /// </summary>
    public struct WoWSpellInfo
    {
        public int castTime;
        public int cost;
        public string name;
    }

    public class MeSendable
    {
        public int Energy { get; set; }
        public int Exp { get; set; }
        public ulong Guid { get; set; }
        public int Health { get; set; }
        public int Level { get; set; }
        public int MaxEnergy { get; set; }
        public int MaxExp { get; set; }
        public int MaxHealth { get; set; }
        public string Name { get; set; }
        public Vector3 Pos { get; set; }
        public float Rotation { get; set; }

        public MeSendable ConvertFromMe(Me me)
        {
            Name = me.Name;
            Guid = me.Guid;

            Pos = me.pos;
            Rotation = me.Rotation;

            Level = me.Level;

            Health = me.Health;
            MaxHealth = me.MaxHealth;

            Energy = me.Energy;
            MaxEnergy = me.MaxEnergy;

            Exp = me.Exp;
            MaxExp = me.MaxExp;
            return this;
        }
    }

    public class RegisterData
    {
        public RegisterData(string base64Image, MeSendable me)
        {
            Me = me;
            Base64Image = base64Image;
        }

        public string Base64Image { get; set; }
        public MeSendable Me { get; set; }
    }
}