using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Interfaces
{
    public interface IAction
    {
        Start StartAction { get; }
        DoThings StartDoThings { get; }
        Exit StartExit { get; }
    }
}