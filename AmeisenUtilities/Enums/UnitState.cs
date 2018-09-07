namespace AmeisenBotUtilities
{
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
}