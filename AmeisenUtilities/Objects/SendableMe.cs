namespace AmeisenUtilities
{
    public class SendableMe
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

        public SendableMe ConvertFromMe(Me me)
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
}