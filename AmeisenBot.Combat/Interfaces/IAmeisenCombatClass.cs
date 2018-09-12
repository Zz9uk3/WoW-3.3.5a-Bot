namespace AmeisenCombatEngine.Interfaces
{
    public interface IAmeisenCombatClass
    {
        void Init();
        void HandleBuffs();
        void HandleHealing();
        void HandleTanking();
        void HandleAttacking();
        void Exit();
    }
}
