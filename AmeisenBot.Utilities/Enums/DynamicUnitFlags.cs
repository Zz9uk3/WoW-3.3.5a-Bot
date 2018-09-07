namespace AmeisenBotUtilities
{
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
}