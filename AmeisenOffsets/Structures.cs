namespace AmeisenUtilities
{
    /// <summary>
    /// Simple X,Y & Z struct
    /// </summary>
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
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
        GAMEOBJ = 5,
        DYNOBJ = 6,
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
        MOVE_TO_GROUPLEADER,
        LOOT_TARGET,
        TARGET_ENTITY,
        TARGET_MYSELF,
        ATTACK_TARGET,
        USE_SPELL,
        INTERACT_TARGET,
        MOVE_TO_TARGET,
    }
}
