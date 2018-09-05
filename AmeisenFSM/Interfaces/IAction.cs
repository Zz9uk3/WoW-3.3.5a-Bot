using static AmeisenFSM.Objects.Delegates;

namespace AmeisenFSM.Interfaces
{
    public interface IAction
    {
        Start StartAction { get; }
        DoThings StartDoThings { get; }
        Exit StartExit { get; }
    }
}