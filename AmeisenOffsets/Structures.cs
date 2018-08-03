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
        MOVE = 0x4,
        INTERACT = 0x5,
        LOOT = 0x6,
        ATTACK = 0xA
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
