namespace AmeisenUtilities
{
    /// <summary>
    /// Simple X,Y & Z struct
    /// </summary>
    public struct Vector3
    {
        public double x;
        public double y;
        public double z;
    }

    public struct Credentials
    {
        public string charname;
        public string username;
        public string password;
        public int charSlot;
    }

    /// <summary>
    /// Weird values, need to investigate this crap
    /// but hey its working... 
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
    /// LogLevels
    /// </summary>
    public enum LogLevel
    {
        VERBOSE,
        DEBUG,
        WARNING,
        ERROR
    }

    /// <summary>
    /// WoW Object types
    /// </summary>
    public enum WoWObjectType
    {
        ITEM = 1,
        CONTAINER = 2,
        UNIT = 3,
        PLAYER = 4,
        GAMEOBJECT = 5,
        DYNOBJECT = 6,
        CORPSE = 7
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
    /// Action types, most of them should be pretty
    /// self-explaining
    /// </summary>
    public enum AmeisenActionType
    {
        USE_SPELL,
        USE_SPELL_ON_ME,
        INTERACT_TARGET,
        MOVE_TO_POSITION,
        MOVE_NEAR_TARGET,
        FORCE_MOVE_TO_POSITION,
        FORCE_MOVE_NEAR_TARGET,
        TARGET_ENTITY
    }

    /// <summary>
    /// UnitFlags
    /// </summary>
    public enum UnitFlags
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
    /// WoW Buff/Debuff info
    /// </summary>
    public struct WoWAuraInfo
    {
        public string name;
        public int stacks;
        public int duration;
        public string expirationTime;
    }

    /// <summary>
    /// WoW Spell info
    /// </summary>
    public struct WoWSpellInfo
    {
        public string name;
        public int castTime;
        public int cost;
    }
}
