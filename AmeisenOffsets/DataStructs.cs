namespace AmeisenUtilities
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public enum Interaction
    {
        MOVE = 0x4,
        INTERACT = 0x5,
        LOOT = 0x6,
        ATTACK = 0xA
    }
}
