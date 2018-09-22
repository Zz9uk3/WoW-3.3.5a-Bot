using AmeisenBotData;

namespace AmeisenCombatEngine.Interfaces
{
    public interface IAmeisenCombatClass
    {
        AmeisenDataHolder AmeisenDataHolder { get; set; }

        void Init();

        void HandleBuffs();

        void HandleHealing();

        void HandleTanking();

        void HandleAttacking();

        void Exit();
    }
}