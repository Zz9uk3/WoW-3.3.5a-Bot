namespace AmeisenBotUtilities
{
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
}