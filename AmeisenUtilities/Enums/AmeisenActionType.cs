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
}