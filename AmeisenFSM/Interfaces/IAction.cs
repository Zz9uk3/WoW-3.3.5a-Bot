using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
